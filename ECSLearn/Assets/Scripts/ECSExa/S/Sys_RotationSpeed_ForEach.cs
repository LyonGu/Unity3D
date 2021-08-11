using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class Sys_RotationSpeed_ForEach : SystemBase
{
    protected override void OnUpdate()
    {
        var deltaTime = Time.DeltaTime;
        Entities.ForEach((ref Rotation rotation, in Com_RotationSpeed_ForEach rotationSpeed) =>
        {
            rotation.Value = math.mul(math.normalize(rotation.Value),
                quaternion.AxisAngle(math.up(), rotationSpeed.RadiansPerSecond * deltaTime));
        }).Schedule();
        
        // 如果使用Schedule， forEach里就不能使用Time.DeltaTime
    }
}
