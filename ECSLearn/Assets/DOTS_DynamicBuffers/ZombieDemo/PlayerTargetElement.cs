using Unity.Entities;

[InternalBufferCapacity(500)]
public struct PlayerTargetElement : IBufferElementData {

    public Entity targetEntity;

}
