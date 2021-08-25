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
            zombieSpawnTimer = 0.03f;
            SpawnZombie();
        }
    }

    private void SpawnZombie() {
        if(GameHandler.pfZombieEntity == Entity.Null)
            return;
        Entity zombieEntity = EntityManager.Instantiate(GameHandler.pfZombieEntity);
        EntityManager.SetComponentData(zombieEntity, new Translation { Value = GetRandomDir() * random.NextFloat(12f, 15f) });
        EntityManager.AddComponentData(zombieEntity, new ZombieEntityCom { e = zombieEntity });
    }

    private float3 GetRandomDir() {
        float3 dir = new float3(random.NextFloat(-1f, 1f), random.NextFloat(-1f, 1f), 0f);
        return math.normalize(dir);
    }

}
