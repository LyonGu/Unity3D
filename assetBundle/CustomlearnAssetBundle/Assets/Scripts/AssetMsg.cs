using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AssetMsg
 {
	private AssetBundleMgr assetBundleMgr;
	private static AssetMsg _instance;
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
		assetBundleMgr = AssetBundleMgr.GetInstance();
	 }
	 public T LoadAsset<T>(string path) where T : Object
	 {
		//后续加上缓存 todo
		if(string.IsNullOrEmpty(path))
		{
			return null;
		}

#if UNITY_EDITOR
		path = "Assets/" + path;
		T obj = AssetDatabase.LoadAssetAtPath<T>(path);

		if(typeof(T) == typeof(GameObject))
		{
			return GameObject.Instantiate(obj);
		}
		return obj;
#else
		//todo 使用AssetBundle
		// GameObject gameObj = (GameObject)assetBundleMgr.LoadAssetNew(_assetNameTest);
        // Instantiate(gameObj);


#endif
		return null;
	 }

}
