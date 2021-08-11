using System.Collections;
using System.Collections.Generic;
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

        Entities.ForEach((Entity entity, int entityInQueryIndex, in SpawnerFromEntity spawnerFromEntity) =>
        {

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


