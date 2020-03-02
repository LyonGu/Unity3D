using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AssetMsg
 {
	private AssetBundleMgr assetBundleMgr;
	private static AssetMsg _instance;

    private Dictionary<string, string> _DicFilePath = new Dictionary<string, string>();
    private Dictionary<string, string> _DicABPath = new Dictionary<string, string>();
	public static AssetMsg GetInstance()
	 {
		if (_instance == null)
		{
			_instance = new AssetMsg();
		}
		return _instance;
	 }

	 private AssetMsg()
	 {
		#if UNITY_EDITOR
			InitLookUp();
		#else
			assetBundleMgr = AssetBundleMgr.GetInstance();
		#endif
	 }

	   public void InitLookUp()
    {
        string path = "Assets/Resources/lookup.txt";
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

	 public T LoadAsset<T>(string assetName, bool isCache = true) where T : Object
	 {
		//后续加上缓存 todo
		if(string.IsNullOrEmpty(assetName))
		{
			return null;
		}

#if UNITY_EDITOR
		string path = "Assets/AB_Res/" + _DicFilePath[assetName];
		T obj = AssetDatabase.LoadAssetAtPath<T>(path);

		if(typeof(T) == typeof(GameObject))
		{
			return GameObject.Instantiate(obj);
		}
		return obj;
#else
		//todo 使用AssetBundle
		T obj = (T)assetBundleMgr.LoadAsset(assetName, isCache);
		if(typeof(T) == typeof(GameObject))
		{
			return GameObject.Instantiate(obj);
		}
		return obj;
#endif
	 }

}
