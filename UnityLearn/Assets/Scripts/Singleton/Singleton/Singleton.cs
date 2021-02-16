using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISingleton
{
    /// <summary>
    /// 单例初始化(继承当前接口的类都需要实现该方法)
    /// </summary>
    void OnSingletonInit();
}

public abstract class Singleton<T> : ISingleton where T : Singleton<T>
{
    /// <summary>
    /// 静态实例
    /// </summary>
    protected static T mInstance;

    /// <summary>
    /// 标签锁：确保当一个线程位于代码的临界区时，另一个线程不进入临界区。
    /// 如果其他线程试图进入锁定的代码，则它将一直等待（即被阻止），直到该对象被释放
    /// </summary>
    static object mLock = new object();

    public static T Instance
    {
        get
        {
            lock (mLock)
            {
                if (mInstance == null)
                {
                   mInstance = SingletonCreator.CreateSingleton<T>();
                }
            }

            return mInstance;
        }
    }


    /// <summary>
    /// 资源释放
    /// </summary>
    public virtual void Dispose()
    {
        mInstance = null;
    }

    /// <summary>
    /// 单例初始化方法
    /// </summary>
    public virtual void OnSingletonInit()
    {
    }
}
