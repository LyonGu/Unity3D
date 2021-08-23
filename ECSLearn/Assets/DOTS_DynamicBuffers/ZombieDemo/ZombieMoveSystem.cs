using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class ZombieMoveSystem : ComponentSystem {
    
    protected override void OnUpdate() {
        Entities.WithAll<Tag_Zombie>().ForEach((ref Translation translation) => {
            float3 playerPosition = float3.zero;
            float3 moveDir = math.normalize(playerPosition - translation.Value);

            float moveSpeed = 1.8f;
            translation.Value += moveDir * moveSpeed * Time.DeltaTime;
        });
    }

}
