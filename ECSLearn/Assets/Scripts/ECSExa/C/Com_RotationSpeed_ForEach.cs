using System.Collections;
using System.Collections.Generic;
using Unity.Entities;

using UnityEngine;

[GenerateAuthoringComponent] //可以直接拖到gameobject上
public struct Com_RotationSpeed_ForEach : IComponentData
{
    public float RadiansPerSecond;
}


public struct RotationSpeed_IJobChunk : IComponentData
{
    public float RadiansPerSecond;
}