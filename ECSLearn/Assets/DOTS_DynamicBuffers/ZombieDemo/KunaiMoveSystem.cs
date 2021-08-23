using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class KunaiMoveSystem : ComponentSystem {

    protected override void OnUpdate() {
        Entities.ForEach((Entity kunaiEntity, ref Translation kunaiTranslation, ref Kunai kunai) => {
            // Move towards target distance
            float3 moveDir = math.normalize(kunai.targetPosition - kunaiTranslation.Value);
            float moveSpeed = 20f;
            kunaiTranslation.Value += moveDir * moveSpeed * Time.DeltaTime;

            float3 kunaiTranslationValue = kunaiTranslation.Value;

            // Check if any targets 遍历所有zombieEntity，跟kunaiEntity距离比较，进行伤害判定
            Entities.ForEach((Entity zombieEntity, ref Translation zombieTranslation, ref ZombieHealth zombieHealth) => {
                float attackDistance = 1f;
                if (math.distancesq(kunaiTranslationValue, zombieTranslation.Value) < attackDistance * attackDistance) {
                    // Attack!
                    zombieHealth.Value--;
                    PostUpdateCommands.DestroyEntity(kunaiEntity);

                    if (zombieHealth.Value <= 0) {
                        // Zombie dead
                        PostUpdateCommands.DestroyEntity(zombieEntity);
                    }
                }
            });

            float destroyDistance = 1f;
            if (math.distancesq(kunaiTranslationValue, kunai.targetPosition) < destroyDistance*destroyDistance) {
                PostUpdateCommands.DestroyEntity(kunaiEntity);
            }
        });
    }

}
