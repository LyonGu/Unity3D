using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace EntitysExample
{
    [GenerateAuthoringComponent]
    public struct ExpensiveTarget : IComponentData
    {
        public Entity entity;
    }
    
    [GenerateAuthoringComponent]
    public struct Target : IComponentData
    {
        public Entity entity;
    }
    public class BatchedChaserSystem : SystemBase
    {
        private EntityQuery query; // Initialized in Oncreate()

        [BurstCompile]
        private struct BatchedChaserSystemJob : IJobEntityBatch
        {
            // Read-write data in the current chunk
            public ComponentTypeHandle<Translation> PositionTypeHandleAccessor;

            // Read-only data in the current chunk
            [ReadOnly]
            public ComponentTypeHandle<Target> TargetTypeHandleAccessor;

            // Read-only data stored (potentially) in other chunks
            [ReadOnly]
            //[NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<LocalToWorld> EntityPositions;

            // Non-entity data
            public float deltaTime;
            public int frameCount;
            public int batchCount; //一般外部参数，这里只是为了打印下日志 没有实际意义

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                
//                Debug.Log($"BatchedChaserSystemJob Count = {batchInChunk.Count}  Capacity = {batchInChunk.Capacity} batchIndex = {batchIndex}  frameCount = {frameCount}");
                
                // 以下的解释很清楚了
                // 每次Execute调用处理Entity个数为 最大为batchInChunk.Capacity/batchesPerChunk，意味着把一个chunk上的数据分批处理了
                
                // Within Execute(), the scope of the ArchetypeChunk is limited to the current batch.
                // For example, these NativeArrays will have Length = batchInChunk.BatchEntityCount,
                // where batchInChunk.BatchEntityCount is roughly batchInChunk.Capacity divided by the
                // batchesInChunk parameter passed to ScheduleParallelBatched().
                NativeArray<Translation> positions = batchInChunk.GetNativeArray<Translation>(PositionTypeHandleAccessor);
                NativeArray<Target> targets = batchInChunk.GetNativeArray<Target>(TargetTypeHandleAccessor);
        
                for (int i = 0; i < positions.Length; i++)
                {
                    Entity targetEntity = targets[i].entity;
                    float3 targetPosition = EntityPositions[targetEntity].Position;
                    float3 chaserPosition = positions[i].Value;

                    float3 displacement = (targetPosition - chaserPosition);
                    positions[i] = new Translation { Value = chaserPosition + displacement * deltaTime };
                }
            }
        }

        protected override void OnCreate()
        {
            query = this.GetEntityQuery(typeof(Translation), ComponentType.ReadOnly<Target>());
            
            for (int i = 0; i < 1000; i++)
            {
                var e = EntityManager.CreateEntity(typeof(Translation), ComponentType.ReadOnly<Target>(),typeof(LocalToWorld));
                EntityManager.SetComponentData(e, new Target(){entity = e});
            }
        }

        protected override void OnUpdate()
        {
            var job = new BatchedChaserSystemJob();
            job.PositionTypeHandleAccessor = this.GetComponentTypeHandle<Translation>(false);
            job.TargetTypeHandleAccessor = this.GetComponentTypeHandle<Target>(true);

            job.EntityPositions = this.GetComponentDataFromEntity<LocalToWorld>(true);
            job.deltaTime = this.Time.DeltaTime;
            job.frameCount = UnityEngine.Time.frameCount;

            //每个chunk使用多少批次处理，
            int batchesPerChunk = 1; // Partition each chunk into this many batches. Each batch will be processed concurrently.
            
           
            /*
             *  //https://zhuanlan.zhihu.com/p/361627288
             * batchesPerChunk默认情况下是1，此时和IJobChunk的效果是一样的，
             * 根据Chunk个数去生成对应数量的Job，但是这样不够灵活，如果某个Job计算量特别大，但是里面用到的实体又都在一个Chunk里，就无法再给它”减负“了。
             * 此时可以借助batchesPerChunk将这个Job进行进一步的分割，batchesPerChunk的值就是你想把这个Chunk中的实体分成几批对应的也就会生成几个Job了
                
                会反应出一个Job调度后最终实际生成了几个Job
                
                感觉解释跟我实际测试不符，默认为1没什么区别 而且效率最高
             */
            
            this.Dependency = job.ScheduleParallel(query, batchesPerChunk, this.Dependency);
            
        }
    }
}
