using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class PlayerAttackSystem : ComponentSystem {

    private float attackTimer;

    protected override void OnUpdate() {
        attackTimer -= Time.DeltaTime;
        if (attackTimer <= 0f) {
            // Attack!
            attackTimer = .6f;

            float3 playerPosition = float3.zero;

            EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            Entities.ForEach((DynamicBuffer<PlayerTargetElement> targetDynamicBuffer) => {
                for (int i = 0; i < targetDynamicBuffer.Length; i++) {
                    Entity targetEntity = targetDynamicBuffer[i].targetEntity;

                    if (targetEntity != Entity.Null && EntityManager.Exists(targetEntity)) {
                        // Has Target 
                        //GetComponentDataFromEntity 拿到所有Entity的Translation数据
                        ComponentDataFromEntity<Translation> translationComponentData = GetComponentDataFromEntity<Translation>(true);
                        float3 targetPosition = translationComponentData[targetEntity].Value;
                        
                        //创建一个子弹entity
                        Entity kunaiEntity = entityCommandBuffer.Instantiate(GameHandler.pfKunaiEntity);
                        float3 aimDirection = math.normalize(targetPosition - playerPosition);

                        entityCommandBuffer.SetComponent(kunaiEntity, new Translation { Value = playerPosition });
                        entityCommandBuffer.SetComponent(kunaiEntity, new Rotation { Value = quaternion.EulerXYZ(0, 0, GetAngleFromVector(aimDirection) - math.PI / 2f) });
                        entityCommandBuffer.SetComponent(kunaiEntity, new Kunai { targetPosition = targetPosition , e = kunaiEntity});
                    }
                }
            });
            
            //excute ecb
            entityCommandBuffer.Playback(EntityManager);
        }
    }
    
    private float GetAngleFromVector(float3 dir) {
        dir = math.normalize(dir);
        float n = math.atan2(dir.y, dir.x);
        return n;
    }


}
