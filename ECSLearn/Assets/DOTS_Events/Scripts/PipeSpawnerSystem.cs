/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Jobs;

public class PipeSpawnerSystem : ComponentSystem {

    private bool firstUpdate = true;
    private int spawnedPipeCount;
    private float pipeSpawnXPosition = 10f;
    private float pipeSpawnTimer;
    private Random random;
    private float randomGapHeightMin;
    private float randomGapHeightMax;
    private float randomGapSizeMin;
    private float randomGapSizeMax;

    protected override void OnCreate() {
        random = new Random(56);

        spawnedPipeCount = 0;
        randomGapHeightMin = 4.5f;
        randomGapHeightMax = 5.5f;
        randomGapSizeMin = 4f;
        randomGapSizeMax = 8f;
    }

    protected override void OnUpdate() {
        if (HasSingleton<GameState>()) {
            GameState gameState = GetSingleton<GameState>();

            if (gameState.state == GameState.State.Playing) {
                if (firstUpdate) {
                    firstUpdate = false;

                    //SpawnPipeGap(3f, 7f, 2f);
                    //SpawnPipe(0f, 2f, true);
                    //SpawnPipe(2f, 6f, false);
                    //SpawnPipe(4f, 3f);
                    //SpawnPipe(-2f, 10f);
                }

                pipeSpawnTimer -= Time.DeltaTime;
                if (pipeSpawnTimer <= 0f) {
                    float pipeSpawnMax = 1f;
                    pipeSpawnTimer = pipeSpawnMax;
                    spawnedPipeCount++;
                    TestPipeGapDifficulty();

                    float gapSize = random.NextFloat(randomGapSizeMin, randomGapSizeMax);
                    float gapHeight = random.NextFloat(randomGapHeightMin, randomGapHeightMax);

                    float totalWorldHeight = 10f;
                    if (gapHeight + gapSize / 2f >= totalWorldHeight) {
                        // Clipped on top
                        gapSize = totalWorldHeight - gapHeight - .1f;
                    }
                    if (gapHeight - gapSize / 2f <= 0f) {
                        // Clipped on bottom
                        gapSize = gapHeight - .1f;
                    }

                    SpawnPipeGap(pipeSpawnXPosition, gapHeight, gapSize);
                }
            }
        }
    }

    private void SpawnPipeGap(float xPosition, float gapHeight, float gapSize) {
        float totalWorldHeight = 10f;
        SpawnPipe(xPosition, gapHeight - gapSize / 2f, true);
        SpawnPipe(xPosition, totalWorldHeight - gapHeight - gapSize / 2f, false);
    }

    private void SpawnPipe(float xPosition, float height, bool isBottom) {
        Entity pfPipe = GetSingleton<PrefabEntityComponent>().prefabEntity;

        Entity pipeEntity = EntityManager.Instantiate(pfPipe);
        float pipeYPosition = isBottom ? -5f : +5f;
        EntityManager.SetComponentData(pipeEntity, new Translation {
            Value = new float3(xPosition, pipeYPosition, 0f)
        });

        EntityManager.SetComponentData(pipeEntity, new Pipe {
            isBottom = isBottom
        });

        DynamicBuffer<LinkedEntityGroup> childBuffer = EntityManager.GetBuffer<LinkedEntityGroup>(pipeEntity);

        Entity pipeHeadEntity = childBuffer[1].Value;
        Entity pipeBodyEntity = childBuffer[2].Value;

        if (isBottom) {
            // Bottom Pipe
            float headHeight = .5f;
            EntityManager.SetComponentData(pipeHeadEntity, new Translation {
                Value = new float3(0f, height - headHeight / 2f, -.1f)
            });

            EntityManager.SetComponentData(pipeBodyEntity, new Translation {
                Value = new float3(0f, height / 2f, .1f)
            });
        } else {
            // Top Pipe
            float headHeight = .5f;
            EntityManager.SetComponentData(pipeHeadEntity, new Translation {
                Value = new float3(0f, -height + headHeight / 2f, -.1f)
            });

            EntityManager.SetComponentData(pipeBodyEntity, new Translation {
                Value = new float3(0f, -height / 2f, .1f)
            });
        }


        float pipeWidth = .8f;
        EntityManager.SetComponentData(pipeBodyEntity, new NonUniformScale {
            Value = new float3(pipeWidth, height, 0f)
        });

        PhysicsCollider physicsCollider = EntityManager.GetComponentData<PhysicsCollider>(pipeEntity);

        float colliderOffsetHeight;
        if (isBottom) {
            colliderOffsetHeight = height / 2f;
        } else {
            colliderOffsetHeight = -height / 2f;
        }
        BlobAssetReference<Collider> collider = BoxCollider.Create(new BoxGeometry {
            Size = new float3(pipeWidth, height, 1f),
            Center = new float3(0f, colliderOffsetHeight, 0f),
            Orientation = quaternion.identity,
            BevelRadius = 0f
        }, physicsCollider.Value.Value.Filter, new Material { Flags = Material.MaterialFlags.IsTrigger });

        EntityManager.SetComponentData(pipeEntity, new PhysicsCollider {
            Value = collider,
        });
    }

    private void TestPipeGapDifficulty() {
        //UnityEngine.Debug.Log(spawnedPipeCount);

        switch (spawnedPipeCount) {
            case 20:
                randomGapHeightMin = 4.5f;
                randomGapHeightMax = 5.5f;
                randomGapSizeMin = 5f;
                randomGapSizeMax = 8f;
                break;
            case 40:
                randomGapHeightMin = 4.0f;
                randomGapHeightMax = 6.0f;
                randomGapSizeMin = 4f;
                randomGapSizeMax = 8f;
                break;
            case 60:
                randomGapHeightMin = 3.5f;
                randomGapHeightMax = 6.5f;
                randomGapSizeMin = 3.0f;
                randomGapSizeMax = 6f;
                break;
        }
    }

}