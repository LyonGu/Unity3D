using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class Sys_RotationSpeedAuthoring_IJobChunk : MonoBehaviour,IConvertGameObjectToEntity
{
    // Start is called before the first frame update
    public float DegreesPerSecond = 360.0F; 
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var data = new RotationSpeed_IJobChunk { RadiansPerSecond = math.radians(DegreesPerSecond) };
        dstManager.AddComponentData(entity, data);
    }
}
