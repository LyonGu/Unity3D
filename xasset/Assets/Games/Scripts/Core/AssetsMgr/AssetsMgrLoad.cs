using System;
using System.Collections.Generic;
using System.Diagnostics;
using DXGame.core;
using DXGame.structs;
using GameLog;
using GamePool;
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
            
            public static void LoadAsync<T>(string assetName, Action<T,int> complete) where T:UnityObject
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
            
            public static void LoadAsync<T>(string assetName, Action<T, AssetRequest,int> complete) where T:UnityObject
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
                    LoadAsync(assetName, complete);
                }
                else
                {
                    Load(assetName, complete);        
                }
            }

        public static void LoadSceneAsync(string sceneName, Action complete)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                LogUtils.Error($"【AssetsMgr.LoadScene】 sceneName is empty===========");
                return;
            }
            int assetLogicId = NameToId(sceneName);
            string path = Assets.GetAssetPathByName(sceneName);
            SceneAssetRequest request = Assets.LoadSceneAsync(path, false);
            request.completed = (_request) =>
            {
                if (_request == null)
                    return;
                if (!string.IsNullOrEmpty(_request.error))
                {
                    LogUtils.Error($"【AssetsMgr.LoadScene】 _request.error is {_request.error}===========");
                    _request.Release();
                    return;
                }

                if (_request.asset == null)
                {
                    request.Release();
                    return;
                }
                complete?.Invoke();
            };
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
        //无效实例化数量
        private static int INVALIDINSTANTCOUNT = -1;
        //单帧最大实例化数量
        private static int INSTANTMAXCOUNT = 5; 
        
        //单帧最大实例化时间，单位毫秒
        private static int INSTANTMAXTIME = 10;

        //《callBackId, InstantDoneCallData》
        public static Dictionary<int, InstantDoneCallData> InstanstRequestDoneCallMap = new Dictionary<int, InstantDoneCallData>(64);

        public static StructArray<GameObjectInstantiateRequest> InstantiateRequestList = new StructArray<GameObjectInstantiateRequest>(128);

        //《InstantiateRequestId, GameObjectInstantiateRequest》
        public static Dictionary<int, GameObjectInstantiateRequest> InstantiateRequestListMap = new Dictionary<int, GameObjectInstantiateRequest>(128);
        public class InstantDoneCallData
        {
            public int callBackId;
            public Action<bool> completed; //实例化所有回调
            public Action<float> progress; //实例化一个进度回调，切场景预实例化使用
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
                                    requestData.instantCount = INVALIDINSTANTCOUNT;
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
                                  bool isOk = requestData.instantCount == INVALIDINSTANTCOUNT ? false : true;
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
                LoadAsync<GameObject>(assetName, (obj, assstLogicId) =>
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

        public static void CreatePoolGameObject(string assetName, int count, Action<bool> complete = null, Action<float> progress = null, int preLoadGroupId = -1)
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

            LoadAsync<GameObject>(assetName, (obj, request, rassstLogicId) =>
            {
                InstantDoneCallData callData = new InstantDoneCallData
                {
                    completed = complete,
                    progress = progress,
                    callBackId = _PoolCallBackId

                };
                InstanstRequestDoneCallMap[_PoolCallBackId] = callData;

                _InstantRequestId++;
                if (preLoadGroupId != -1)
                {
                    //预加载
                    AddGroupIdMap(preLoadGroupId, _InstantRequestId);
                }
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



        #endregion

        #region PreLoad 预加载

        //预加载ID
        private static int _preLoadGroupId = 0;
        //当前预加载ID
        private static int _curPreLoadGroupId = -1; 
        //<预加载Id,List<实例化请求ID>>
        private static Dictionary<int, Dictionary<int,bool>> _groupIdMap = new Dictionary<int, Dictionary<int, bool>>(16);


        private static void AddGroupIdMap(int preLoadGroupId, int instantRequestId)
        {
            if (!_groupIdMap.TryGetValue(preLoadGroupId, out var dict))
            {
                dict = DictionaryPool<int, bool>.Get();
                _groupIdMap.Add(_preLoadGroupId, dict);
                dict.Add(instantRequestId,true);
            }
            else
            {
                dict.Add(instantRequestId,true);
            }
  
        }

        public static void StopPreLoadGroupGameObject(int PreLoadGroupId)
        {
            //根据PreLoadGroupId ==》 找到一堆 InstanctRequestId
            //根据InstanctRequestId 找到实例化数据
            if (_groupIdMap.TryGetValue(PreLoadGroupId, out var idDict))
            {
                int InstantiateRequestCount = InstantiateRequestList.Count;
                if (InstantiateRequestCount > 0)
                {
                    for (int i = 0; i < InstantiateRequestCount; i++)
                    {
                        ref var data = ref InstantiateRequestList[i];
                        if (idDict.TryGetValue(data.requestId,out bool _))
                        {

                            data.instantCount = INVALIDINSTANTCOUNT; //实例化数量标志为-1， GameObjectInstantQueue.Update去判断

                            //已经实例化出来进入池子里的需要卸载 TODO
                            /*
                                gameObjectPool 数据要清空
                                对应的AssetRequest也要release
                             */
                            string assetName = IdToName(data.assetLogicId);
                            StructArray<PoolGetRequest> aPool;
                            if (gameObjectPool.TryGetValue(assetName, out aPool))
                            {
                                int poolGetRequestCount = aPool.Count;
                                if (poolGetRequestCount > 0)
                                {
                                    for (int j = 0; j < poolGetRequestCount; j++)
                                    {
                                        ref var poolGetRequest = ref aPool[j];
                                        UnLoadGameObject(poolGetRequest.obj, data.assetLogicId);
                                    }
                                }
                                gameObjectPool.Remove(assetName);
                            }
                            
                           
                        }
                    }

                    
                }
                _groupIdMap.Remove(PreLoadGroupId);
                DictionaryPool<int, bool>.Release(idDict);
            }

        }
        //预实例化一组GameObject对象
        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetNameArray">资源名列表</param>
        /// <param name="instantCountArray">实例化数量列表</param>
        /// <param name="progress">预实例化进度回调</param>
        /// <param name="complete">预实例化完成回调</param>
        public static void PreLoadGroupGameObject(string [] assetNameArray, int[] instantCountArray, Action<float> progress, Action complete)
        {
            if (assetNameArray.Length != instantCountArray.Length)
                return;
            if (_curPreLoadGroupId != -1)
            {
                //当前已经在进行预加载
                StopPreLoadGroupGameObject(_curPreLoadGroupId);
            }
            _preLoadGroupId++;
            _curPreLoadGroupId = _preLoadGroupId;
            int assetCount = assetNameArray.Length;
            int totalInstantCount = 0;
            int InstantDoneCount = 0;
            for (int i = 0; i < assetCount; i++)
            {
                string assetName = assetNameArray[i];
                if (string.IsNullOrEmpty(assetName))
                    continue;
                int instantCount = instantCountArray[i];
                if (instantCount <= 0)
                    instantCount = 1;

                //判断资源是否已经实例化过
                if (gameObjectPool.TryGetValue(assetName, out var aPool))
                {
                    //判断池子里是否有
                    if (aPool.Count >= instantCount)
                    {
                        continue;
                    }

                }
                totalInstantCount += instantCount;
                CreatePoolGameObject(assetName, instantCount, 
                    (isOk) =>
                        {
                            if (isOk)
                            {
                                //某个资源完成所有实例化回调
                                if (InstantDoneCount == totalInstantCount)
                                {
                                    complete();
                                }
                            }
               
                        },

                    (progressValue) =>
                        {
                            InstantDoneCount++;
                            //某个资源完成一次实例化回调
                            progress((float)InstantDoneCount / totalInstantCount);
                        }
                    );
            }
        }

        #endregion

        #region Clear 清理 

        public static void Clear()
        {
            ClearInstantQueue();
            ClearGameObjectPool();
            
        }

        //清理GameObject对象池数据
        public static void ClearGameObjectPool()
        {

            foreach (var item in gameObjectPool)
            {
                StructArray<PoolGetRequest> aPool = item.Value;
                int count = aPool.Count;
                if (count > 0)
                {
                    for (int j = 0; j < count; j++)
                    {
                        ref var poolGetRequest = ref aPool[j];
                        UnLoadGameObject(poolGetRequest.obj, poolGetRequest.requestId);
                    }
                }
                aPool.Clear(true); //只是把Count置为0，下次还能复用内存
            }
            gameObjectPool.Clear();
        }

        //清理实例化队列数据
        public static void ClearInstantQueue()
        {
            InstantiateRequestList.Clear(true);
        }

        #endregion


    }

}

