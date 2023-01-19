using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameTimer
{
    public class TimerManager: Singleton<TimerManager>
    {
        private TickTimer _tickTimer;
        private FrameTimer _frameTimer;

        public void Init()
        {
            TickTimer timer = new TickTimer(0, false);
            _tickTimer = timer;
            
            FrameTimer frameTimer = new FrameTimer((ulong)Time.frameCount);
            _frameTimer = frameTimer;

        }

        public void Update()
        {
            if (_tickTimer != null)
            {
                _tickTimer.UpdateTask();
                _tickTimer.HandleTask();
            }

            if (_frameTimer != null)
            {
                _frameTimer.UpdateTask();
            }
        }


        public void Clear()
        {
            if (_tickTimer != null)
            {
                _tickTimer.Reset();
                _tickTimer = null;
            }
            
            if (_frameTimer != null)
            {
                _frameTimer.Reset();
                _frameTimer = null;
            }
        }


        #region 定时器操作
        /// <summary>
        /// 添加一个定时任务，按时间计算
        /// </summary>
        /// <param name="delay">延迟时间，单位毫秒</param>
        /// <param name="taskCB">任务执行回调</param>
        /// <param name="cancelCB">任务取消回到</param>
        /// <param name="count">重复次数，-1为循环调用</param>
        /// <returns></returns>
        public int AddTickTimerTask(uint delay, Action<int> taskCB, Action<int> cancelCB = null, int count = 1)
        {
            if (_tickTimer != null)
            {
                int tid = _tickTimer.AddTask(delay, taskCB, cancelCB, count);
                return tid;
            }
            return -1;
        }
        
        public int AddTickTimerTask(GameObject obj, uint delay, Action<int> taskCB, Action<int> cancelCB = null, int count = 1)
        {
            if (_tickTimer != null)
            {
                int tid = _tickTimer.AddTask(delay, taskCB, cancelCB, count);
                TimerTickObserver observer = obj.AddMissingComponent<TimerTickObserver>();
                observer.timerId = tid;
                return tid;
            }
            return -1;
        }
        
        /// <summary>
        /// 取消一个定时任务
        /// </summary>
        /// <param name="tid">任务id</param>
        public bool RemoveTickTimerTask(int tid)
        {
            if (_tickTimer != null)
            {
                return _tickTimer.DeleteTask(tid);
            }

            return false;
        }
        
        
        /// <summary>
        /// 添加一个定时任务，按帧数计算
        /// </summary>
        /// <param name="delay">延迟帧数</param>
        /// <param name="taskCB">任务执行回调</param>
        /// <param name="cancelCB">任务取消回到</param>
        /// <param name="count">重复次数，-1为循环调用</param>
        /// <returns></returns>
        public int AddFrameTimerTask(uint delay, Action<int> taskCB, Action<int> cancelCB = null, int count = 1)
        {
            if (_frameTimer != null)
            {
                int tid = _frameTimer.AddTask(delay, taskCB, cancelCB, count);
                return tid;
            }
            return -1;
        }
        
        public int AddFrameTimerTask(GameObject obj, uint delay, Action<int> taskCB, Action<int> cancelCB = null, int count = 1)
        {
            if (_frameTimer != null)
            {
                int tid = _frameTimer.AddTask(delay, taskCB, cancelCB, count);
                TimerFrameObserver observer = obj.AddMissingComponent<TimerFrameObserver>();
                observer.timerId = tid;
                return tid;
            }
            return -1;
        }
        
        /// <summary>
        /// 取消一个定时任务
        /// </summary>
        /// <param name="tid">任务id</param>
        public bool RemoveFrameTimerTask(int tid)
        {
            if (_frameTimer != null)
            {
                return _frameTimer.DeleteTask(tid);
            }

            return false;
        }

        #endregion
    }
}