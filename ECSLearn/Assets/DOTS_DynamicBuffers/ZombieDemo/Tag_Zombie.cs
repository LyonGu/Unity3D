using Unity.Entities;
using UnityEngine.Rendering;

[GenerateAuthoringComponent]
public struct Tag_Zombie : IComponentData { }


public struct ZombieEntityCom : IComponentData
{
    public Entity e;
}