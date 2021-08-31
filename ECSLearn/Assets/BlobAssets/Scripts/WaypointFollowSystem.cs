using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[DisableAutoCreation]
public class WaypointFollowSystem : JobComponentSystem {

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        float deltaTime = Time.DeltaTime;

        return Entities.WithAll<Tag_Player>().ForEach((ref WaypointFollow waypointFollow, ref Translation translation) => {
            ref WaypointBlobAsset waypointBlobAsset = ref waypointFollow.waypointBlobAssetRef.Value;
            float3 waypointPosition = waypointBlobAsset.waypointArray[waypointFollow.waypointIndex].position;

            float3 dirToWaypoint = math.normalizesafe(waypointPosition - translation.Value);
            float moveSpeed = 3f;
            translation.Value += dirToWaypoint * moveSpeed * deltaTime;

            float reachedWaypointDistance = .1f;
            if (math.distance(translation.Value, waypointPosition) < reachedWaypointDistance) {
                // Reached Waypoint
                waypointFollow.waypointIndex = (waypointFollow.waypointIndex + 1) % waypointBlobAsset.waypointArray.Length;
            }
        }).Schedule(inputDeps);
    }

}
