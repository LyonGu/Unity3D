using System.Reflection;
using UnityEngine;
/// <summary>
/// 静态类：创建MonoBehaviour类型的单例
/// </summary>
internal static class MonoSingletonCreator
{
  
    /// <summary>
    /// 泛型方法：创建MonoBehaviour单例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T CreateMonoSingleton<T>() where T : MonoBehaviour, ISingleton
    {
        T instance = null;

        //判断T实例存在的条件是否满足
        if (!Application.isPlaying)
            return instance;

        //判断当前场景中是否存在T实例
        //instance = Object.FindObjectOfType<T>();
        //if (instance != null)
        //{
        //    instance.OnSingletonInit();
        //    return instance;
        //}

        ////MemberInfo：获取有关成员属性的信息并提供对成员元数据的访问
        //MemberInfo info = typeof(T);
        ////获取T类型 自定义属性，并找到相关路径属性，利用该属性创建T实例
        //var attributes = info.GetCustomAttributes(true);
        //foreach (var atribute in attributes)
        //{
        //    var defineAttri = atribute as MonoSingletonPath;
        //    if (defineAttri == null)
        //    {
        //        continue;
        //    }

        //    instance = CreateComponentOnGameObject<T>(defineAttri.PathInHierarchy, true);
        //    break;
        //}

        //如果还是无法找到instance  则主动去创建同名Obj 并挂载相关脚本 组件
        if (instance == null)
        {
            var obj = new GameObject(typeof(T).Name);
            Object.DontDestroyOnLoad(obj);
            instance = obj.AddComponent<T>();
        }

        instance.OnSingletonInit();
        return instance;
    }

    ///// <summary>
    ///// 在GameObject上创建T组件（脚本）
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    ///// <param name="path">路径（应该就是Hierarchy下的树结构路径）</param>
    ///// <param name="dontDestroy">不要销毁 标签</param>
    ///// <returns></returns>
    //private static T CreateComponentOnGameObject<T>(string path, bool dontDestroy) where T : MonoBehaviour
    //{
    //    var obj = FindGameObject(path, true, dontDestroy);
    //    if (obj == null)
    //    {
    //        obj = new GameObject("Singleton of " + typeof(T).Name);
    //        if (dontDestroy)
    //        {
    //            Object.DontDestroyOnLoad(obj);
    //        }
    //    }

    //    return obj.AddComponent<T>();
    //}

    ///// <summary>
    ///// 查找Obj（对于路径 进行拆分）
    ///// </summary>
    ///// <param name="path">路径</param>
    ///// <param name="build">true</param>
    ///// <param name="dontDestroy">不要销毁 标签</param>
    ///// <returns></returns>
    //private static GameObject FindGameObject(string path, bool build, bool dontDestroy)
    //{
    //    if (string.IsNullOrEmpty(path))
    //    {
    //        return null;
    //    }

    //    var subPath = path.Split('/');
    //    if (subPath == null || subPath.Length == 0)
    //    {
    //        return null;
    //    }

    //    return FindGameObject(null, subPath, 0, build, dontDestroy);
    //}

    ///// <summary>
    ///// 查找Obj（一个嵌套查找Obj的过程）
    ///// </summary>
    ///// <param name="root">父节点</param>
    ///// <param name="subPath">拆分后的路径节点</param>
    ///// <param name="index">下标</param>
    ///// <param name="build">true</param>
    ///// <param name="dontDestroy">不要销毁 标签</param>
    ///// <returns></returns>
    //private static GameObject FindGameObject(GameObject root, string[] subPath, int index, bool build,
    //    bool dontDestroy)
    //{
    //    GameObject client = null;

    //    if (root == null)
    //    {
    //        client = GameObject.Find(subPath[index]);
    //    }
    //    else
    //    {
    //        var child = root.transform.Find(subPath[index]);
    //        if (child != null)
    //        {
    //            client = child.gameObject;
    //        }
    //    }

    //    if (client == null)
    //    {
    //        if (build)
    //        {
    //            client = new GameObject(subPath[index]);
    //            if (root != null)
    //            {
    //                client.transform.SetParent(root.transform);
    //            }

    //            if (dontDestroy && index == 0)
    //            {
    //                GameObject.DontDestroyOnLoad(client);
    //            }
    //        }
    //    }

    //    if (client == null)
    //    {
    //        return null;
    //    }

    //    return ++index == subPath.Length ? client : FindGameObject(client, subPath, index, build, dontDestroy);
    //}
}
