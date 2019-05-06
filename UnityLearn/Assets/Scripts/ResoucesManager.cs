using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ResourceType 
{
   //非GameObject类型
    Texture,
    Sprite,
    Material,
    Shader,
    AudioClip,
    AnimationClip,
			
    //GameObject类型
    Prefab
};



public class ResourceObj
{
    public Object obj;
    public ResourceType type;
    public string viewName;      //属于哪个界面
    public string pathName;      //资源的路径

    public ResourceObj(Object rObj, ResourceType rtype, string rPahName, string rViewName)
    {
        obj = rObj;
        type = rtype;
        pathName = rPahName;
        viewName = rViewName;
    }
}

/*
    针对Resource文件加下的的资源管理
 */
public class ResoucesManager  {

    public static ResoucesManager _instance;

    private int count = (int)ResourceType.Prefab;
    private List<Dictionary<string, ResourceObj>> _resourcesList;

  

    private ResoucesManager()
    { 
        _resourcesList = new List<Dictionary<string, ResourceObj>>();
        for (int i = 0; i < count; i++)
        {
            Dictionary<string, ResourceObj> dict = new Dictionary<string, ResourceObj>();
            _resourcesList.Add(dict);
        }

    }
    public ResoucesManager getInstance()
    {
        if (_instance == null)
        {
            _instance = new ResoucesManager();
        }
        return _instance;
    }


    //添加资源
    //public void addResource(ResourceType type, string path, string viewName)
    //{
    //    string path = getResourceRootPath(type);
    //    path = path + name + "_" + viewName;

    //    int index = (int)type;
    //    if (!_resourcesList[index].ContainsKey(path))
    //    {
    //        Object obj = Resources.Load<Object>(path);
    //        ResourceObj resObj = new ResourceObj(obj, type, path, viewName);
    //        _resourcesList[index].Add(path, resObj);
    //    }
    //}


    //获取一个资源
    public Object getResouce(ResourceType type, string name, string viewName)
    {
        int index = (int)type;
        string path = getResourceKey(type, name, viewName);
        if (_resourcesList[index].ContainsKey(path))
        {
            ResourceObj resObj = _resourcesList[index][path];
            return resObj.obj;
        }
        else
        {
            Object obj = Resources.Load<Object>(path);
            ResourceObj resObj = new ResourceObj(obj, type, path, viewName);
            _resourcesList[index].Add(path, resObj);
            return obj;
        }
    }

    //删除一个资源
    public void removeResouce(ResourceType type, string name, string viewName)
    {
        int index = (int)type;
        string path = getResourceKey(type, name, viewName);
        if (_resourcesList[index].ContainsKey(path))
        { 
           
            ResourceObj resObj = _resourcesList[index][path];
            _resourcesList[index].Remove(path);

            Object obj = resObj.obj;
            Resources.UnloadAsset(obj);
            if (resObj.type == ResourceType.Prefab)
            {
                obj = null;
                Resources.UnloadUnusedAssets();
            }

        }
    }

    //删除某个view的所有资源
    public void removeResoucesByView(string viewName)
    {
        for (int i = 0; i < count; i++)
        {
            Dictionary<string, ResourceObj> dict = _resourcesList[i];
            foreach (KeyValuePair<string, ResourceObj> kv in dict)
            {
                ResourceObj resObj = kv.Value;
                if (resObj.viewName == viewName)
                {
                    //不能一边遍历一遍删除

                    //dict.Remove(resObj.pathName);
                    //Object obj = resObj.obj;
                    //Resources.UnloadAsset(obj);
                    //if (resObj.type == ResourceType.Prefab)
                    //{
                    //    obj = null;
                    //    Resources.UnloadUnusedAssets();
                    //}
                }

            }
        }
    }


    //返回一个资源存储的key值
    public string getResourceKey(ResourceType type, string name, string viewName)
    {
        string path = getResourceRootPath(type);
        path = path + name + "_" + viewName;
        return path;
    }
    public string getResourceRootPath(ResourceType type)
    {
        string path = string.Empty;
        switch (type)
        {
            case ResourceType.Texture:
            case ResourceType.Sprite:
                path = "Textures/";
                break;
            case ResourceType.Material:
                path = "Materials/";
                break;
            case ResourceType.Shader:
                path = "Shaders/";
                break;
            case ResourceType.AudioClip:
                path = "Audios/";
                break;
            case ResourceType.AnimationClip:
                path = "Animations/";
                break;
            case ResourceType.Prefab:
                path = "Prefabs/";
                break;
            default:
                break;
        }
        return path;
    }


    
}
