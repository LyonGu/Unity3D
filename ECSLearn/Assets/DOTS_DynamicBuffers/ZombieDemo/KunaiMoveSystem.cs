using System.ComponentModel;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

//public class KunaiMoveSystem : ComponentSystem {
//
//    protected override void OnUpdate() {
//        Entities.ForEach((Entity kunaiEntity, ref Translation kunaiTranslation, ref Kunai kunai) => {
//            // Move towards target distance
//            float3 moveDir = math.normalize(kunai.targetPosition - kunaiTranslation.Value);
//            float moveSpeed = 20f;
//            kunaiTranslation.Value += moveDir * moveSpeed * Time.DeltaTime;
//
//            float3 kunaiTranslationValue = kunaiTranslation.Value;
//
//            // Check if any targets 遍历所有zombieEntity，跟kunaiEntity距离比较，进行伤害判定
//            Entities.ForEach((Entity zombieEntity, ref Translation zombieTranslation, ref ZombieHealth zombieHealth) => {
//                float attackDistance = 1f;
//                if (math.distancesq(kunaiTranslationValue, zombieTranslation.Value) < attackDistance * attackDistance) {
//                    // Attack!
//                    zombieHealth.Value--;
//                    PostUpdateCommands.DestroyEntity(kunaiEntity);
//
//                    if (zombieHealth.Value <= 0) {
//                        // Zombie dead
//                        PostUpdateCommands.DestroyEntity(zombieEntity);
//                    }
//                }
//            });
//
//            float destroyDistance = 1f;
//            if (math.distancesq(kunaiTranslationValue, kunai.targetPosition) < destroyDistance*destroyDistance) {
//                PostUpdateCommands.DestroyEntity(kunaiEntity);
//            }
//        });
//    }
//
//}

// 使用SystemBase
//public class KunaiMoveSystem_EX : SystemBase
//{
//    private EntityQuery _entityQuery;
//    EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;
//    protected override void OnCreate()
//    {
//        base.OnCreate();
//        _entityQuery = GetEntityQuery(typeof(Translation), typeof(Tag_Zombie), typeof(ZombieHealth));

//        m_EndSimulationEcbSystem = World
//            .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
//    }


//    protected override void OnUpdate()
//    {


//        //拿到所有Zombie的位置信息
//        int count = _entityQuery.CalculateEntityCount();
//        NativeList<Entity> zEntitys = new NativeList<Entity>(count, Allocator.Temp);
//        NativeList<Translation> zTranslations = new NativeList<Translation>(count, Allocator.Temp);
//        NativeList<ZombieHealth> zZombieHealths = new NativeList<ZombieHealth>(count, Allocator.Temp);

//        Entities
//            //.WithBurst()
//            .ForEach((Entity e, int entityInQueryIndex, ref Translation translation, ref ZombieHealth zombieHealth,
//            in Tag_Zombie tag_Zombie) =>
//        {
//            zEntitys.Add(e);
//            zTranslations.Add(translation);
//            zZombieHealths.Add(zombieHealth);
//        }).Run();

//        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
//        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
//        float DeltaTime = Time.DeltaTime;
//        Entities
//            //.WithBurst()
//            .ForEach((Entity kunaiEntity, ref Translation kunaiTranslation, ref Kunai kunai) =>
//        {
//            // Move towards target distance
//            float3 moveDir = math.normalize(kunai.targetPosition - kunaiTranslation.Value);
//            float moveSpeed = 20f;
//            kunaiTranslation.Value += moveDir * moveSpeed * DeltaTime;

//            float attackDistance = 1f;
//            float3 kunaiTranslationValue = kunaiTranslation.Value;

//            int len = zTranslations.Length;
//            for (int i = 0; i < len; i++)
//            {
//                var zombieTranslation = zTranslations[i];
//                var zombieHealth = zZombieHealths[i];
//                if (math.distancesq(kunaiTranslationValue, zombieTranslation.Value) < attackDistance * attackDistance)
//                {

//                    // Attack!
//                    zombieHealth.Value--;
//                    var zEntity = zEntitys[i];
//                    bool existZ = entityManager.Exists(zEntity);
//                    if (existZ)
//                    {
//                        ecb.SetComponent(zEntity, new ZombieHealth()
//                        {
//                            Value = zombieHealth.Value
//                        });
//                    }

//                    bool exists1 = entityManager.Exists(kunaiEntity);
//                    if (exists1)
//                        ecb.DestroyEntity(kunaiEntity);
//                    //                    EntityManager.DestroyEntity(kunaiEntity);

//                    if (zombieHealth.Value <= 0)
//                    {
//                        // Zombie dead
//                        if (existZ)
//                            ecb.DestroyEntity(zEntity);
//                        //                        EntityManager.DestroyEntity(zEntitys[i]);
//                    }
//                    //                    break;
//                }
//            }

//        }).Run();
//        ecb.Playback(entityManager);
//        ecb.Dispose();
//        zEntitys.Dispose();
//        zTranslations.Dispose();
//        zZombieHealths.Dispose();
//    }

//}

// 使用多线程优化 
public class KunaiMoveSystem_JobEX : SystemBase
{
    private EntityQuery Zquery;
    private EntityQuery Kquery;

    private EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;
    protected override void OnCreate()
    {
        base.OnCreate();
        Zquery = GetEntityQuery(typeof(Translation), typeof(Tag_Zombie), typeof(ZombieHealth));
        Kquery = GetEntityQuery(typeof(Translation), typeof(Kunai));

        m_EndSimulationEcbSystem = World
            .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    struct KunaiData
    {
        public Entity e;
        public Translation pos;
        public int entityInQueryIndex;
    }
    protected override void OnUpdate()
    {


        EntityCommandBuffer.ParallelWriter commandBuffer
            = m_EndSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

        int count = Kquery.CalculateEntityCount();
        NativeArray<KunaiData> kunaiDataArray = new NativeArray<KunaiData>(count, Allocator.TempJob);
        float deltaTime = Time.DeltaTime;
        JobHandle MoveJobHandle = Entities
            //            .WithBurst(FloatMode.Default, FloatPrecision.Standard, false)
            .ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, in Kunai kunai) =>
            {
                float3 moveDir = math.normalize(kunai.targetPosition - translation.Value);
                float moveSpeed = 20f;
                translation.Value += moveDir * moveSpeed * deltaTime;
                translation = new Translation()
                {
                    Value = translation.Value
                };
                kunaiDataArray[entityInQueryIndex] = new KunaiData()
                {
                    e = entity,
                    pos = translation,
                    entityInQueryIndex = entityInQueryIndex
                };
            })
            .WithName("MoveKunais")
            .ScheduleParallel(this.Dependency); //开多个子线程

        this.Dependency = Entities
            //            .WithBurst(FloatMode.Default, FloatPrecision.Standard, false)  默认就这样
            .ForEach((Entity entity, int entityInQueryIndex, ref ZombieHealth zombieHealth, in Translation translation, in Tag_Zombie tag_Zombie) =>
            {
                int count = kunaiDataArray.Length;
                float attackDistance = 1f;
                for (int i = 0; i < count; i++)
                {
                    var kunaiEntity = kunaiDataArray[i].e;
                    var kunaiPosition = kunaiDataArray[i].pos;
                    var kunaiEntityInQueryIndex = kunaiDataArray[i].entityInQueryIndex;
                    if (math.distancesq(kunaiPosition.Value, translation.Value) < attackDistance * attackDistance)
                    {
                        zombieHealth.Value--;
                        commandBuffer.DestroyEntity(kunaiEntityInQueryIndex, kunaiEntity);
                        if (zombieHealth.Value <= 0)
                        {
                            commandBuffer.DestroyEntity(entityInQueryIndex, entity);
                        }
                    }

                }
            })
            .WithName("CheckDisAndDestroyEntities")
            .WithDisposeOnCompletion(kunaiDataArray)
            .WithReadOnly(kunaiDataArray)
            .ScheduleParallel(MoveJobHandle);  //这个可以开启多个线程
                                               //        this.Dependency
                                               //            = JobHandle.CombineDependencies(MoveJobHandle, checkJobHandle);
        m_EndSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);

    }
}




//// 使用多线程job 并行优化 
//public class KunaiMoveSystem_JobEX1 : SystemBase
//{
//    private EntityQuery Zquery;
//    private EntityQuery Kquery;

//    private EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;
//    protected override void OnCreate()
//    {
//        base.OnCreate();
//        Zquery = GetEntityQuery(typeof(Translation), typeof(Tag_Zombie), typeof(ZombieHealth));
//        Kquery = GetEntityQuery(typeof(Translation), typeof(Kunai));

//        m_EndSimulationEcbSystem = World
//            .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
//    }

//    struct KuaiData
//    {
//        public Entity e;
//        public Translation pos;
//        public int entityInQueryIndex;
//    }

//    struct MoveJob : IJobEntityBatchWithIndex
//    {
//        //[DeallocateOnJobCompletion] 
//        public NativeArray<KuaiData> OutputArray;
//        public float deltaTime;

//        public float frameCount;

//        public ComponentTypeHandle<Translation> PositionTypeHandleAccessor;

//        [Unity.Collections.ReadOnly] public ComponentTypeHandle<Kunai> KunaiTypeHandleAccessor;




//        public void Execute(ArchetypeChunk batchInChunk, int chunkIndex, int firstEntityIndex)
//        {
//            NativeArray<Translation> positions = batchInChunk.GetNativeArray<Translation>(PositionTypeHandleAccessor);
//            NativeArray<Kunai> kunais = batchInChunk.GetNativeArray<Kunai>(KunaiTypeHandleAccessor);

//            for (int i = 0; i < positions.Length; i++)
//            {
//                var kunai = kunais[i];
//                var kunaiTranslation = positions[i];
//                float3 moveDir = math.normalize(kunai.targetPosition - kunaiTranslation.Value);
//                float moveSpeed = 20f;
//                kunaiTranslation.Value += moveDir * moveSpeed * deltaTime;
//                positions[i] = new Translation()
//                {
//                    Value = kunaiTranslation.Value
//                };
//                int index = i + firstEntityIndex; //batchIndex从0开始
//                                                  //                Debug.Log(
//                                                  //                    $"Kquery index=============={index}  {chunkIndex}  {firstEntityIndex} {OutputArray.Length} frameCount = {frameCount}");
//                OutputArray[index] = new KuaiData()
//                {
//                    entityInQueryIndex = index,
//                    pos = kunaiTranslation,
//                    e = kunai.e
//                };
//            }
//        }

//    }

//    struct CheckJob : IJobEntityBatchWithIndex
//    {
//        [DeallocateOnJobCompletion]
//        [Unity.Collections.ReadOnly]
//        public NativeArray<KuaiData> KuaiDatas;

//        public EntityCommandBuffer.ParallelWriter commandBufferCreate;

//        [Unity.Collections.ReadOnly]
//        public ComponentTypeHandle<Translation> PositionTypeHandleAccessor;

//        [Unity.Collections.ReadOnly]
//        public ComponentTypeHandle<ZombieEntityCom> ZombieEntityComTypeHandleAccessor;

//        public ComponentTypeHandle<ZombieHealth> ZombieHealthTypeHandleAccessor;

//        public void Execute(ArchetypeChunk batchInChunk, int chunkIndex, int firstEntityIndex)
//        {
//            NativeArray<Translation> positions = batchInChunk.GetNativeArray<Translation>(PositionTypeHandleAccessor);
//            NativeArray<ZombieEntityCom> zombieEntityComs = batchInChunk.GetNativeArray<ZombieEntityCom>(ZombieEntityComTypeHandleAccessor);
//            NativeArray<ZombieHealth> zombieHealths = batchInChunk.GetNativeArray<ZombieHealth>(ZombieHealthTypeHandleAccessor);

//            float attackDistance = 1f;

//            int count = KuaiDatas.Length;

//            for (int i = 0; i < count; i++)
//            {
//                var kuaiE = KuaiDatas[i].e;
//                var kuaiPos = KuaiDatas[i].pos;
//                var kuaientityInQueryIndex = KuaiDatas[i].entityInQueryIndex;

//                int ZCount = positions.Length;
//                for (int j = 0; j < ZCount; j++)
//                {
//                    var zombieTranslation = positions[j];
//                    var zombieHealth = zombieHealths[j];
//                    if (math.distancesq(kuaiPos.Value, zombieTranslation.Value) < attackDistance * attackDistance)
//                    {
//                        zombieHealth.Value--;
//                        zombieHealths[j] = new ZombieHealth()
//                        {
//                            Value = zombieHealth.Value
//                        };
//                        commandBufferCreate.DestroyEntity(kuaientityInQueryIndex, kuaiE);
//                        if (zombieHealth.Value <= 0)
//                        {
//                            int zombieIndex = firstEntityIndex + j;
//                            var zombieEntity = zombieEntityComs[j].e;
//                            commandBufferCreate.DestroyEntity(zombieIndex, zombieEntity);
//                        }
//                    }
//                }
//            }
//        }
//    }


//    protected override void OnUpdate()
//    {
//        int count = Kquery.CalculateEntityCount();
//        int frameCount = UnityEngine.Time.frameCount;
//        //        Debug.Log($"Kquery.CalculateEntityCount =============={count}  frameCount = {frameCount}");
//        NativeArray<KuaiData> KuaiDatas = new NativeArray<KuaiData>(count, Allocator.TempJob);
//        //move job
//        float deltaTime = Time.DeltaTime;
//        var movejob = new MoveJob()
//        {
//            PositionTypeHandleAccessor = this.GetComponentTypeHandle<Translation>(false),
//            KunaiTypeHandleAccessor = this.GetComponentTypeHandle<Kunai>(true),
//            deltaTime = deltaTime,
//            OutputArray = KuaiDatas,
//            frameCount = frameCount

//        };

//        JobHandle moveJobHandle = movejob.ScheduleParallel(Kquery, 1, this.Dependency);


//        EntityCommandBuffer.ParallelWriter commandBufferCreate
//            = m_EndSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();
//        var checkJob = new CheckJob()
//        {
//            PositionTypeHandleAccessor = this.GetComponentTypeHandle<Translation>(true),
//            ZombieEntityComTypeHandleAccessor = this.GetComponentTypeHandle<ZombieEntityCom>(true),
//            ZombieHealthTypeHandleAccessor = this.GetComponentTypeHandle<ZombieHealth>(),
//            commandBufferCreate = commandBufferCreate,
//            KuaiDatas = KuaiDatas
//        };
//        //check Job
//        JobHandle checkJobHandle = checkJob.ScheduleParallel(Zquery,1, moveJobHandle);
//        this.Dependency = JobHandle.CombineDependencies(moveJobHandle, checkJobHandle);
//        m_EndSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);


//    }
//}