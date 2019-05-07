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
    public List<string> viewList;       //属于哪个界面 :一对多的关系(一个界面还可以引用多次)
    public string pathName;             //资源的路径
    public int refCount = 0;            //资源的引用计数

    public ResourceObj(Object rObj, ResourceType rtype, string rPahName)
    {
        obj = rObj;
        type = rtype;
        pathName = rPahName;
        viewList = new List<string>();
    }

    public void addRefCount(string viewName)
    {
        refCount++;
        viewList.Add(viewName);
       
    }

    public void reduceRefCount(string viewName)
    {
        refCount--;
        viewList.Remove(viewName);//相同的元素也只会删除一个
    }

}

/*
    针对Resource文件加下的的资源管理
 * 
 * 仅仅用资源路径名称作为key存储缓存，
 *  ：一个界面使用多个相同资源==》只会存储一份资源内存
 *    多个界面使用同一份资源  ==》只会存储一份资源内存
 *    
 *  viewName 是为了来做引用计数管理的
 */
public class ResourcesManager  {

    public static ResourcesManager _instance;

    private int count = (int)ResourceType.Prefab;
    private List<Dictionary<string, ResourceObj>> _resourcesList;
    private Dictionary<string, List<ResourceObj>> _resourcesDict; // 根据view来存储资源


    private ResourcesManager()
    { 
        _resourcesList = new List<Dictionary<string, ResourceObj>>();
        for (int i = 0; i < count; i++)
        {
            Dictionary<string, ResourceObj> dict = new Dictionary<string, ResourceObj>();
            _resourcesList.Add(dict);
        }

        _resourcesDict = new Dictionary<string, List<ResourceObj>>();

    }
    static public ResourcesManager getInstance()
    {
        if (_instance == null)
        {
            _instance = new ResourcesManager();
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

    //手动添加引用计数
    public void addResourceRefCount(ResourceType type, string name, string viewName)
    {
        int index = (int)type;
        string path = getResourceKey(type, name);
        if (_resourcesList[index].ContainsKey(path))
        {
            ResourceObj resObj = _resourcesList[index][path];
            resObj.addRefCount(viewName);
        }
    }

    //手动减少引用计数
    public void reduceResourceRefCount(ResourceType type, string name, string viewName)
    {
        int index = (int)type;
        string path = getResourceKey(type, name);
        if (_resourcesList[index].ContainsKey(path))
        {
            ResourceObj resObj = _resourcesList[index][path];
            resObj.reduceRefCount(viewName);
        }
    }
    /*
        一个界面引用多个name相同的资源     --》ok
     *  多个界面引用同一个资源             --》ok
     *  一个界面以后用多个不同资源         --》ok
     *  多个界面以后用多个不同资源         --》ok
     */
    //获取一个资源
    public Object getResouce(ResourceType type, string name, string viewName)
    {
        int index = (int)type;
        string path = getResourceKey(type, name);
        ResourceObj resObj = null;
        if (_resourcesList[index].ContainsKey(path))
        {
            resObj = _resourcesList[index][path];
        }
        else
        {
            Object obj = Resources.Load<Object>(path);
            resObj = new ResourceObj(obj, type, path);
            _resourcesList[index].Add(path, resObj);
        }
        //引用管理
        resObj.addRefCount(viewName);

        //页面资源管理
        addViewCache(viewName, resObj);
        return resObj.obj;
    }

    private void addViewCache(string viewName, ResourceObj resObj)
    {
        //页面资源管理
        if (!_resourcesDict.ContainsKey(viewName))
        {
            List<ResourceObj> listResObj = new List<ResourceObj>();
            _resourcesDict.Add(viewName, listResObj);
            listResObj.Add(resObj);
        }
        else
        {
            List<ResourceObj> listResObj = _resourcesDict[viewName];
            if (!listResObj.Contains(resObj)) //页面的list里只存一份资源引用
            {
                listResObj.Add(resObj);
            }

        }
    }

    private void deleteViewCache(string viewName, ResourceObj resObj)
    {
        //页面资源管理
        if (_resourcesDict.ContainsKey(viewName))
        {
            List<ResourceObj> listResObj = _resourcesDict[viewName];
            if (listResObj.Contains(resObj))
            {
                listResObj.Remove(resObj);
            }
        }

    }

    //删除一个资源
    public void removeResouce(ResourceType type, string name, string viewName)
    {
        int index = (int)type;
        string path = getResourceKey(type, name);
        if (_resourcesList[index].ContainsKey(path))
        { 
           
            ResourceObj resObj = _resourcesList[index][path];
            //引用管理
            resObj.reduceRefCount(viewName);

           
            if (resObj.refCount <= 0)
            {
                deleteViewCache(viewName, resObj);
                //真正的卸载资源
                unLoadResouce(resObj);
            }
        }
    }

    //删除某个view的所有资源,如果其他view有引用，资源不会删除
    public void removeResoucesByView(string viewName)
    {
        //listResObj只存储了一份资源引用
        List<ResourceObj> listResObj = _resourcesDict[viewName];
        int len = listResObj.Count;
        for (int i = len - 1; i >= 0; i--)
        {
            ResourceObj resObj = listResObj[i];
            resObj.reduceRefCount(viewName);
            if (resObj.refCount <= 0)
            {
                deleteViewCache(viewName, resObj);
                //真正的卸载资源
                unLoadResouce(resObj);
            }
        }

    }

    private void unLoadResouce(ResourceObj resObj)
    {
        int index = (int)resObj.type;
        string path = resObj.pathName;

        //当引用计数小于0的时候才卸载资源
        _resourcesList[index].Remove(path);
        Object obj = resObj.obj;
        Resources.UnloadAsset(obj);
        if (resObj.type == ResourceType.Prefab)
        {
            obj = null;
            Resources.UnloadUnusedAssets();
        }
    }

    //返回一个资源存储的key值
    private string getResourceKey(ResourceType type, string name)
    {
        string path = getResourceRootPath(type);
        path = path + name;
        return path;
    }
    private string getResourceRootPath(ResourceType type)
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
