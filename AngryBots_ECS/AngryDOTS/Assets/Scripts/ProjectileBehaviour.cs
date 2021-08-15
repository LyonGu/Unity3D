using Unity.Entities;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ProjectileBehaviour : MonoBehaviour, IConvertGameObjectToEntity
{
	[Header("Movement")]
	public float speed = 50f;

	[Header("Life Settings")]
	public float lifeTime = 2f;

	Rigidbody projectileRigidbody;


	void Start()
	{
		projectileRigidbody = GetComponent<Rigidbody>();
		Invoke("RemoveProjectile", lifeTime);
	}

	void Update()
	{
		Vector3 movement = transform.forward * speed * Time.deltaTime;
		projectileRigidbody.MovePosition(transform.position + movement);
	}

	void OnTriggerEnter(Collider theCollider)
	{

		if (theCollider.CompareTag("Enemy") || theCollider.CompareTag("Environment"))
			RemoveProjectile();
	}

	void RemoveProjectile()
	{
		Destroy(gameObject);
	}

	public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
	{
		
		//这个脚本挂在子弹的prefab上，子弹的prefab又绑定到了Player的脚本上
		//Player转成Entity时，子弹prefab上的脚本都会被调用，这个脚本继承了IConvertGameObjectToEntity，就会调用Convert方法
		//相当于直接把子弹的GameObject转成Entity了，并且加上了一些组件：MoveForward MoveSpeed TimeToLive
		//MoveForwardSystem里会改变子弹的位置
		manager.AddComponent(entity, typeof(MoveForward));

		MoveSpeed moveSpeed = new MoveSpeed { Value = speed };		
		manager.AddComponentData(entity, moveSpeed);

		TimeToLive timeToLive = new TimeToLive { Value = lifeTime };
		manager.AddComponentData(entity, timeToLive);
	}
}
