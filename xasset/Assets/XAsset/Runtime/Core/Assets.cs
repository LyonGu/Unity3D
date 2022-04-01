//
// Assets.cs
//
// Author:
//       fjy <jiyuan.feng@live.com>
//
// Copyright (c) 2020 fjy
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#define LOG_ENABLE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace libx
{
    public sealed class Assets : MonoBehaviour
    {
        public static readonly string ManifestAsset = "Assets/Manifest.asset";
        public static readonly string Extension = ".unity3d";

        public static bool runtimeMode = true;
        public static Func<string, Type, Object> loadDelegate = null;
        private const string TAG = "[Assets]";

        [Conditional("LOG_ENABLE")]
        private static void Log(string s)
        {
            Debug.Log(string.Format("{0}{1}", TAG, s));
        }

        #region API

        /// <summary>
        /// 读取所有资源路径
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllAssetPaths()
        {
            var assets = new List<string>();
            if (!runtimeMode)
            {
                //开发模式
                foreach (var item in _assetNameToPath)
                {
                    assets.Add(item.Value);
                }
                return assets.ToArray();
            }
            
            assets.AddRange(_assetToBundles.Keys);
            return assets.ToArray();
        }

        public static string basePath { get; set; }

        public static string updatePath { get; set; } 
        public static string baseURL { get; set; }
        public static void AddSearchPath(string path)
        { 
            _searchPaths.Add(path);
        }

        public static ManifestRequest Initialize()
        {
            var instance = FindObjectOfType<Assets>();
            if (instance == null)
            {
                instance = new GameObject("Assets").AddComponent<Assets>();
                DontDestroyOnLoad(instance.gameObject);
            } 
            
            //初始化路径信息
            if (string.IsNullOrEmpty(basePath))
            {
                basePath = Application.streamingAssetsPath + Path.DirectorySeparatorChar;
            }

            if (string.IsNullOrEmpty(updatePath))
            {
                updatePath = Application.persistentDataPath + Path.DirectorySeparatorChar;
            }

            Clear();
            Log(string.Format(
                "Initialize with: runtimeMode={0}\nbasePath：{1}\nupdatePath={2}",
                runtimeMode, basePath, updatePath));
            
            //ManifestRequest 利用ManifestRequest去加载一个对应的bundle
            //主要用于加载Manifest.asset配置
            var request = new ManifestRequest {name = ManifestAsset};
            NameToId(ManifestAsset);
            AddAssetRequest(request);
            return request;
        }

        public static void Clear()
        { 
            _searchPaths.Clear();
            _activeVariants.Clear();
            _assetToBundles.Clear();
            _bundleToDependencies.Clear();
        }

        private static SceneAssetRequest _runningScene;
        
        //异步加载scene
        public static SceneAssetRequest LoadSceneAsync(string path, bool additive)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("invalid path");
                return null;
            }

            path = GetExistPath(path);
            //SceneAssetRequestAsync 
            var asset = new SceneAssetRequestAsync(path, additive);
            if (! additive)
            {
                //释放上一个场景
                if (_runningScene != null)
                {
                    _runningScene.Release();;
                    _runningScene = null;
                }
                _runningScene = asset;
            }
            //加载资源
            asset.Load();
            //资源引用计数
            asset.Retain();
            _scenes.Add(asset); //放入场景加载列表
            Log(string.Format("LoadScene:{0}", path));
            return asset;
        }

        public static void UnloadScene(SceneAssetRequest scene)
        {
            scene.Release();
        }

        //异步加载资源，path为资源寻址路径，Type为资源类型
        public static AssetRequest LoadAssetAsync(string path, Type type)
        {
            return LoadAsset(path, type, true);
        }

        //同步加载资源，path为资源寻址路径，Type为资源类型
        public static AssetRequest LoadAsset(string path, Type type)
        {
            return LoadAsset(path, type, false);
        }

        public static void UnloadAsset(AssetRequest asset)
        {
            asset.Release();
        }

        #endregion

        #region Private

        public static string GetAssetPathByName(string name)
        {
            if (_assetNameToPath.TryGetValue(name, out string path))
                return path;
            return string.Empty;
        }

        public static void AddAssetPath2Name(string flieName, string path)
        {
            if (!_assetNameToPath.ContainsKey(flieName))
            {
                _assetNameToPath.Add(flieName,path);
            }
        }

        internal static void OnLoadManifest(Manifest manifest)
        {
            _activeVariants.AddRange(manifest.activeVariants); 
            
            /*
                bundle名字是利用md5算法把文件路径转成hash值，当做bundleName
                
                manifest.dirs = dirs.ToArray();  //文件对应文件夹信息
                manifest.assets = assets.ToArray();  // //bundle下标，对应文件夹下标，文件名字
                manifest.bundles = bundleRefs.ToArray(); //bundle的信息（名字，大小，hash值，依赖项，文件夹下标）
             */
            var assets = manifest.assets;
            var dirs = manifest.dirs;
            var bundles = manifest.bundles;
            
            //存储bundle的依赖信息
            //key：当前bundle
            //value: 当前bundle依赖的bundle
            foreach (var item in bundles)
                _bundleToDependencies[item.name] = Array.ConvertAll(item.deps, id => bundles[id].name);
            
            //asset与bundle的对应关系
            //Key: asset的全路径
            //value：对应的bundleName
            foreach (var item in assets)
            {
                var path = string.Format("{0}/{1}", dirs[item.dir], item.name);
                //item.bundle只是一个下标
                if (item.bundle >= 0 && item.bundle < bundles.Length)
                {
//                    Debug.Log($"_assetToBundles path==={path}  name = {bundles[item.bundle].name}");
                    _assetToBundles[path] = bundles[item.bundle].name;
                    
                    string flieName = string.Intern(item.name);
                    // int index = flieName.IndexOf('.');
                    // flieName = flieName.Substring(0, index); //去除后缀名
                   flieName = Path.GetFileNameWithoutExtension(flieName);
                    if (!_assetNameToPath.ContainsKey(flieName))
                    {
                        _assetNameToPath.Add(flieName,path);
                    }
                }
                else
                {
                    Debug.LogError(string.Format("{0} bundle {1} not exist.", path, item.bundle));
                }
            }
        }
        
        //所有AssetRequest的dictionary  <RequestName, AssetRequest> 其实可以用id才存
        private static Dictionary<string, AssetRequest> _assets = new Dictionary<string, AssetRequest>();
        
        //一个AssetRequest上name和ID的映射
        private static Dictionary<int, string> _IdToRequestName = new Dictionary<int, string>(128);
        private static Dictionary<string, int> _RequestNameToId = new Dictionary<string, int>(128);
        private static int _requestNameMappingId = 0;
        
        private static List<AssetRequest> _loadingAssets = new List<AssetRequest>(); //需要加载的assetsRequest

        private static List<SceneAssetRequest> _scenes = new List<SceneAssetRequest>(); //需要加载的SceneAssetRequest

        private static List<AssetRequest> _unusedAssets = new List<AssetRequest>(); //需要释放的assetsRequest

        public static int NameToId(string name)
        {
            name = string.Intern(name);
            if (_RequestNameToId.TryGetValue(name, out int id))
            {
                return id;
            }
            else
            {
                _requestNameMappingId++;
                _RequestNameToId.Add(name,_requestNameMappingId);
                _IdToRequestName.Add(_requestNameMappingId, name);
                return _requestNameMappingId;
            }
        }
        
        public static string IdToName(int id)
        {
            
            if (_IdToRequestName.TryGetValue(id, out string name))
            {
                return name;
            }
            else
            {
                return string.Empty;
            }
        }

        private void Update()
        {
            UpdateAssets();
            UpdateBundles();
        }

        private static void UpdateAssets()
        {
            for (var i = 0; i < _loadingAssets.Count; ++i)
            {
                var request = _loadingAssets[i];
                if (request.Update()) //每一帧检测BundleAssetRequest的状态，返回true继续执行循环
                    continue;
                _loadingAssets.RemoveAt(i);
                --i;
            }
            //移除无用资产
            foreach (var item in _assets)
            {
                //IsUnused 是Reference的方法 会去判断引用计数 
                //loadState为Loaded时 isDone就为true
                if (item.Value.isDone && item.Value.IsUnused())
                {
                    _unusedAssets.Add(item.Value); //大部分加入的是BundleAssetRequest类型
                }
            }

            if (_unusedAssets.Count > 0)
            {
                for (var i = 0; i < _unusedAssets.Count; ++i)
                {
                    var request = _unusedAssets[i]; 
                    Log(string.Format("UnloadAsset:{0}", request.name));
                    _assets.Remove(request.name);
                    request.Unload();  //自身bundle引用计数减一，依赖的bundle引用计数减一
                } 
                _unusedAssets.Clear();
            }
            
            //遍历加载scene的请求
            for (var i = 0; i < _scenes.Count; ++i)
            {
                var request = _scenes[i];
                if (request.Update() || !request.IsUnused())//request 其实就是SceneAssetRequestAsync
                    continue;
                _scenes.RemoveAt(i);
                Log(string.Format("UnloadScene:{0}", request.name));
                request.Unload(); 
                --i;
            }
        }

        private static void AddAssetRequest(AssetRequest request)
        {
            //放入列表里，Update不断遍历状态，并且调用不同request的load方法
            _assets.Add(request.name, request);
            _loadingAssets.Add(request); //正在下载列表，调用requrest的Update方法
            request.Load();
        }

        public static AssetRequest TryGetAssetRequest(string requestName)
        {
            if (string.IsNullOrEmpty(requestName))
                return null;
            AssetRequest request;
            if (!_assets.TryGetValue(requestName, out request))
            {
                return null;
            }

            return request;
        }

        public static AssetRequest TryGetAssetRequest(int requestNameId)
        {
            string requestName = IdToName(requestNameId);
            return TryGetAssetRequest(requestName);
        }

        private static AssetRequest LoadAsset(string path, Type type, bool async)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("invalid path");
                return null;
            }

            path = GetExistPath(path);

            //先尝试从已加载Asset获取目标Asset
            AssetRequest request;
            if (_assets.TryGetValue(path, out request))
            {
                request.Retain(); //引用计数+1
                _loadingAssets.Add(request); //Update方法里会遍历这个list
                return request;
            }

            //如果没找到就需要去获取AB
            string assetBundleName;
            //如果此AB已存在于本地记录（从Manifest文件读取的），就直接取得AB名，准备加载AB
            if (GetAssetBundleName(path, out assetBundleName))
            {
                request = async
                    ? new BundleAssetRequestAsync(assetBundleName)
                    : new BundleAssetRequest(assetBundleName);
            }
            else
            {
                //如果此AB在本地记录未找到
                //如果是网络路径/本地路径（注意一定要有以下前缀之一，否则会被忽略而取不到对象）
                if (path.StartsWith("http://", StringComparison.Ordinal) ||
                    path.StartsWith("https://", StringComparison.Ordinal) ||
                    path.StartsWith("file://", StringComparison.Ordinal) ||
                    path.StartsWith("ftp://", StringComparison.Ordinal) ||
                    path.StartsWith("jar:file://", StringComparison.Ordinal))
                    request = new WebAssetRequest();
                else
                    //如果是本地路径（事实上这个是用AssetDatabase.LoadAssetAtPath去编辑器找的，所以想要读取本地的非AB文件还是用上面的UWR吧，注意加上前缀)
                    request = new AssetRequest();
            }

            request.name = path;
            NameToId(path);
            request.assetType = type;
            //新增资产请求
            AddAssetRequest(request);
            //引用计数+1
            request.Retain();
            Log(string.Format("LoadAsset:{0}", path));
            return request;
        }

        #endregion

        #region Paths

        private static List<string> _searchPaths = new List<string>();

        private static string GetExistPath(string path)
        {
#if UNITY_EDITOR
            if (!runtimeMode)
            {
//                if (File.Exists(path))
//                    return path;
//
//                foreach (var item in _searchPaths)
//                {
//                    var existPath = string.Format("{0}/{1}", item, path);
//                    if (File.Exists(existPath))
//                        return existPath;
//                }
//
//                Debug.LogError("找不到资源路径" + path);
                return path;
            }
#endif
            if (_assetToBundles.ContainsKey(path))
                return path;

            foreach (var item in _searchPaths)
            {
                var existPath = string.Format("{0}/{1}", item, path);
                if (_assetToBundles.ContainsKey(existPath))
                    return existPath;
            }

            Debug.LogWarning("资源没有收集打包" + path);
            return path;
        }

        #endregion

        #region Bundles

        private static readonly int MAX_BUNDLES_PERFRAME = 0;

        private static Dictionary<string, BundleRequest> _bundles = new Dictionary<string, BundleRequest>(); //加载过的bundle

        private static List<BundleRequest> _loadingBundles = new List<BundleRequest>(); //正在加载的bundles

        private static List<BundleRequest> _unusedBundles = new List<BundleRequest>(); //卸载的bundels

        private static List<BundleRequest> _toloadBundles = new List<BundleRequest>();

        private static List<string> _activeVariants = new List<string>();

        //记录每个filePath对应的bundle信息
        private static Dictionary<string, string> _assetToBundles = new Dictionary<string, string>();
    
        //记录每个bundle对应的依赖bundle信息
        private static Dictionary<string, string[]> _bundleToDependencies = new Dictionary<string, string[]>();
        
        //记录每个asset名字和路径的对应关系
        private static Dictionary<string, string> _assetNameToPath = new Dictionary<string, string>(200);

        internal static bool GetAssetBundleName(string path, out string assetBundleName)
        {
            return _assetToBundles.TryGetValue(path, out assetBundleName);
        }

        internal static string[] GetAllDependencies(string bundle)
        {
            string[] deps;
            if (_bundleToDependencies.TryGetValue(bundle, out deps))
                return deps;

            return new string[0];
        }

        internal static BundleRequest LoadBundle(string assetBundleName)
        {
            return LoadBundle(assetBundleName, false);
        }

        internal static BundleRequest LoadBundleAsync(string assetBundleName)
        {
            return LoadBundle(assetBundleName, true);
        }

        internal static void UnloadBundle(BundleRequest bundle)
        {
            bundle.Release();
        }  

        internal static BundleRequest LoadBundle(string assetBundleName, bool asyncMode)
        {
            if (string.IsNullOrEmpty(assetBundleName))
            {
                Debug.LogError("assetBundleName == null");
                return null;
            }

            assetBundleName = RemapVariantName(assetBundleName);
            
            //得到加载路径，persitantpath
            var path = GetDataPath(assetBundleName) + assetBundleName;

            BundleRequest bundle;
            //判断对应的bundle是否加载过
            if (_bundles.TryGetValue(path, out bundle))
            {
                bundle.Retain();
                _loadingBundles.Add(bundle); //为什么加载过的还要放进loading列表里？？防止同一帧加载多个，然后update里用状态判断
                return bundle;
            }
            //bundle未加载过
            if (path.StartsWith("http://", StringComparison.Ordinal) ||
                path.StartsWith("https://", StringComparison.Ordinal) ||
                path.StartsWith("file://", StringComparison.Ordinal) ||
                path.StartsWith("ftp://", StringComparison.Ordinal))
                bundle = new WebBundleRequest(); //加载网络Bundle
            else
                bundle = asyncMode ? new BundleRequestAsync() : new BundleRequest();

            bundle.name = path;
            NameToId(path);
            _bundles.Add(path, bundle);

            if (MAX_BUNDLES_PERFRAME > 0 && (bundle is BundleRequestAsync || bundle is WebBundleRequest))
            {
                _toloadBundles.Add(bundle);
            }
            else
            {
                bundle.Load();
                _loadingBundles.Add(bundle); //加入需要加载的bundle列表
                Log("LoadBundle: " + path);
            } 

            bundle.Retain();
            return bundle;
        }

        private static string GetDataPath(string bundleName)
        {
            if (string.IsNullOrEmpty(updatePath))
                return basePath;

            if (File.Exists(updatePath + bundleName))
                return updatePath;

            return basePath;
        }

        private static void UpdateBundles()
        {
            var max = MAX_BUNDLES_PERFRAME;
            if (_toloadBundles.Count > 0 && max > 0 && _loadingBundles.Count < max)
                for (var i = 0; i < Math.Min(max - _loadingBundles.Count, _toloadBundles.Count); ++i)
                {
                    var item = _toloadBundles[i];
                    if (item.loadState == LoadState.Init)
                    {
                        item.Load();
                        _loadingBundles.Add(item);
                        _toloadBundles.RemoveAt(i);
                        --i;
                    }
                } 

            for (var i = 0; i < _loadingBundles.Count; i++)
            {
                var item = _loadingBundles[i];
                if (item.Update()) //刷新每个BundleRequest的状态，item加载完成后会返回false
                    continue;
                _loadingBundles.RemoveAt(i); //加载完成从列表里移除
                --i;
            }

            //移除无用AB
            foreach (var item in _bundles)
            {
                if (item.Value.isDone && item.Value.IsUnused())
                {
                    _unusedBundles.Add(item.Value);
                }
            } 
            
            if (_unusedBundles.Count <= 0) return;
            {
                for (var i = 0; i < _unusedBundles.Count; i++)
                {
                    var item = _unusedBundles[i];
                    if (item.isDone)
                    {
                        item.Unload(); //对应的bundleRequest进行卸载
                        _bundles.Remove(item.name); //从已经加载过的列表中移除
                        Log("UnloadBundle: " + item.name); 
                    }  
                }
                _unusedBundles.Clear();
            }
        }

        private static string RemapVariantName(string assetBundleName)
        {
            var bundlesWithVariant = _activeVariants;
            // Get base bundle path
            var baseName = assetBundleName.Split('.')[0];

            var bestFit = int.MaxValue;
            var bestFitIndex = -1;
            // Loop all the assetBundles with variant to find the best fit variant assetBundle.
            for (var i = 0; i < bundlesWithVariant.Count; i++)
            {
                var curSplit = bundlesWithVariant[i].Split('.');
                var curBaseName = curSplit[0];
                var curVariant = curSplit[1];

                if (curBaseName != baseName)
                    continue;

                var found = bundlesWithVariant.IndexOf(curVariant);

                // If there is no active variant found. We still want to use the first
                if (found == -1)
                    found = int.MaxValue - 1;

                if (found >= bestFit)
                    continue;
                bestFit = found;
                bestFitIndex = i;
            }

            if (bestFit == int.MaxValue - 1)
                Debug.LogWarning(
                    "Ambiguous asset bundle variant chosen because there was no matching active variant: " +
                    bundlesWithVariant[bestFitIndex]);

            return bestFitIndex != -1 ? bundlesWithVariant[bestFitIndex] : assetBundleName;
        }

        #endregion
    }
}