using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class Sys_Spawner_FromMonoBehaviour : MonoBehaviour
{

    public GameObject prefab;

    public int CountX = 10;

    public int CountY = 10;
    // Start is called before the first frame update
    private BlobAssetStore blobAssetStore;
    void Start()
    {
        
        //性能太差
        blobAssetStore = new BlobAssetStore();
        var setting = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, blobAssetStore);
        var entityFromPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, setting); //用Prefab中创建了一个实体对象
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        for (int i = 0; i < CountX; i++)
        {
            for (int j = 0; j < CountY; j++)
            {
                var instance = entityManager.Instantiate(entityFromPrefab);
                var position = transform.TransformPoint(new float3(i * 1.3F, noise.cnoise(new float2(i, j) * 0.21F) * 2, j * 1.3F));
                entityManager.SetComponentData(instance, new Translation { Value = position });
                
            }
        }

        //StartCoroutine(TestCreatePrefab());
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        if(blobAssetStore!=null)
            blobAssetStore.Dispose();
    }
}
