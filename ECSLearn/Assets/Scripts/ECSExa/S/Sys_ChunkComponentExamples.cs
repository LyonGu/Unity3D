
namespace EntityExample
{
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using Unity.Transforms;
    using static Unity.Mathematics.math;

    //chunk component包含适用于特定chunk中所有entities的数据
    /*
    尽管chunk component可以单个块具有唯一的值，但它们仍然是该chunk中entity archetype的一部分。
    因此，如果您从实体中删除了一个chunk component，ECS会将该entity移动到另一个chunk（可能是一个新的chunk）。
    同样，如果将chunk component添加到entity，则ECS会将该entity移至其他chunk，因为其archetype会更改；
    chunk component的添加不会影响原始chunk中的其余entities。
     */
    //ECS为每个chunk创建chunk component，并存储具有该archetype的实体
    /*
     使用chunk component和通用component之间的主要区别在于，您使用不同的功能来添加，设置和删除它们

    目的	功能
    介绍	IComponentData

    ArchetypeChunk方法	
        读	GetChunkComponentData （ArchetypeChunkComponentType ）
        检查	[HasChunkComponent （ArchetypeChunkComponentType ）]
        写	SetChunkComponentData （ArchetypeChunkComponentType ，T）

    EntityManager方法	
        创建	AddChunkComponentData （Entity）
        创建	AddChunkComponentData （EntityQuery，T）
        创建	AddComponents（Entity，ComponentTypes）
        获取类型信息	[GetComponentTypeHandle]
        读	[GetChunkComponentData （ArchetypeChunk）]
        读	GetChunkComponentData （Entity）
        检查	HasChunkComponent （Entity）
        删除	RemoveChunkComponent （Entity）
        删除	RemoveChunkComponentData （EntityQuery）
        写	EntityManager.SetChunkComponentData （ArchetypeChunk，T）
     */

    public struct GeneralPurposeComponentA : IComponentData
    {
        public int Lifetime;
    }
    #region declare-chunk-component

    //ChunkComponent 也是继承IComponentData
    public struct ChunkComponentA : IComponentData
    {
        public float Value;
    }
    #endregion

    #region full-chunk-example

    public class ChunkComponentExamples : SystemBase
    {
        private EntityQuery ChunksWithChunkComponentA;
        protected override void OnCreate()
        {
            EntityQueryDesc ChunksWithComponentADesc = new EntityQueryDesc()
            {
                //通过这些方法使用ComponentType.ChunkComponent 或[ComponentType.ChunkComponentReadOnly ]。
                //否则，ECS将该组件视为通用组件，而不是chunk component。
                All = new ComponentType[] {
                    ComponentType.ChunkComponent<ChunkComponentA>()  //必须使用ComponentType.ChunkComponent
                }
            };
            ChunksWithChunkComponentA
                = GetEntityQuery(ChunksWithComponentADesc);
        }

        [BurstCompile]
        struct ChunkComponentCheckerJob : IJobEntityBatch
        {
            public ComponentTypeHandle<ChunkComponentA> ChunkComponentATypeHandle;
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var compValue
                    = batchInChunk.GetChunkComponentData(ChunkComponentATypeHandle);
                //...
                var squared = compValue.Value * compValue.Value;
                batchInChunk.SetChunkComponentData(ChunkComponentATypeHandle,
                    new ChunkComponentA() { Value = squared });
            }
        }

        protected override void OnUpdate()
        {
            var job = new ChunkComponentCheckerJob()
            {
                ChunkComponentATypeHandle
                    = GetComponentTypeHandle<ChunkComponentA>()  //获取类型
            };
            this.Dependency
                = job.ScheduleParallel(ChunksWithChunkComponentA, 1,
                               this.Dependency);
        }
    }
    #endregion

    #region aabb-chunk-component

    public struct ChunkAABB : IComponentData
    {
        public AABB Value;
    }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateBefore(typeof(UpdateAABBSystem))]
    public class AddAABBSystem : SystemBase
    {
        EntityQuery queryWithoutChunkComponent;
        protected override void OnCreate()
        {
            queryWithoutChunkComponent
                = GetEntityQuery(new EntityQueryDesc()
                {
                    All = new ComponentType[] {
                    ComponentType.ReadOnly<LocalToWorld>()
                },
                    None = new ComponentType[]{
                    ComponentType.ChunkComponent<ChunkAABB>()
                }
                });
        }

        protected override void OnUpdate()
        {
            // This is a structural change and a sync point  居然是一个同步点
            EntityManager.AddChunkComponentData<ChunkAABB>(
                queryWithoutChunkComponent,
                new ChunkAABB()
            );
        }
    }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class UpdateAABBSystem : SystemBase
    {
        EntityQuery queryWithChunkComponent;
        protected override void OnCreate()
        {
            queryWithChunkComponent
                = GetEntityQuery(new EntityQueryDesc()
                {
                    All = new ComponentType[]
                      {
                          ComponentType.ReadOnly<LocalToWorld>(),
                          ComponentType.ChunkComponent<ChunkAABB>()
                      }
                });
        }

        //需要加BurstCompile 标签才会执行burst，否则只是多线程执行效率会降低
        [BurstCompile]
        struct AABBJob : IJobEntityBatch
        {
            [ReadOnly]
            public ComponentTypeHandle<LocalToWorld> LocalToWorldTypeHandleInfo;
            public ComponentTypeHandle<ChunkAABB> ChunkAabbTypeHandleInfo;
            public uint L2WChangeVersion;
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                bool chunkHasChanges
                    = batchInChunk.DidChange(LocalToWorldTypeHandleInfo,
                                      L2WChangeVersion);

                if (!chunkHasChanges)
                    return; // early out if the chunk transforms haven't changed

                //ArchetypeChunk.GetNativeArray =>chunk里对应的组件的array
                NativeArray<LocalToWorld> transforms
                    = batchInChunk.GetNativeArray<LocalToWorld>(LocalToWorldTypeHandleInfo);
                UnityEngine.Bounds bounds = new UnityEngine.Bounds();
                bounds.center = transforms[0].Position;
                for (int i = 1; i < transforms.Length; i++)
                {
                    bounds.Encapsulate(transforms[i].Position);
                }
                batchInChunk.SetChunkComponentData(
                    ChunkAabbTypeHandleInfo,
                    new ChunkAABB() { Value = bounds.ToAABB() });
            }
        }

        protected override void OnUpdate()
        {
            var job = new AABBJob()
            {
                LocalToWorldTypeHandleInfo
                    = GetComponentTypeHandle<LocalToWorld>(true),
                ChunkAabbTypeHandleInfo
                    = GetComponentTypeHandle<ChunkAABB>(false),
                L2WChangeVersion = this.LastSystemVersion
            };
            this.Dependency
                = job.ScheduleParallel(queryWithChunkComponent, 1, this.Dependency);
        }
    }
    #endregion

    //snippets
    public class ChunkComponentSnippets : SystemBase
    {
        protected override void OnUpdate()
        {
            //throw new System.NotImplementedException();
        }

        private void snippets()
        {
            #region component-list-chunk-component

            ComponentType[] compTypes = {
                ComponentType.ChunkComponent<ChunkComponentA>(),
                ComponentType.ReadOnly<GeneralPurposeComponentA>()
            };
            Entity entity = EntityManager.CreateEntity(compTypes);
            #endregion

            #region em-snippet
            //使用此方法时，不能立即为chunk component设置值
            EntityManager.AddChunkComponentData<ChunkComponentA>(entity);
            #endregion

            #region desc-chunk-component

            EntityQueryDesc ChunksWithoutComponentADesc
                = new EntityQueryDesc()
                {
                    None = new ComponentType[]{
                    ComponentType.ChunkComponent<ChunkComponentA>()
                }
                };
            EntityQuery ChunksWithoutChunkComponentA
                = GetEntityQuery(ChunksWithoutComponentADesc);

            //可以直接设置值
            EntityManager.AddChunkComponentData<ChunkComponentA>(
                ChunksWithoutChunkComponentA,
                new ChunkComponentA() { Value = 4 });
            #endregion

            #region use-chunk-component

            EntityQueryDesc ChunksWithChunkComponentADesc
                = new EntityQueryDesc()
                {
                    All = new ComponentType[] {
                    ComponentType.ChunkComponent<ChunkComponentA>()
                }
                };
            #endregion

            #region archetype-chunk-component
            //通过archetype 创建包含chunkCompoent的Entity，感觉跟通用组件差不多用法
            EntityArchetype ArchetypeWithChunkComponent
                = EntityManager.CreateArchetype(
                ComponentType.ChunkComponent(typeof(ChunkComponentA)),
                ComponentType.ReadWrite<GeneralPurposeComponentA>());
            Entity newEntity
                = EntityManager.CreateEntity(ArchetypeWithChunkComponent);
            #endregion
            {
                EntityQuery ChunksWithChunkComponentA = default;
                #region read-chunk-component
                //读取chunkComponent  EntityQuery.CreateArchetypeChunkArray
                NativeArray<ArchetypeChunk> chunks
                    = ChunksWithChunkComponentA.CreateArchetypeChunkArray(
                        Allocator.TempJob);

                foreach (var chunk in chunks)
                {
                    var compValue =
                     EntityManager.GetChunkComponentData<ChunkComponentA>(chunk);

                    //compValue.Value
                }
                chunks.Dispose();
                #endregion
            }

            #region read-entity-chunk-component

            if (EntityManager.HasChunkComponent<ChunkComponentA>(entity))
            {
                ChunkComponentA chunkComponentValue =
                 EntityManager.GetChunkComponentData<ChunkComponentA>(entity);
            }
            #endregion

            {
                ArchetypeChunk chunk = default;
                #region set-chunk-component

                EntityManager.SetChunkComponentData<ChunkComponentA>(
                    chunk, new ChunkComponentA() { Value = 7 });
                #endregion
            }

            #region set-entity-chunk-component

            var entityChunk = EntityManager.GetChunk(entity);
            EntityManager.SetChunkComponentData<ChunkComponentA>(
                entityChunk,
                new ChunkComponentA() { Value = 8 });
            #endregion
        }
    }

}
