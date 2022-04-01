using System;
using System.Collections;
using System.Collections.Generic;
using GameLog;
using libx;
using UnityEngine;
namespace Game
{
    public static partial class AssetsMgr
    {
        public static void Init()
        {
            InitPool();
        }

        public static void Dispose()
        {
            
        }

        public static AssetRequest TryGetAssetRequest(string assetRequestName)
        {
            return Assets.TryGetAssetRequest(assetRequestName);
        }

        public static AssetRequest TryGetAssetRequest(int assetRequestNameId)
        {
            return Assets.TryGetAssetRequest(assetRequestNameId);
        }


        public static void Update()
        {
            GameObjectInstantQueue.Update();
        }
    }

}

