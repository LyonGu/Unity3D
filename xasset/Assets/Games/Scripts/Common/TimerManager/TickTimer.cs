using System;
using System.Collections.Concurrent;
using System.Threading;
using GameLog;

namespace GameTimer
{
    public class TickTimer:Timer
    {
        class TickTaskPack {
            public int tid;
            public Action<int> cb;
            public TickTaskPack(int tid, Action<int> cb) {
                this.tid = tid;
                this.cb = cb;
            }
        }
        
        class TickTask {
            public int tid;
            public uint delay;
            public int count;
            public double destTime;
            public Action<int> taskCB;
            public Action<int> cancelCB;

            public double startTime;
            public ulong loopIndex;

            public TickTask(int tid, uint delay, int count, double destTime, Action<int> taskCB, Action<int> cancelCB, double startTime)
            {
                this.tid = tid;
                this.delay = delay;
                this.count = count;
                this.destTime = destTime;
                this.taskCB = taskCB;
                this.cancelCB = cancelCB;
                this.startTime = startTime;
                this.loopIndex = 0;
            }
        }
        
        private readonly DateTime startDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        private readonly ConcurrentDictionary<int, TickTask> taskDic;
        private readonly bool setHandle;
        private readonly ConcurrentQueue<TickTaskPack> packQue;
        private const string tidLock = "TickTimer_tidLock";
        private readonly Thread timerThread;
        
        public TickTimer(int interval = 0, bool setHandle = true)
        {
            taskDic = new ConcurrentDictionary<int, TickTask>();
            this.setHandle = setHandle;
            if(setHandle)
                packQue = new ConcurrentQueue<TickTaskPack>();

            if (interval != 0)
            {
                //使用独立线程驱动
                void StartTick() {
                    try {
                        while(true) {
                            UpdateTask();
                            Thread.Sleep(interval);
                        }
                    }
                    catch(ThreadAbortException e) {
                        LogUtils.Warn($"Tick Thread Abort:{e}.");
                    }
                }
                timerThread = new Thread(new ThreadStart(StartTick));
                timerThread.Start();
            }
        }

        public override int AddTask(uint delay, Action<int> taskCB, Action<int> cancelCB, int count = 1)
        {
            int tid = GenerateTid();
            double startTime = GetUTCMilliseconds();
            double destTime = startTime + delay;
            TickTask task = new TickTask(tid, delay, count, destTime, taskCB, cancelCB, startTime);
            if(taskDic.TryAdd(tid, task)) {
                return tid;
            }
            else 
            {
                LogUtils.Warn($"key:{tid} already exist.");
                return -1;
            }
        }

        public override bool DeleteTask(int tid)
        {
            if (taskDic.TryRemove(tid, out TickTask task))
            {
                if(setHandle && task.cancelCB != null) {
                    packQue.Enqueue(new TickTaskPack(tid, task.cancelCB));
                }
                else {
                    task.cancelCB?.Invoke(tid);
                }
                return true;
            }
            else
            {
                LogUtils.Warn($"tid:{tid} remove failed.");
                return false;
            }
        }

        public override void Reset()
        {
            if(null != packQue && !packQue.IsEmpty) {
                LogUtils.Warn("Callback Queue is not Empty.");
            }
            taskDic?.Clear();
            if(timerThread != null) {
                timerThread.Abort();
            }
        }

        public void UpdateTask()
        {
            double nowTime = GetUTCMilliseconds();
            foreach (var item in taskDic)
            {
                TickTask task = item.Value;
                if(nowTime < task.destTime) {
                    continue;
                }
                
                ++task.loopIndex;
                if(task.count > 0) {
                    --task.count;
                    if(task.count == 0) {
                        FinsisTask(task.tid);
                    }
                    else {
                        task.destTime = task.startTime + task.delay * (task.loopIndex + 1);
                        CallTaskCB(task.tid, task.taskCB);
                    }
                }
                else {
                    task.destTime = task.startTime + task.delay * (task.loopIndex + 1);
                    CallTaskCB(task.tid, task.taskCB);
                }
            }
        }
        public void HandleTask() {
            while(packQue != null && packQue.Count > 0) {
                if(packQue.TryDequeue(out TickTaskPack pack)) {
                    pack.cb.Invoke(pack.tid);
                }
                else {
                    LogUtils.Error("packQue Dequeue Data Error.");
                }
            }
        }
        protected override int GenerateTid()
        {
            lock(tidLock) {
                while(true) {
                    ++tid;
                    if(tid == int.MaxValue) {
                        tid = 0;
                    }
                    if(!taskDic.ContainsKey(tid)) {
                        return tid;
                    }
                }
            }
        }
        
        private double GetUTCMilliseconds() {
            TimeSpan ts = DateTime.UtcNow - startDateTime;
            return ts.TotalMilliseconds;
        }
        
        void FinsisTask(int tid) {
            //线程安全字典，遍历过程中删除无影响。
            if(taskDic.TryRemove(tid, out TickTask task)) {
                CallTaskCB(tid, task.taskCB);
                task.taskCB = null;
            }
            else {
                LogUtils.Warn($"Remove tid:{tid} task in Dic Failed.");
            }
        }
        
        void CallTaskCB(int tid, Action<int> taskCB) {
            if(setHandle) {
                packQue.Enqueue(new TickTaskPack(tid, taskCB));
            }
            else {
                taskCB.Invoke(tid);
            }
        }
    }
}
