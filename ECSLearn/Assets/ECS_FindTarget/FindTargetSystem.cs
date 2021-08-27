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
using UnityEngine;
using Unity.Collections;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;

//原始code
//public class FindTargetSystem : ComponentSystem {
//
//    protected override void OnUpdate() {
//        Entities.WithNone<HasTarget>().WithAll<Unit>().ForEach((Entity entity, ref Translation unitTranslation) => {
//            // Code running on all entities with "Unit" Tag
//
//            float3 unitPosition = unitTranslation.Value;
//            Entity closestTargetEntity = Entity.Null;
//            float3 closestTargetPosition = float3.zero;
//
//            Entities.WithAll<Target>().ForEach((Entity targetEntity, ref Translation targetTranslation) => { 
//                // Cycling through all entities with "Target" Tag
//
//                if (closestTargetEntity == Entity.Null) {
//                    // No target
//                    closestTargetEntity = targetEntity;
//                    closestTargetPosition = targetTranslation.Value;
//                } else {
//                    if (math.distance(unitPosition, targetTranslation.Value) < math.distance(unitPosition, closestTargetPosition)) {
//                        // This target is closer
//                        closestTargetEntity = targetEntity;
//                        closestTargetPosition = targetTranslation.Value;
//                    }
//                }
//            });
//
//            // Closest Target
//            if (closestTargetEntity != Entity.Null) {
//                PostUpdateCommands.AddComponent(entity, new HasTarget { targetEntity = closestTargetEntity });
//            }
//        });
//    }
//
//}


// 原始code
//public class FindTargetJobSystem : JobComponentSystem {
//
//    private struct EntityWithPosition {
//        public Entity entity;
//        public float3 position;
//    }
//
//    [RequireComponentTag(typeof(Unit))]
//    [ExcludeComponent(typeof(HasTarget))]
//    [BurstCompile]
//    private struct FindTargetJob : IJobForEachWithEntity<Translation> {
//
//        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<EntityWithPosition> targetArray;
//        public EntityCommandBuffer.ParallelWriter entityCommandBuffer;
//
//        public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation) {
//            float3 unitPosition = translation.Value;
//            Entity closestTargetEntity = Entity.Null;
//            float3 closestTargetPosition = float3.zero;
//
//            for (int i=0; i<targetArray.Length; i++) {
//                // Cycling through all target entities
//                EntityWithPosition targetEntityWithPosition = targetArray[i];
//
//                if (closestTargetEntity == Entity.Null) {
//                    // No target
//                    closestTargetEntity = targetEntityWithPosition.entity;
//                    closestTargetPosition = targetEntityWithPosition.position;
//                } else {
//                    if (math.distance(unitPosition, targetEntityWithPosition.position) < math.distance(unitPosition, closestTargetPosition)) {
//                        // This target is closer
//                        closestTargetEntity = targetEntityWithPosition.entity;
//                        closestTargetPosition = targetEntityWithPosition.position;
//                    }
//                }
//            }
//
//            // Closest Target
//            if (closestTargetEntity != Entity.Null) {
//                entityCommandBuffer.AddComponent(index, entity, new HasTarget { targetEntity = closestTargetEntity });
//            }
//        }
//
//    }
//    
//    [RequireComponentTag(typeof(Unit))]
//    [ExcludeComponent(typeof(HasTarget))]
//    [BurstCompile]
//    private struct FindTargetBurstJob : IJobForEachWithEntity<Translation> {
//
//        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<EntityWithPosition> targetArray;
//        public NativeArray<Entity> closestTargetEntityArray;
//
//        public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation) {
//            float3 unitPosition = translation.Value;
//            Entity closestTargetEntity = Entity.Null;
//            float3 closestTargetPosition = float3.zero;
//
//            for (int i=0; i<targetArray.Length; i++) {
//                // Cycling through all target entities
//                EntityWithPosition targetEntityWithPosition = targetArray[i];
//
//                if (closestTargetEntity == Entity.Null) {
//                    // No target
//                    closestTargetEntity = targetEntityWithPosition.entity;
//                    closestTargetPosition = targetEntityWithPosition.position;
//                } else {
//                    if (math.distance(unitPosition, targetEntityWithPosition.position) < math.distance(unitPosition, closestTargetPosition)) {
//                        // This target is closer
//                        closestTargetEntity = targetEntityWithPosition.entity;
//                        closestTargetPosition = targetEntityWithPosition.position;
//                    }
//                }
//            }
//
//            closestTargetEntityArray[index] = closestTargetEntity;
//        }
//
//    }
//    
//    [RequireComponentTag(typeof(Unit))]
//    [ExcludeComponent(typeof(HasTarget))]
//    private struct AddComponentJob : IJobForEachWithEntity<Translation> {
//
//        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> closestTargetEntityArray;
//        public EntityCommandBuffer.ParallelWriter entityCommandBuffer;
//
//        public void Execute(Entity entity, int index, ref Translation translation) {
//            if (closestTargetEntityArray[index] != Entity.Null) {
//                entityCommandBuffer.AddComponent(index, entity, new HasTarget { targetEntity = closestTargetEntityArray[index] });
//            }
//        }
//
//    }
//
//
//    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;
//
//    protected override void OnCreate() {
//        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
//        base.OnCreate();
//    }
//    protected override JobHandle OnUpdate(JobHandle inputDeps) {
//        EntityQuery targetQuery = GetEntityQuery(typeof(Target), ComponentType.ReadOnly<Translation>());
//
//        NativeArray<Entity> targetEntityArray = targetQuery.ToEntityArray(Allocator.TempJob);
//        NativeArray<Translation> targetTranslationArray = targetQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
//
//        NativeArray<EntityWithPosition> targetArray = new NativeArray<EntityWithPosition>(targetEntityArray.Length, Allocator.TempJob);
//
//        for (int i = 0; i < targetEntityArray.Length; i++) {
//            targetArray[i] = new EntityWithPosition {
//                entity = targetEntityArray[i],
//                position = targetTranslationArray[i].Value,
//            };
//        }
//
//        targetEntityArray.Dispose();
//        targetTranslationArray.Dispose();
//        
//        EntityQuery unitQuery = GetEntityQuery(typeof(Unit), ComponentType.Exclude<HasTarget>());
//        NativeArray<Entity> closestTargetEntityArray = new NativeArray<Entity>(unitQuery.CalculateEntityCount(), Allocator.TempJob);
//       
//
//        FindTargetBurstJob findTargetBurstJob = new FindTargetBurstJob {
//            targetArray = targetArray,
//            closestTargetEntityArray = closestTargetEntityArray
//        };
//        JobHandle jobHandle = findTargetBurstJob.Schedule(this, inputDeps);
//
//        AddComponentJob addComponentJob = new AddComponentJob {
//            closestTargetEntityArray = closestTargetEntityArray,
//            entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
//        };
//        jobHandle = addComponentJob.Schedule(this, jobHandle);
//        
//        endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);
//
//        return jobHandle;
//    }
//
//}


//进一步优化
//public class FindTargetJobSystem_Ex : SystemBase
//{
//    private EntityQuery targetQuery;
//    private EntityQuery unitQuery;
//    private EntityCommandBufferSystem ecbs;
//
//    
//    private struct TargetInfo
//    {
//        public Entity entity;
//        public float3 position;
//        public int entityInQueryIndex;
//    }
//    protected override void OnCreate()
//    {
//        base.OnCreate();
//        targetQuery = GetEntityQuery(
//            typeof(Target), 
//            ComponentType.ReadOnly<Translation>(),
//            ComponentType.ReadOnly<TargetSelf>(),
//            ComponentType.Exclude<TargetOrigin>() 
//            );
//        
//        ecbs = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
//        
//        
//        unitQuery = GetEntityQuery(
//            typeof(Unit), 
//            ComponentType.ReadOnly<Translation>(),
//            ComponentType.ReadOnly<UnitSelf>(),
//            ComponentType.Exclude<HasTarget>(),  //排除HasTarget 组件
//            ComponentType.Exclude<UnitOrigin>() 
//            );
//
//    }
//    
//    [BurstCompile]
//    private struct SaveTargetInfoJob: IJobEntityBatchWithIndex
//    {
//        public NativeArray<TargetInfo> TargetInfoArray;
//        
//        [ReadOnly]
//        public ComponentTypeHandle<Translation> PositionTypeHandleAccessor;
//        
//        [ReadOnly]
//        public ComponentTypeHandle<TargetSelf> TargetSelfTypeHandleAccessor;
//        public void Execute(ArchetypeChunk batchInChunk, int batchIndex, int indexOfFirstEntityInQuery)
//        {
//            NativeArray<Translation> positions = batchInChunk.GetNativeArray<Translation>(PositionTypeHandleAccessor);
//            NativeArray<TargetSelf> targetSelfs = batchInChunk.GetNativeArray<TargetSelf>(TargetSelfTypeHandleAccessor);
//            for (int i = 0; i < positions.Length; i++)
//            {
//                int index = indexOfFirstEntityInQuery + i;
//                var targetSelf = targetSelfs[i];
//                var position = positions[i];
//                TargetInfoArray[index] = new TargetInfo
//                {
//                    entity = targetSelf.self,
//                    position = position.Value,
//                    entityInQueryIndex = index
//                };
//            }
//        }
//    }
//    
//    [BurstCompile]
//    private struct FindTargetJob : IJobEntityBatchWithIndex
//    {
//        [DeallocateOnJobCompletion]
//        [ReadOnly]
//        public NativeArray<TargetInfo> TargetInfoArray;
//        
//        [ReadOnly]
//        public ComponentTypeHandle<Translation> PositionTypeHandleAccessor;
//        
//        [ReadOnly]
//        public ComponentTypeHandle<UnitSelf> UnitSelfTypeHandleAccessor;
//
//        public EntityCommandBuffer.ParallelWriter ecb;
//        
//        public void Execute(ArchetypeChunk batchInChunk, int batchIndex, int indexOfFirstEntityInQuery)
//        {
//            NativeArray<Translation> positions = batchInChunk.GetNativeArray<Translation>(PositionTypeHandleAccessor);
//            NativeArray<UnitSelf> unitSelfs = batchInChunk.GetNativeArray<UnitSelf>(UnitSelfTypeHandleAccessor);
//            int targetCount = TargetInfoArray.Length;
//            int length = positions.Length;
//            for (int i = 0; i < length; i++)
//            {
//                var position = positions[i].Value; //Unit的位置
//                Entity closestTargetEntity = Entity.Null;
//                float3 closestTargetPosition = float3.zero;
//                for (int j = 0; j < targetCount; j++)
//                {
//                    var targetInfo = TargetInfoArray[j];
//                    if (closestTargetEntity == Entity.Null)
//                    {
//                        closestTargetEntity = targetInfo.entity;
//                        closestTargetPosition = targetInfo.position;
//                    }
//                    else
//                    {
//                        if (math.distancesq(position, targetInfo.position) < math.distancesq(position, closestTargetPosition))
//                        {
//                            closestTargetEntity = targetInfo.entity;
//                            closestTargetPosition = targetInfo.position;
//                        }
//                    }
//                }
//                
//                // Closest Target
//                if (closestTargetEntity != Entity.Null)
//                {
//                    int index = indexOfFirstEntityInQuery + i;
//                    var unitEntity = unitSelfs[i].self;
//                    ecb.AddComponent(index, unitEntity, new HasTarget { targetEntity = closestTargetEntity });
//  
//                }
//            }
//            
//           
//        }
//    }
//    protected override void OnUpdate()
//    {
//        //信息存储
//        int count = targetQuery.CalculateEntityCount();
//        NativeArray<TargetInfo> TargetInfoArray = new NativeArray<TargetInfo>(count, Allocator.TempJob);
//        var saveTargetInfoJob = new SaveTargetInfoJob
//        {
//            TargetInfoArray = TargetInfoArray,
//            PositionTypeHandleAccessor = this.GetComponentTypeHandle<Translation>(true),
//            TargetSelfTypeHandleAccessor = this.GetComponentTypeHandle<TargetSelf>(true),
//        };
//        
//        
//        this.Dependency = saveTargetInfoJob.ScheduleParallel(targetQuery, 1, this.Dependency);
//
//        var FindTargetJob = new FindTargetJob
//        {
//            TargetInfoArray = TargetInfoArray,
//            PositionTypeHandleAccessor = this.GetComponentTypeHandle<Translation>(true),
//            UnitSelfTypeHandleAccessor = this.GetComponentTypeHandle<UnitSelf>(true),
//            ecb = ecbs.CreateCommandBuffer().AsParallelWriter()
//        };
//        this.Dependency = FindTargetJob.ScheduleParallel(unitQuery, 1, this.Dependency);
////        this.Dependency = JobHandle.CombineDependencies(saveTargetInfoJobHandle, FindTargetJobHandle);
//        ecbs.AddJobHandleForProducer(this.Dependency);
//    }
//}


//进一步优化 使用象限系统
[UpdateAfter(typeof(QuadrantSystem))]
public class FindTargetJobSystem_Ex2 : SystemBase
{
    private EntityQuery targetQuery;
    private EntityQuery unitQuery;
    private EntityCommandBufferSystem ecbs;

    
    private struct TargetInfo
    {
        public Entity entity;
        public float3 position;
        public int entityInQueryIndex;
    }
    protected override void OnCreate()
    {
        base.OnCreate();
        targetQuery = GetEntityQuery(
            typeof(Target), 
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<TargetSelf>(),
            ComponentType.Exclude<TargetOrigin>() 
            );
        
        ecbs = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        
        
        unitQuery = GetEntityQuery(
            typeof(Unit), 
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<UnitSelf>(),
            ComponentType.Exclude<HasTarget>(),  //排除HasTarget 组件
            ComponentType.Exclude<UnitOrigin>() 
            );

    }
    
    [BurstCompile]
    private struct SaveTargetInfoJob: IJobEntityBatchWithIndex
    {
        public NativeArray<TargetInfo> TargetInfoArray;
        
        [ReadOnly]
        public ComponentTypeHandle<Translation> PositionTypeHandleAccessor;
        
        [ReadOnly]
        public ComponentTypeHandle<TargetSelf> TargetSelfTypeHandleAccessor;
        public void Execute(ArchetypeChunk batchInChunk, int batchIndex, int indexOfFirstEntityInQuery)
        {
            NativeArray<Translation> positions = batchInChunk.GetNativeArray<Translation>(PositionTypeHandleAccessor);
            NativeArray<TargetSelf> targetSelfs = batchInChunk.GetNativeArray<TargetSelf>(TargetSelfTypeHandleAccessor);
            for (int i = 0; i < positions.Length; i++)
            {
                int index = indexOfFirstEntityInQuery + i;
                var targetSelf = targetSelfs[i];
                var position = positions[i];
                TargetInfoArray[index] = new TargetInfo
                {
                    entity = targetSelf.self,
                    position = position.Value,
                    entityInQueryIndex = index
                };
            }
        }
    }
    
    [BurstCompile]
    private struct FindTargetJob : IJobEntityBatchWithIndex
    {
        [DeallocateOnJobCompletion]
        [ReadOnly]
        public NativeArray<TargetInfo> TargetInfoArray;
        
        [ReadOnly]
        public ComponentTypeHandle<Translation> PositionTypeHandleAccessor;
        
        [ReadOnly]
        public ComponentTypeHandle<UnitSelf> UnitSelfTypeHandleAccessor;

        public EntityCommandBuffer.ParallelWriter ecb;
        
        public void Execute(ArchetypeChunk batchInChunk, int batchIndex, int indexOfFirstEntityInQuery)
        {
            NativeArray<Translation> positions = batchInChunk.GetNativeArray<Translation>(PositionTypeHandleAccessor);
            NativeArray<UnitSelf> unitSelfs = batchInChunk.GetNativeArray<UnitSelf>(UnitSelfTypeHandleAccessor);
            int targetCount = TargetInfoArray.Length;
            int length = positions.Length;
            for (int i = 0; i < length; i++)
            {
                var position = positions[i].Value; //Unit的位置
                Entity closestTargetEntity = Entity.Null;
                float3 closestTargetPosition = float3.zero;
                for (int j = 0; j < targetCount; j++)
                {
                    var targetInfo = TargetInfoArray[j];
                    if (closestTargetEntity == Entity.Null)
                    {
                        closestTargetEntity = targetInfo.entity;
                        closestTargetPosition = targetInfo.position;
                    }
                    else
                    {
                        if (math.distancesq(position, targetInfo.position) < math.distancesq(position, closestTargetPosition))
                        {
                            closestTargetEntity = targetInfo.entity;
                            closestTargetPosition = targetInfo.position;
                        }
                    }
                }
                
                // Closest Target
                if (closestTargetEntity != Entity.Null)
                {
                    int index = indexOfFirstEntityInQuery + i;
                    var unitEntity = unitSelfs[i].self;
                    ecb.AddComponent(index, unitEntity, new HasTarget { targetEntity = closestTargetEntity });
  
                }
            }
            
           
        }
    }
    
    //使用象限寻找目标
    [BurstCompile]
    private struct FindTargetWithQuadrantJob : IJobEntityBatchWithIndex
    {
        
        [ReadOnly]
        public ComponentTypeHandle<Translation> PositionTypeHandleAccessor;
        
        [ReadOnly]
        public ComponentTypeHandle<UnitSelf> UnitSelfTypeHandleAccessor;
        
        [ReadOnly]
        public ComponentTypeHandle<QuadrantEntity> QuadrantEntityTypeHandleAccessor;

        public EntityCommandBuffer.ParallelWriter ecb;
        
        [ReadOnly] public NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap;
        
        public void Execute(ArchetypeChunk batchInChunk, int batchIndex, int indexOfFirstEntityInQuery)
        {
            NativeArray<Translation> positions = batchInChunk.GetNativeArray<Translation>(PositionTypeHandleAccessor);
            NativeArray<UnitSelf> unitSelfs = batchInChunk.GetNativeArray<UnitSelf>(UnitSelfTypeHandleAccessor);
            NativeArray<QuadrantEntity> quadrantEntitys = batchInChunk.GetNativeArray<QuadrantEntity>(QuadrantEntityTypeHandleAccessor);
            int length = positions.Length;
            for (int i = 0; i < length; i++)
            {
                var position = positions[i].Value; //Unit的位置
                Entity closestTargetEntity = Entity.Null;

                //得到每个Unit所处的象限
                float closestTargetDistance = float.MaxValue;
                int hashMapKey = QuadrantSystem.GetPositionHashMapKey(position);
                
                //只遍历相邻象限的目标
                var unitPosition = position;
                var quadrantEntity = quadrantEntitys[i];
                FindTarget(hashMapKey, unitPosition, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance);
                FindTarget(hashMapKey + 1, unitPosition, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance);
                FindTarget(hashMapKey - 1, unitPosition, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance);
                FindTarget(hashMapKey + QuadrantSystem.quadrantYMultiplier, unitPosition, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance);
                FindTarget(hashMapKey - QuadrantSystem.quadrantYMultiplier, unitPosition, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance);
                FindTarget(hashMapKey + 1 + QuadrantSystem.quadrantYMultiplier, unitPosition, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance);
                FindTarget(hashMapKey - 1 + QuadrantSystem.quadrantYMultiplier, unitPosition, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance);
                FindTarget(hashMapKey + 1 - QuadrantSystem.quadrantYMultiplier, unitPosition, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance);
                FindTarget(hashMapKey - 1 - QuadrantSystem.quadrantYMultiplier, unitPosition, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance);
                
                // Closest Target
                if (closestTargetEntity != Entity.Null)
                {
                    int index = indexOfFirstEntityInQuery + i;
                    var unitEntity = unitSelfs[i].self;
                    ecb.AddComponent(index, unitEntity, new HasTarget { targetEntity = closestTargetEntity });
  
                }
            }
            
           
        }
        
        private void FindTarget(int hashMapKey, float3 unitPosition, QuadrantEntity quadrantEntity, ref Entity closestTargetEntity, ref float closestTargetDistance) {
            QuadrantData quadrantData;
            NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
            if (quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadrantData, out nativeMultiHashMapIterator)) {
                do {
                    if (quadrantEntity.typeEnum != quadrantData.quadrantEntity.typeEnum) {
                        if (closestTargetEntity == Entity.Null) {
                            // No target
                            closestTargetEntity = quadrantData.entity;
                            closestTargetDistance = math.distancesq(unitPosition, quadrantData.position);
                        } else {
                            if (math.distancesq(unitPosition, quadrantData.position) < closestTargetDistance) {
                                // This target is closer
                                closestTargetEntity = quadrantData.entity;
                                closestTargetDistance = math.distancesq(unitPosition, quadrantData.position);
                            }
                        }
                    }
                } while (quadrantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
            }
        }
    }
    
    
    protected override void OnUpdate()
    {

        if (GameHandleSpawnAuthoring.Instance.useQuadrantSystem)
        {
            var FindTargetWithQuadrantJob = new FindTargetWithQuadrantJob
            {
                quadrantMultiHashMap = QuadrantSystem.quadrantMultiHashMap,
                PositionTypeHandleAccessor = this.GetComponentTypeHandle<Translation>(true),
                UnitSelfTypeHandleAccessor = this.GetComponentTypeHandle<UnitSelf>(true),
                QuadrantEntityTypeHandleAccessor = this.GetComponentTypeHandle<QuadrantEntity>(true),
                ecb = ecbs.CreateCommandBuffer().AsParallelWriter()
            };
            this.Dependency = FindTargetWithQuadrantJob.ScheduleParallel(unitQuery, 1, this.Dependency);
        }
        else
        {
            //信息存储
            int count = targetQuery.CalculateEntityCount();
            NativeArray<TargetInfo> TargetInfoArray = new NativeArray<TargetInfo>(count, Allocator.TempJob);
            var saveTargetInfoJob = new SaveTargetInfoJob
            {
                TargetInfoArray = TargetInfoArray,
                PositionTypeHandleAccessor = this.GetComponentTypeHandle<Translation>(true),
                TargetSelfTypeHandleAccessor = this.GetComponentTypeHandle<TargetSelf>(true),
            };
        
        
            this.Dependency = saveTargetInfoJob.ScheduleParallel(targetQuery, 1, this.Dependency);
            var FindTargetJob = new FindTargetJob
            {
                TargetInfoArray = TargetInfoArray,
                PositionTypeHandleAccessor = this.GetComponentTypeHandle<Translation>(true),
                UnitSelfTypeHandleAccessor = this.GetComponentTypeHandle<UnitSelf>(true),
                ecb = ecbs.CreateCommandBuffer().AsParallelWriter()
            };
            this.Dependency = FindTargetJob.ScheduleParallel(unitQuery, 1, this.Dependency);
        }
        
        ecbs.AddJobHandleForProducer(this.Dependency);
    }
}
