using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Samples.FixedTimestepSystem
{
    public struct VariableRateSpawner : IComponentData
    {
        public Entity Prefab;
        public float3 SpawnPos;
    }

    public partial class VariableRateSpawnerSystem : SystemBase
    {
        private BeginSimulationEntityCommandBufferSystem ecbSystem;
        protected override void OnCreate()
        {
            ecbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            //只要对应的entity在，每一帧都会执行
            float spawnTime = (float)Time.ElapsedTime; //Time.ElapsedTime 从world创建开始到现在的时间
            var ecb = ecbSystem.CreateCommandBuffer();
            Entities
                .WithName("VariableRateSpawner")
                .ForEach((in VariableRateSpawner spawner) =>
                {
                    //代码逻辑在子线程执行，但是最后的创建entity会回到主线程
                    var projectileEntity = ecb.Instantiate(spawner.Prefab);
                    var spawnPos = spawner.SpawnPos;
                    spawnPos.y += 0.3f * math.sin(5.0f * spawnTime);
                    ecb.SetComponent(projectileEntity, new Translation {Value = spawnPos});
                    ecb.SetComponent(projectileEntity, new Projectile
                    {
                        SpawnTime = spawnTime,
                        SpawnPos = spawnPos,
                    });
                }).Schedule();
            ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
