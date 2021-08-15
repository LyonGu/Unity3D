using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class RemoveDeadSystem : ComponentSystem
{
	protected override void OnUpdate()
	{
		Entities.ForEach((Entity entity, ref Health health, ref Translation pos) =>
		{
			if (health.Value <= 0)
			{
				if (EntityManager.HasComponent(entity, typeof(PlayerTag)))
				{
					//玩家死亡
					Settings.PlayerDied();
				}

				else if (EntityManager.HasComponent(entity, typeof(EnemyTag)))
				{
					//敌人死亡  PostUpdateCommands其实也是一个ECB
					PostUpdateCommands.DestroyEntity(entity);
					
					//播放一个特效
					BulletImpactPool.PlayBulletImpact(pos.Value);
				}
			}
		});
	}
}