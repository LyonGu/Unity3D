using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


public struct ComponentA : IComponentData
{
    public int value;
}

public struct ComponentB : IComponentData
{
    public int value;
}

public struct ComponentC : IComponentData
{
    public int value;
}

public struct SharedComponentA : ISharedComponentData
{
    public int value;
}