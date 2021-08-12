using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class Sys_LifeTimeSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem m_EntityCommandBufferSystem;
    protected override void OnCreate()
    {
        base.OnCreate();
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }
    
    protected override void OnUpdate()
    {
        float delatTime = UnityEngine.Time.deltaTime;
        var ecb = m_EntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        Entities.ForEach((Entity entity, int entityInQueryIndex, ref Com_LifeTimeComponent lifeTimeComponent) =>
        {
            
            lifeTimeComponent.LifeTime -=delatTime;
            if (lifeTimeComponent.LifeTime <= 0)
            {
                //job内的代码就算加入到EBS中，逻辑也还是在子线程中执行，先加入到队列里，最后会回到主线程操作
                ecb.DestroyEntity(entityInQueryIndex,entity);
            }
        }).ScheduleParallel();
        
        //把Job添加到m_EntityCommandBufferSystem里
        m_EntityCommandBufferSystem.AddJobHandleForProducer(this.Dependency);
    }
}
