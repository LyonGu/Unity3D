using System;
using System.Collections.Generic;
using DXGame.structs;
using UnityEngine;
using UnityEngine.Events;

namespace GamePool
{
    /// <summary>
    /// Generic object pool.
    /// </summary>
    /// <typeparam name="T">Type of the object pool.</typeparam>
    public class ObjectPool<T> where T : new()
    {
        //使用stack或者Queue效率会高点
        readonly Stack<T> m_Stack = new Stack<T>();
        
        //定义获取以及回收的操作，可不传
        readonly UnityAction<T> m_ActionOnGet;
        readonly UnityAction<T> m_ActionOnRelease;
        
        //只是一个安全性检查标志，当把一个对象放回对象池里时，如果该对象已经在对象池，会打印错误日志
        readonly bool m_CollectionCheck = true;

        /// <summary>
        /// Number of objects in the pool. 池子里所有对象
        /// </summary>
        public int countAll { get; private set; }
        /// <summary>
        /// Number of active objects in the pool. 池子里激活对象，从池子里拿出的对象
        /// </summary>
        public int countActive { get { return countAll - countInactive; } }
        /// <summary>
        /// Number of inactive objects in the pool. 池子里未激活对象，留在池子里的对象
        /// </summary>
        public int countInactive { get { return m_Stack.Count; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="actionOnGet">Action on get.</param>
        /// <param name="actionOnRelease">Action on release.</param>
        /// <param name="collectionCheck">True if collection integrity should be checked.</param>
        public ObjectPool(UnityAction<T> actionOnGet, UnityAction<T> actionOnRelease, bool collectionCheck = true)
        {
            m_ActionOnGet = actionOnGet;
            m_ActionOnRelease = actionOnRelease;
            m_CollectionCheck = collectionCheck;
        }

        /// <summary>
        /// Get an object from the pool. 从池子里拿一个对象
        /// </summary>
        /// <returns>A new object from the pool.</returns>
        public T Get()
        {
            T element;
            if (m_Stack.Count == 0)
            {
                //池子里为空，创建一个新的
                element = new T();
                countAll++;
            }
            else
            {
                //直接从栈顶拿出一个
                element = m_Stack.Pop();
            }
            
            //如果获取对象是设置回调不为空，执行回调
            if (m_ActionOnGet != null)
                m_ActionOnGet(element);
            return element;
        }
        
        /// <summary>
        /// Release an object to the pool. 回收一个对象到池子里
        /// </summary>
        /// <param name="element">Object to release.</param>
        public void Release(T element)
        {
#if UNITY_EDITOR // keep heavy checks in editor
            if (m_CollectionCheck && m_Stack.Count > 0)
            {
                if (m_Stack.Contains(element))
                {
                    Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
                    //TODO
                }
    
            }
#endif
            //回收时的回调
            if (m_ActionOnRelease != null)
                m_ActionOnRelease(element);
            m_Stack.Push(element);
        }

        /// <summary>
        /// Pooled object.
        /// </summary>
        public struct PooledObject : IDisposable
        {
            readonly T m_ToReturn;
            readonly ObjectPool<T> m_Pool;

            internal PooledObject(T value, ObjectPool<T> pool)
            {
                m_ToReturn = value;
                m_Pool = pool;
            }

            /// <summary>
            /// Disposable pattern implementation.
            /// </summary>
            void IDisposable.Dispose() => m_Pool.Release(m_ToReturn);
        }
        
        /// <summary>
        /// Get et new PooledObject. 从池子里获取一个元素，包了一层
        /// </summary>
        /// <param name="v">Output new typed object.</param>
        /// <returns>New PooledObject</returns>
        public PooledObject Get(out T v) => new PooledObject(v = Get(), this);

       
    }

    /// <summary>
    /// Generic pool. 通用对象池，没有回调的处理，内部使用的是ObjectPool
    /// </summary>
    /// <typeparam name="T">Type of the objects in the pull.</typeparam>
    public static class GenericPool<T>
        where T : new()
    {
        // Object pool to avoid allocations.
        static readonly ObjectPool<T> s_Pool = new ObjectPool<T>(null, null);

        /// <summary>
        /// Get a new object.
        /// </summary>
        /// <returns>A new object from the pool.</returns>
        public static T Get() => s_Pool.Get();

        /// <summary>
        /// Get a new PooledObject
        /// </summary>
        /// <param name="value">Output typed object.</param>
        /// <returns>A new PooledObject.</returns>
        public static ObjectPool<T>.PooledObject Get(out T value) => s_Pool.Get(out value);

        /// <summary>
        /// Release an object to the pool.
        /// </summary>
        /// <param name="toRelease">Object to release.</param>
        public static void Release(T toRelease) => s_Pool.Release(toRelease);
    }

    /// <summary>
    /// Generic pool without collection checks. 没有安全性检查的对象池，但回收一个对象时 不进行该对象是否已经在对象池里的检测
    /// This class is an alternative for the GenericPool for object that allocate memory when they are being compared.
    /// It is the case for the CullingResult class from Unity, and because of this in HDRP HDCullingResults generates garbage whenever we use ==, .Equals or ReferenceEquals.
    /// This pool doesn't do any of these comparison because we don't check if the stack already contains the element before releasing it.
    /// </summary>
    /// <typeparam name="T">Type of the objects in the pull.</typeparam>
    public static class UnsafeGenericPool<T>
        where T : new()
    {
        // Object pool to avoid allocations.
        static readonly ObjectPool<T> s_Pool = new ObjectPool<T>(null, null, false);

        /// <summary>
        /// Get a new object.
        /// </summary>
        /// <returns>A new object from the pool.</returns>
        public static T Get() => s_Pool.Get();

        /// <summary>
        /// Get a new PooledObject
        /// </summary>
        /// <param name="value">Output typed object.</param>
        /// <returns>A new PooledObject.</returns>
        public static ObjectPool<T>.PooledObject Get(out T value) => s_Pool.Get(out value);

        /// <summary>
        /// Release an object to the pool.
        /// </summary>
        /// <param name="toRelease">Object to release.</param>
        public static void Release(T toRelease) => s_Pool.Release(toRelease);
    }

    /// <summary>
    /// List Pool.  使用ObjectPool实现的listPool
    /// </summary>
    /// <typeparam name="T">Type of the objects in the pooled lists.</typeparam>
    public static class ListPool<T>
    {
        // Object pool to avoid allocations.
        // 参数l其实就是List<T>
        static readonly ObjectPool<List<T>> s_Pool = new ObjectPool<List<T>>(null, l => l.Clear());

        /// <summary>
        /// Get a new List
        /// </summary>
        /// <returns>A new List</returns>
        public static List<T> Get() => s_Pool.Get();

        /// <summary>
        /// Get a new list PooledObject.
        /// </summary>
        /// <param name="value">Output typed List.</param>
        /// <returns>A new List PooledObject.</returns>
        public static ObjectPool<List<T>>.PooledObject Get(out List<T> value) => s_Pool.Get(out value);

        /// <summary>
        /// Release an object to the pool.
        /// </summary>
        /// <param name="toRelease">List to release.</param>
        public static void Release(List<T> toRelease) => s_Pool.Release(toRelease);
    }

    /// <summary>
    /// HashSet Pool. 使用ObjectPool实现的HashSetPool
    /// </summary>
    /// <typeparam name="T">Type of the objects in the pooled hashsets.</typeparam>
    public static class HashSetPool<T>
    {
        // Object pool to avoid allocations.
        static readonly ObjectPool<HashSet<T>> s_Pool = new ObjectPool<HashSet<T>>(null, l => l.Clear());

        /// <summary>
        /// Get a new HashSet
        /// </summary>
        /// <returns>A new HashSet</returns>
        public static HashSet<T> Get() => s_Pool.Get();

        /// <summary>
        /// Get a new list PooledObject.
        /// </summary>
        /// <param name="value">Output typed HashSet.</param>
        /// <returns>A new HashSet PooledObject.</returns>
        public static ObjectPool<HashSet<T>>.PooledObject Get(out HashSet<T> value) => s_Pool.Get(out value);

        /// <summary>
        /// Release an object to the pool.
        /// </summary>
        /// <param name="toRelease">hashSet to release.</param>
        public static void Release(HashSet<T> toRelease) => s_Pool.Release(toRelease);
    }

    /// <summary>
    /// Dictionary Pool. 使用ObjectPool实现的Dictionary Pool
    /// </summary>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    public static class DictionaryPool<TKey, TValue>
    {
        // Object pool to avoid allocations.
        static readonly ObjectPool<Dictionary<TKey, TValue>> s_Pool
            = new ObjectPool<Dictionary<TKey, TValue>>(null, l => l.Clear());

        /// <summary>
        /// Get a new Dictionary
        /// </summary>
        /// <returns>A new Dictionary</returns>
        public static Dictionary<TKey, TValue> Get() => s_Pool.Get();

        /// <summary>
        /// Get a new dictionary PooledObject.
        /// </summary>
        /// <param name="value">Output typed Dictionary.</param>
        /// <returns>A new Dictionary PooledObject.</returns>
        public static ObjectPool<Dictionary<TKey, TValue>>.PooledObject Get(out Dictionary<TKey, TValue> value)
            => s_Pool.Get(out value);

        /// <summary>
        /// Release an object to the pool.
        /// </summary>
        /// <param name="toRelease">Dictionary to release.</param>
        public static void Release(Dictionary<TKey, TValue> toRelease) => s_Pool.Release(toRelease);
    }
    
    
    public static class QueuePool<T>
    {
        // Object pool to avoid allocations.
        // 参数l其实就是DXQueue<T>
        static readonly ObjectPool<DXQueue<T>> s_Pool = new ObjectPool<DXQueue<T>>(null, l => l.Clear());

        /// <summary>
        /// Get a new List
        /// </summary>
        /// <returns>A new List</returns>
        public static DXQueue<T> Get() => s_Pool.Get();

        /// <summary>
        /// Get a new list PooledObject.
        /// </summary>
        /// <param name="value">Output typed List.</param>
        /// <returns>A new List PooledObject.</returns>
        public static ObjectPool<DXQueue<T>>.PooledObject Get(out DXQueue<T> value) => s_Pool.Get(out value);

        /// <summary>
        /// Release an object to the pool.
        /// </summary>
        /// <param name="toRelease">List to release.</param>
        public static void Release(DXQueue<T> toRelease) => s_Pool.Release(toRelease);
    }
}
