using Unity.Entities;

public struct WaypointFollow_Done : IComponentData {

    public BlobAssetReference<WaypointBlobAsset_Done> waypointBlobAssetReference;
    public int waypointIndex;

}
