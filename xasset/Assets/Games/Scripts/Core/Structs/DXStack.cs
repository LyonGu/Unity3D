using System;

namespace DXGame.structs
{
    /// <summary>
    /// 使用数组封装的一个结构，支持多线程是亮点
    /// 支持末尾添加和删除元素
    /// 支持末尾获取元素
    /// 支持指定下标删除元素
    /// </summary>

    public sealed class DXStack<T>
    {
        object _syncObject;

        bool _sync = false;

        public bool sync
        {
            get
            {
                return _sync;
            }
        }

        T[] _buffer;

        int _size = 0;

        public int Count
        {
            get
            {
                return _size;
            }
        }

        public int Capacity
        {
            get
            {
                if (_buffer == null)
                    return 0;
                return _buffer.Length;
            }
        }

        const int DEFAULT_SIZE = 8;

        public DXStack()
        {
            _buffer = new T[DEFAULT_SIZE];
        }

        public DXStack(int capacity, bool sync = false)
        {
            //支持多线程
            _buffer = new T[capacity];
            _sync = sync;
            if (_sync)
                _syncObject = new object();
        }

        void AllocateMore()
        {
            if (_buffer == null)
            {
                _buffer = new T[DEFAULT_SIZE];
            }
            else
            {
                //2倍扩容
                int len = _buffer.Length << 1;
                if (len < DEFAULT_SIZE) len = DEFAULT_SIZE;
                T[] newList = new T[len];
                _buffer.CopyTo(newList, 0);
                _buffer = null;
                _buffer = newList;
            }
        }

        public void Clear()
        {
            if (_sync)
            {
                lock (_syncObject)
                {
                    InternalClear();
                }
            }
            else
            {
                InternalClear();
            }
        }

        void InternalClear()
        {
            Array.Clear(_buffer, 0, _buffer.Length);
            _size = 0;
        }

        //加一个元素到末尾
        public void Push(T item)
        {
            if (_sync)
            {
                lock (_syncObject)
                {
                    InternalPush(item);
                }
            }
            else
            {
                InternalPush(item);
            }
        }

        void InternalPush(T item)
        {
            if (_size == _buffer.Length) AllocateMore();
            _buffer[_size] = item;
            _size++;
        }
        //从末尾拿一个元素，会从数组中删除元素（对应的内存被复写）
        public T Pop()
        {
            if (_sync)
            {
                lock (_syncObject)
                {
                    return InternalPop();
                }
            }
            else
            {
                return InternalPop();
            }
        }

        T InternalPop()
        {
            if (_size == 0) return default(T);

            int tailIndex = _size - 1;
            T t = _buffer[tailIndex];
            _size--;
            _buffer[tailIndex] = default(T);
            return t;
        }

        //返回最后一个元素，不会从数组里删除
        public T Top
        {
            get
            {
                if (_size == 0) return default(T);

                T t = _buffer[_size - 1];
                return t;
            }
        }

        public T RemoveAtInternal(int index)
        {
            if (index < 0 || index >= this._size)
                throw new ArgumentOutOfRangeException();

            T v = this._buffer[index];
            //一个扩展方法，意思就是把数组后面的元素都往前挪一位
            this._buffer.Move(index + 1, index, this._size - index - 1);
            this._size--;
            return v;
        }

        public T RemoveAt(int index)
        {
            if (this._sync)
            {
                lock (_syncObject)
                {
                    return RemoveAtInternal(index);
                }
            }
            else
            {
                return RemoveAtInternal(index);
            }
        }
    }
}
