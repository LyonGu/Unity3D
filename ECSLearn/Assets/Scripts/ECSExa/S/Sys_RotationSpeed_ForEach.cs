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
        var deltaTime = Time.DeltaTime;
        //Rotation是系统自带的组件，我们的Cube在转换为实体时，也会自动附加Rotation组件
        //jobHandle 可以作为其他job的依赖项
        Entities.ForEach((ref Rotation rotation, in Com_RotationSpeed_ForEach rotationSpeed) =>
        {
            rotation.Value = math.mul(math.normalize(rotation.Value),
                quaternion.AxisAngle(math.up(), rotationSpeed.RadiansPerSecond * deltaTime));
        }).Schedule(); //多线程
        
        
        
        

//        Entities.ForEach((ref Rotation rotation, in Com_RotationSpeed_ForEach rotationSpeed) =>
//        {
//            var deltaTime = Time.DeltaTime;
//            rotation.Value = math.mul(math.normalize(rotation.Value),
//                quaternion.AxisAngle(math.up(), rotationSpeed.RadiansPerSecond * deltaTime));
//        }).WithoutBurst().Run();  //在主线程上

        
        // 如果使用Schedule， forEach里就不能使用Time.DeltaTime
    }
}


