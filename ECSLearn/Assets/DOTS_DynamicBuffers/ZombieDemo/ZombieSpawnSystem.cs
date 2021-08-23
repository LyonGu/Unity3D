using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class ZombieSpawnSystem : ComponentSystem {

    private Entity pfZombieEntity;

    private float zombieSpawnTimer;
    private Unity.Mathematics.Random random;

    protected override void OnCreate() {
        random = new Unity.Mathematics.Random(56);
    }

    protected override void OnUpdate() {
        zombieSpawnTimer -= Time.DeltaTime;
        if (zombieSpawnTimer <= 0f) {
            // Spawn Zombie
            zombieSpawnTimer = .3f;
            SpawnZombie();
        }
    }

    private void SpawnZombie() {
        Entity zombieEntity = EntityManager.Instantiate(GameHandler.pfZombieEntity);
        EntityManager.SetComponentData(zombieEntity, new Translation { Value = GetRandomDir() * random.NextFloat(12f, 15f) });
    }

    private float3 GetRandomDir() {
        float3 dir = new float3(random.NextFloat(-1f, 1f), random.NextFloat(-1f, 1f), 0f);
        return math.normalize(dir);
    }

}
