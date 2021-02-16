using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 通过属性器实现 Mono 单例
 */
/// <summary>
/// 继承Mono的属性单例？
/// </summary>
/// <typeparam name="T"></typeparam>
public static class MonoSingletonProperty<T> where T : MonoBehaviour, ISingleton
{
    private static T mInstance;

    public static T Instance
    {
        get
        {
            if (null == mInstance)
            {
                mInstance = MonoSingletonCreator.CreateMonoSingleton<T>();
            }

            return mInstance;
        }
    }

    public static void Dispose()
    {
        Object.Destroy(mInstance.gameObject);
        mInstance = null;
    }
}
