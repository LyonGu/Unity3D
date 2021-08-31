using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

//[DisableAutoCreation]
public class EntitySpawner_Done : JobComponentSystem {

    private int spawnCount = 80;
    private float spawnTimer;
    private float spawnTimerMax = .15f;

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        if (spawnCount > 0) {
            spawnTimer -= Time.DeltaTime;

            if (spawnTimer <= 0f) {
                spawnTimer += spawnTimerMax;
                spawnCount--;
                PrefabEntityHolder prefabEntityHolder = GetSingleton<PrefabEntityHolder>();

                Entity spawnedEntity = EntityManager.Instantiate(prefabEntityHolder.entityPrefab);

                EntityManager.AddComponentData(spawnedEntity, new WaypointFollow_Done {
                    waypointBlobAssetReference = WaypointBlobAssetConstructor_Done.blobAssetReference,
                    waypointIndex = 0,
                });
            }
        }

        return default;
    }

}
