using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Samples.FixedTimestepSystem
{
    public struct FixedRateSpawner : IComponentData
    {
        public Entity Prefab;
        public float3 SpawnPos;
    }

    // This system is virtually identical to VariableRateSpawner; the key difference is that it updates in the
    // FixedStepSimulationSystemGroup instead of the default SimulationSystemGroup.
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class FixedRateSpawnerSystem : SystemBase
    {
        private EndFixedStepSimulationEntityCommandBufferSystem ecbSystem;
        protected override void OnCreate()
        {
            //EndFixedStepSimulationEntityCommandBufferSystem 是属于FixedStepSimulationSystemGroup里的
            ecbSystem = World.GetExistingSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            float spawnTime = (float)Time.ElapsedTime;
            var ecb = ecbSystem.CreateCommandBuffer();
            Entities
                .WithName("FixedRateSpawner")
                .ForEach((in FixedRateSpawner spawner) =>
                {
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
