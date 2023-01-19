using System;
using System.Collections.Generic;
using DXGame.core;
using DXGame.structs;
using Game;
using libx;
using UnityEditor.VersionControl;
using UnityEngine;

namespace GamePool
{
    public class GameObjectPool
    {
        private readonly DateTime startDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        public string assetPath; //对象池名字 可以是资源路径名字
        public int poolId; //对象池唯一id
       
        public Action<GameObjectPool> _completeAllHandle; //所有对象实例化完成回调
        public Action<float> _progressHandle; //所有对象实例化进度回调
        public Action<GameObject> _completeHandle; //单个对象实例化完成回调
        
        public Action<GameObject> _getHandle; //从对象池里获取时的回调
        public Action<GameObject> _releaseHandle; //放进对象池时的回调
        public Action<GameObject> _destoryHandle; //从对象池时里销毁的回调
        public int count; //初始化总数量
        private double _remainTime; //停留在池子里的最大时间，超过这个时间仍未被使用会自动清理

        public static int _LogicID = 0;

        public static int GetLogicId()
        {
            return _LogicID++;
        }
        
        private double GetUTCMilliseconds() {
            TimeSpan ts = DateTime.UtcNow - startDateTime;
            return ts.TotalMilliseconds;
        }
        

        //使用struct结构避免GC，使用托管指针避免struct内存拷贝消耗
        public struct GameObjectInfo
        {
            public int LogicID;  //对象唯一ID
            public double startTime;  //进入池子的时间（倒计时使用）
            public int PoolId; //对象池ID 也可以理解为资源路径对应ID，可以通过它反向查询资源路径 GameObjectPoolManager
            public GameObject gObj;
            
            public bool Equals(GameObjectInfo other)
            {
                return this.LogicID == other.LogicID;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is GameObjectInfo))
                {
                    return false;
                }
                GameObjectInfo other = (GameObjectInfo)obj;
                return this.LogicID == other.LogicID;
            }

            public override int GetHashCode()
            {
                return this.LogicID.GetHashCode();
            }

            public override string ToString()
            {
                return this.LogicID.ToString();
            }

            public static bool operator ==(GameObjectInfo left, GameObjectInfo right)
            {
                return left.Equals(right);
            }
            public static bool operator !=(GameObjectInfo left, GameObjectInfo right)
            {
                return !(left == right);
            }
            
        }
        
        private StructArray<GameObjectInfo> pool = new StructArray<GameObjectInfo>(128);

        private GameObject _assetObj;
        public GameObjectPool(int poolId, string assetPath, int count, 
            Action<GameObjectPool> completeAllHandle, 
            Action<GameObject> completeHandle = null, 
            Action<float> progressHandle = null,
            Action<GameObject> getHandle = null,
            Action<GameObject> ReleaseHandle = null,
            Action<GameObject> DestoryHandle = null)
        {
            this.poolId = poolId;
            this.assetPath = assetPath;
            this.count = count;
            _completeAllHandle = completeAllHandle;
            _progressHandle = progressHandle;
            _completeHandle = completeHandle;
            _getHandle = getHandle;
            _releaseHandle = ReleaseHandle;
            _destoryHandle = DestoryHandle;
        }

        public void Init()
        {
            int hasCount = pool.Count;
            if(count <= hasCount)
                return;

            int InstantiateCount = count - hasCount;
            string assetName = Assets.GetNameByAssetPath(assetPath);
            int _countIndex = 0;
            for (int i = 0; i < InstantiateCount; i++)
            {
                AssetsMgr.LoadAsync<GameObject>(assetName, (obj, assetLogicId) =>
                {
                    _assetObj = obj;
                    var go = GameObject.Instantiate(obj);
                    go.SetActive(false);
                    GameObjectDestroyListener listener = go.AddMissingComponent<GameObjectDestroyListener>();
                    listener.SetData(poolId);
                    
                    //这里有可能一次实例化多个造成卡顿，可以生成实例化队列数据，放到update里分帧处理 TODO
 
                    ref GameObjectInfo info = ref CreateGameObjectInfo(go);
                    pool.Add(ref info);
                    _completeHandle?.Invoke(go);
                    _countIndex++;
                    float progress = _countIndex / InstantiateCount;
                    _progressHandle.Invoke(progress);
                    if (_countIndex == InstantiateCount)
                    {
                        _completeAllHandle.Invoke(this);
                    }

                });
            }

        }

        private ref GameObjectInfo CreateGameObjectInfo(GameObject go)
        {
            ref GameObjectInfo info = ref pool.AddRef(); //避免内存拷贝,传入的是托管指针
            info.LogicID = go.GetInstanceID();
            info.startTime = GetUTCMilliseconds();
            info.PoolId = poolId;
            info.gObj = go;
            return ref info;
        }


        //清理对象池操作
        public void Clear()
        {
            int count = pool.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    ref GameObjectInfo info = ref pool[i];
                    var gObj = info.gObj;
                    _destoryHandle?.Invoke(gObj);
                    GameObject.Destroy(gObj);
                }
                pool.Clear(true);
            }
        }

        public void DestroyGameObjectFromPool(GameObject obj, bool isCallDestroy = false)
        {
            if (obj == null)
                return;

            for (int i = 0; i < pool.Count; i++)
            {
                ref GameObjectInfo info = ref pool[i];
                if (info.gObj == obj)
                {
                    pool.RemoveAt(i);
                    _destoryHandle?.Invoke(info.gObj);
                    if(isCallDestroy)
                        GameObject.Destroy(info.gObj);
                    return;
                }
            }
        }

        //从对象池里获取对象
        public GameObject Get()
        {
            bool isOk = pool.Pop(out GameObjectInfo info);
            if (isOk)
            {
                _getHandle?.Invoke(info.gObj);
                return info.gObj;
            }
            else
            {
                var go = GameObject.Instantiate(_assetObj);
                go.SetActive(true);
                go.AddMissingComponent<GameObjectDestroyListener>();
                return go;
            }
        }
        

        //放入对象池
        public void Release(GameObject obj)
        {
            _releaseHandle?.Invoke(obj);
            ref GameObjectInfo info = ref pool.AddRef(); //避免内存拷贝,传入的是托管指针
            info.LogicID = obj.GetInstanceID();
            info.startTime = GetUTCMilliseconds();
            info.PoolId = poolId;
            info.gObj = obj;
            
        }


        public void Update()
        {
            int count = pool.Count;
            if (count > 0)
            {
                double nowTime = GetUTCMilliseconds();
                for (int i = count-1; i >=0; i--)
                {
                    ref GameObjectInfo info = ref pool[i];
                    double endTime = info.startTime + _remainTime;
                    if (nowTime > endTime)
                    {
                        pool.RemoveAt(i);
                        _destoryHandle?.Invoke(info.gObj);
                        GameObject.Destroy(info.gObj);
                    }
                }
                
            }
        }
    }
    
}