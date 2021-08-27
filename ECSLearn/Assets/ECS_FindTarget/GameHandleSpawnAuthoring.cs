
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

public class GameHandleSpawnAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {

    public static GameHandleSpawnAuthoring Instance { get; private set; }
    

    public static Entity pfTargetEntity;
    public static Entity pfUnityEntity;

    public GameObject targetPrefab;
    public GameObject unitPrefab;

    

    private void Awake() {
        Instance = this;
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        pfTargetEntity = conversionSystem.GetPrimaryEntity(targetPrefab);
        dstManager.AddComponentData(pfTargetEntity, new Target());
        dstManager.AddComponentData(pfTargetEntity, new Scale { Value = 0.5f });
        dstManager.AddComponentData(pfTargetEntity, new TargetSelf { self = pfTargetEntity });
        dstManager.AddComponentData(pfTargetEntity, new TargetOrigin());
        
        pfUnityEntity = conversionSystem.GetPrimaryEntity(unitPrefab);
        dstManager.AddComponentData(pfUnityEntity, new Unit());
        dstManager.AddComponentData(pfUnityEntity, new Scale { Value = 1.5f });
        dstManager.AddComponentData(pfUnityEntity, new UnitSelf { self = pfUnityEntity});
        dstManager.AddComponentData(pfUnityEntity, new UnitOrigin());
        
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs) {
        referencedPrefabs.Add(targetPrefab);
        referencedPrefabs.Add(unitPrefab);
    }

}

