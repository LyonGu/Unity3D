using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class GameHandleSpawn : SystemBase
{
    private float spawnTargetTimer;
    private bool isSpwanEnityDone = false;
    private Unity.Mathematics.Random random;
    protected override void OnCreate()
    {
        base.OnCreate();
        random = new Unity.Mathematics.Random(56);
    }

    protected override void OnUpdate()
    {
        spawnTargetTimer -= Time.DeltaTime;
        if (spawnTargetTimer < 0) {
            spawnTargetTimer = .1f;

            if (!isSpwanEnityDone)
            {
                isSpwanEnityDone = true;
                for (int i = 0; i < 100; i++) {
                    SpawnUnitEntity();
                }

            }

            for (int i = 0; i < 200; i++) {
                SpawnTargetEntity();
            }
        }
    }
    
    private void SpawnTargetEntity() 
    {
        if(GameHandleSpawnAuthoring.pfTargetEntity == Entity.Null)
            return;
        var targetEntity = EntityManager.Instantiate(GameHandleSpawnAuthoring.pfTargetEntity);
        float3 pos = new float3(random.NextFloat(-8, +8f), random.NextFloat(-5, +5f), 0f);
        EntityManager.SetComponentData<Translation>(targetEntity, 
            new Translation { 
                Value = pos
            }
        );
        
        EntityManager.SetComponentData<TargetSelf>(targetEntity, 
            new TargetSelf { 
                self = targetEntity
            }
        );
        EntityManager.RemoveComponent<TargetOrigin>(targetEntity);
    }
    
    private void SpawnUnitEntity() 
    {
        if(GameHandleSpawnAuthoring.pfUnityEntity == Entity.Null)
            return;
        var unitEntity = EntityManager.Instantiate(GameHandleSpawnAuthoring.pfUnityEntity);
        float3 pos = new float3(random.NextFloat(-8, +8f), random.NextFloat(-5, +5f), 0f);
        EntityManager.SetComponentData<Translation>(unitEntity, 
            new Translation { 
                Value = pos
            }
        );
        
        EntityManager.SetComponentData<UnitSelf>(unitEntity, 
            new UnitSelf { 
                self = unitEntity
            }
        );
        EntityManager.RemoveComponent<UnitOrigin>(unitEntity);
    }
}

//public class GameHandleSpawn_EX : SystemBase
//{
//    
//}
    
