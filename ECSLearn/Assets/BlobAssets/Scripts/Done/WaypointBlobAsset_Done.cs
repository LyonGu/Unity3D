using Unity.Entities;
using Unity.Mathematics;


public struct Waypoint_Done {

    public float3 position;

}

public struct WaypointBlobAsset_Done {

    public BlobArray<Waypoint_Done> waypointArray;
    public BlobPtr<Waypoint_Done> singleWaypoint;
    public BlobString blobString;

}
