using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class Sys_EntityQuery : JobComponentSystem
{

    private EntityQuery _entityQuery;

    protected override void OnCreate()
    {
        base.OnCreate();
        
        //创建筛选器
        //GetEntityQuery
        _entityQuery = GetEntityQuery(typeof(ComponentA), typeof(ComponentB));
        
        //使用EntityQueryDesc创建
        //ComponentType.ReadOnly代表这个组件筛选出来之后是只读的
        var query = new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(ComponentA), ComponentType.ReadOnly<ComponentB>() }
        };  
        var m_query1 = GetEntityQuery(query);
        
        //选出value = 1的那些共享组件的实体  SetSharedComponentFilter
        //SetSharedComponentFilter函数可以在任何时候调用，并不仅限于OnCreate函数，比如你可以在OnUpdate调用
        var m_query2 = GetEntityQuery(typeof(SharedComponentA));        
        m_query2.SetSharedComponentFilter(new SharedComponentA { value = 1 });//调用SetSharedComponentFilter函数指定更细致的筛选条件：SharedComponentA的num字段必须等于1
        
        //SetChangedVersionFilter
        //需要筛选同时包含ComponentA和ComponentB ComponentC的实体，并且，只有ComponentA的内容发生了改变的那些实体
        //"某个组件被修改过" ==> 该组件在其他System中被筛选了且被标记为读写
        var m_query3 = GetEntityQuery(typeof(ComponentA), typeof(ComponentB),typeof(ComponentC));
        m_query3.SetChangedVersionFilter(typeof(ComponentA));
        
        //Adds a query that must return entities for the system to run
        //给system添加一个筛选器
        RequireForUpdate(_entityQuery);

    }

    //IJobForEach 被废弃了
    struct EntityQueryJob : IJobForEach<ComponentA, ComponentB>
    {
        public void Execute(ref ComponentA c, ref ComponentB c1)
        {
            c.value += 1;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        return new EntityQueryJob().Schedule(_entityQuery, inputDeps);
    }
}

//namespace UseTest
//{
//    public struct C1 : IComponentData { }
//
//    [WriteGroup(typeof(C1))]
//    public struct C2 : IComponentData { }
//
//    [WriteGroup(typeof(C1))]
//    public struct C3 : IComponentData { }
//
//    public class ECSSystem : SystemBase
//    {
//        private EntityQuery query;
//        EntityManager entityManager;
//        protected override void OnCreate()
//        {
//            var queryDescription = new EntityQueryDesc
//            {
//                All = new ComponentType[] {
//                    ComponentType.ReadWrite<C3>() },
//                Options = EntityQueryOptions.FilterWriteGroup
//            };
//            query = GetEntityQuery(queryDescription);
//
//            //添加Entity测试
//            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
//            entityManager.CreateEntity(ComponentType.ReadWrite<C1>(), ComponentType.ReadWrite<C3>());
//            entityManager.CreateEntity(ComponentType.ReadWrite<C3>());
//        }
//        
//        struct EntityQueryJob1111 : IJobForEachWithEntity<C3>
//        {
//            public int frameCount;
//
//
//            public void Execute(Entity entity, int index, ref C3 c0)
//            {
//                Debug.Log($"EntityQueryJob1111===={frameCount} {index}");
//            }
//        }
//
//        protected override void OnUpdate()
//        {
//            var job = new EntityQueryJob1111();
//            job.frameCount = UnityEngine.Time.frameCount;
//            this.Dependency = job.Schedule(query, this.Dependency);
//
//        }
//    }
//
//}



