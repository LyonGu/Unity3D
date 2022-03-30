using System;
using System.Collections.Generic;
using System.Threading;
using DXGame;

namespace DXExtension.Struct
{
    //并行单元数据接口
    public interface IParallelItem
    {
        float progress { get; }
        bool isDone { get; }
        bool isError { get; }
        string error { get; }

        /// <summary>
        /// execute in any thread  任一线程执行
        /// </summary>
        void Run();

        /// <summary>
        /// execute in main thread 主线程执行
        /// </summary>
        void Update();

        /// <summary>
        /// execute in main thread 主线程执行
        /// </summary>
        void Dispose();
    }

    public class ParallelList
    {
        bool _runnig = false;

        public bool running
        {
            get { return _runnig; }
        }

        int _totalCount = 0;

        public int totalCount
        {
            get
            {
                return _totalCount;
            }
        }

        int _doneCount = 0;

        public int completeCount
        {
            get
            {
                return _doneCount - _errorTasks.Count;
            }
        }

        public float totalProgress
        {
            get
            {
                if (totalCount == 0) return 1f;

                return 1f * _doneCount / totalCount + _curPer / totalCount;
            }
        }

        public float completedProgress
        {
            get
            {
                if (totalCount == 0) return 1f;

                return 1f * (_doneCount - _errorTasks.Count) / totalCount + _curPer / totalCount;
            }
        }

        bool _isCompleted = false;

        public bool isCompleted
        {
            get
            {
                return _isCompleted;
            }
        }

        public int errorCount
        {
            get
            {
                return _errorTasks.Count;
            }
        }

        //等待执行对列
        List<IParallelItem> _waitExeTasks;
        //执行遇到错误对列
        List<IParallelItem> _errorTasks;
        //正在执行对列
        List<IParallelItem> _executeTasks;

        Action<List<IParallelItem>> _completeCallback;
        Action<IParallelItem> _completeOneCallback;
        float _curPer;
        int _maxExeNum;
        bool _autoExecute = false;
        bool _useMainThread = true;
        private bool _ignoreError = false;

        public ParallelList(int maxExeNum = -1, bool useMainThread = true, bool ignoreError = false)
        {
            _maxExeNum = maxExeNum < 0 ? 32 : maxExeNum;
            _waitExeTasks = new List<IParallelItem>(_maxExeNum);
            _errorTasks = new List<IParallelItem>(_maxExeNum);
            _executeTasks = new List<IParallelItem>(_maxExeNum);
            _useMainThread = useMainThread; //默认使用主线程
            _ignoreError = ignoreError;
        }

        //添加执行单元
        public void addNode(IParallelItem task)
        {
            _waitExeTasks.Add(task); //update方法里会填充到_executeTasks队列里
            _totalCount++;
        }

        public void removeWaitNode(IParallelItem task)
        {
            int index = _waitExeTasks.IndexOf(task);
            if (index > 0)
            {
                _waitExeTasks.RemoveAt(index);
                _totalCount--;
            }
        }

        public void removeErrorNode()
        {
            if (_runnig || _totalCount == 0 || _errorTasks.Count == 0) return;
            _totalCount -= _errorTasks.Count;
            _errorTasks.Clear();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="completedAllCallback">执行完所有回调</param>
        /// <param name="completeOneCallback">执行完单个回调</param>
        public void Start(Action<List<IParallelItem>> completedAllCallback, Action<IParallelItem> completeOneCallback)
        {
            if (_runnig)
            {
                return;
            }

            //设置完成一个任务回调和完成所有任务的回调
            _completeCallback = completedAllCallback;
            _completeOneCallback = completeOneCallback;

            if (_errorTasks.Count > 0)
            {
                _waitExeTasks.AddRange(_errorTasks);
                _errorTasks.Clear();
            }

            _isCompleted = false;
            _runnig = true;
            _autoExecute = true;
        }

        //GameMain的Update方法里会调用
        public void Update()
        {
            if (!_runnig) return;

            if (_autoExecute)
            {
                //填充executeTasks对列并自行task.run方法，异步的化丢到线程池里去
                FillExeList();
            }
            else
            {
                CheckGroupComplete();
            }

            int len = _executeTasks.Count;
            if (len > 0)
            {
                //当前进度
                _curPer = 0f;
                for (int i = len - 1; i >= 0; i--)
                {
                    var task = _executeTasks[i];
                    _curPer += task.progress; //累计当前进度
                    if (task.isError || task.isDone)
                    {
                        //出错或者做完就从队列里移出
                        _executeTasks.RemoveAt(i);
                        _doneCount++;
                        if (task.isError)
                        {
                            _autoExecute = _ignoreError;
//                            XDebug.LogError(new DefaultDebugContext(this), task.error);
                            _errorTasks.Add(task);
                        }

                        if (_completeOneCallback != null)
                            _completeOneCallback(task);

                        task.Dispose();
                    }
                    else
                        task.Update();
                }

                _curPer /= len;
            }
        }

        public List<IParallelItem> Clear()
        {
            List<IParallelItem> lst = null;
            if (_totalCount > _doneCount)
            {
                lst = new List<IParallelItem>();
                if (_waitExeTasks.Count > 0)
                {
                    foreach (var item in _waitExeTasks)
                    {
                        item.Dispose();
                    }

                    lst.AddRange(_waitExeTasks);
                }

                if (_executeTasks.Count > 0)
                {
                    foreach (var item in _executeTasks)
                    {
                        item.Dispose();
                    }

                    lst.AddRange(_executeTasks);
                }

                if (_errorTasks.Count > 0)
                {
                    lst.AddRange(_errorTasks);
                }
            }

            _runnig = false;
            _completeCallback = null;
            _completeOneCallback = null;
            _doneCount = _totalCount = 0;
            _autoExecute = false;

            _waitExeTasks.Clear();
            _errorTasks.Clear();
            _executeTasks.Clear();
            return lst;
        }

        void FillExeList()
        {
            while (_waitExeTasks.Count > 0 && _executeTasks.Count < _maxExeNum)
            {
                //从等待对列里移出加入到执行对列里
                //从后往前移出，避免数组移动复制
                int index = _waitExeTasks.Count - 1;
                var task = _waitExeTasks[index];
                _waitExeTasks.RemoveAt(index);
                _executeTasks.Add(task);

                if (_useMainThread)
                {
                    try
                    {
                        task.Run();
                    }
                    catch (Exception e)
                    {
//                        XDebug.LogError($"{task.GetType()} error={e.ToString()}");
                        _doneCount++; //出错了就算执行完了
                        _errorTasks.Add(task);
                        task.Dispose();
                        continue;
                    }
                }
                else
                {
                    //使用多线程执行
                    ThreadPool.QueueUserWorkItem((t) =>
                    {
                        try
                        {
                            (t as IParallelItem).Run();
                        }
                        catch (Exception e)
                        {
//                            XDebug.LogException(new DefaultDebugContext(this), e);
                            _doneCount++;
                            _errorTasks.Add(task);
                            task.Dispose();
                        }
                    }, task); //task就是callback需要的参数t
                }
            }

            CheckGroupComplete();
        }

        void CheckGroupComplete()
        {
            if (_executeTasks.Count == 0)
            {
                _runnig = false;
                _isCompleted = true;
                if (_completeCallback != null)
                    _completeCallback(_errorTasks);

                _completeOneCallback = null;
                _completeCallback = null;
            }
        }
    }
}
