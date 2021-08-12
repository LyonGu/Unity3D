using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

public class Sys_RotationSpeedSystem_IJobChunk : SystemBase
{
    private EntityQuery _query;

    protected override void OnCreate()
    {
        base.OnCreate();
        //构建一个筛选查询对象
        _query = GetEntityQuery(typeof(Rotation), ComponentType.ReadOnly<RotationSpeed_IJobChunk>());
    }
    [BurstCompile]
    struct RotationSpeedJob : IJobEntityBatch
    {
        public ComponentTypeHandle<Rotation> RotationTypeHandle;
        [ReadOnly] public ComponentTypeHandle<RotationSpeed_IJobChunk> RotationSpeedTypeHandle;
        public float DeltaTime;
//        public uint LastSystemVersion;
        public void Execute(ArchetypeChunk chunk, int batchIndex)
        {
//            var RotationTypeHandleChanged = chunk.DidChange(RotationTypeHandle, LastSystemVersion);
//            var RotationSpeedTypeChanged = chunk.DidChange(RotationSpeedTypeHandle, LastSystemVersion);
//
//            // 如果没有component变化，就跳过当前chunk
//            if (!(RotationTypeHandleChanged || RotationSpeedTypeChanged))
//                return;

            var chunkRotations = chunk.GetNativeArray(RotationTypeHandle); //获得这个块里的所有实体的Rotation组件
            var chunkRotationSpeeds = chunk.GetNativeArray(RotationSpeedTypeHandle);
            for (var i = 0; i < chunk.Count; i++)
            {
                var rotation = chunkRotations[i];
                var rotationSpeed = chunkRotationSpeeds[i];                // Rotate something about its up vector at the speed given by RotationSpeed_IJobChunk.
                chunkRotations[i] = new Rotation
                {
                    Value = math.mul(math.normalize(rotation.Value),
                        quaternion.AxisAngle(math.up(), rotationSpeed.RadiansPerSecond * DeltaTime))
                };
            }

           
        }
    }
    protected override void OnUpdate()
    {
        var job = new RotationSpeedJob();
        job.RotationSpeedTypeHandle = GetComponentTypeHandle<RotationSpeed_IJobChunk>(true);
        job.RotationTypeHandle= GetComponentTypeHandle<Rotation>();
        job.DeltaTime = Time.DeltaTime;
//        job.LastSystemVersion = this.LastSystemVersion;

        Dependency = job.ScheduleParallel(_query, 1, Dependency);
    }
}
