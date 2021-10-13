using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 普通单例创建类
/// </summary>
internal static class SingletonCreator
{
    public static T CreateSingleton<T>() where T : class, ISingleton
    {
        var instance = ObjectFactory.CreateNonPublicConstructorObject<T>();
        instance.OnSingletonInit();
        return instance;
    }
}
