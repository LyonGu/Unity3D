using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoSingletonQF<T> : MonoBehaviour, ISingleton where T : MonoSingletonQF<T>
{

    /// <summary>
    /// 静态实例
    /// </summary>
    protected static T mInstance;

    /// <summary>
    /// 静态属性：封装相关实例对象
    /// </summary>
    public static T Instance
    {
        get
        {
            if (mInstance == null && !mOnApplicationQuit)
            {
                mInstance = MonoSingletonCreator.CreateMonoSingleton<T>();
            }

            return mInstance;
 
        }
    }

    /// <summary>
    /// 实现接口的单例初始化
    /// </summary>
    public virtual void OnSingletonInit()
    {
    }

    /// <summary>
    /// 资源释放
    /// </summary>
    public virtual void Dispose()
    {
        Destroy(gameObject);
    }


    /// <summary>
    /// 当前应用程序是否结束 标签
    /// </summary>
    protected static bool mOnApplicationQuit = false;

    /// <summary>
    /// 应用程序退出：释放当前对象并销毁相关GameObject
    /// </summary>
    protected virtual void OnApplicationQuit()
    {
        mOnApplicationQuit = true;
        if (mInstance == null) return;
        Destroy(mInstance.gameObject);
        mInstance = null;
    }

    /// <summary>
    /// 释放当前对象
    /// </summary>
    protected virtual void OnDestroy()
    {
        mInstance = null;
    }

    /// <summary>
    /// 判断当前应用程序是否退出
    /// </summary>
    public static bool IsApplicationQuit
    {
        get { return mOnApplicationQuit; }
    }

}
