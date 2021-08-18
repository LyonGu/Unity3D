using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace EntitysExample
{
    [InternalBufferCapacity(8)]
    public struct MyBufferElement : IBufferElementData //数据是存储在chunk之外的
    {
        // Actual value each buffer element will store.
        public int Value;

        // The following implicit conversions are optional, but can be convenient.
        public static implicit operator int(MyBufferElement e)
        {
            return e.Value;
        }

        public static implicit operator MyBufferElement(int e)
        {
            return new MyBufferElement { Value = e };
        }
    }
    public partial class DynamicBufferJobSystem : SystemBase
    {
        private EntityQuery query;

        protected override void OnCreate()
        {
            //Create a query to find all entities with a dynamic buffer
            // containing MyBufferElement
            EntityQueryDesc queryDescription = new EntityQueryDesc();
            queryDescription.All = new[] {ComponentType.ReadOnly<MyBufferElement>()};
            query = GetEntityQuery(queryDescription);
            
            //测试
            int enityCount = 10;
            EntityArchetype archetype = EntityManager.CreateArchetype(typeof(MyBufferElement));
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
           
            for (int i = 0; i < enityCount; i++)
            {
                //根据Archetype创建Entity
                EntityManager.CreateEntity(typeof(MyBufferElement));
            }
            
            
        }
        //要访问IJobChunkjob中的单个缓冲区，请将缓冲区数据类型传递给job，然后使用该数据类型获取BufferAccessor
        [BurstCompatible]
        public struct BuffersInChunks : IJobEntityBatch
        {
            //The data type and safety object
            public BufferTypeHandle<MyBufferElement> BufferTypeHandle;

            //An array to hold the output, intermediate sums
            public NativeArray<int> sums;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                //A buffer accessor is a list of all the buffers in the chunk
                BufferAccessor<MyBufferElement> buffers
                    = batchInChunk.GetBufferAccessor(BufferTypeHandle);

                for (int c = 0; c < batchInChunk.Count; c++)
                {
                    //An individual dynamic buffer for a specific entity
                    DynamicBuffer<MyBufferElement> buffer = buffers[c];
                    for(int i = 0; i < buffer.Length; i++)
                    {
                        sums[batchIndex] += buffer[i].Value;
                    }
                }
            }
        }

        //Sums the intermediate results into the final total
        [BurstCompatible]
        public struct SumResult : IJob
        {
            [DeallocateOnJobCompletion] public NativeArray<int> sums;
            public NativeArray<int> result;
            public void Execute()
            {
                for(int i  = 0; i < sums.Length; i++)
                {
                    result[0] += sums[i];
                }
            }
        }

        protected override void OnUpdate()
        {
            //Create a native array to hold the intermediate sums
            //CalculateChunkCount 可以计算chunk的数量
            int chunksInQuery = query.CalculateChunkCount();
            NativeArray<int> intermediateSums
                = new NativeArray<int>(chunksInQuery, Allocator.TempJob);

            //Schedule the first job to add all the buffer elements
            BuffersInChunks bufferJob = new BuffersInChunks();
            bufferJob.BufferTypeHandle = GetBufferTypeHandle<MyBufferElement>();
            bufferJob.sums = intermediateSums;
            this.Dependency = bufferJob.ScheduleParallel(query, 1, this.Dependency);

            //Schedule the second job, which depends on the first
            SumResult finalSumJob = new SumResult();
            finalSumJob.sums = intermediateSums;
            NativeArray<int> finalSum = new NativeArray<int>(1, Allocator.TempJob);
            finalSumJob.result = finalSum;
            this.Dependency = finalSumJob.Schedule(this.Dependency);

            this.CompleteDependency();
            Debug.Log("Sum of all buffers: " + finalSum[0]);
            finalSum.Dispose();
        }
    }
}


