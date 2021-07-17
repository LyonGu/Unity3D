//
// Requests.cs
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace libx
{
    public enum LoadState
    {
        Init, //初始化
        LoadAssetBundle, //加载ab包
        LoadAsset, //加载对应asset资源
        Loaded, //加载完毕
        Unload // 卸载
    }

    public class AssetRequest : Reference, IEnumerator
    {
        private LoadState _loadState = LoadState.Init;
        private List<Object> _requires;
        public Type assetType;

        public Action<AssetRequest> completed;
        public string name;

        public AssetRequest()
        {
            asset = null;
            loadState = LoadState.Init;
        }

        public LoadState loadState
        {
            get { return _loadState; }
            protected set
            {
                _loadState = value;
                if (value == LoadState.Loaded)
                {
                    Complete();
                }
            }
        }

        private void Complete()
        {
            if (completed != null)
            {
                completed(this);
                completed = null;
            }
        }

        public virtual bool isDone
        {
            get { return loadState == LoadState.Loaded || loadState == LoadState.Unload; }
        }

        public virtual float progress
        {
            get { return 1; }
        }

        public virtual string error { get; protected set; }

        public string text { get; protected set; }

        public byte[] bytes { get; protected set; }

        public Object asset { get; internal set; }

        private bool checkRequires
        {
            get { return _requires != null; }
        }

        private void UpdateRequires()
        {
            for (var i = 0; i < _requires.Count; i++)
            {
                var item = _requires[i];
                if (item != null)
                    continue;
                Release();
                _requires.RemoveAt(i);
                i--;
            }

            if (_requires.Count == 0)
                _requires = null;
        }

        internal virtual void Load()
        {
            if (!Assets.runtimeMode && Assets.loadDelegate != null)
                asset = Assets.loadDelegate(name, assetType);
            if (asset == null) error = "error! file not exist:" + name;
            loadState = LoadState.Loaded;
        }

        internal virtual void Unload()
        {
            if (asset == null)
                return;

            if (!Assets.runtimeMode)
                if (!(asset is GameObject))
                    Resources.UnloadAsset(asset);

            asset = null;
            loadState = LoadState.Unload;
        }

        internal virtual bool Update()
        {
            if (checkRequires)
                UpdateRequires();
            if (!isDone)
                return true;
            if (completed == null)
                return false;
            try
            {
                completed.Invoke(this);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            completed = null;
            return false;
        }

        #region IEnumerator implementation

        public bool MoveNext()
        {
            return !isDone; //isDone 为true时，MoveNext返回false，退出协程
        }

        internal virtual void LoadImmediate()
        {
        }

        public void Reset()
        {
        }

        public object Current
        {
            get { return null; } //每一帧都等待
        }

        #endregion
    }

    //主要用于加载Manifest
    public class ManifestRequest : AssetRequest
    {
        private string assetName;
        private BundleRequest request;

        public int version { get; private set; }

        public override float progress
        {
            get
            {
                if (isDone) return 1;

                if (loadState == LoadState.Init) return 0;

                if (request == null) return 1;

                return request.progress;
            }
        }

        internal override void Load()
        {
            assetName = Path.GetFileName(name);
            if (Assets.runtimeMode)
            {
                var assetBundleName = assetName.Replace(".asset", ".unity3d").ToLower();
                //加载对应bundle  这个其实是一个BundleRequestAsync
                request = Assets.LoadBundleAsync(assetBundleName);
                loadState = LoadState.LoadAssetBundle;
            }
            else
            {
                loadState = LoadState.Loaded;
            }
        }

        internal override bool Update()
        {
            if (!base.Update()) return false;

            if (loadState == LoadState.Init) return true;

            if (request == null)
            {
                loadState = LoadState.Loaded;
                error = "request == null";
                return false;
            }

            if (request.isDone)
            {
                if (request.assetBundle == null)
                {
                    error = "assetBundle == null";
                }
                else
                {
                    //ab包加载完毕，加载对应Manifest
                    var manifest = request.assetBundle.LoadAsset<Manifest>(assetName);
                    if (manifest == null)
                        error = "manifest == null";
                    else
                        Assets.OnLoadManifest(manifest); //记录bundle的依赖关系以及asset和bundle的对应关系
                }
                //设置加载状态为 LoadState.Loaded
                loadState = LoadState.Loaded;
                return false;
            }

            return true;
        }

        internal override void Unload()
        {
            if (request != null)
            {
                request.Release();
                request = null;
            }

            loadState = LoadState.Unload;
        }
    }

    public class BundleAssetRequest : AssetRequest
    {
        protected readonly string assetBundleName;
        protected BundleRequest BundleRequest;
        protected List<BundleRequest> children = new List<BundleRequest>();

        public BundleAssetRequest(string bundle)
        {
            assetBundleName = bundle;
        }

        internal override void Load()
        {
            //先加载对应bundle
            BundleRequest = Assets.LoadBundle(assetBundleName);
            var names = Assets.GetAllDependencies(assetBundleName);

            //bundle如果有依赖项优先加载依赖项
            foreach (var item in names) 
                children.Add(Assets.LoadBundle(item));
            var assetName = Path.GetFileName(name);
            var ab = BundleRequest.assetBundle;

            //从bundle中加载asset
            if (ab != null) 
                asset = ab.LoadAsset(assetName, assetType);
            if (asset == null) error = "asset == null";
            loadState = LoadState.Loaded;
        }

        internal override void Unload()
        {
            if (BundleRequest != null)
            {
                BundleRequest.Release();
                BundleRequest = null;
            }

            if (children.Count > 0)
            {
                foreach (var item in children) item.Release();
                children.Clear();
            }

            asset = null;
        }
    }

    public class BundleAssetRequestAsync : BundleAssetRequest
    {
        private AssetBundleRequest _request;

        public BundleAssetRequestAsync(string bundle)
            : base(bundle)
        {
        }

        public override float progress
        {
            get
            {
                if (isDone) return 1;

                if (loadState == LoadState.Init) return 0;

                if (_request != null) return _request.progress * 0.7f + 0.3f;

                if (BundleRequest == null) return 1;

                var value = BundleRequest.progress;
                var max = children.Count;
                if (max <= 0)
                    return value * 0.3f;

                for (var i = 0; i < max; i++)
                {
                    var item = children[i];
                    value += item.progress;
                }

                return value / (max + 1) * 0.3f;
            }
        }

        private bool OnError(BundleRequest bundleRequest)
        {
            error = bundleRequest.error;
            if (!string.IsNullOrEmpty(error))
            {
                loadState = LoadState.Loaded;
                return true;
            }

            return false;
        }

        internal override bool Update()
        {
            if (!base.Update()) return false;

            if (loadState == LoadState.Init) return true;

            if (_request == null)
            {
                if (!BundleRequest.isDone) return true;
                if (OnError(BundleRequest)) return false;

                for (var i = 0; i < children.Count; i++)
                {
                    var item = children[i];
                    if (!item.isDone) return true;
                    if (OnError(item)) return false;
                }

                var assetName = Path.GetFileName(name);
                _request = BundleRequest.assetBundle.LoadAssetAsync(assetName, assetType);
                if (_request == null)
                {
                    error = "request == null";
                    loadState = LoadState.Loaded;
                    return false;
                }

                return true;
            }

            if (_request.isDone)
            {
                asset = _request.asset;
                loadState = LoadState.Loaded;
                if (asset == null) error = "asset == null";
                return false;
            }

            return true;
        }

        internal override void Load()
        {
            BundleRequest = Assets.LoadBundleAsync(assetBundleName);
            var bundles = Assets.GetAllDependencies(assetBundleName);
            foreach (var item in bundles) children.Add(Assets.LoadBundleAsync(item));
            loadState = LoadState.LoadAssetBundle;
        }

        internal override void Unload()
        {
            _request = null;
            loadState = LoadState.Unload;
            base.Unload();
        }

        internal override void LoadImmediate()
        {
            BundleRequest.LoadImmediate();
            foreach (var item in children) item.LoadImmediate();
            if (BundleRequest.assetBundle != null)
            {
                var assetName = Path.GetFileName(name);
                asset = BundleRequest.assetBundle.LoadAsset(assetName, assetType);
            }

            loadState = LoadState.Loaded;
            if (asset == null) error = "asset == null";
        }
    }

    public class SceneAssetRequest : AssetRequest
    {
        protected readonly string sceneName;
        public string assetBundleName;
        protected BundleRequest BundleRequest;
        protected List<BundleRequest> children = new List<BundleRequest>(); //依赖的bundle

        public SceneAssetRequest(string path, bool addictive)
        {
            name = path;
            Assets.GetAssetBundleName(path, out assetBundleName);
            sceneName = Path.GetFileNameWithoutExtension(name);
            loadSceneMode = addictive ? LoadSceneMode.Additive : LoadSceneMode.Single;
        }

        public LoadSceneMode loadSceneMode { get; protected set; }

        public override float progress
        {
            get { return 1; }
        }

        internal override void Load()
        {
            if (!string.IsNullOrEmpty(assetBundleName))
            {
                BundleRequest = Assets.LoadBundle(assetBundleName);
                if (BundleRequest != null)
                {
                    var bundles = Assets.GetAllDependencies(assetBundleName);
                    foreach (var item in bundles) children.Add(Assets.LoadBundle(item));
                    SceneManager.LoadScene(sceneName, loadSceneMode);
                }
            }
            else
            {
                SceneManager.LoadScene(sceneName, loadSceneMode);
            }

            loadState = LoadState.Loaded;
        }

        internal override void Unload()
        {
            if (BundleRequest != null)
                BundleRequest.Release();

            if (children.Count > 0)
            {
                foreach (var item in children) item.Release();
                children.Clear();
            }

            if (loadSceneMode == LoadSceneMode.Additive)
                if (SceneManager.GetSceneByName(sceneName).isLoaded)
                    SceneManager.UnloadSceneAsync(sceneName);

            BundleRequest = null;
            loadState = LoadState.Unload;
        }
    }

    public class SceneAssetRequestAsync : SceneAssetRequest
    {
        private AsyncOperation _request;

        public SceneAssetRequestAsync(string path, bool addictive)
            : base(path, addictive)
        {
        }

        public override float progress
        {
            get
            {
                if (isDone) return 1;

                if (loadState == LoadState.Init) return 0;

                if (_request != null) return _request.progress * 0.7f + 0.3f;

                if (BundleRequest == null) return 1;

                var value = BundleRequest.progress;
                var max = children.Count;
                if (max <= 0)
                    return value * 0.3f;

                for (var i = 0; i < max; i++)
                {
                    var item = children[i];
                    value += item.progress;
                }

                return value / (max + 1) * 0.3f;
            }
        }

        private bool OnError(BundleRequest bundleRequest)
        {
            error = bundleRequest.error;
            if (!string.IsNullOrEmpty(error))
            {
                loadState = LoadState.Loaded;
                return true;
            }

            return false;
        }

        internal override bool Update()
        {
            if (!base.Update()) return false;

            if (loadState == LoadState.Init) return true;

            if (_request == null)
            {
                if (BundleRequest == null)
                {
                    error = "bundle == null";
                    loadState = LoadState.Loaded;
                    return false;
                }
                
                //场景对应的bundle是否加载完成
                if (!BundleRequest.isDone) return true;

                if (OnError(BundleRequest)) return false;
                //场景bundle所依赖的bundle是否加载完成
                for (var i = 0; i < children.Count; i++)
                {
                    var item = children[i];
                    if (!item.isDone) return true;
                    if (OnError(item)) return false;
                }
                //所有bundle都加载完成才开始加载场景
                LoadScene();

                return true;
            }

            if (_request.isDone)
            {
                loadState = LoadState.Loaded;
                return false;
            }

            return true;
        }

        private void LoadScene()
        {
            try
            {
                //异步加载场景
                _request = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
                loadState = LoadState.LoadAsset;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                error = e.Message;
                loadState = LoadState.Loaded;
            }
        }

        internal override void Load()
        {
            if (!string.IsNullOrEmpty(assetBundleName))
            {
                BundleRequest = Assets.LoadBundleAsync(assetBundleName);
                var bundles = Assets.GetAllDependencies(assetBundleName);
                foreach (var item in bundles) children.Add(Assets.LoadBundleAsync(item));
                loadState = LoadState.LoadAssetBundle;
            }
            else
            {
                LoadScene();
            }
        }

        internal override void Unload()
        {
            base.Unload();
            _request = null;
        }
    }

    public class WebAssetRequest : AssetRequest
    {
        private UnityWebRequest _www;

        public override float progress
        {
            get
            {
                if (isDone) return 1;
                if (loadState == LoadState.Init) return 0;

                if (_www == null) return 1;

                return _www.downloadProgress;
            }
        }

        public override string error
        {
            get { return _www.error; }
        }


        internal override bool Update()
        {
            if (!base.Update()) return false;

            if (loadState == LoadState.LoadAsset)
            {
                if (_www == null)
                {
                    error = "www == null";
                    return false;
                }

                if (!string.IsNullOrEmpty(_www.error))
                {
                    error = _www.error;
                    loadState = LoadState.Loaded;
                    return false;
                }

                if (_www.isDone)
                {
                    GetAsset();
                    loadState = LoadState.Loaded;
                    return false;
                }

                return true;
            }

            return true;
        }

        private void GetAsset()
        {
            if (assetType == typeof(Texture2D))
                asset = DownloadHandlerTexture.GetContent(_www);
            else if (assetType == typeof(AudioClip))
                asset = DownloadHandlerAudioClip.GetContent(_www);
            else if (assetType == typeof(TextAsset))
                text = _www.downloadHandler.text;
            else
                bytes = _www.downloadHandler.data;
        }

        internal override void Load()
        {
            if (assetType == typeof(AudioClip))
            {
                _www = UnityWebRequestMultimedia.GetAudioClip(name, AudioType.WAV);
            }
            else if (assetType == typeof(Texture2D))
            {
                _www = UnityWebRequestTexture.GetTexture(name);
            }
            else
            {
                _www = new UnityWebRequest(name);
                _www.downloadHandler = new DownloadHandlerBuffer();
            }

            _www.SendWebRequest();
            loadState = LoadState.LoadAsset;
        }

        internal override void Unload()
        {
            if (asset != null)
            {
                Object.Destroy(asset);
                asset = null;
            }

            if (_www != null)
                _www.Dispose();

            bytes = null;
            text = null;
            loadState = LoadState.Unload;
        }
    }

    public class BundleRequest : AssetRequest
    {
        public string assetBundleName { get; set; }

        public AssetBundle assetBundle
        {
            get { return asset as AssetBundle; }
            internal set { asset = value; }
        }

        internal override void Load()
        {
            asset = AssetBundle.LoadFromFile(name);
            if (assetBundle == null)
                error = name + " LoadFromFile failed.";
            loadState = LoadState.Loaded;
        }

        internal override void Unload()
        {
            if (assetBundle == null)
                return;
            assetBundle.Unload(true);
            assetBundle = null;
            loadState = LoadState.Unload;
        }
    }

    public class BundleRequestAsync : BundleRequest
    {
        private AssetBundleCreateRequest _request;

        public override float progress
        {
            get
            {
                if (isDone) return 1;
                if (loadState == LoadState.Init) return 0;
                if (_request == null) return 1;
                return _request.progress;
            }
        }

        internal override bool Update()
        {
            if (!base.Update()) return false;

            if (loadState == LoadState.LoadAsset)
                if (_request.isDone)
                {
                    //异步加载完成，assetBundle赋值
                    assetBundle = _request.assetBundle;
                    if (assetBundle == null) error = string.Format("unable to load assetBundle:{0}", name);
                    //状态标记为LoadState.Loaded
                    loadState = LoadState.Loaded;
                    return false;
                }

            return true;
        }

        internal override void Load()
        {
            if (_request == null)
            {
                _request = AssetBundle.LoadFromFileAsync(name); //异步加载对应的bundle
                if (_request == null)
                {
                    error = name + " LoadFromFile failed.";
                    return;
                }
                
                //标记状态为LoadState.LoadAsset，Update里次状态会检测_request是否加载完成
                loadState = LoadState.LoadAsset;
            }
        }

        internal override void Unload()
        {
            _request = null;
            loadState = LoadState.Unload;
            base.Unload();
        }

        internal override void LoadImmediate()
        {
            Load();
            assetBundle = _request.assetBundle;
            if (assetBundle != null) Debug.LogWarning("LoadImmediate:" + assetBundle.name);
            loadState = LoadState.Loaded;
        }
    }

    public class WebBundleRequest : BundleRequest
    {
        private UnityWebRequest _request;
        public bool cache;
        public Hash128 hash;

        public override float progress
        {
            get
            {
                if (isDone) return 1;
                if (loadState == LoadState.Init) return 0;

                if (_request == null) return 1;

                return _request.downloadProgress;
            }
        }

        internal override void Load()
        {
            _request = cache
                ? UnityWebRequestAssetBundle.GetAssetBundle(name, hash)
                : UnityWebRequestAssetBundle.GetAssetBundle(name);
            _request.SendWebRequest();
            loadState = LoadState.LoadAssetBundle;
        }

        internal override void Unload()
        {
            if (_request != null)
            {
                _request.Dispose();
                _request = null;
            }

            loadState = LoadState.Unload;
            base.Unload();
        }
    }
}