namespace Doc.CodeSamples.Tests
{
    using Unity.Entities;
    using Unity.Transforms;
    using Unity.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Unity.Mathematics;
    using Random = Unity.Mathematics.Random;
    
    /*xxx 代表components
     *1 Entities.ForEach( Entity e, int entityInQueryIndex, xxxx)
     *2 Entities.ForEach(int entityInQueryIndex, xxxx)
     *3 Entities.ForEach(xxxx)
     */

    #region entities-foreach-example

    partial class ApplyVelocitySystem : SystemBase
    {
        protected override void OnUpdate()
        {
            //要访问与entity关联的component，您必须将该component类型的参数传递给lambda函数。
            //编译器会自动将传递给函数的所有components作为必需components添加到entity query中
            Entities
                .ForEach((ref Translation translation,
                in Velocity velocity) =>
                {
                    translation.Value += velocity.Value;
                })
                .Schedule();
        }
    }
    #endregion
    #region job-with-code-example

    public partial class RandomSumJob : SystemBase
    {
        private uint seed = 1;

        protected override void OnUpdate()
        {
            Random randomGen = new Random(seed++); //随机种子
            NativeArray<float> randomNumbers
                = new NativeArray<float>(500, Allocator.TempJob);
            //Job.WithCode构造是一种将函数作为单个后台job运行的简便方法。
            //您甚至可以在主线程上运行Job.WithCode，并且仍然可以利用Burst编译来加快执行速度。
            Job.WithCode(() =>
            {
                for (int i = 0; i < randomNumbers.Length; i++)
                {
                    randomNumbers[i] = randomGen.NextFloat(); //返回一个0~1的随机数
                }
            }).Schedule(); //只能使用Schedule
            
            
            // To get data out of a job, you must use a NativeArray
            // even if there is only one value
            ///想要在job外获取data，你必须使用一个本地化数组
            // 即使他只有一个值
            NativeArray<float> result
                = new NativeArray<float>(1, Allocator.TempJob);

            Job.WithCode(() =>
            {
                for (int i = 0; i < randomNumbers.Length; i++)
                {
                    result[0] += randomNumbers[i];
                }
            }).Schedule();

//            Job
//                .WithDisposeOnCompletion(result)
//                .WithDisposeOnCompletion(randomNumbers)
//                .WithCode(() => { });

            // This completes the scheduled jobs to get the result immediately, but for
            // better efficiency you should schedule jobs early in the frame with one
            // system and get the results late in the frame with a different system.
            this.CompleteDependency();
            UnityEngine.Debug.Log("The sum of "
                + randomNumbers.Length + " numbers is " + result[0]);

            randomNumbers.Dispose();
            result.Dispose();
        }
    }

    #endregion

    //Used to verify the BuffersByEntity example (not shown in docs)
    public partial class MakeData : SystemBase
    {
        protected override void OnCreate()
        {
            var sum = 0;
            for (int i = 0; i < 100; i++)
            {
                var ent = EntityManager.CreateEntity(typeof(IntBufferElement));
                var buff = EntityManager.GetBuffer<IntBufferElement>(ent).Reinterpret<int>();
                for (int j = 0; j < 5; j++)
                {
                    buff.Add(j);
                    sum += j;
                }
            }

            UnityEngine.Debug.Log("Sum should equal " + sum);
        }

        protected override void OnUpdate()
        {
        }
    }

    public struct IntBufferData : IBufferElementData
    {
        public int Value;
    }

    #region dynamicbuffer

    public partial class BufferSum : SystemBase
    {
        private EntityQuery query;

        //Schedules the two jobs with a dependency between them
        protected override void OnUpdate()
        {
            //The query variable can be accessed here because we are
            //using WithStoreEntityQueryInField(query) in the entities.ForEach below
            //创建一个本地化数组，该数组具有足够的空间来为query选择的每个entity存储一个值
            //使用WithStoreEntityQueryInField来存储query，下一帧调用OnUpdate的时候就能准确计算出个数了
            int entitiesInQuery = query.CalculateEntityCount();

            //Create a native array to hold the intermediate sums
            //(one element per entity)
            NativeArray<int> intermediateSums
                = new NativeArray<int>(entitiesInQuery, Allocator.TempJob);

            //Schedule the first job to add all the buffer elements
            Entities
                .ForEach((int entityInQueryIndex, in DynamicBuffer<IntBufferData> buffer) =>
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    intermediateSums[entityInQueryIndex] += buffer[i].Value;
                }
            })
                .WithStoreEntityQueryInField(ref query)
                .WithName("IntermediateSums")
                .ScheduleParallel(); // Execute in parallel for each chunk of entities

            //Schedule the second job, which depends on the first  自动依赖第一个job
            Job
                .WithCode(() =>
            {
                int result = 0;
                for (int i = 0; i < intermediateSums.Length; i++)
                {
                    result += intermediateSums[i];
                }
                //Not burst compatible:
                Debug.Log("Final sum is " + result);
            })
                .WithDisposeOnCompletion(intermediateSums)
                .WithoutBurst()
                .WithName("FinalSum")
                .Schedule(); // Execute on a single, background thread
        }
    }
    #endregion

    public struct Source : IComponentData
    {
        public int Value;
    }
    public struct Destination : IComponentData
    {
        public int Value;
    }

    public partial class WithAllExampleSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            #region entity-query

            Entities.WithAll<LocalToWorld>()
                .WithAny<Rotation, Translation, Scale>()
                .WithNone<LocalToParent>() //LocalToParent 表示从本地空间到父级本地空间的转换  
                .ForEach((ref Destination outputData, in Source inputData) =>
                {
                    /* do some work */
                })
                .Schedule();
            #endregion
        }
    }

    public struct Data : IComponentData
    {
        public float Value;
    }
    public partial class WithStoreQuerySystem : SystemBase
    {
        #region store-query

        private EntityQuery query;
        protected override void OnUpdate()
        {
            //CalculateEntityCount 使用此计数来创建一个本地化数组，该数组具有足够的空间来为query选择的每个entity存储一个值
            int dataCount = query.CalculateEntityCount();
            NativeArray<float> dataSquared
                = new NativeArray<float>(dataCount, Allocator.Temp);
            Entities
                .WithStoreEntityQueryInField(ref query) //[WithStoreEntityQueryInField（ref query）]和ref参数修饰符。此函数将query的引用分配给您提供的字段
                .ForEach((int entityInQueryIndex, in Data data) =>
                {
                    dataSquared[entityInQueryIndex] = data.Value * data.Value;
                })
                .ScheduleParallel();

            Job
                .WithCode(() =>
            {
                //Use dataSquared array...
                var v = dataSquared[dataSquared.Length - 1];
            })
                .WithDisposeOnCompletion(dataSquared)
                .Schedule();
        }

        #endregion
    }

    public partial class WithChangeExampleSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            #region with-change-filter

            Entities
                .WithChangeFilter<Source>() //当具有Source组件的另一个entity发生更改时,才处理该当前entity component
                .ForEach((ref Destination outputData,
                    in Source inputData) =>
                    {
                        /* Do work */
                    })
                .ScheduleParallel();
            #endregion
        }
    }

    public struct Cohort : ISharedComponentData
    {
        public int Value;
    }
    public struct DisplayColor : IComponentData
    {
        public int Value;
    }

    public class ColorTable
    {
        public static DisplayColor GetNextColor(int current) {return new DisplayColor();}
    }

    #region with-shared-component

    public partial class ColorCycleJob : SystemBase
    {
        protected override void OnUpdate()
        {
            List<Cohort> cohorts = new List<Cohort>();
            //具有shared component的entity与其他具有相同shared component值的entities分组在一起
            EntityManager.GetAllUniqueSharedComponentData<Cohort>(cohorts);
            foreach (Cohort cohort in cohorts)
            {
                DisplayColor newColor = ColorTable.GetNextColor(cohort.Value);
                Entities.WithSharedComponentFilter(cohort)
                    .ForEach((ref DisplayColor color) => { color = newColor; })
                    .ScheduleParallel();
            }
        }
    }
    #endregion

    public partial class ReadWriteModExample : SystemBase
    {
        protected override void OnUpdate()
        {
            #region read-write-modifiers

            Entities.ForEach(
                (ref Destination outputData,
                    in Source inputData) =>
                {
                    outputData.Value = inputData.Value;
                })
                .ScheduleParallel();
            #endregion
        }
    }

    #region basic-ecb

    public partial class MyJobSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem commandBufferSystem;

        protected override void OnCreate()
        {
            commandBufferSystem = World
                .DefaultGameObjectInjectionWorld
                .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            EntityCommandBuffer.ParallelWriter commandBuffer
                = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();

            //.. The rest of the job system code
        }
    }
    #endregion

    public struct Movement : IComponentData
    {
        public float3 Value;
    }

    public partial class EFESystem : SystemBase
    {
        protected override void OnUpdate()
        {
            #region lambda-params
            //可以获取当前Entity作为参数
            Entities.ForEach(
                (Entity entity,
                    int entityInQueryIndex,
                    ref Translation translation,
                    in Movement move) => { /* .. */})
                #endregion
                .Run();
        }
    }
}



namespace Doc.CodeSamples.Tests
{
    using Unity.Entities;

    struct Data1 : IComponentData{}
    struct Data2 : IComponentData{}
    struct Data3 : IComponentData{}
    struct Data4 : IComponentData{}
    struct Data5 : IComponentData{}
    struct Data6 : IComponentData{}
    struct Data7 : IComponentData{}
    struct Data8 : IComponentData{}
    struct Data9 : IComponentData{}
    struct Data10 : IComponentData{}
    struct Data11 : IComponentData{}

    #region lambda-params-many

    static class BringYourOwnDelegate
    {
        // Declare the delegate that takes 12 parameters. T0 is used for the Entity argument
        [Unity.Entities.CodeGeneratedJobForEach.EntitiesForEachCompatible]
        public delegate void CustomForEachDelegate<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
            (T0 t0, in T1 t1, in T2 t2, in T3 t3, in T4 t4, in T5 t5,
             in T6 t6, in T7 t7, in T8 t8, in T9 t9, in T10 t10, in T11 t11);

        // Declare the function overload
        public static TDescription ForEach<TDescription, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
            (this TDescription description, CustomForEachDelegate<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> codeToRun)
            where TDescription : struct, Unity.Entities.CodeGeneratedJobForEach.ISupportForEachWithUniversalDelegate =>
            LambdaForEachDescriptionConstructionMethods.ThrowCodeGenException<TDescription>();
    }

    // A system that uses the custom delegate and overload
    public partial class MayParamsSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach(
                    (Entity entity0,
                        in Data1 d1,
                        in Data2 d2,
                        in Data3 d3,
                        in Data4 d4,
                        in Data5 d5,
                        in Data6 d6,
                        in Data7 d7,
                        in Data8 d8,
                        in Data9 d9,
                        in Data10 d10,
                        in Data11 d11
                        ) => {/* .. */})
                .Run();
        }
    }

    #endregion
}

namespace Doc.CodeSamples.Tests
{
    #region full-ecb-pt-one

    // ParticleSpawner.cs
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using Unity.Transforms;

    public struct Velocity : IComponentData
    {
        public float3 Value;
    }

    public struct TimeToLive : IComponentData
    {
        public float LifeLeft;
    }

    public partial class ParticleSpawner : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem commandBufferSystem;

        protected override void OnCreate()
        {
            commandBufferSystem = World
                .DefaultGameObjectInjectionWorld
                .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            EntityCommandBuffer.ParallelWriter commandBufferCreate
                = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
            EntityCommandBuffer.ParallelWriter commandBufferCull
                = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();

            float dt = Time.DeltaTime;
            Random rnd = new Random((uint)(dt * 100000));
            //rnd.InitState((uint)(dt * 100000));

            //依赖于上一帧job执行完 MoveJobHandle, cullJobHandle
            JobHandle spawnJobHandle = Entities
                .ForEach((int entityInQueryIndex,
                    in SpawnParticles spawn,
                    in LocalToWorld center) =>
                {
                    int spawnCount = spawn.Rate;
                    for (int i = 0; i < spawnCount; i++)
                    {
                        Entity spawnedEntity = commandBufferCreate
                            .Instantiate(entityInQueryIndex,
                            spawn.ParticlePrefab);

                        LocalToWorld spawnedCenter = center;
                        Translation spawnedOffset = new Translation()
                        {
                            Value = center.Position +
                                rnd.NextFloat3(-spawn.Offset, spawn.Offset)  //返回一个随机的float3
                        };
                        Velocity spawnedVelocity = new Velocity()
                        {
                            Value = rnd.NextFloat3(-spawn.MaxVelocity, spawn.MaxVelocity)
                        };
                        TimeToLive spawnedLife = new TimeToLive()
                        {
                            LifeLeft = spawn.Lifetime
                        };

                        commandBufferCreate.SetComponent(entityInQueryIndex,
                            spawnedEntity,
                            spawnedCenter);
                        commandBufferCreate.SetComponent(entityInQueryIndex,
                            spawnedEntity,
                            spawnedOffset);
                        commandBufferCreate.AddComponent(entityInQueryIndex,
                            spawnedEntity,
                            spawnedVelocity);
                        commandBufferCreate.AddComponent(entityInQueryIndex,
                            spawnedEntity,
                            spawnedLife);
                    }
                })
                .WithName("ParticleSpawning")
                .Schedule(this.Dependency);

            JobHandle MoveJobHandle = Entities
                .ForEach((ref Translation translation, in Velocity velocity) =>
            {
                translation = new Translation()
                {
                    Value = translation.Value + velocity.Value * dt
                };
            })
                .WithName("MoveParticles")
                .Schedule(spawnJobHandle);

            JobHandle cullJobHandle = Entities
                .ForEach((Entity entity, int entityInQueryIndex, ref TimeToLive life) =>
            {
                life.LifeLeft -= dt;
                if (life.LifeLeft < 0)
                    commandBufferCull.DestroyEntity(entityInQueryIndex, entity);
            })
                .WithName("CullOldEntities")
                .Schedule(this.Dependency);

            this.Dependency
                = JobHandle.CombineDependencies(MoveJobHandle, cullJobHandle);

            commandBufferSystem.AddJobHandleForProducer(spawnJobHandle);
            commandBufferSystem.AddJobHandleForProducer(cullJobHandle);
        }
    }
    #endregion
}

namespace Doc.CodeSamples.Tests
{
    #region full-ecb-pt-two

    // SpawnParticles.cs
    using Unity.Entities;
    using Unity.Mathematics;

    [GenerateAuthoringComponent]
    public struct SpawnParticles : IComponentData
    {
        public Entity ParticlePrefab;
        public int Rate;
        public float3 Offset;
        public float3 MaxVelocity;
        public float Lifetime;
    }
    #endregion
}
