using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

//原始code
//public class ZombieMoveSystem : ComponentSystem {
//    
//    protected override void OnUpdate() {
//        Entities.WithAll<Tag_Zombie>().ForEach((ref Translation translation) => {
//            float3 playerPosition = float3.zero;
//            float3 moveDir = math.normalize(playerPosition - translation.Value);
//
//            float moveSpeed = 1.8f;
//            translation.Value += moveDir * moveSpeed * Time.DeltaTime;
//        });
//    }
//
//}


//使用SystemBase 默认会开启Burst
//public class ZombieMoveSystem_EX1 : SystemBase {
//    
//    protected override void OnUpdate()
//    {
//        float deltaTime = Time.DeltaTime;
//        //默认会使用burst
//        Entities.WithAll<Tag_Zombie>().ForEach((ref Translation translation) => {
//            float3 playerPosition = float3.zero;
//            float3 moveDir = math.normalize(playerPosition - translation.Value);
//
//            float moveSpeed = 1.8f;
//            translation.Value += moveDir * moveSpeed * deltaTime;
//        }).Run();
//    }
//}


// 使用job
public class ZombieMoveSystem_EX2 : SystemBase {
    private EntityQuery query; 
    protected override void OnCreate()
    {
        query = this.GetEntityQuery(typeof(Translation),typeof(Tag_Zombie));
    }
    [BurstCompile]
    private struct ZombieMoveJob : IJobEntityBatch
    {
        [ReadOnly]
        public float deltaTime;
        
        public ComponentTypeHandle<Translation> PositionTypeHandleAccessor;
        
        public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
        {
            NativeArray<Translation> positions = batchInChunk.GetNativeArray<Translation>(PositionTypeHandleAccessor);
            float3 playerPosition = float3.zero;
            float moveSpeed = 1.8f;
            for (int i = 0; i < positions.Length; i++)
            {
                float3 position = positions[i].Value;
                float3 moveDir = math.normalize(playerPosition - position);
                
                position += moveDir * moveSpeed * deltaTime;
                positions[i] = new Translation()
                {
                    Value = position
                };
            }
        }
    }

    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        var job = new ZombieMoveJob()
        {
            PositionTypeHandleAccessor = this.GetComponentTypeHandle<Translation>(false),
            deltaTime = deltaTime
        };
        
        this.Dependency = job.ScheduleParallel(query, 1, this.Dependency);
    }
}