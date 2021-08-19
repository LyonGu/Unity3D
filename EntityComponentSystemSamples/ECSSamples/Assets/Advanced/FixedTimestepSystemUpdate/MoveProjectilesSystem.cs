using System.ComponentModel;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Samples.FixedTimestepSystem
{
    public struct Projectile : IComponentData
    {
        public float SpawnTime;
        public float3 SpawnPos;
        public Entity entity;
    }
    public partial class MoveProjectilesSystem : SystemBase
    {
        BeginSimulationEntityCommandBufferSystem m_beginSimEcbSystem;
        protected override void OnCreate()
        {
            m_beginSimEcbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = m_beginSimEcbSystem.CreateCommandBuffer().AsParallelWriter();
            float timeSinceLoad = (float) Time.ElapsedTime;
            float projectileSpeed = 5.0f;
            Entities
                .WithName("MoveProjectiles")
                .ForEach((Entity projectileEntity, int entityInQueryIndex, ref Translation translation, in Projectile projectile) =>
                {
                    float aliveTime = (timeSinceLoad - projectile.SpawnTime);
                    if (aliveTime > 5.0f)
                    {
                        //销毁会回到主线程
                        ecb.DestroyEntity(entityInQueryIndex, projectileEntity);
                    }
                    translation.Value.x = projectile.SpawnPos.x + aliveTime * projectileSpeed;
                }).ScheduleParallel();
            m_beginSimEcbSystem.AddJobHandleForProducer(Dependency);
        }
    }
    
    
    //换成jobs试试   ecb is not declared [ReadOnly] in a IJobParallelFor job. The container does not support parallel writing
//    public partial class MoveProjectilesSystem1 : SystemBase
//    {
//        BeginSimulationEntityCommandBufferSystem m_beginSimEcbSystem;
//        EntityQuery m_Query;
//        protected override void OnCreate()
//        {
//            m_Query = GetEntityQuery(typeof(Translation), ComponentType.ReadOnly<Projectile>());
//            m_beginSimEcbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
//        }
//        [BurstCompatible]
//        struct ProjectileJob : IJobEntityBatch
//        {
//            public float projectileSpeed;
//            public float timeSinceLoad;
//            public EntityCommandBuffer ecb;
//            
//            public ComponentTypeHandle<Translation> TranslationsHandle;
//            [Unity.Collections.ReadOnly]public ComponentTypeHandle<Projectile> ProjectilesHandle;
//            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
//            {
//                var ecbParallelWriter = ecb.AsParallelWriter();
//                var chunkTranslations = batchInChunk.GetNativeArray(TranslationsHandle);
//                var chunkProjectiles = batchInChunk.GetNativeArray(ProjectilesHandle);
//                for (var i = 0; i < batchInChunk.Count; i++)
//                {
//                    var translation = chunkTranslations[i];
//                    var projectile = chunkProjectiles[i];
//
//                    
//                    float aliveTime = (timeSinceLoad - projectile.SpawnTime);
//                    if (aliveTime > 5.0f)
//                    {
//                        //销毁会回到主线程
//                        ecbParallelWriter.DestroyEntity(i,projectile.entity);
////                        ecb.DestroyEntity(projectile.entity);
//                    }
//                    float x = projectile.SpawnPos.x + aliveTime * projectileSpeed;
//                    float y = translation.Value.y;
//                    float z = translation.Value.z;
//                    chunkTranslations[i] = new Translation()
//                    {
//                        Value = new float3(x,y,z)
//                    };
//
//                }
//
//            }
//        }
//
//        protected override void OnUpdate()
//        {
////            var ecb = m_beginSimEcbSystem.CreateCommandBuffer().AsParallelWriter();
////            float timeSinceLoad = (float) Time.ElapsedTime;
////            float projectileSpeed = 5.0f;
////            Entities
////                .WithName("MoveProjectiles")
////                .ForEach((Entity projectileEntity, int entityInQueryIndex, ref Translation translation, in Projectile projectile) =>
////                {
////                    float aliveTime = (timeSinceLoad - projectile.SpawnTime);
////                    if (aliveTime > 5.0f)
////                    {
////                        //销毁会回到主线程
////                        ecb.DestroyEntity(entityInQueryIndex, projectileEntity);
////                    }
////                    translation.Value.x = projectile.SpawnPos.x + aliveTime * projectileSpeed;
////                }).ScheduleParallel();
////            m_beginSimEcbSystem.AddJobHandleForProducer(Dependency);
//
//            var ecb = m_beginSimEcbSystem.CreateCommandBuffer();
//            float timeSinceLoad = (float) Time.ElapsedTime;
//            float projectileSpeed = 5.0f;
//            ProjectileJob job = new ProjectileJob();
//            job.ecb = ecb;
//            job.projectileSpeed = projectileSpeed;
//            job.timeSinceLoad = timeSinceLoad;
//            job.TranslationsHandle = GetComponentTypeHandle<Translation>();
//            job.ProjectilesHandle = GetComponentTypeHandle<Projectile>(true);
//
////            job.Schedule(this.Dependency);
//            this.Dependency = job.ScheduleParallel(m_Query, 1, this.Dependency);
//            m_beginSimEcbSystem.AddJobHandleForProducer(this.Dependency);
//        }
//    }
}
