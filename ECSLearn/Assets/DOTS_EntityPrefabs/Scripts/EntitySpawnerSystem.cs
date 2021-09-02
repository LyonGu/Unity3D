using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace DOTS_EntityPrefabs.Scripts
{
    
    //继承ComponentSystem，Entities.ForEach直接在主线程执行，以后可能会被废弃，建议使用SystemBase
    [DisableAutoCreation]
    public class EntitySpawnerSystem : ComponentSystem {

        private float spawnTimer;
        private Random random;

        protected override void OnCreate() {
            random = new Random(56);
        }

        protected override void OnUpdate() {
            spawnTimer -= Time.DeltaTime;

            if (spawnTimer <= 0f) 
            {
                spawnTimer = .5f;
                // Spawn!
//                PrefabEntityComponent prefabEntityComponent = GetSingleton<PrefabEntityComponent>();
//
//                Entity spawnedEntity = EntityManager.Instantiate(prefabEntityComponent.prefabEntity);
//
//                EntityManager.SetComponentData(spawnedEntity, 
//                    new Translation { Value = new float3(random.NextFloat(-5f, 5f), random.NextFloat(-5f, 5f), 0) }
//                );

                Entities.ForEach((ref PrefabEntityComponent prefabEntityComponent) => {
                    //在主线程上执行
                    /*System.Diagnostics.Stopwatch stopwatch1 = new System.Diagnostics.Stopwatch();
                    stopwatch1.Start();*/
                    //for (int i = 0; i < 1000; i++)
                    {
                        Entity spawnedEntity = EntityManager.Instantiate(prefabEntityComponent.prefabEntity);

                        EntityManager.SetComponentData(spawnedEntity, 
                            new Translation { Value = new float3(random.NextFloat(-5f, 5f), random.NextFloat(-5f, 5f), 0) }
                        );
                    }
                    /*stopwatch1.Stop();
                    var totalT2 = stopwatch1.Elapsed.Milliseconds;*/
                    // Debug.Log($"EntityCommandBuffer create Entity===={totalT2} 毫秒");

                    

                });
            
            

                /*
            Entity spawnedEntity = EntityManager.Instantiate(PrefabEntities_V2.prefabEntity);

            EntityManager.SetComponentData(spawnedEntity, 
                new Translation { Value = new float3(random.NextFloat(-5f, 5f), random.NextFloat(-5f, 5f), 0) }
            );
            */
            }
        }

    }

//    public class EntitySpawnerSystem_Ex : SystemBase
//    {
//        private BeginInitializationEntityCommandBufferSystem _beginInitializationEntityCommandBufferSystem;
//        protected override void OnCreate()
//        {
//            base.OnCreate();
//            _beginInitializationEntityCommandBufferSystem =
//                World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
//        }
//
//        protected override void OnUpdate()
//        {
//            var commandBuffer = _beginInitializationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
//             Random random = new Random(100);
//            Entities.WithName("EntitySpawnerSystem_ExHxp")
//                .ForEach((Entity entity, int entityInQueryIndex, in PrefabEntityComponent spawnerFromEntity) =>
//            {
//                for (int i = 0; i < 1000; i++)
//                {
//                    var instance = commandBuffer.Instantiate(entityInQueryIndex, spawnerFromEntity.prefabEntity);
//                   
//                    commandBuffer.AddComponent(entityInQueryIndex, instance, new Translation { Value = new float3(random.NextFloat(-5f, 5f), random.NextFloat(-5f, 5f), 0) });
//                }
//               
//  
//
//            }).Schedule();
//        
//            /* 把Job添加到EntityCommandBufferSystem */ 
//            //EntityCommandBufferSystem每次执行队列的任务后，都会清空，所以不用担心
//            //最后Entity的产生还是在主线程
//            _beginInitializationEntityCommandBufferSystem.AddJobHandleForProducer(this.Dependency);
//        }
//    }
}
