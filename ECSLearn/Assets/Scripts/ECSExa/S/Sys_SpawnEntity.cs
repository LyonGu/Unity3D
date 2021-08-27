using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class Sys_SpawnEntity : SystemBase
{
    private Entity _entity;
    protected override void OnCreate()
    {
        base.OnCreate();
        
//        //通过archetype 创建Entity
//        EntityArchetype archetype = EntityManager.CreateArchetype(typeof(ComponentA), typeof(ComponentB));
//        EntityManager.CreateEntity(archetype,1);
//        
//        //通过Component组合 创建Entity
//        EntityManager.CreateEntity(typeof(ComponentA), ComponentType.ReadOnly<ComponentB>());
//        
//        //先创建Entity，再加component
        var e = EntityManager.CreateEntity();
//        ComponentTypes types = new ComponentTypes(typeof(ComponentA), ComponentType.ReadOnly<ComponentB>());
//        EntityManager.AddComponents(e,types);
        EntityManager.AddComponent(e, typeof(ComponentA));
        EntityManager.AddComponent(e, typeof(ComponentB));
        EntityManager.AddComponent(e, ComponentType.ReadOnly<ComponentC>());

        _entity = e;
//        
//        //EntityManager.Instantiate一个Entity
//        EntityManager.Instantiate(e);
//        
//        //主动创建一个system
//        Hxp.DissableAutoCreate.Test.Sys_DissableAutoSystem s = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<Hxp.DissableAutoCreate.Test.Sys_DissableAutoSystem>();
//        //还可以主动添加到个group里
////        SimulationSystemGroup simulationSystemGroup = World.GetOrCreateSystem<SimulationSystemGroup>();
////        simulationSystemGroup.AddSystemToUpdateList(s);
//
//        

    }

    private float time = 0.5f;
    private bool isCheck = true;
    protected override void OnUpdate()
    {
//        //OnUpdate默认每帧都会调用
//        Debug.Log($"Sys_SpawnEntity OnUpdate==========={UnityEngine.Time.frameCount}");
//
//        if (isCheck)
//        {
//            time -= UnityEngine.Time.deltaTime;
//            if (time <= 0)
//            {
//                isCheck = false;
//                Debug.Log($"Sys_SpawnEntity DestroyEntity entity==========={UnityEngine.Time.frameCount}");
//                EntityManager.DestroyEntity(_entity);
//            }
//        }
//
//        // 一旦使用了Entities.ForEach，会把相关query注册给system，如果对应的Enity被destroy了，Onupdate就不会调用了
//        Entities.ForEach((ref ComponentA componentA,ref ComponentB componentB,ref ComponentC componentC) =>
//        {
//            
//        }).Run();




    }
}
