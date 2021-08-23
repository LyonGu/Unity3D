using Unity.Entities;

[InternalBufferCapacity(5)]
public struct PlayerTargetElement : IBufferElementData {

    public Entity targetEntity;

}
