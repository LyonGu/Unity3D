using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

[UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
public class WaypointBlobAssetConstructor : GameObjectConversionSystem {

    protected override void OnUpdate() {
        BlobAssetReference<WaypointBlobAsset> waypointBlobAssetReference;

        using (BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp)) {
            ref WaypointBlobAsset waypointBlobAsset = ref blobBuilder.ConstructRoot<WaypointBlobAsset>();
            BlobBuilderArray<Waypoint> waypointArray = blobBuilder.Allocate(ref waypointBlobAsset.waypointArray, 3);
            waypointArray[0] = new Waypoint { position = new float3(0, 0, 0) };
            waypointArray[1] = new Waypoint { position = new float3(5, 0, 0) };
            waypointArray[2] = new Waypoint { position = new float3(2.5f, 2.5f, 0) };

            waypointBlobAssetReference = 
                blobBuilder.CreateBlobAssetReference<WaypointBlobAsset>(Allocator.Persistent);

            Debug.Log(waypointBlobAssetReference.Value.waypointArray[1].position);
        }

        EntityQuery playerEntityQuery = DstEntityManager.CreateEntityQuery(typeof(Tag_Player));
        Entity playerEntity = playerEntityQuery.GetSingletonEntity();

        DstEntityManager.AddComponentData(playerEntity, new WaypointFollow {
            waypointBlobAssetRef = waypointBlobAssetReference,
            waypointIndex = 0,
        });
    }

}
