using Unity.Entities;

[GenerateAuthoringComponent]
public struct PrefabEntityHolder : IComponentData {

    public Entity entityPrefab;

}
