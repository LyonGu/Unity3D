using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Entities;
using UnityEngine;

public class SpawnerSystem_FromEntityByComandBuffer : SystemBase
{
    private BeginInitializationEntityCommandBufferSystem _beginInitializationEntityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        _beginInitializationEntityCommandBufferSystem =
            World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var commandBuffer = _beginInitializationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        Debug.Log($"SpawnerSystem_FromEntityByComandBuffer OnUpdate ====    {UnityEngine.Time.frameCount}");
        
//        Debug.Log($"SpawnerSystem_FromEntityByComandBuffer OnUpdate ManagedThreadId ====    {Thread.CurrentThread.ManagedThreadId}");
        //这句调完之后好像是给system加上了对应的筛选条件，当对应的筛序条件不满足，连OnUpdate都不会被调用
        Entities.ForEach((Entity entity, int entityInQueryIndex, in SpawnerFromEntity spawnerFromEntity) =>
        {

            //job内的代码就算加入到EBS中，逻辑也还是在子线程中执行，先加入到队列里，最后会回到主线程操作
//            Debug.Log($"SpawnerSystem_FromEntityByComandBuffer OnUpdate ManagedThreadId ====    {Thread.CurrentThread.ManagedThreadId}"
            /* 在Buffer中创建实体 */
            var instance = commandBuffer.Instantiate(entityInQueryIndex, spawnerFromEntity.prefab);
            commandBuffer.AddComponent(entityInQueryIndex, instance, new SpawnerFromCommandBuffer());
            //将筛选出来的实体删除了 下一帧就不会调用OnUpdate了  
            Debug.Log($"Spawner_FromEntity Convert1 ====  {entity.Index}  {spawnerFromEntity.prefab.Index}");
            commandBuffer.DestroyEntity(entityInQueryIndex, spawnerFromEntity.prefab);
            commandBuffer.DestroyEntity(entityInQueryIndex, entity);

        }).Schedule();
        
        /* 把Job添加到EntityCommandBufferSystem */ 
        //EntityCommandBufferSystem每次执行队列的任务后，都会清空，所以不用担心
        //最后Entity的产生还是在主线程
        _beginInitializationEntityCommandBufferSystem.AddJobHandleForProducer(this.Dependency);
    }
}

public struct SpawnerFromEntity:IComponentData
{
    public Entity prefab;
}

public struct SpawnerFromCommandBuffer:IComponentData { }


