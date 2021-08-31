using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

public class WaypointFollowSystem_Done : JobComponentSystem {

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        float deltaTime = Time.DeltaTime;

        return Entities.WithAll<Tag_Player>().ForEach((ref Translation translation, ref WaypointFollow_Done waypointFollow) => {
            float3 waypointPosition = waypointFollow.waypointBlobAssetReference.Value.waypointArray[waypointFollow.waypointIndex].position;
            float3 moveDir = math.normalizesafe(waypointPosition - translation.Value);
            float moveSpeed = 3f;
            translation.Value += moveSpeed * moveDir * deltaTime;

            if (math.distance(waypointPosition, translation.Value) < .1f) {
                waypointFollow.waypointIndex = (waypointFollow.waypointIndex + 1) % waypointFollow.waypointBlobAssetReference.Value.waypointArray.Length;
            }
        }).Schedule(inputDeps);
    }

}
