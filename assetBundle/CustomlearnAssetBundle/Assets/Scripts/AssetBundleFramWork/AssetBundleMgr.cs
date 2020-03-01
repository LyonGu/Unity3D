/***
 *
 *  功能：
 *  1: 提取“Menifest 清单文件”，缓存本脚本。
 *  2：以“场景”为单位，管理整个项目中所有的AssetBundle 包。
 *
 *
 */
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AssetBundleMgr:MonoBehaviour
{
    //本类实例
    private static AssetBundleMgr _Instance;
    //场景集合
    private Dictionary<string, MultiABMgr> _DicAllScenes = new Dictionary<string, MultiABMgr>();
    //AssetBundle （清单文件） 系统类
    private AssetBundleManifest _ManifestObj = null;

    private Dictionary<string, string> _DicFilePath = new Dictionary<string, string>();
    private Dictionary<string, string> _DicABPath = new Dictionary<string, string>();


    private  AssetBundleMgr(){}

    //得到本类实例
    public static AssetBundleMgr GetInstance()
    {
        if (_Instance==null)
        {
            _Instance = new GameObject("_AssetBundleMgr").AddComponent<AssetBundleMgr>();
        }
        return _Instance;
    }

    void Awake()
    {
        InitLookUp();
        //加载Manifest清单文件
        // StartCoroutine(ABManifestLoader.GetInstance().LoadMainifestFile());
        ABManifestLoader.GetInstance().LoadMainifestFileNew();
    }


    /// <summary>
    /// 加载(AB 包中)资源
    /// </summary>
    /// <param name="scenesName">场景名称</param>
    /// <param name="abName">AssetBundle 包名称</param>
    /// <param name="assetName">资源名称</param>
    /// <param name="isCache">是否使用缓存</param>
    /// <returns></returns>
    public UnityEngine.Object LoadAsset(string scenesName, string abName, string assetName ,bool isCache = true)
    {

        string abNameT = GetABPath(assetName);
        if (_DicAllScenes.ContainsKey(scenesName))
        {
            MultiABMgr multObj = _DicAllScenes[scenesName];
            return multObj.LoadAsset(abNameT, assetName, isCache);
        }

        Debug.LogError(GetType()+ "/LoadAsset()/找不到场景名称，无法加载（AB包中）资源,请检查！  scenesName="+ scenesName);
        return null;
    }

     private void LoadAllABComplete(string abName)
    {
        Debug.Log("所有AB包都加载完毕=========");
    }

    private bool _isLoadoneMainfest = false;
    private void LoadMainFest(string abName)
    {
        _isLoadoneMainfest = true;

    }
    public UnityEngine.Object LoadAssetNew(string assetName ,bool isCache = true)
    {

        // 先加载AB包
        string abNameT = GetABPath(assetName);
        LoadAssetBundlePackNew(abNameT);

        if (_DicAllScenes.ContainsKey(abNameT))
        {
            MultiABMgr multObj = _DicAllScenes[abNameT];
            return multObj.LoadAsset(abNameT, assetName, isCache);
        }
        return null;
    }


     public void LoadAssetBundlePackNew(string abName, DelLoadComplete loadAllCompleteHandle = null)
    {
        //参数检查
        if ( string.IsNullOrEmpty(abName))
        {
            Debug.LogError(GetType()+ "/LoadAssetBundlePack()/ScenesName Or abName is null ,请检查！");
            return;
        }

        _ManifestObj = ABManifestLoader.GetInstance().GetABManifest();
        if (_ManifestObj==null)
        {
            Debug.LogError(GetType() + "/LoadAssetBundlePack()/_ManifestObj is null ,请先确保加载Manifest清单文件！");
            return;
        }


        //把当前场景加入集合中。
        if (!_DicAllScenes.ContainsKey(abName))
        {
            MultiABMgr multiMgrObj = new MultiABMgr("",abName, loadAllCompleteHandle);
            _DicAllScenes.Add(abName, multiMgrObj);
        }

        //调用下一层（“多包管理类”）
        MultiABMgr tmpMultiMgrObj = _DicAllScenes[abName];
        if (tmpMultiMgrObj==null)
        {
            Debug.LogError(GetType() + "/LoadAssetBundlePack()/tmpMultiMgrObj is null ,请检查！");
        }
        //调用“多包管理类”的加载指定AB包。加载指定ab包以及会把该包所依赖的ab包也加载
        // StartCoroutine(tmpMultiMgrObj.LoadAssetBundeler(abName));
        tmpMultiMgrObj.LoadAssetBundelerNew(abName);
    }


    /// <summary>
    /// 释放资源。
    /// </summary>
    /// <param name="scenesName">场景名称</param>
    public void DisposeAllAssets(string scenesName)
    {
        if (_DicAllScenes.ContainsKey(scenesName))
        {
            MultiABMgr multObj = _DicAllScenes[scenesName];
            multObj.DisposeAllAsset();
        }
        else {
            Debug.LogError(GetType() + "/DisposeAllAssets()/找不到场景名称，无法释放资源，请检查！  scenesName=" + scenesName);
        }
    }

    public string GetFilePath(string file)
    {
        return _DicFilePath[file];
    }

    public string GetABPath(string file)
    {
        return _DicABPath[file];
    }

    public void InitLookUp()
    {
        string path =PathTool.GetABOutPath() + "/lookup.txt";
        StreamReader sr =new StreamReader(path);
        string result = sr.ReadToEnd();
        string[] lines = result.Split('\n');
        foreach (var item in lines)
        {
            string data = item;
            if (data!="")
            {
                string[] names = data.Split(':');
                string abName = names[0];
                string filePath = names[1];
                string fileName = names[2];

                if (_DicABPath.ContainsKey(fileName))
                {
                    Debug.LogError("_DicABPath is have exsit key :"+ fileName);
                    return;
                }
                // _DicABPath[fileName] = PathTool.GetWWWPath() + "/" + abName;
                _DicABPath[fileName] = abName;

                if (_DicFilePath.ContainsKey(fileName))
                {
                    Debug.LogError("_DicFilePath is have exsit key :"+ fileName);
                    return;
                }
                _DicFilePath[fileName] = filePath;
            }

        }


    }

}//Class_end



