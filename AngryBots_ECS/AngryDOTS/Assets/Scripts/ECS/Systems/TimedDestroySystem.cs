using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;


[UpdateAfter(typeof(MoveForwardSystem))]
public class TimedDestroySystem : JobComponentSystem
{
	//拿到已经存在的EBS
	EndSimulationEntityCommandBufferSystem buffer;

	protected override void OnCreateManager()
	{
		buffer = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}

	//只关心有TimeToLive组件的entity
	[BurstCompile]
	struct CullingJob : IJobForEachWithEntity<TimeToLive>
	{
		public EntityCommandBuffer.Concurrent commands;
		public float dt;

		public void Execute(Entity entity, int jobIndex, ref TimeToLive timeToLive)
		{
			//代码逻辑在子线程里
			timeToLive.Value -= dt;
			if (timeToLive.Value <= 0f)
				commands.DestroyEntity(jobIndex, entity); //销毁Entity的最终命令会在主线程里执行
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var job = new CullingJob
		{
			commands = buffer.CreateCommandBuffer().ToConcurrent(), //创建一个ECB
			dt = Time.deltaTime
		};

		var handle = job.Schedule(this, inputDeps);
		buffer.AddJobHandleForProducer(handle);

		return handle;
	}
}

