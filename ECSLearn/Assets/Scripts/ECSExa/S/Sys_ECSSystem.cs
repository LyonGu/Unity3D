using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace EntitysExample
{
    public struct Position : IComponentData
    {
        public float3 Value;
    }

    public struct Velocity : IComponentData
    {
        public float3 Value;
    }

    public partial class ECSSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();

    
            EntityArchetype entityArchetype = EntityManager.CreateArchetype(typeof(Velocity), typeof(Position));
//            EntityManager.CreateEntity(entityArchetype, 1000);
        }

        protected override void OnUpdate()
        {
            // Local variable captured in ForEach
            float dT = Time.DeltaTime;
            //想验证下是否开启了Burst==> 默认开启了Burst Entities.ForEach
            Entities
                .WithName("Update_Displacement")
                .ForEach(
                    (ref Position position, in Velocity velocity) =>
                    {
                        position = new Position()
                        {
                            Value = position.Value + velocity.Value * dT
                        };
                    }
                )
                .ScheduleParallel();
            
            /*
             *   job.Run() ==> 主线程执行
				 job.Schedule() ==> 使用一个子线程执行
				 job.ScheduleParallel() ==> 使用多个子线程并行执行

             */
        }
    }

}