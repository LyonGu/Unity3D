using Unity.Entities;

[GenerateAuthoringComponent]
public struct PlayerSingleTarget : IComponentData {

    public Entity targetEntity;

}
