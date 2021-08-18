using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using NUnit.Framework;

// The files in this namespace are used to compile/test the code samples in the documentation.
namespace Doc.CodeSamples.Tests
{
    #region lookup-foreach
    public partial class TrackingSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = this.Time.DeltaTime;

            Entities
                .ForEach((ref Rotation orientation,
                in LocalToWorld transform,
                in Target target) =>
                {
                    // Check to make sure the target Entity still exists and has
                    // the needed component
                    if (!HasComponent<LocalToWorld>(target.entity))
                        return;

                    // Look up the entity data
                    LocalToWorld targetTransform
                        = GetComponent<LocalToWorld>(target.entity);
                    float3 targetPosition = targetTransform.Position;

                    // Calculate the rotation 另一种旋转方式，超向目标
                    float3 displacement = targetPosition - transform.Position;
                    float3 upReference = new float3(0, 1, 0);
                    quaternion lookRotation =
                        quaternion.LookRotationSafe(displacement, upReference);

                    orientation.Value =
                        math.slerp(orientation.Value, lookRotation, deltaTime);
                })
                .ScheduleParallel();
        }
    }
    #endregion
    #region lookup-foreach-buffer

    public struct BufferData : IBufferElementData
    {
        public float Value;
    }
    public partial class BufferLookupSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            BufferFromEntity<BufferData> buffersOfAllEntities
                = this.GetBufferFromEntity<BufferData>(true);

            Entities
                .ForEach((ref Rotation orientation,
                in LocalToWorld transform,
                in Target target) =>
                {
                    // Check to make sure the target Entity with this buffer type still exists
                    if (!buffersOfAllEntities.HasComponent(target.entity))
                        return;

                    // Get a reference to the buffer
                    DynamicBuffer<BufferData> bufferOfOneEntity =
                        buffersOfAllEntities[target.entity];

                    // Use the data in the buffer
                    float avg = 0;
                    for (var i = 0; i < bufferOfOneEntity.Length; i++)
                    {
                        avg += bufferOfOneEntity[i].Value;
                    }
                    if (bufferOfOneEntity.Length > 0)
                        avg /= bufferOfOneEntity.Length;
                })
                .ScheduleParallel();
        }
    }
    #endregion
    #region lookup-ijobchunk

    public class MoveTowardsEntitySystem : SystemBase
    {
        private EntityQuery query;

        [BurstCompile]
        private struct MoveTowardsJob : IJobEntityBatch
        {
            // Read-write data in the current chunk
            public ComponentTypeHandle<Translation> PositionTypeHandleAccessor;
            
            public ComponentTypeHandle<Unity.Transforms.Rotation> RotationTypeHandleAccessor;
            
            // Read-only data in the current chunk
            [ReadOnly]
            public ComponentTypeHandle<Target> TargetTypeHandleAccessor;

            // Read-only data stored (potentially) in other chunks
            [ReadOnly]
            public ComponentDataFromEntity<LocalToWorld> EntityPositions;
            

            // Non-entity data
            public float deltaTime;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                // Get arrays of the components in chunk
                NativeArray<Translation> positions
                    = batchInChunk.GetNativeArray<Translation>(PositionTypeHandleAccessor);
                NativeArray<Target> targets
                    = batchInChunk.GetNativeArray<Target>(TargetTypeHandleAccessor);
                
                NativeArray<Unity.Transforms.Rotation> rotations
                    = batchInChunk.GetNativeArray<Unity.Transforms.Rotation>(RotationTypeHandleAccessor);

                for (int i = 0; i < positions.Length; i++)
                {
                    // Get the target Entity object
                    Entity targetEntity = targets[i].entity;

                    // Check that the target still exists
                    if (!EntityPositions.HasComponent(targetEntity))
                        continue;

                    // Update translation to move the chasing enitity toward the target
                    float3 targetPosition = EntityPositions[targetEntity].Position; //获取目标target的位置
                    float3 chaserPosition = positions[i].Value;
                    quaternion curRotation = rotations[i].Value;

                    //朝目标移动
                    float3 displacement = targetPosition - chaserPosition;
                    positions[i] = new Translation
                    {
                        Value = chaserPosition + displacement * deltaTime
                    };
                    
                    //转向目标
                    float3 upReference = new float3(0, 1, 0);
                    quaternion lookRotation =
                        quaternion.LookRotationSafe(displacement, upReference);
                    rotations[i] = new Unity.Transforms.Rotation()
                    {
                        Value = math.slerp(curRotation, lookRotation, deltaTime)
                    };
                   
                }
            }
        }

        protected override void OnCreate()
        {
            // Select all entities that have Translation and Target Componentx
            query = this.GetEntityQuery
                (
                    typeof(Translation),
                    typeof(Unity.Transforms.Rotation),
                    ComponentType.ReadOnly<Target>()
                );
        }

        protected override void OnUpdate()
        {
            // Create the job
            var job = new MoveTowardsJob();

            // Set the chunk data accessors
            job.PositionTypeHandleAccessor =
                this.GetComponentTypeHandle<Translation>(false);
            job.TargetTypeHandleAccessor =
                this.GetComponentTypeHandle<Target>(true);
            
            job.RotationTypeHandleAccessor = this.GetComponentTypeHandle<Unity.Transforms.Rotation>(false);

            // Set the component data lookup field
            //这个是为了拿到所有localWorld的数据
            job.EntityPositions = this.GetComponentDataFromEntity<LocalToWorld>(true);

            // Set non-ECS data fields
            job.deltaTime = this.Time.DeltaTime;

            // Schedule the job using Dependency property
            this.Dependency = job.ScheduleParallel(query, 1, this.Dependency);
        }
    }
    #endregion

    public class Snippets : SystemBase
    {
        private EntityQuery query;
        protected override void OnCreate()
        {
            // Select all entities that have Translation and Target Componentx
            query = this.GetEntityQuery(typeof(Translation), ComponentType.ReadOnly<Target>());
        }

        [BurstCompile]
        private struct ChaserSystemJob : IJobEntityBatch
        {
            // Read-write data in the current chunk
            public ComponentTypeHandle<Translation> PositionTypeHandleAccessor;

            // Read-only data in the current chunk
            [ReadOnly]
            public ComponentTypeHandle<Target> TargetTypeHandleAccessor;

            // Read-only data stored (potentially) in other chunks
            #region lookup-ijobchunk-declare

            [ReadOnly]
            public ComponentDataFromEntity<LocalToWorld> EntityPositions;
            #endregion

            // Non-entity data
            public float deltaTime;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                // Get arrays of the components in chunk
                NativeArray<Translation> positions
                    = batchInChunk.GetNativeArray<Translation>(PositionTypeHandleAccessor);
                NativeArray<Target> targets
                    = batchInChunk.GetNativeArray<Target>(TargetTypeHandleAccessor);

                for (int i = 0; i < positions.Length; i++)
                {
                    // Get the target Entity object
                    Entity targetEntity = targets[i].entity;

                    // Check that the target still exists
                    if (!EntityPositions.HasComponent(targetEntity))
                        continue;

                    // Update translation to move the chasing enitity toward the target
                    #region lookup-ijobchunk-read

                    float3 targetPosition = EntityPositions[targetEntity].Position;
                    #endregion
                    float3 chaserPosition = positions[i].Value;

                    float3 displacement = targetPosition - chaserPosition;
                    positions[i] = new Translation { Value = chaserPosition + displacement * deltaTime };
                }
            }
        }

        protected override void OnUpdate()
        {
            // Create the job
            #region lookup-ijobchunk-set

            var job = new ChaserSystemJob();
            //拿到所有Entity的LocalToWorld组件数据，使用Entity作为下标获取
            job.EntityPositions = this.GetComponentDataFromEntity<LocalToWorld>(true);
            #endregion
            // Set the chunk data accessors
            job.PositionTypeHandleAccessor = this.GetComponentTypeHandle<Translation>(false);
            job.TargetTypeHandleAccessor = this.GetComponentTypeHandle<Target>(true);


            // Set non-ECS data fields
            job.deltaTime = this.Time.DeltaTime;

            // Schedule the job using Dependency property
            this.Dependency = job.ScheduleParallel(query, 1, this.Dependency);
        }
    }
}
