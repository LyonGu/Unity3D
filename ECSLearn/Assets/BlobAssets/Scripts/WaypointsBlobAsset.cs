using Unity.Entities;
using Unity.Mathematics;


public struct Waypoint {
    public float3 position;
}

public struct WaypointBlobAsset {
    public BlobArray<Waypoint> waypointArray;
}