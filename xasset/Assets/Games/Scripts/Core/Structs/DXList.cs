using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DXGame.structs
{
    /// <summary>
    ///  注释用数组简单封装了下，跟使用C#自带的List没什么区别
    /// </summary>

    public delegate bool BoolAction<T>(T t);
    public delegate bool BoolAction<T0, T1>(T0 t0, T1 t1);
    public sealed class DXList<T>
    {
        T[] _buffer;

        int _count = 0;
        public int Count
        {
            get
            {
                return this._count;
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
        bool _operatorLock = false;
        public DXList()
        {
            _buffer = new T[DEFAULT_SIZE];
        }

        public DXList(int capacity)
        {
            _buffer = new T[capacity];

        }

        /// <summary>
        /// For 'foreach' functionality.
        /// </summary>

        [DebuggerHidden]
        [DebuggerStepThrough]
        public IEnumerator<T> GetEnumerator()
        {
            if (_buffer != null)
            {
                for (int i = 0; i < this._count; ++i)
                {
                    yield return _buffer[i];
                }
            }
        }

        [DebuggerHidden]
        public T this[int i]
        {
            get
            {
                if (i < 0 || i >= this._count)
                    throw new ArgumentOutOfRangeException("index");

                return _buffer[i];
            }
            set
            {
                if (i < 0 || i >= this._count)
                    throw new ArgumentOutOfRangeException("index");

                _buffer[i] = value;
            }
        }

        void AllocateMore()
        {
            int len = _buffer.Length << 1;
            if (len < DEFAULT_SIZE) len = DEFAULT_SIZE;
            T[] newList = new T[len];
            _buffer.CopyTo(newList, 0);
            _buffer = null;
            _buffer = newList;
        }
        public void Clear()
        {
            this._count = 0;
            Array.Clear(_buffer, 0, this._count);
        }

        public void Add(T item)
        {
            //末尾插入
            InsertAt(item, this._count);
        }

        public void Remove(T item)
        {
            int index = Array.IndexOf(_buffer, item);
            if (index >= 0)
            {
                //自复制，往前挪覆盖目标位置元素
                Array.Copy(_buffer, index + 1, _buffer, index, this._count - index - 1);
                //给末尾加一个
                _buffer[this._count - 1] = default(T);
                this._count--;
            }
        }

        public void AddRange(IList<T> c)
        {
            if (c == null || c.Count == 0) return;

            for (int i = 0; i < c.Count; i++)
            {
                Add(c[i]); //末尾插入
            }
        }

        public void AddRange(DXList<T> c)
        {
            if (c == null || c == this || c.Count == 0) return;

            for (int i = 0; i < c.Count; i++)
            {
                Add(c[i]);
            }
        }

        public void Unshift(T item)
        {
            //头插入
            InsertAt(item, 0);
        }

        //第一个元素出栈
        public T Shift()
        {
            if (this._operatorLock)
            {
                throw new System.Exception("operate can not be sync");
            }
            if (this._count == 0) return default(T);

            //拿出第一个元素
            T t = _buffer[0];
            //往前挪，覆盖第一个元素，相当于第一个元素出栈
            Array.Copy(_buffer, 1, _buffer, 0, this._count - 1);
            this._count--;
            return t;
        }
        //最后一个元素出栈
        public T Pop()
        {
            if (this._operatorLock)
            {
                throw new System.Exception("operate can not be sync");
            }
            if (this._count == 0) return default(T);

            int tail = this._count - 1;
            //拿出最后一个元素
            T t = _buffer[tail];
            _buffer[tail] = default(T);
            this._count--;
            return t;
        }
        //目标位置插入元素
        public void InsertAt(T item, int index)
        {
            if (this._operatorLock)
            {
                throw new System.Exception("operate can not be sync");
            }

            if (index > this._count)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (this._count == _buffer.Length) AllocateMore();

            if (index == this._count)
            {
                _buffer[index] = item;
            }
            else
            {
                if (this._operatorLock)
                {
                    throw new System.Exception("operate can not be sync");
                }
                //自复制，往后挪出空位，然后index位置写入
                Array.Copy(_buffer, index, _buffer, index + 1, this._count - index);
                _buffer[index] = item;
            }
            this._count++;
        }

        //算法有问题呀？？？
        public void ForeachAndRemove(BoolAction<T> callback)
        {
            this._operatorLock = true;
            if (_buffer != null && this._count > 0)
            {
                int len = this._count;
                int offset = 0;
                for (int i = 0; i < len; i++)
                {
                    T t = _buffer[i];

                    if (offset > 0)
                    {
                        _buffer[i - offset] = t; //用下个元素覆盖上个元素
                    }

                    if (callback(t))
                    {
                        this._count--;
                        offset++;
                    }

                    if (i >= this._count)
                    {
                        _buffer[i] = default(T);
                    }
                }
            }
            this._operatorLock = false;
        }

        public void ForeachAndRemove(BoolAction<T, object> callback, object state)
        {
            this._operatorLock = true;
            if (_buffer != null && this._count > 0)
            {
                int len = this._count;
                int offset = 0;
                for (int i = 0; i < len; i++)
                {
                    T t = _buffer[i];

                    if (offset > 0)
                    {
                        _buffer[i - offset] = t;
                    }

                    if (callback(t, state))
                    {
                        this._count--;
                        offset++;
                    }

                    if (i >= this._count)
                    {
                        _buffer[i] = default(T);
                    }
                }
            }
            this._operatorLock = false;
        }

    }
}
