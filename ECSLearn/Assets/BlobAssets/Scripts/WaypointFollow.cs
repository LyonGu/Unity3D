using Unity.Entities;

public struct WaypointFollow : IComponentData {

    public BlobAssetReference<WaypointBlobAsset> waypointBlobAssetRef;
    public int waypointIndex;

}
