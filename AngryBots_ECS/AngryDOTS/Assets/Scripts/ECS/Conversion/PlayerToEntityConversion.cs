using Unity.Entities;
using UnityEngine;

public class PlayerToEntityConversion : MonoBehaviour, IConvertGameObjectToEntity
{
	public float healthValue = 1f;


	public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
	{
		manager.AddComponent(entity, typeof(PlayerTag));

		Health health = new Health { Value = healthValue };
		manager.AddComponentData(entity, health);
		

		/*
		 *当前GameObject上绑定了ConvertToEntity脚本，把这个GameObject转成Entity时
		 *如果当前GameObject上的其他脚本中有个prefab的序列化字段，引用的prefab刚好也挂了一个继承IConvertGameObjectToEntity的脚本A
		 *同时也会执行脚本A的Convert方法
		 */
	}
}