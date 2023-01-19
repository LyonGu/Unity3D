
using System;
using System.Collections.Generic;
using GamePool;
using UnityEngine;
using UnityEngine.Rendering;

//未考虑一帧实例化数量太多的情况，其实需要分帧考虑 再update里
public class GameObjectPoolManager : MonoSingletonQF<GameObjectPoolManager>
{
    public static Dictionary<int, GameObjectPool> PoolsDic = new Dictionary<int, GameObjectPool>();
    private static int _PoolID = 0;
    private static Dictionary<string, int> _RespathToIdDic = new Dictionary<string, int>(256);
    private static Dictionary<int, string> _IdToRespathDic = new Dictionary<int, string>(256);
    private static int GetID()
    {
        return _PoolID++;
    }

    public override void OnSingletonInit()
    {
    }

    private void Awake()
    {
       DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        foreach (var item in PoolsDic)
        {
            GameObjectPool pool = item.Value;
            pool.Update();
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Clear();
    }
    
    public static int GetPoolIdByResPath(string resPath)
    {
        if (_RespathToIdDic.TryGetValue(resPath, out int _pooId))
        {
            return _pooId;
        }
        return -1;
    }
    
    public static string GetResPathByPoolId(int pooId)
    {
        if (_IdToRespathDic.TryGetValue(pooId, out string resPath))
        {
            return resPath;
        }
        return string.Empty;
    }


    #region 管理对象池
    
    private static GameObjectPool GetPool(int id)
    {
        if (PoolsDic.TryGetValue(id, out GameObjectPool pool))
        {
            return pool;
        }
        return null;
    }
    
    
    private static void Add(GameObjectPool pool)
    {
        if(pool == null)
            return;
        if (PoolsDic.ContainsKey(pool.poolId)) return;
        PoolsDic.Add(pool.poolId, pool);
    }
    

    /// <summary>
    /// 添加一个对象池,内部为异步加载
    /// </summary>
    /// <param name="resPath">资源路径</param>
    /// <param name="count">初始化数量</param>
    /// <param name="completeAllHandle">所有对象实例化完成回调</param>
    /// <param name="completeHandle">单个对象实例化完成回调</param>
    /// <param name="progressHandle">实例化完成进度</param>
    /// <param name="GetHandle">从对象池里获取对象时的回调</param>
    /// <param name="ReleaseHandle">放进对象池时的回调</param>
    /// <param name="DestoryHandle">从对象池时里销毁的回调</param>
    /// <returns></returns>
    public static int Add(string resPath, int count,  
        Action<GameObjectPool> completeAllHandle,
        Action<GameObject> completeHandle = null, 
        Action<float> progressHandle = null,
        Action<GameObject> GetHandle = null,
        Action<GameObject> ReleaseHandle = null,
        Action<GameObject> DestoryHandle = null)
    {
        if(string.IsNullOrEmpty(resPath))
            return -1;
        
        //添加时先判断下是否已经有对应的对象池了
        if (_RespathToIdDic.TryGetValue(resPath, out int _pooId))
        {
            return _pooId;
        }
        
        int poolId = GetID();
        GameObjectPool pool = new GameObjectPool(poolId, resPath, count, (_pool) =>
        {
            completeAllHandle?.Invoke(_pool);
        }, completeHandle, progressHandle,GetHandle, ReleaseHandle, DestoryHandle);
        pool.Init();
        Add(pool);
        _RespathToIdDic.Add(resPath, poolId);
        _IdToRespathDic.Add(poolId,resPath);
        return poolId;
    }

    /// <summary>
    /// 删除一个对象池
    /// </summary>
    /// <param name="poolId">对象池id</param>
    public static void Remove(int poolId)
    {
        if (PoolsDic.TryGetValue(poolId, out GameObjectPool pool))
        {
            pool.Clear();
            PoolsDic.Remove(poolId);
            string resPath = _IdToRespathDic[poolId];
            _RespathToIdDic.Remove(resPath);
            _IdToRespathDic.Remove(poolId);
        }
    }
    
    /// <summary>
    /// 删除一个对象池
    /// </summary>
    /// <param name="resPath">对象池对应资源路径</param>
    public static void Remove(string resPath)
    {
        if (_RespathToIdDic.TryGetValue(resPath, out int _pooId))
        {
            Remove(_pooId);
        }
    }

    /// <summary>
    /// 清理所有对象池
    /// </summary>
    public static void Clear()
    {
        foreach (var item in PoolsDic)
        {
            item.Value.Clear();
        }
        PoolsDic.Clear();
        _RespathToIdDic.Clear();
        _IdToRespathDic.Clear();
    }

    #endregion

    #region 从对象池获取对象
    public static GameObject GetGameObject(int poolId)
    {
        GameObjectPool pool = GetPool(poolId);
        if (pool != null)
        {
            return pool.Get();
        }
        return null;
    }
    

    public static GameObject GetGameObject(string resPath)
    {
        int poolId = GetPoolIdByResPath(resPath);
        return GetGameObject(poolId);
    }
    
    #endregion
    
    
    #region 对象放入对象池
    public static void ReleaseGameObject(int poolId, GameObject obj)
    {
        if(obj == null)
            return;
        GameObjectPool pool = GetPool(poolId);
        if (pool != null)
        {
            pool.Release(obj);
        }
    }
    
    public static void ReleaseGameObject(string resPath, GameObject obj)
    {
        int poolId = GetPoolIdByResPath(resPath);
        ReleaseGameObject(poolId, obj);
    }
    #endregion

    #region 从对象池里销毁一个对象

    public static void DestroyGameObject(int poolId, GameObject obj)
    {
        GameObjectPool pool = GetPool(poolId);
        if (pool != null)
        {
            pool.DestroyGameObjectFromPool(obj);
        }
    }

    #endregion
}
