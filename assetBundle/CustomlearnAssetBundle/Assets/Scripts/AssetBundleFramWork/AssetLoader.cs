﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 1： 管理与加载指定AB的资源。
 * 2： 加载具有“缓存功能”的资源，带选用参数。
 * 3： 卸载、释放AB资源。
 * 4： 查看当前AB资源。
 */
public class AssetLoader: System.IDisposable {

	//当前Assetbundle
	private AssetBundle _currentAssetBundle;
	private string _loadName;

	//缓存容器集合 哈希表
	private Hashtable _htTable;

	public AssetLoader(AssetBundle abObj, string loadName = "")
	{
		if (abObj != null)
		{
			_currentAssetBundle = abObj;
			_loadName = loadName;
			_htTable = new Hashtable();
		}

	}

	/// 加载当前包中指定的资源
	public Object LoadAsset(string assetName, bool isCache = false)
	{
		return LoadResource<Object>(assetName, isCache);
	}

	private T LoadResource<T>(string assetName, bool isCache) where T: Object
	{
		if (_htTable.ContainsKey(assetName))
		{
			return _htTable[assetName] as T;
		}

		//正式加载
		T tmpTResource = _currentAssetBundle.LoadAsset<T>(assetName); // 最后都要调用c#的方法
		if(tmpTResource != null && isCache)
		{
			_htTable.Add(assetName, tmpTResource);
		}
		else if (tmpTResource == null)
		{
			Debug.LogError(GetType()+ "/LoadResource<T>()/参数 tmpTResources==null, 请检查！");
		}
		return tmpTResource;
	}


	// 卸载GameObject的资源 用Resources.UnloadUnusedAssets
	/*
		_prefabObj = null;
        Resources.UnloadUnusedAssets();
	*/
	//卸载指定资源(非GameObject)
	public bool UnLoadAsset(Object asset, bool isRemoveUnUseAssets = false)
	{
		if(asset != null)
		{
			// (编辑器下和真包下可能不一样，编辑器可能同一帧卸载不掉，设备上没有问题)
			Resources.UnloadAsset(asset); //不在Resoucers目录下的资源也能删除
            if (isRemoveUnUseAssets)
            {
                asset = null;
                Resources.UnloadUnusedAssets();
            }
            return true;
		}
		Debug.LogError(GetType()+ "/UnLoadAsset()/参数 asset==null ,请检查！");
        return false;
	}


        /// 释放当前AssetBundle内存镜像资源(解压缩数据)

        public void Dispose()
        {
            _currentAssetBundle.Unload(false);
        }

        /// 释放当前AssetBundle内存镜像资源,且释放内存资源（只要是从该AB包加载的object都会被卸载，包括gameobject以及其他object(texture sprite matiral。。。。)）。
        ///
        public void DisposeALL()
        {
            _currentAssetBundle.Unload(true);
        }


        /// 查询当前AssetBundle中包含的所有资源名称。
        public string[] RetriveAllAssetName()
        {
            return _currentAssetBundle.GetAllAssetNames();
        }

}

