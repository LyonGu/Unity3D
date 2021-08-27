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

[DisableAutoCreation]
[UpdateAfter(typeof(QuadrantSystem))]
public class FindTargetJobSystemQuadrant : JobComponentSystem {

    private struct EntityWithPosition {
        public Entity entity;
        public float3 position;
    }
    
    [RequireComponentTag(typeof(Target))]
    [BurstCompile]
    // Fill single array with Target Entity and Position
    private struct FillArrayEntityWithPositionJob : IJobForEachWithEntity<Translation> {

        public NativeArray<EntityWithPosition> targetArray;

        public void Execute(Entity entity, int index, ref Translation translation) {
            targetArray[index] = new EntityWithPosition {
                entity = entity,
                position = translation.Value
            };
        }
    }

    [RequireComponentTag(typeof(Unit))]
    [ExcludeComponent(typeof(HasTarget))]
    [BurstCompile]
    // Find Closest Target
    private struct FindTargetBurstJob : IJobForEachWithEntity<Translation> {

        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<EntityWithPosition> targetArray;
        public NativeArray<Entity> closestTargetEntityArray;

        public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation) {
            float3 unitPosition = translation.Value;
            Entity closestTargetEntity = Entity.Null;
            float closestTargetDistance = float.MaxValue;

            for (int i=0; i<targetArray.Length; i++) {
                // Cycling through all target entities
                EntityWithPosition targetEntityWithPosition = targetArray[i];

                if (closestTargetEntity == Entity.Null) {
                    // No target
                    closestTargetEntity = targetEntityWithPosition.entity;
                    closestTargetDistance = math.distancesq(unitPosition, targetEntityWithPosition.position);
                } else {
                    if (math.distancesq(unitPosition, targetEntityWithPosition.position) < closestTargetDistance) {
                        // This target is closer
                        closestTargetEntity = targetEntityWithPosition.entity;
                        closestTargetDistance = math.distancesq(unitPosition, targetEntityWithPosition.position);
                    }
                }
            }

            closestTargetEntityArray[index] = closestTargetEntity;
        }

    }
    
    [RequireComponentTag(typeof(Unit))]
    [ExcludeComponent(typeof(HasTarget))]
    // Add HasTarget Component to Entities that have a Closest Target
    private struct AddComponentJob : IJobForEachWithEntity<Translation> {

        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> closestTargetEntityArray;
        public EntityCommandBuffer.ParallelWriter entityCommandBuffer;

        public void Execute(Entity entity, int index, ref Translation translation) {
            if (closestTargetEntityArray[index] != Entity.Null) {
                entityCommandBuffer.AddComponent(index, entity, new HasTarget { targetEntity = closestTargetEntityArray[index] });
            }
        }

    }

    
    [RequireComponentTag(typeof(Unit))]
    [ExcludeComponent(typeof(HasTarget))]
    [BurstCompile]
    private struct FindTargetQuadrantSystemJob : IJobForEachWithEntity<Translation, QuadrantEntity> {

        [ReadOnly] public NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap;
        public NativeArray<Entity> closestTargetEntityArray;

        public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation, [ReadOnly] ref QuadrantEntity quadrantEntity) {
            float3 unitPosition = translation.Value;
            Entity closestTargetEntity = Entity.Null;
            float closestTargetDistance = float.MaxValue;
            int hashMapKey = QuadrantSystem.GetPositionHashMapKey(translation.Value);

            FindTarget(hashMapKey, unitPosition, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance);
            FindTarget(hashMapKey + 1, unitPosition, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance);
            FindTarget(hashMapKey - 1, unitPosition, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance);
            FindTarget(hashMapKey + QuadrantSystem.quadrantYMultiplier, unitPosition, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance);
            FindTarget(hashMapKey - QuadrantSystem.quadrantYMultiplier, unitPosition, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance);
            FindTarget(hashMapKey + 1 + QuadrantSystem.quadrantYMultiplier, unitPosition, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance);
            FindTarget(hashMapKey - 1 + QuadrantSystem.quadrantYMultiplier, unitPosition, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance);
            FindTarget(hashMapKey + 1 - QuadrantSystem.quadrantYMultiplier, unitPosition, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance);
            FindTarget(hashMapKey - 1 - QuadrantSystem.quadrantYMultiplier, unitPosition, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance);

            closestTargetEntityArray[index] = closestTargetEntity;
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

    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

    protected override void OnCreate() {
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        EntityQuery targetQuery = GetEntityQuery(typeof(Target), ComponentType.ReadOnly<Translation>());

        bool useQuadrantSystem = GameHandlerQuadrant.instance.useQuadrantSystem;
        if (useQuadrantSystem) {
            EntityQuery unitQuery = GetEntityQuery(typeof(Unit), ComponentType.Exclude<HasTarget>());
            NativeArray<Entity> closestTargetEntityArray = new NativeArray<Entity>(unitQuery.CalculateEntityCount(), Allocator.TempJob);

            FindTargetQuadrantSystemJob findTargetQuadrantSystemJob = new FindTargetQuadrantSystemJob {
                quadrantMultiHashMap = QuadrantSystem.quadrantMultiHashMap,
                closestTargetEntityArray = closestTargetEntityArray,
            };
            JobHandle jobHandle = findTargetQuadrantSystemJob.Schedule(this, inputDeps);

            // Add HasTarget Component to Entities that have a Closest Target
            AddComponentJob addComponentJob = new AddComponentJob {
                closestTargetEntityArray = closestTargetEntityArray,
                entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            };
            jobHandle = addComponentJob.Schedule(this, jobHandle);

            endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

            return jobHandle;
        } else {

            NativeArray<EntityWithPosition> targetArray = new NativeArray<EntityWithPosition>(targetQuery.CalculateEntityCount(), Allocator.TempJob);

            // Fill single array with Entity and Position
            FillArrayEntityWithPositionJob fillArrayEntityWithPositionJob = new FillArrayEntityWithPositionJob {
                targetArray = targetArray
            };
            JobHandle jobHandle = fillArrayEntityWithPositionJob.Schedule(this, inputDeps);
            
            EntityQuery unitQuery = GetEntityQuery(typeof(Unit), ComponentType.Exclude<HasTarget>());
            NativeArray<Entity> closestTargetEntityArray = new NativeArray<Entity>(unitQuery.CalculateEntityCount(), Allocator.TempJob);

            // Find Closest Target
            FindTargetBurstJob findTargetBurstJob = new FindTargetBurstJob {
                targetArray = targetArray,
                closestTargetEntityArray = closestTargetEntityArray
            };
            jobHandle = findTargetBurstJob.Schedule(this, jobHandle);

            // Add HasTarget Component to Entities that have a Closest Target
            AddComponentJob addComponentJob = new AddComponentJob {
                closestTargetEntityArray = closestTargetEntityArray,
                entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            };
            jobHandle = addComponentJob.Schedule(this, jobHandle);

            endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

            return jobHandle;
        }
    }

}

