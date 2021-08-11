using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


public class Sys_SpawnerAuthoring_FromEntity : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    // Start is called before the first frame update
    public GameObject prefab;
    
    //DeclareReferencedPrefabs函数是做什么用呢？是为了让GameObjectConversionSystem对象知道我们的Prefab预制体的存在，以便通过预制体创建实体。
    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(prefab);
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var cEn = conversionSystem.GetPrimaryEntity(prefab); //将GameObject对象转换为Entity对象  这一步会产生一个Entity对象
        Debug.Log($"Spawner_FromEntity Convert1 ====  {cEn.Index}");
        var spawnerData = new SpawnerFromEntity
        {
            // The referenced prefab will be converted due to DeclareReferencedPrefabs.
            // So here we simply map the game object to an entity reference to that prefab.
            prefab = cEn, 
        };
        Debug.Log($"Spawner_FromEntity Convert2 ====  {entity.Index}");
        dstManager.AddComponentData(entity, spawnerData);
    }
}
