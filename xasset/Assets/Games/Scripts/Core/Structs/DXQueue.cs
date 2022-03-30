using System;

namespace DXGame.structs
{

    /// <summary>
    /// 使用数组封装的一个结构，支持多线程，最大亮点是移除一个头部元素不会进行Array.Copy
    /// 支持头尾获取元素 head，tail
    /// 支持末尾添加元素 Enqueue
    /// 支持从头部拿出一个元素，不会进行Array.copy  ==》 Dequeue
    /// </summary>
    public sealed class DXQueue<T>
    {
        private T[] _buffer;
        private int _size = 0;
        private int _head = 0;
        private int _tail = 0;

        public int Count
        {
            get
            {
                return this._size;
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
        object _syncObj;

        bool _sync = false;

        public bool isSync
        {
            get
            {
                return _sync;
            }
        }

        public T head
        {
            get
            {
                if (Count == 0) return default(T);
                return _buffer[this._head];
            }
        }

        public T tail
        {
            get
            {
                if (Count == 0) return default(T);
                return _buffer[this._tail - 1];
            }
        }

        public DXQueue()
        {
            _buffer = new T[DEFAULT_SIZE];
        }

        public DXQueue(int capacity, bool sync = false)
        {
            _buffer = new T[capacity];
            _sync = sync;
            if (_sync)
                _syncObj = new object();
        }

        private void SetCapacity(int capacity)
        {
            T[] objArray = new T[capacity];
            if (this._size > 0)
            {
                if (this._head < this._tail)
                {
                    Array.Copy(this._buffer, this._head, (Array)objArray, 0, this._size);
                }
                else
                {
                    Array.Copy(this._buffer, this._head, (Array)objArray, 0, this._buffer.Length - this._head);
                    Array.Copy(this._buffer, 0, (Array)objArray, this._buffer.Length - this._head, this._tail);
                }
            }

            this._buffer = objArray;
            this._head = 0;
            this._tail = this._size == capacity ? 0 : this._size;
        }

        public void Clear()
        {
            if (Count == 0) return;

            if (_sync)
            {
                lock (_syncObj)
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
            if (this._head < this._tail)
            {
                Array.Clear(this._buffer, this._head, this._size);
            }
            else
            {
                Array.Clear(this._buffer, this._head, this._buffer.Length - this._head);
                Array.Clear(this._buffer, 0, this._tail);
            }

            this._head = 0;
            this._tail = 0;
            this._size = 0;
        }

        //末尾添加一个元素，然后移动游标位置this._tail
        public void Enqueue(T item)
        {
            if (_sync)
            {
                lock (_syncObj)
                {
                    InternalEnqueue(item);
                }
            }
            else
                InternalEnqueue(item);
        }

        void InternalEnqueue(T item)
        {
            if (this._size == this._buffer.Length)
            {
                int capacity = this._buffer.Length << 1;
                if (capacity < DEFAULT_SIZE)
                    capacity = DEFAULT_SIZE;
                this.SetCapacity(capacity);
            }

            this._buffer[this._tail] = item;
            this._tail = (this._tail + 1) % this._buffer.Length;
            ++this._size;
        }

        //从头部拿出一个元素，不会进行Array.copy
        //只是单纯的复写了对应内存，然后移动游标位置this._head
        public T Dequeue()
        {
            if (Count == 0)
                return default;

            if (_sync)
            {
                lock (_syncObj)
                {
                    return InternalDeQueue();
                }
            }
            else
                return InternalDeQueue();
        }

        T InternalDeQueue()
        {
            T obj = this._buffer[this._head];
            this._buffer[this._head] = default(T);
            this._head = (this._head + 1) % this._buffer.Length;
            --this._size;
            return obj;
        }
    }
}
