using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class GameHandler : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {

    public static GameHandler Instance { get; private set; }
    

    public static Entity pfZombieEntity;
    public static Entity pfKunaiEntity;

    public GameObject pfZombie;
    public GameObject pfKunai;


    private void Awake() {
        Instance = this;
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        pfZombieEntity = conversionSystem.GetPrimaryEntity(pfZombie);
        pfKunaiEntity = conversionSystem.GetPrimaryEntity(pfKunai);
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs) {
        referencedPrefabs.Add(pfZombie);
        referencedPrefabs.Add(pfKunai);
    }

}
