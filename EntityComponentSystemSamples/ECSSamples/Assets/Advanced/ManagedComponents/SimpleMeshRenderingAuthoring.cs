// NOTE: This is a per project ifdfef,
//       the samples in this project are run in both modes for testing purposes.
//       In a normal game project this ifdef is not required.

#if !UNITY_DISABLE_MANAGED_COMPONENTS
using System;
using Unity.Entities;
using UnityEngine;
using Unity.Transforms;

[ConverterVersion("joe", 2)]
class SimpleMeshRenderingAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public Mesh Mesh = null;
    public Color Color = Color.white;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        //放在subscene里，GameObject不用ConvertToEntity脚本就能直接被调用
        // Assets in subscenes can either be created during conversion and embedded in the scene
        var material = new Material(Shader.Find("Standard"));
        material.color = Color;
        // ... Or be an asset that is being referenced.

        dstManager.AddComponentData(entity, new SimpleMeshRenderer
        {
            Mesh = Mesh,
            Material = material,
        });
       
        dstManager.AddSharedComponentData(entity, new SimpleMeshRendererEx
        {
            Mesh = Mesh,
            Material = material,
        });
    }
}

public class SimpleMeshRenderer : IComponentData
{
    public Mesh     Mesh;
    public Material Material;
}

//为什么不用Shared component Data
//Shared component Data 中如果有引用类型，需要实现Equals接口和GetHashCode接口
public struct SimpleMeshRendererEx : ISharedComponentData, IEquatable<SimpleMeshRendererEx>
{
    public Mesh     Mesh;
    public Material Material;
    public bool Equals(SimpleMeshRendererEx other)
    {
        if (Material == null)
            return false;
        int otherId = other.Material.GetInstanceID();
        int id = this.Material.GetInstanceID();
        return otherId == id;
    }
    
    public override int GetHashCode()
    {
        if (Material == null)
            return 0;
        return Material.GetInstanceID();
    }
}

[ExecuteAlways]
[AlwaysUpdateSystem]
[UpdateInGroup(typeof(PresentationSystemGroup))]
class SimpleMeshRendererSystem : ComponentSystem
{
    override protected void OnUpdate()
    {
        Entities.ForEach((SimpleMeshRenderer renderer, ref LocalToWorld localToWorld) =>
        {
            Graphics.DrawMesh(renderer.Mesh, localToWorld.Value, renderer.Material, 0);
        });
    }
}

#endif
