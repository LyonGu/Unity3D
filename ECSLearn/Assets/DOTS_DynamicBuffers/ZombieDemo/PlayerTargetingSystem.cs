using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

public class PlayerTargetingSystem : ComponentSystem {
    
    protected override void OnUpdate() {
        Entities.ForEach((ref Translation playerTranslation, DynamicBuffer<PlayerTargetElement> targetDynamicBuffer) => {
            // Get targets within range
            float3 playerPosition = playerTranslation.Value;

            targetDynamicBuffer.Clear();
            NativeList<Entity> zombieEntityList = new NativeList<Entity>(Allocator.Temp);

            Entities.WithAll<Tag_Zombie>().ForEach((Entity zombieEntity, ref Translation zombieTranslation) => {
                float targetRange = 12f;
                float zombieDistance = math.distance(playerPosition, zombieTranslation.Value);

                if (zombieDistance < targetRange) {
                    // Within range
                    zombieEntityList.Add(zombieEntity);
                }
            });

            foreach (Entity zombieEntity in zombieEntityList) {
                Entity targetEntity = zombieEntity;
                if (targetDynamicBuffer.Length < 5) {
                    targetDynamicBuffer.Add(new PlayerTargetElement { targetEntity = targetEntity });
                }
            }

            zombieEntityList.Dispose();
        });

        /*
        Entities.ForEach((ref PlayerSingleTarget playerSingleTarget, ref Translation playerTranslation) => {
            // Get closest target
            Entity closestTargetEntity = Entity.Null;
            float closestDistance = float.MaxValue;

            float3 playerPosition = playerTranslation.Value;

            Entities.WithAll<Tag_Zombie>().ForEach((Entity zombieEntity, ref Translation zombieTranslation) => {
                float targetRange = 12f;
                float zombieDistance = math.distance(playerPosition, zombieTranslation.Value);

                if (zombieDistance < targetRange) {
                    // Within range
                    if (closestTargetEntity == Entity.Null) {
                        closestTargetEntity = zombieEntity;
                        closestDistance = zombieDistance;
                    } else {
                        if (zombieDistance < closestDistance) {
                            // Closer
                            closestTargetEntity = zombieEntity;
                            closestDistance = zombieDistance;
                        }
                    }
                }
            });

            playerSingleTarget.targetEntity = closestTargetEntity;
        });
        */
    }

}
