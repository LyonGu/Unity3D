using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DOTS_EntityPrefabs.Scripts
{
    public class EntitySpawnerSystem : ComponentSystem {

        private float spawnTimer;
        private Random random;

        protected override void OnCreate() {
            random = new Random(56);
        }

        protected override void OnUpdate() {
            spawnTimer -= Time.DeltaTime;

            if (spawnTimer <= 0f) {
                spawnTimer = .5f;
                // Spawn!
                PrefabEntityComponent prefabEntityComponent = GetSingleton<PrefabEntityComponent>();

                Entity spawnedEntity = EntityManager.Instantiate(prefabEntityComponent.prefabEntity);

                EntityManager.SetComponentData(spawnedEntity, 
                    new Translation { Value = new float3(random.NextFloat(-5f, 5f), random.NextFloat(-5f, 5f), 0) }
                );

                /*
            Entities.ForEach((ref PrefabEntityComponent prefabEntityComponent) => {
                Entity spawnedEntity = EntityManager.Instantiate(prefabEntityComponent.prefabEntity);

                EntityManager.SetComponentData(spawnedEntity, 
                    new Translation { Value = new float3(random.NextFloat(-5f, 5f), random.NextFloat(-5f, 5f), 0) }
                );
            });
            */

                /*
            Entity spawnedEntity = EntityManager.Instantiate(PrefabEntities_V2.prefabEntity);

            EntityManager.SetComponentData(spawnedEntity, 
                new Translation { Value = new float3(random.NextFloat(-5f, 5f), random.NextFloat(-5f, 5f), 0) }
            );
            */
            }
        }

    }
}
