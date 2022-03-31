using System;
using GameLog;
using libx;
using UnityObject = UnityEngine.Object;
namespace Game
{
    public static partial class AssetsMgr
    {
        #region Load

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
            
            public static void LoadAyns<T>(string assetName, Action<T> complete) where T:UnityObject
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
                        return;
                    complete?.Invoke(_request.asset as T);
                };
            }

        #endregion
       
    }

}

