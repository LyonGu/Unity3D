using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


public class Sys_RotationSpeed_ForEach : SystemBase
{
    protected override void OnUpdate()
    {
        var deltaTime = UnityEngine.Time.deltaTime;
        var frameCount = UnityEngine.Time.frameCount;

        //Rotation是系统自带的组件，我们的Cube在转换为实体时，也会自动附加Rotation组件
        //jobHandle 可以作为其他job的依赖项
        this.Dependency = Entities.ForEach((ref Rotation rotation, in Com_RotationSpeed_ForEach rotationSpeed) =>
        {
            rotation.Value = math.mul(math.normalize(rotation.Value),
                quaternion.AxisAngle(math.up(), rotationSpeed.RadiansPerSecond * deltaTime));
        }).Schedule(this.Dependency); //多线程
        
        
        //OnUpdate 每帧调用一次，就算有多个满足的Entity也只调用一次，如果没有筛序到对应的Entity，就不会调用
        //lamda表达式 有几个满足筛选条件的Entity就调用几次
        
        
//        Entities.ForEach((Entity entity, int entityInQueryIndex, ref Translation translation
//            ) =>
//            {
//                Debug.Log($"Sys_RotationSpeed_ForEach========OnUpdate2  {frameCount}  {entity.Index}");
//            }).Run();

//        Entities.ForEach((Entity entity, int entityInQueryIndex, ref Translation translation
//        ) =>
//        {
//            Debug.Log($"Sys_RotationSpeed_ForEach========OnUpdate2  {frameCount}  {entity.Index}");
//        }).Schedule();

        
        


//        Entities.ForEach((ref Rotation rotation, in Com_RotationSpeed_ForEach rotationSpeed) =>
//        {
//            var deltaTime = Time.DeltaTime;
//            rotation.Value = math.mul(math.normalize(rotation.Value),
//                quaternion.AxisAngle(math.up(), rotationSpeed.RadiansPerSecond * deltaTime));
//        }).WithoutBurst().Run();  //在主线程上


        // 如果使用Schedule， forEach里就不能使用Time.DeltaTime
    }
}


