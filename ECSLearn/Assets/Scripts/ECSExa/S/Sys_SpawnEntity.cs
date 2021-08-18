using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class Sys_SpawnEntity : SystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();
        
        //通过archetype 创建Entity
        EntityArchetype archetype = EntityManager.CreateArchetype(typeof(ComponentA), typeof(ComponentB));
        EntityManager.CreateEntity(archetype,1);
        
        //通过Component组合 创建Entity
        EntityManager.CreateEntity(typeof(ComponentA), ComponentType.ReadOnly<ComponentB>());
        
        //先创建Entity，再加component
        var e = EntityManager.CreateEntity();
//        ComponentTypes types = new ComponentTypes(typeof(ComponentA), ComponentType.ReadOnly<ComponentB>());
//        EntityManager.AddComponents(e,types);
        EntityManager.AddComponent(e, typeof(ComponentA));
        EntityManager.AddComponent(e, typeof(ComponentB));
        EntityManager.AddComponent(e, ComponentType.ReadOnly<ComponentC>());
        
        //EntityManager.Instantiate一个Entity
        EntityManager.Instantiate(e);

    }

    protected override void OnUpdate()
    {
        
        
    }
}
