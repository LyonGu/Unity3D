using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

//移动system，向前方移动
namespace Unity.Transforms
{
	public class MoveForwardSystem : JobComponentSystem
	{
		[BurstCompile]
		[RequireComponentTag(typeof(MoveForward))]
		struct MoveForwardRotation : IJobForEach<Translation, Rotation, MoveSpeed>
		{
			public float dt;

			public void Execute(ref Translation pos, [ReadOnly] ref Rotation rot, [ReadOnly] ref MoveSpeed speed)
			{
				//math.forward(rot.Value) 往物体Z轴方向移动
				pos.Value = pos.Value + (dt * speed.Value * math.forward(rot.Value));
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			var moveForwardRotationJob = new MoveForwardRotation
			{
				dt = Time.deltaTime
			};

			return moveForwardRotationJob.Schedule(this, inputDeps);
		}
	}
}