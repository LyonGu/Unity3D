/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Burst;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

public class UnitMoveToTargetSystem : ComponentSystem {

    protected override void OnUpdate()
    {
        float DeltaTime = Time.DeltaTime;
        Entities.ForEach((Entity unitEntity, ref HasTarget hasTarget, ref Translation translation) => {
            if (World.DefaultGameObjectInjectionWorld.EntityManager.Exists(hasTarget.targetEntity)) {
                Translation targetTranslation = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<Translation>(hasTarget.targetEntity);

                float3 targetDir = math.normalize(targetTranslation.Value - translation.Value);
                float moveSpeed = 5f;
                translation.Value += targetDir * moveSpeed * DeltaTime;

                if (math.distance(translation.Value, targetTranslation.Value) < .2f) {
                    // Close to target, destroy it
                    PostUpdateCommands.DestroyEntity(hasTarget.targetEntity);
                    PostUpdateCommands.RemoveComponent(unitEntity, typeof(HasTarget));
                }
            } else {
                // Target Entity already destroyed
                PostUpdateCommands.RemoveComponent(unitEntity, typeof(HasTarget));
            }
        });
    }

}

//[UpdateAfter(typeof(FindTargetJobSystem_Ex))]
//public class UnitMoveToTargetSystem_Ex : SystemBase
//{
//
//    private EntityCommandBufferSystem _entityCommandBufferSystem;
//
//    private EntityQuery unitHaveTargetQuery;
//    private EntityQuery TargetQuery;
//    protected override void OnCreate()
//    {
//        base.OnCreate();
//        _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
//        unitHaveTargetQuery = GetEntityQuery(
//            typeof(Unit),
//            typeof(Translation),
//            ComponentType.ReadOnly<UnitSelf>(),
//            ComponentType.ReadOnly<HasTarget>()
//        );
//        
//        TargetQuery = GetEntityQuery(
//            typeof(Target),
//            ComponentType.ReadOnly<Translation>(),
//            ComponentType.ReadOnly<TargetSelf>()
//        );
//    }
//    private struct TargetInfo
//    {
//        public float3 position;
//        public int entityInQueryIndex;
//        public Entity entity;
//    }
//    
//    [BurstCompile]
//    private struct SaveTargetPosJob : IJobEntityBatchWithIndex
//    {
//        [Unity.Collections.ReadOnly]
//        public ComponentTypeHandle<Translation> PositionTypeHandleAccessor;
//        
//        [Unity.Collections.ReadOnly]
//        public ComponentTypeHandle<TargetSelf> TargetSelfTypeHandleAccessor;
//        
//  
//        public NativeHashMap<Entity, TargetInfo>.ParallelWriter targetInfoHashMap;
//        public void Execute(ArchetypeChunk batchInChunk, int batchIndex, int indexOfFirstEntityInQuery)
//        {
//            NativeArray<Translation> positions = batchInChunk.GetNativeArray<Translation>(PositionTypeHandleAccessor);
//            NativeArray<TargetSelf> targetSelfs = batchInChunk.GetNativeArray<TargetSelf>(TargetSelfTypeHandleAccessor);
//            int length = positions.Length;
//            for (int i = 0; i < length; i++)
//            {
//                var position = positions[i].Value;
//                var targetEntity = targetSelfs[i].self;
//
//                int targetQueryIndex = indexOfFirstEntityInQuery + i;
//                targetInfoHashMap.TryAdd(targetEntity, new TargetInfo()
//                {
//                    position = position,
//                    entityInQueryIndex = targetQueryIndex,
//                    entity = targetEntity
//                });
//
//
//            }
//        }
//    }
//    
//    [BurstCompile]
//    private struct MoveToTargetJob : IJobEntityBatchWithIndex
//    {
//        //[DeallocateOnJobCompletion]
//        [Unity.Collections.ReadOnly]
//        public NativeHashMap<Entity, TargetInfo> targetInfoHashMap;
//        
//        public ComponentTypeHandle<Translation> PositionTypeHandleAccessor;
//        
//        [Unity.Collections.ReadOnly]
//        public ComponentTypeHandle<UnitSelf> UnitSelfTypeHandleAccessor;
//        
//        [Unity.Collections.ReadOnly]
//        public ComponentTypeHandle<HasTarget> HasTargetTypeHandleAccessor;
//
//        public float DeltaTime;
//
//        public EntityCommandBuffer.ParallelWriter ecb;
//        
//        // 被其他chunk存储的只读数据（潜在的）
//        [Unity.Collections.ReadOnly]
//        public ComponentDataFromEntity<LocalToWorld> EntityPositions;
//        
//        public void Execute(ArchetypeChunk batchInChunk, int batchIndex, int indexOfFirstEntityInQuery)
//        {
//            
//            NativeArray<Translation> positions = batchInChunk.GetNativeArray<Translation>(PositionTypeHandleAccessor);
//            NativeArray<UnitSelf> unitSelfs = batchInChunk.GetNativeArray<UnitSelf>(UnitSelfTypeHandleAccessor);
//            NativeArray<HasTarget> hasTargets = batchInChunk.GetNativeArray<HasTarget>(HasTargetTypeHandleAccessor);
//            int length = positions.Length;
//            float moveSpeed = 5f;
//            for (int i = 0; i < length; i++)
//            {
//                var position = positions[i].Value; // Unit的位置
//                var targetEntity = hasTargets[i].targetEntity;
//                int UnitQueryIndex = indexOfFirstEntityInQuery + i;
//                var UnitEntity = unitSelfs[i].self;
//                
//                
//                //需要判断targetEntity是否存在
//                if(EntityPositions.HasComponent(targetEntity))
//                {
//   
//                    var targetInfo = targetInfoHashMap[targetEntity];
//                    var targetEntityPosition = targetInfo.position;
//
//                    float3 targetDir = math.normalize(targetEntityPosition - position);
//                    position += targetDir * moveSpeed * DeltaTime;
//
//                    positions[i] = new Translation
//                    {
//                        Value = position
//                    };
//                
//                   
//                    if (math.distance(position, targetEntityPosition) < .2f)
//                    {
//                        // Close to target, destroy it
//                        int targetEntityInQueryIndex = targetInfo.entityInQueryIndex;
//                        ecb.DestroyEntity(targetEntityInQueryIndex, targetEntity);
//                        
//                        ecb.RemoveComponent(UnitQueryIndex, UnitEntity,ComponentType.ReadOnly<HasTarget>());
//                    }
//                }
//                else
//                {
//                    ecb.RemoveComponent(UnitQueryIndex, UnitEntity,ComponentType.ReadOnly<HasTarget>());
//                }
//                
//            }
//
//        }
//    }
//    protected override void OnUpdate()
//    {
//        
//        int count = TargetQuery.CalculateEntityCount();
//        NativeHashMap<Entity,TargetInfo> targetInfoHashMap = new NativeHashMap<Entity,TargetInfo>(count, Allocator.TempJob);
//        
//        var saveTargetPosJob = new SaveTargetPosJob
//        {
//            targetInfoHashMap = targetInfoHashMap.AsParallelWriter(),
//            PositionTypeHandleAccessor = this.GetComponentTypeHandle<Translation>(true),
//            TargetSelfTypeHandleAccessor = this.GetComponentTypeHandle<TargetSelf>(true),
//        };
//        this.Dependency = saveTargetPosJob.ScheduleParallel(TargetQuery, 1, this.Dependency);
//        
//        float DeltaTime = Time.DeltaTime;
//        var MoveToTargetJob = new MoveToTargetJob
//        {
//            DeltaTime = DeltaTime,
//            targetInfoHashMap = targetInfoHashMap,
//            PositionTypeHandleAccessor = this.GetComponentTypeHandle<Translation>(),
//            UnitSelfTypeHandleAccessor = this.GetComponentTypeHandle<UnitSelf>(true),
//            HasTargetTypeHandleAccessor = this.GetComponentTypeHandle<HasTarget>(true),
//            ecb = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
//            EntityPositions = this.GetComponentDataFromEntity<LocalToWorld>(true)  //所有的Entity的LocalToWorld数据
//        };
//        this.Dependency = MoveToTargetJob.ScheduleParallel(unitHaveTargetQuery, 1, this.Dependency);
//        _entityCommandBufferSystem.AddJobHandleForProducer(this.Dependency);
//        this.Dependency.Complete();
//        targetInfoHashMap.Dispose(); // NativeHashMap 不好用。。。。放在job里不能自己
//        
//
//    }
//
//}
