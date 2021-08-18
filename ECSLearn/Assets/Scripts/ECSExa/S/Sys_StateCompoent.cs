using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Profiling;

namespace EntitysExample
{
    
     public struct GeneralPurposeComponentA : IComponentData
    {
        public int Lifetime;
    }

    public struct StateComponentB : ISystemStateComponentData
    {
        public int State;
    }

    public partial class StatefulSystem : SystemBase
    {
        private EntityCommandBufferSystem ecbSource;

        protected override void OnCreate()
        {
            ecbSource = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();

            // Create some test entities
            // This runs on the main thread, but it is still faster to use a command buffer
            //ecb创建Entity会快？
            
            EntityArchetype archetype = EntityManager.CreateArchetype(typeof(GeneralPurposeComponentA));
            int enityCount = 1;

            //entityManager 直接创建Entity
//
//            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
//            stopwatch.Start();
//            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
//           
//            for (int i = 0; i < enityCount; i++)
//            {
//                //根据Archetype创建Entity
//                Entity newEntity = entityManager.CreateEntity(archetype);
//                entityManager.SetComponentData(newEntity, new GeneralPurposeComponentA() { Lifetime = i });
//            }
//            
//            stopwatch.Stop();
//            var totalT1 = stopwatch.Elapsed.Milliseconds;
//            Debug.Log($"EntityManager create Entity===={totalT1} 毫秒");
            
            //EntityCommandBuffer 直接创建Entity
            System.Diagnostics.Stopwatch stopwatch1 = new System.Diagnostics.Stopwatch();
            stopwatch1.Start();
            EntityCommandBuffer creationBuffer = new EntityCommandBuffer(Allocator.Temp);
            for (int i = 0; i < enityCount; i++)
            {
                //根据Archetype创建Entity
                Entity newEntity = creationBuffer.CreateEntity(archetype);
                creationBuffer.SetComponent<GeneralPurposeComponentA>
                (
                    newEntity,
                    new GeneralPurposeComponentA() { Lifetime = i }
                );
            }
            //Execute the command buffer
            creationBuffer.Playback(EntityManager);
            
//            stopwatch1.Stop();
//            var totalT2 = stopwatch1.Elapsed.Milliseconds;
//            Debug.Log($"EntityCommandBuffer create Entity===={totalT2} 毫秒");
//            Debug.Log($"create {enityCount} Entity Time (EntityManager - ECB) ===={totalT1 - totalT2} 毫秒");
            
            /*
             *10000 个Entity  EntityManager创建比 EntityCommandBuffer 多1.3毫秒左右
             * 1000 个Entity  EntityManager创建比 EntityCommandBuffer 少4毫秒左右
             * 100 个Entity  EntityManager创建比 EntityCommandBuffer 少5毫秒左右
             *
             * 总结： entity数量越少，还是使用entityManager创建较好
             */
        }

        protected override void OnUpdate()
        {
            EntityCommandBuffer.ParallelWriter parallelWriterECB = ecbSource.CreateCommandBuffer().AsParallelWriter();

            // Entities with GeneralPurposeComponentA but not StateComponentB
            Entities
                .WithNone<StateComponentB>()
                .ForEach(
                    (Entity entity, int entityInQueryIndex, in GeneralPurposeComponentA gpA) =>
                    {
                    // Add an ISystemStateComponentData instance
                    parallelWriterECB.AddComponent<StateComponentB>
                        (
                            entityInQueryIndex,
                            entity,
                            new StateComponentB() { State = 1 }
                        );
                    })
                .ScheduleParallel();
            ecbSource.AddJobHandleForProducer(this.Dependency);

            // Create new command buffer
            parallelWriterECB = ecbSource.CreateCommandBuffer().AsParallelWriter();

            // Entities with both GeneralPurposeComponentA and StateComponentB
            Entities
                .WithAll<StateComponentB>()
                .ForEach(
                    (Entity entity,
                     int entityInQueryIndex,
                     ref GeneralPurposeComponentA gpA) =>
                    {
                    // Process entity, in this case by decrementing the Lifetime count
                    gpA.Lifetime--;

                    // If out of time, destroy the entity
                    if (gpA.Lifetime <= 0)
                        {
                            parallelWriterECB.DestroyEntity(entityInQueryIndex, entity);
                        }
                    })
                .ScheduleParallel();
            ecbSource.AddJobHandleForProducer(this.Dependency);

            // Create new command buffer
            parallelWriterECB = ecbSource.CreateCommandBuffer().AsParallelWriter();

            // Entities with StateComponentB but not GeneralPurposeComponentA
            Entities
                .WithAll<StateComponentB>()
                .WithNone<GeneralPurposeComponentA>()
                .ForEach(
                    (Entity entity, int entityInQueryIndex) =>
                    {
                    // This system is responsible for removing any ISystemStateComponentData instances it adds
                    // Otherwise, the entity is never truly destroyed.
                    parallelWriterECB.RemoveComponent<StateComponentB>(entityInQueryIndex, entity);
                    })
                .ScheduleParallel();
            ecbSource.AddJobHandleForProducer(this.Dependency);

        }

        protected override void OnDestroy()
        {
            // Implement OnDestroy to cleanup any resources allocated by this system.
            // (This simplified example does not allocate any resources, so there is nothing to clean up.)
      
        }
    }

}

