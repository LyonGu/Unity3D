using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

//[DisableAutoCreation]
[UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
public class WaypointBlobAssetConstructor_Done : GameObjectConversionSystem {

    public static BlobAssetReference<WaypointBlobAsset_Done> blobAssetReference;

    protected override void OnUpdate() {
        using (BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp)) {
            ref WaypointBlobAsset_Done waypointBlobAsset = ref blobBuilder.ConstructRoot<WaypointBlobAsset_Done>();
            
            WaypointBlobAssetAuthoring_Done waypointAuthoring = GetEntityQuery(typeof(WaypointBlobAssetAuthoring_Done)).ToComponentArray<WaypointBlobAssetAuthoring_Done>()[0];

            BlobBuilderArray<Waypoint_Done> waypointBlobBuilderArray = blobBuilder.Allocate(ref waypointBlobAsset.waypointArray, waypointAuthoring.transformArray.Length);

            for (int i = 0; i < waypointAuthoring.transformArray.Length; i++) { 
                Transform waypointTransform = waypointAuthoring.transformArray[i];
                waypointBlobBuilderArray[i] = new Waypoint_Done { position = waypointTransform.position };
            }

            //blobBuilder.AllocateString(ref waypointBlobAsset.blobString, "Test String!");

            ref Waypoint_Done waypoint = ref blobBuilder.Allocate(ref waypointBlobAsset.singleWaypoint);
            waypoint = new Waypoint_Done { position = new float3(56, 0, 0) };

            blobAssetReference = blobBuilder.CreateBlobAssetReference<WaypointBlobAsset_Done>(Allocator.Persistent);
        }
    }

}
