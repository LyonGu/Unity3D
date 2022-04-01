
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DXGame.core
{
    public class StructArray<T> : IEnumerable<T> where T : struct
    {
        //使用托管指针定义委托参数，避免内存复制
        public delegate void SetHandler(int index, ref T t);

        public delegate void AddHandler(ref T t);

        public delegate bool RemoveHandler(ref T t);

        const int DEFALUT_CAPCITY = 4;
        private T[] _buffer;

        private int _count;
        public int Count => _count;

        public StructArray()
        {
            _buffer = new T[DEFALUT_CAPCITY];
        }

        public StructArray(int capacity)
        {
            _buffer = new T[capacity];
        }

        public void Clear(bool isClear = false)
        {
            if (this._count <= 0) return;
            this._count = 0;
            if(isClear)
                Array.Clear(this._buffer, 0, this._count);
        }

        void AllocateMore()
        {
            if (this._count < _buffer.Length) return;

            int size = _buffer.Length << 1; //2倍扩容
            if (size < DEFALUT_CAPCITY) size = DEFALUT_CAPCITY;
            if (size < this._count) size = this._count;
            Array.Resize(ref _buffer, size);
        }

        [Conditional("UNITY_EDITOR"), Conditional("ENABLE_LOG")]
        void CheckIndex(int index)
        {
            if (index < 0 || index >= _count)
                throw new IndexOutOfRangeException("StructArray");
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
                for (int i = 0; i < _count; ++i)
                {
                    yield return _buffer[i];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        //返回一个ref结构，避免结构体复制
        public ref T this[int index]
        {
            get
            {
                CheckIndex(index);
                return ref _buffer[index];
            }
        }
        //扩容
        public void Fill(int len)
        {
            if (len > this._buffer.Length)
            {
                this.AllocateMore();
            }

            this._count = len;

        }

        //末尾添加一个元素 传入托管指针ref
        public void Add(ref T t)
        {
            AllocateMore();
            this._buffer[this._count] = t;
            this._count++;
        }

        //返回末尾元素，ref，外部修改后内部同时生效
        //不发生扩容的话没有内存copy
        public ref T AddRef()
        {
            AllocateMore();
            this._count++;
            return ref this._buffer[this._count - 1];
        }

        //删除指定位置元素，会发生内存copy
        public T RemoveAt(int index)
        {
            CheckIndex(index);
            T t = _buffer[index];
            if (index == _count - 1)
                _buffer[_count - 1] = default; //最后一个
            else
            {
                //给数组实现的扩展方法
                //把后面的数据往前移一位，其实也是调用Array.Copy
                _buffer.Move(index + 1, index, _count - index - 1);
                _buffer[_count - 1] = default;
            }

            _count--;
            return t;
        }

        //插入指定位置元素，会发生内存copy
        public void InsertAt(int index, ref T t)
        {
            AllocateMore();
            CheckIndex(index);

            if (index == _count)
                _buffer[_count] = t;
            else
            {
                _buffer.Move(index, index + 1, _count - index);
                _buffer[index] = t;
            }

            _count++;
        }

        public void InsertAt(int index, AddHandler action)
        {
            AllocateMore();
            CheckIndex(index);

            if (index != _count)
                _buffer.Move(index, index + 1, _count - index);

            action(ref _buffer[index]); //避免内存copy
            _count++;
        }

        //获取末尾元素，并清除对应内存
        public bool Pop(out T t)
        {
            t = default;
            if (this._count == 0) return false;

            int index = _count - 1;
            t = this[index];
            RemoveAt(index);
            return true;
        }
        //获取头部元素，并清除对应内存
        public bool Shift(out T t)
        {
            t = default;
            if (this._count == 0) return false;

            t = _buffer[0];
            Array.Copy(_buffer, 1, _buffer, 0, this._count - 1);
            this._count--;
            return true;
        }

        public void RemoveAndSwapback(int index)
        {
            CheckIndex(index);
            this._count--;
            if (this._count > 0)
            {
                if (this._count == index)
                {
                    this._buffer[index] = default;
                }
                else
                {
                    this._buffer[index] = this._buffer[this._count];
                }
            }
        }
    }
}
