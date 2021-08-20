using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

// 作用于命名空间下，Hxp.DissableAutoCreate.Test下所有system都不会自动运行了
[assembly: DisableAutoCreation]
namespace Hxp.DissableAutoCreate.Test
{
   
    //[DisableAutoCreation]  如果只想针对某个特定的system，就直接在classs上方添加属性
    public class Sys_DissableAutoSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            Debug.Log("Sys_DissableAutoSystem=========");
        }

        protected override void OnUpdate()
        {
            // Assign values to local variables captured in your job here, so that it has
            // everything it needs to do its work when it runs later.
            // For example,
            //     float deltaTime = Time.DeltaTime;

            // This declares a new kind of job, which is a unit of work to do.
            // The job is declared as an Entities.ForEach with the target components as parameters,
            // meaning it will process all entities in the world that have both
            // Translation and Rotation components. Change it to process the component
            // types you want.
        
        
        
//        Entities.ForEach((ref Translation translation, in Rotation rotation) => {
//            // Implement the work to perform for each entity here.
//            // You should only access data that is local or that is a
//            // field on this job. Note that the 'rotation' parameter is
//            // marked as 'in', which means it cannot be modified,
//            // but allows this job to run in parallel with other jobs
//            // that want to read Rotation component data.
//            // For example,
//            //     translation.Value += math.mul(rotation.Value, new float3(0, 0, 1)) * deltaTime;
//        }).Schedule();
        }
    }
    public class Sys_DissableAutoSystem1 : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            Debug.Log("Sys_DissableAutoSystem1=========");
        }

        protected override void OnUpdate()
        {
            // Assign values to local variables captured in your job here, so that it has
            // everything it needs to do its work when it runs later.
            // For example,
            //     float deltaTime = Time.DeltaTime;

            // This declares a new kind of job, which is a unit of work to do.
            // The job is declared as an Entities.ForEach with the target components as parameters,
            // meaning it will process all entities in the world that have both
            // Translation and Rotation components. Change it to process the component
            // types you want.
        
        
        
//        Entities.ForEach((ref Translation translation, in Rotation rotation) => {
//            // Implement the work to perform for each entity here.
//            // You should only access data that is local or that is a
//            // field on this job. Note that the 'rotation' parameter is
//            // marked as 'in', which means it cannot be modified,
//            // but allows this job to run in parallel with other jobs
//            // that want to read Rotation component data.
//            // For example,
//            //     translation.Value += math.mul(rotation.Value, new float3(0, 0, 1)) * deltaTime;
//        }).Schedule();
        }
    }
}
