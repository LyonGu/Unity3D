using System;
using System.Collections.Generic;
using DXGame.core;
using GameLog;
using libx;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Game
{
    public static partial class AssetsMgr
    {
        private static GameObject _poolRootGameObject;
        private static Transform _poolRootTransform;
        public static  void InitPool()
        {
            _poolRootGameObject = new GameObject();
            _poolRootGameObject.name = "_AssetGameObjectPool";
            _poolRootTransform = _poolRootGameObject.transform;
            GameObject.DontDestroyOnLoad(_poolRootGameObject);
        }

        #region AssetLoad
            
            public static void Load<T>(string assetName, Action<T> complete) where T:UnityObject
            {
                if (string.IsNullOrEmpty(assetName))
                {
                    LogUtils.Error($"【AssetsMgr.Load】 assetName is empty===========");
                    return;
                }
                
                string path = Assets.GetAssetPathByName(assetName);
                if (string.IsNullOrEmpty(assetName))
                {
                    LogUtils.Error($"【AssetsMgr.Load】 path is empty, assetName is {assetName}===========");
                    return;
                }
                var request = Assets.LoadAsset(path, typeof(T));
                if (request == null  || !string.IsNullOrEmpty (request.error)) 
                {
                    LogUtils.Error($"【AssetsMgr.LoadAyns】 request.error is {request.error}===========");
                    request.Release ();
                    return;
                 
                }
                complete?.Invoke(request.asset as T);
            }
            
            public static void LoadAsyn<T>(string assetName, Action<T> complete) where T:UnityObject
            {
                if (string.IsNullOrEmpty(assetName))
                {
                    LogUtils.Error($"【AssetsMgr.LoadAyns】 assetName is empty===========");
                    return;
                }
                
                string path = Assets.GetAssetPathByName(assetName);
                if (string.IsNullOrEmpty(assetName))
                {
                    LogUtils.Error($"【AssetsMgr.LoadAyns】 path is empty, assetName is {assetName}===========");
                    return;
                }
                var request = Assets.LoadAssetAsync(path, typeof(T));
                request.completed = (_request) =>
                {
                    if (_request == null)
                        return;
                    if (!string.IsNullOrEmpty (_request.error)) {
                        LogUtils.Error($"【AssetsMgr.LoadAyns】 _request.error is {_request.error}===========");
                        _request.Release ();
                        return;
                    }

                    if (_request.asset == null)
                    {
                        request.Release ();
                        return;
                    }
                    complete?.Invoke(_request.asset as T);
                };
            }

            public static void LoadSprie(string assetName, Action<Sprite> complete, bool isAsyn = true)
            {
                if (isAsyn)
                {
                    LoadAsyn(assetName, complete);
                }
                else
                {
                    Load(assetName, complete);        
                }
            }

        #endregion

        #region GameObjectGet

        
        //后期可扩展
        public struct PoolGetRequest
        {
            public GameObject obj;
            public string assetName;
        }

        public static Dictionary<string, StructArray<PoolGetRequest>> gameObjectPool =
            new Dictionary<string, StructArray<PoolGetRequest>>();
        private static GameObject InternelCreateGameObject(GameObject obj, Transform paTransform = null)
        {
            if(obj == null)
                return null;
            var gameObject = GameObject.Instantiate(obj);
            if(gameObject == null)
                return null;
            if (paTransform!=null)
            {
                gameObject.transform.SetParent(paTransform, false);
            }

            return gameObject;
        }

        public static void LoadGameObject(string assetName, Action<GameObject> complete, bool isAsyn = true, Transform paTransform = null)
        {
            if (isAsyn)
            {
                LoadAsyn<GameObject>(assetName, (obj) =>
                {
                    var gameObject = InternelCreateGameObject(obj, paTransform);
                    if(gameObject == null)
                        return;
                    complete(gameObject);
                });
            }
            else
            {
                Load<GameObject>(assetName, (obj) =>
                {
                    var gameObject = InternelCreateGameObject(obj, paTransform);
                    if(gameObject == null)
                        return;
                    complete(gameObject);
                });        
            }
        }

        public static void PushToGameObjectPool(string assetName, ref PoolGetRequest pgRequest)
        {
            if(string.IsNullOrEmpty(assetName))
                return;
            StructArray<PoolGetRequest> aPool;
            if (gameObjectPool.TryGetValue(assetName, out aPool))
            {
                aPool.Add(ref pgRequest);
            }
            else
            {
                aPool = new StructArray<PoolGetRequest>();
                gameObjectPool.Add(assetName, aPool);
                aPool.Add(ref pgRequest);
            }
        }

        public static void CreatePoolGameObject(string assetName, int count, Action complete =null)
        {
            if(count <=0) return;
            if(string.IsNullOrEmpty(assetName)) return;
            int realyNeedCount = count;
            StructArray<PoolGetRequest> aPool;
            if (gameObjectPool.TryGetValue(assetName, out aPool))
            {
                //判断池子里是否有
                if (aPool.Count >= count)
                {
                    complete?.Invoke();
                    return;
                }
                
                realyNeedCount = count - aPool.Count;
            }
            int loadDoneCount = 0;
            LoadAsyn<GameObject>(assetName, (obj) =>
            {
                for (int i = 0; i < realyNeedCount; i++)
                {
                    //这里不能一帧实例化这么多，会卡，放到一个实例化列表里 Update分帧处理 TODO
                    var gameObject = InternelCreateGameObject(obj, _poolRootTransform);
                    loadDoneCount++;
                    var req = new PoolGetRequest
                    {
                        assetName = assetName,
                        obj = gameObject
                    };
                    PushToGameObjectPool(assetName, ref req);
                    if (loadDoneCount == realyNeedCount)
                    {
                        complete?.Invoke();
                    }
                }
            });
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetName">资源名字</param>
        /// <param name="complete">从池子里拿出来回调</param>
        /// <param name="paTransform">从池子里拿出来GameObject,需要设置的父节点</param>
        /// <param name="catchCount">池子大小</param>
        public static void PoolGetGameObject(string assetName, Action<GameObject> complete, Transform paTransform = null, int catchCount = 0)
        {
            if(string.IsNullOrEmpty(assetName)) return;
            if (catchCount > 0)
            {
                CreatePoolGameObject(assetName, catchCount, () =>
                {
                    StructArray<PoolGetRequest> aPool;
                    if (gameObjectPool.TryGetValue(assetName, out aPool))
                    {
                        bool isOk = aPool.Pop(out var apRequest);
                        if (isOk)
                        {
                            if (paTransform != null)
                            {
                                apRequest.obj.transform.SetParent(paTransform,false);
                            }
                            complete(apRequest.obj);
                        }
                    }
                   
                });
            }
            else
            {
                StructArray<PoolGetRequest> aPool;
                if (gameObjectPool.TryGetValue(assetName, out aPool))
                {
                    bool isOk = aPool.Pop(out var apRequest);
                    if (isOk)
                    {
                        if (paTransform != null)
                        {
                            apRequest.obj.transform.SetParent(paTransform,false);
                        }
                        complete(apRequest.obj);
                    }
                }
            }
        }
        

        #endregion
       
    }

}

