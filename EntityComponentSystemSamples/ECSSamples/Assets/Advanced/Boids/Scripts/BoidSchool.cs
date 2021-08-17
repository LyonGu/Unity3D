using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Profiling;

namespace Samples.Boids
{
    public struct BoidSchool : IComponentData
    {
        public Entity Prefab;
        public float InitialRadius;
        public int Count;
    }

    public partial class BoidSchoolSpawnSystem : SystemBase
    {
        [BurstCompile]
        struct SetBoidLocalToWorld : IJobParallelFor //并行的job
        {
            [NativeDisableContainerSafetyRestriction]
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<LocalToWorld> LocalToWorldFromEntity;

            public NativeArray<Entity> Entities;
            public float3 Center;
            public float Radius;

            public void Execute(int i)
            {
                var entity = Entities[i];
                var random = new Random(((uint)(entity.Index + i + 1) * 0x9F6ABC1));
                var dir = math.normalizesafe(random.NextFloat3() - new float3(0.5f, 0.5f, 0.5f));
                var pos = Center + (dir * Radius);
                var localToWorld = new LocalToWorld
                {
                    Value = float4x4.TRS(pos, quaternion.LookRotationSafe(dir, math.up()), new float3(1.0f, 1.0f, 1.0f))
                };
                LocalToWorldFromEntity[entity] = localToWorld;
            }
        }

        protected override void OnUpdate()
        {
            
            /*
             *
             * 当您调用Entities.ForEach.Run（）时，作业调度程序会在开始ForEach迭代之前完成system所依赖的所有调度job。==> 先完成依赖项
             * 如果您还使用WithStructuralChanges（）作为构造的一部分，则job调度程序将完成所有正在运行和待调度的jobs。结构更改还会使对component数据的任何直接引用无效
             */
            Entities.WithStructuralChanges().ForEach((Entity entity, int entityInQueryIndex, in BoidSchool boidSchool, in LocalToWorld boidSchoolLocalToWorld) =>
            {
                //分配一个NavtiveContainer,如果NavtiveContainer直接用来写的话，可以用NativeArrayOptions.UninitializedMemory，会提升效率
                var boidEntities = new NativeArray<Entity>(boidSchool.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

                Profiler.BeginSample("Instantiate");
                EntityManager.Instantiate(boidSchool.Prefab, boidEntities);
                Profiler.EndSample();

               
                
                //All component data of type T.==> 返回的是所有Entity的LocalToWorld组件，取到对应Entity上Component需要 localToWorldFromEntity[Entity]
                var localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>();
                //开始创建Job，然后调度
                var setBoidLocalToWorldJob = new SetBoidLocalToWorld
                {
                    LocalToWorldFromEntity = localToWorldFromEntity,
                    Entities = boidEntities,  //NativeContainer
                    Center = boidSchoolLocalToWorld.Position, //中心位置
                    Radius = boidSchool.InitialRadius 
                };
                
                //这个参数不是很明白 64 是使用64个核
                Dependency = setBoidLocalToWorldJob.Schedule(boidSchool.Count, 64, Dependency);
                Dependency = boidEntities.Dispose(Dependency);

                EntityManager.DestroyEntity(entity);
            }).Run(); //主线程执行 不會效率低吗
        }
    }
}
