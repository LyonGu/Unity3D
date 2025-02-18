using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

// This system updates all entities in the scene with both a RotationSpeed_IJobChunk and Rotation component.

// ReSharper disable once InconsistentNaming
//[DisableAutoCreation] 禁止自动创建
public partial class RotationSpeedSystem_IJobChunk : SystemBase
{
    EntityQuery m_Query;

    protected override void OnCreate()
    {
        // Cached access to a set of ComponentData based on a specific query
        m_Query = GetEntityQuery(typeof(Rotation), ComponentType.ReadOnly<RotationSpeed_IJobEntityBatch>());
    }

    //IJobEntityBatch 效率特别高效  Job最好多加上BurstCompile
    // Use the [BurstCompile] attribute to compile a job with Burst. You may see significant speed ups, so try it!
    [BurstCompile]
    struct RotationSpeedJob : IJobEntityBatch
    {
        public float DeltaTime;
        public ComponentTypeHandle<Rotation> RotationTypeHandle;
        [ReadOnly] public ComponentTypeHandle<RotationSpeed_IJobEntityBatch> RotationSpeedTypeHandle;

        public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
        {
            var chunkRotations = batchInChunk.GetNativeArray(RotationTypeHandle);
            var chunkRotationSpeeds = batchInChunk.GetNativeArray(RotationSpeedTypeHandle);
            for (var i = 0; i < batchInChunk.Count; i++)
            {
                var rotation = chunkRotations[i];
                var rotationSpeed = chunkRotationSpeeds[i];

                // Rotate something about its up vector at the speed given by RotationSpeed_IJobChunk.
                chunkRotations[i] = new Rotation
                {
                    Value = math.mul(math.normalize(rotation.Value),
                        quaternion.AxisAngle(math.up(), rotationSpeed.RadiansPerSecond * DeltaTime))
                };

            }
            
            //其他写法
//            var chunkRotations1
//                = batchInChunk.GetChunkComponentData(RotationTypeHandle);
//            batchInChunk.SetChunkComponentData(RotationTypeHandle,
//                new Rotation() { Value = new quaternion(0,0,0,0) });
        }
    }

    // OnUpdate runs on the main thread.
    protected override void OnUpdate()
    {
        // Explicitly declare:
        // - Read-Write access to Rotation
        // - Read-Only access to RotationSpeed_IJobChunk
        var rotationType = GetComponentTypeHandle<Rotation>();
        var rotationSpeedType = GetComponentTypeHandle<RotationSpeed_IJobEntityBatch>(true);

        var job = new RotationSpeedJob()
        {
            RotationTypeHandle = rotationType,
            RotationSpeedTypeHandle = rotationSpeedType,
            DeltaTime = Time.DeltaTime
        };
        
        //这种调度变体将每个匹配块作为单个批处理。每个块都可以并行执行, 效率很高
        Dependency = job.ScheduleParallel(m_Query, 1, Dependency);
    }
}
