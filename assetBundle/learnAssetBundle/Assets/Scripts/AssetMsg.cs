using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AssetMsg
 {

	private static AssetMsg _instance;
	public static AssetMsg GetInstance()
	 {
		if (_instance == null)
		{
			_instance = new AssetMsg();
		}
		return _instance;
	 }

	 public T LoadAsset<T>(string path, bool isPrefab = false) where T : Object
	 {
		//后续加上缓存 todo
		if(string.IsNullOrEmpty(path))
		{
			return null;
		}

#if UNITY_EDITOR
		path = "Assets/" + path;
		T obj = AssetDatabase.LoadAssetAtPath<T>(path);
		if (isPrefab)
		{
			return GameObject.Instantiate(obj);
		}
		return obj;
#else
		//todo 使用AssetBundle
#endif
		return null;
	 }

}
