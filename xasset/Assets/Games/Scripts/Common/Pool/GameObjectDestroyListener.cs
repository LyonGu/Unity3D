using System;
using UnityEngine;

namespace GamePool
{
    public class GameObjectDestroyListener:  MonoBehaviour
    {
        private int _poolId;


        public void SetData(int poolId)
        {
            _poolId = poolId;
        }

        private void OnDestroy()
        {
            GameObjectPoolManager.DestroyGameObject(_poolId, gameObject);
        }
    }
}