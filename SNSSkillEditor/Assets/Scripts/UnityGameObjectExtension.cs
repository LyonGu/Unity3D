using UnityEngine;

public static class UnityGameObjectExtension
{
    public static GameObject FindDirect(this GameObject go,string param0)
    {
        Transform ret = go.transform.Find(param0);
        if (ret != null)
        {
            return ret.gameObject;
        }
        return null;
    }
    
    public static void SetParent(this GameObject go, GameObject parent,bool keepWorld)
    {
        if (go != null && parent != null)
        {
            go.transform.SetParent(parent.transform, keepWorld);
        }
    }
    
    public static Transform FirstOrDefault(this Transform transform, System.Func<Transform, bool> query)
    {
        if (query(transform))
        {
            return transform;
        }
        for (int i = 0; i < transform.childCount; i++)
        {
            var result = FirstOrDefault(transform.GetChild(i), query);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}
