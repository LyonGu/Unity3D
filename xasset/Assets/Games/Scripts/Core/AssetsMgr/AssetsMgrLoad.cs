using System;
using System.Collections.Generic;
using System.Diagnostics;
using DXGame.core;
using DXGame.structs;
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
            
            /// <summary>
            ///  加载一个资源对象，加载完成回调传入assetLogicId用于卸载使用
            /// </summary>
            /// <param name="assetName">资源名字</param>
            /// <param name="complete"></param>
            /// <typeparam name="T"></typeparam>
            public static void Load<T>(string assetName, Action<T,int> complete) where T:UnityObject
            {
                if (string.IsNullOrEmpty(assetName))
                {
                    LogUtils.Error($"【AssetsMgr.Load】 assetName is empty===========");
                    return;
                }
                int assetLogicId = NameToId(assetName);
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
                complete?.Invoke(request.asset as T, assetLogicId);
            }
            
            public static void LoadAsyn<T>(string assetName, Action<T,int> complete) where T:UnityObject
            {
                if (string.IsNullOrEmpty(assetName))
                {
                    LogUtils.Error($"【AssetsMgr.LoadAyns】 assetName is empty===========");
                    return;
                }
                int assetLogicId = NameToId(assetName);
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
                    complete?.Invoke(_request.asset as T, assetLogicId);
                };
            }
            
            public static void LoadAsyn<T>(string assetName, Action<T, AssetRequest,int> complete) where T:UnityObject
            {
                if (string.IsNullOrEmpty(assetName))
                {
                    LogUtils.Error($"【AssetsMgr.LoadAyns】 assetName is empty===========");
                    return;
                }

                int assetLogicId = NameToId(assetName);
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
                    complete?.Invoke(_request.asset as T, _request, assetLogicId);
                };
            }
            public static void LoadSprie(string assetName, Action<Sprite,int> complete, bool isAsyn = true)
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
        
        
        #region UnLoad
            
            //卸载某个资源
            public static void UnLoad(string assetName)
            {
                if (string.IsNullOrEmpty(assetName))
                    return;
                string assetPath = Assets.GetAssetPathByName(assetName);
                AssetRequest request = Assets.TryGetAssetRequest(assetPath);
                if(request!=null)
                    request.Release(); //XAsset库里回去判断该资源是否要释放
            }
            
            public static void UnLoad(int assetLogicId)
            {
                string assetName = IdToName(assetLogicId);
                UnLoad(assetName);
            }
            
            //卸载一个GameObject
            public static void UnLoadGameObject(GameObject obj, int assetLogicId)
            {
                if (obj != null)
                {
                    GameObject.Destroy(obj);
                    UnLoad(assetLogicId);
                }
            }


            #endregion

        #region GameObjectInstant
        //单帧最大实例化数量
        private static int INSTANTMAXCOUNT = 5; 
        
        //单帧最大实例化时间，单位毫秒
        private static int INSTANTMAXTIME = 10; 
        public static Dictionary<int, InstantDoneCallData> InstanstRequestDoneCallMap = new Dictionary<int, InstantDoneCallData>(64);
        public static StructArray<GameObjectInstantiateRequest> InstantiateRequestList = new StructArray<GameObjectInstantiateRequest>(128);
        public struct InstantDoneCallData
        {
            public int callBackId;
            public Action<bool> completed;
        }
        
        public struct GameObjectInstantiateRequest
        {
            public int requestId;     //实例化请求Id
            public int callbackId;  //实例化完成回调id
            public int assetRequestNameLogicId; //AssetRequest.Name映射的id，可以通过这个拿到AssetRequest上面对应的Asset
            public int instantCount; //实例化个数
            public int assetLogicId; //资源逻辑id，通过这个可以找到资源名字
        }
        
        
        private static int _PoolCallBackId = 0;
        private static int _InstantRequestId = 0;
        public static class GameObjectInstantQueue
        {
            
            private static Stopwatch _clock = new Stopwatch();
            public static void Update()
            {
                int InstantiateRequestCount = InstantiateRequestList.Count;
                if (InstantiateRequestCount > 0)
                {
                    _clock.Restart();
                    bool isTimeOut = false;  //实例化时间超时
                    bool isCountOut = false; //实例化数量超过上限
                    int InstantDoneCount = 0;
                    for (var i = InstantiateRequestCount -1; i >= 0 ; i--)
                    {
                        ref var requestData = ref InstantiateRequestList[i];
                        int instantCount = requestData.instantCount;
                        for (int j = 0; j < instantCount; j++)
                        {
                            //先根据AssetRequest拿到对一个的Asset

                            AssetRequest assetRequest = TryGetAssetRequest(requestData.assetRequestNameLogicId);
                            if (assetRequest != null && assetRequest.isDone)
                            {
                                
                                string assetName = IdToName(requestData.assetLogicId);
                                var gameObject = InternelCreateGameObject(assetRequest.asset as GameObject, _poolRootTransform);
                                int assetLogicId = requestData.assetLogicId;
                                var req = new PoolGetRequest
                                {
                                    obj = gameObject,
                                    requestId = assetLogicId
                                };
                                PushToGameObjectPool(assetName, ref req);
                                requestData.instantCount--;
                                InstantDoneCount++;
                                if (INSTANTMAXCOUNT > 0 && InstantDoneCount >= INSTANTMAXCOUNT)
                                {
                                    isCountOut = true;
                                    break;
                                }

                                if (INSTANTMAXTIME > 0 && _clock.ElapsedMilliseconds > INSTANTMAXTIME)
                                {
                                    isTimeOut = true;
                                    break;
                                }
                            }
                            else
                            {
                                if (assetRequest == null || assetRequest.IsUnused())
                                {
                                    //被卸载了
                                    requestData.instantCount = -1;
                                    break;
                                }
                            }
                        }
                        if (requestData.instantCount <= 0)
                        {
                              //所有的都实例化完成了   
                              InstantiateRequestList.RemoveAt(i);
                              if (InstanstRequestDoneCallMap.TryGetValue(requestData.callbackId, out var CallData))
                              {
                                  InstanstRequestDoneCallMap.Remove(requestData.callbackId);
                                  bool isOk = requestData.instantCount == -1 ? false : true;
                                  CallData.completed?.Invoke(isOk);
                                  
                              }
                        }

                        if (isTimeOut || isCountOut)
                        {
                            break;
                        }
                    }
                }
            }
        }

        #endregion

        #region GameObjectGet

        
        //后期可扩展
        public struct PoolGetRequest
        {
            public GameObject obj;
            public int requestId;  //资源名字对应的逻辑ID
        }

        private static int _AssetLogicId = 0;
        public static Dictionary<string, StructArray<PoolGetRequest>> gameObjectPool =
            new Dictionary<string, StructArray<PoolGetRequest>>();
        
  
        private static GameObject InternelCreateGameObject(GameObject obj, Transform paTransform = null)
        {
            if(obj == null)
                return null;
            GameObject gameObject = null;
            if (paTransform!=null)
            {
                gameObject = GameObject.Instantiate(obj, paTransform, false);
            }
            else
            {
                gameObject = GameObject.Instantiate(obj);
            }

            return gameObject;
        }

        public static void LoadGameObject(string assetName, Action<GameObject,int> complete, bool isAsyn = true, Transform paTransform = null)
        {
            if (isAsyn)
            {
                LoadAsyn<GameObject>(assetName, (obj, assstLogicId) =>
                {
                    var gameObject = InternelCreateGameObject(obj, paTransform);
                    if(gameObject == null)
                        return;
                    complete(gameObject,assstLogicId);
                });
            }
            else
            {
                Load<GameObject>(assetName, (obj, assstLogicId) =>
                {
                    var gameObject = InternelCreateGameObject(obj, paTransform);
                    if(gameObject == null)
                        return;
                    complete(gameObject, assstLogicId);
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
                ref var req = ref aPool.AddRef();
                req = pgRequest;
            }
            else
            {
                aPool = new StructArray<PoolGetRequest>();
                gameObjectPool.Add(assetName, aPool);
                ref var req = ref aPool.AddRef();
                req = pgRequest;
            }
        }

        public static void CreatePoolGameObject(string assetName, int count, Action<bool> complete =null)
        {
            if(count <=0) return;
            if(string.IsNullOrEmpty(assetName)) return;
            int assetLogicId = NameToId(assetName);
            int realyNeedCount = count;
            StructArray<PoolGetRequest> aPool;
            if (gameObjectPool.TryGetValue(assetName, out aPool))
            {
                //判断池子里是否有
                if (aPool.Count >= count)
                {
                    complete?.Invoke(true);
                    return;
                }
                
                realyNeedCount = count - aPool.Count;
            }
            int loadDoneCount = 0;
            LoadAsyn<GameObject>(assetName, (obj, request, rassstLogicId) =>
            {
                InstantDoneCallData callData = new InstantDoneCallData
                {
                    completed = complete,
                    callBackId = _PoolCallBackId
                };
                InstanstRequestDoneCallMap[_PoolCallBackId] = callData;

                _InstantRequestId++;
                int assetRequestNameLogicId = Assets.NameToId(request.name);
                GameObjectInstantiateRequest InstantiateRequest = new GameObjectInstantiateRequest
                {
                    requestId =  _InstantRequestId,
                    callbackId = _PoolCallBackId,
                    assetRequestNameLogicId = assetRequestNameLogicId,
                    instantCount =  realyNeedCount,
                    assetLogicId = assetLogicId
                };
                ref var value = ref InstantiateRequestList.AddRef();
                value = InstantiateRequest;
            });
        }
        
        public static Dictionary<int, string> IdNameMap = new Dictionary<int, string>();
        public static Dictionary<string, int> NameIdMap = new Dictionary<string, int>();
        public static int NameToId(string assetName)
        {
            if (!NameIdMap.TryGetValue(assetName, out int assetId))
            {
                _AssetLogicId++;
                string name = string.Intern(assetName);
                IdNameMap[_AssetLogicId] = name;
                NameIdMap[name] = _AssetLogicId;
                return _AssetLogicId;
            }
            else
            {
                return assetId;
            }
        }

        public static string IdToName(int assetId)
        {
            if (IdNameMap.TryGetValue(assetId, out string name))
            {
                return name;
            }

            return string.Empty;
        }

        /// <summary>
        /// 从对象池里拿出一个GameObject
        /// </summary>
        /// <param name="assetName">资源名字</param>
        /// <param name="complete">从池子里拿出来回调</param>
        /// <param name="paTransform">从池子里拿出来GameObject,需要设置的父节点</param>
        /// <param name="catchCount">池子大小</param>
        public static void PoolGetGameObject(string assetName, Action<GameObject, int> complete, Transform paTransform = null, int catchCount = 0)
        {
            if(string.IsNullOrEmpty(assetName)) return;
            if (catchCount > 0)
            {
                CreatePoolGameObject(assetName, catchCount, (isCreaetOk) =>
                {
                    if (isCreaetOk)
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
                                //apRequest.requestId 资源名字对应的logicID
                                complete(apRequest.obj, apRequest.requestId);
                            }
                            else
                            {
                         
                                LoadGameObject(assetName, (obj, assetLogicId) =>
                                {
                                    complete(obj, assetLogicId);
                                }, true, paTransform);
                            }
                        }
                    }
                    else
                    {
                        LogUtils.Error("【AssetsMgr.PoolGetGameObject】CreatePoolGameObject  失败=====");
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
                            apRequest.obj.transform.SetParent(paTransform, false);
                        }
                        complete(apRequest.obj, apRequest.requestId);
                    }
                    else
                    {
                        LoadGameObject(assetName, (obj,assetLogicId) =>
                        {
                            complete(obj, assetLogicId);
                        }, true, paTransform);
                    }
                }
                else
                {
                    LoadGameObject(assetName, (obj,assetLogicId) =>
                    {
                        complete(obj, assetLogicId);
                    }, true, paTransform);
                }
            }
        }

        public static void PoolReturnGameObject(GameObject obj, int requestId)
        {
            if (requestId == -1 || obj == null)
                return;
            string assetName = IdToName(requestId);
            var req = new PoolGetRequest
            {
                obj = obj,
                requestId = requestId
            };
            PushToGameObjectPool(assetName, ref req);

        }

        public static void ClearPool()
        {
            
            foreach (var item in gameObjectPool)
            {
                item.Value.Clear(); //只是把Count置为0，下次还能复用内存
            }
            //gameObjectPool.Clear();
        }

        #endregion

       
    }

}

