using DXGame;
using DXGame.structs;
using System;
using System.Diagnostics;

namespace DXExtension.Struct
{
    public interface IBlockItem
    {
        bool disabled { get; set; }
        void Run(Action complete, Action<float> progress);
    }
    public class BlockList
    {
        public enum Statu
        {
            Idle,
            Working,
            Error,
            Completed,
        }

        Statu _statu = Statu.Idle;
        public Statu statu
        {
            get
            {
                return _statu;
            }
        }

        IBlockItem _curItem;
        DXQueue<IBlockItem> _tasks;
        Action _completeCallback;
        Action<IBlockItem, int> _completeOneCallback;
        int _totalCount = 0;
        int _completeCount = 0;

        float _curPer;
        Stopwatch _sw;

        public float progress
        {
            get
            {
                if (_totalCount == 0) return 1f;

                return 1f * _completeCount / _totalCount + _curPer / _totalCount;
            }
        }

        public int currentCount
        {
            get
            {
                return _tasks.Count;
            }
        }
        public int totalCount
        {
            get
            {
                return _totalCount;
            }
        }

        public BlockList()
        {
            _tasks = new DXQueue<IBlockItem>();
        }
        public BlockList(Action onAllDone = null, Action<IBlockItem, int> onOneDone = null)
        {
            _tasks = new DXQueue<IBlockItem>();

            _completeCallback = onAllDone;
            _completeOneCallback = onOneDone;
        }

        public void Add(IBlockItem task)
        {
            if (_statu == Statu.Working)
            {
//                XDebug.LogError($"BlokList is working,cannot do Add");
                return;
            }

            _tasks.Enqueue(task);
            _totalCount++;
        }

        public bool Start()
        {
            if (_tasks.Count == 0) return false;

            if (_statu == Statu.Working) return false;
            _statu = Statu.Working;

            if (_sw == null)
                _sw = new Stopwatch();

            RunOneTask();

            return true;
        }

        public void Stop()
        {
            _statu = Statu.Idle;
        }

        public void Reset()
        {
            _tasks.Clear();
            _completeCount = _totalCount = 0;
            _statu = Statu.Idle;
        }


        private void TryComplete()
        {
            if (_statu == Statu.Completed) return;

            if (_completeCount == _totalCount)
            {
                _statu = Statu.Completed;
                _completeCallback?.Invoke();
                _completeCount = _totalCount = 0;
            }
        }
        private void RunOneTask()
        {
            while (_curItem == null && currentCount > 0)
            {
                _curItem = _tasks.Dequeue();
                if (_curItem != null && !_curItem.disabled)
                {
                    _sw.Restart();
                    try
                    {
                        _curItem.Run(onOneDone, onOnePrecnt);
                    }
                    catch (Exception e)
                    {
//                        XDebug.LogException(new DefaultDebugContext(this), e);
                        _statu = Statu.Error;
                        return;
                    }
                    break;
                }
                else
                {
                    _curItem = null;
                    _completeCount++;
                }
            }

            TryComplete();
        }

        private void onOneDone()
        {
            int useTime = (int)(_sw.ElapsedMilliseconds);
            _curPer = 0f;
            _completeCount++;
            var completeItem = _curItem;
            _curItem = null;

            if (_completeOneCallback != null)
            {
                try
                {
                    _completeOneCallback(completeItem, useTime);
                }
                catch (Exception e)
                {
//                    XDebug.LogException(new DefaultDebugContext(this), e);
                    return;
                }
            }

            TryComplete();

            if (_statu == Statu.Working)
                RunOneTask();
        }
        private void onOnePrecnt(float obj)
        {
            _curPer = obj;
        }

    }
}
