using Unity.Entities;
using Unity.Transforms;

[UpdateBefore(typeof(CollisionSystem))]
public class PlayerTransformUpdateSystem : ComponentSystem
{
	protected override void OnUpdate()
	{
		if (Settings.IsPlayerDead())
			return;
		//玩家移动
		Entities.WithAll<PlayerTag>().ForEach((ref Translation pos) =>
		{
			pos = new Translation { Value = Settings.PlayerPosition };
		});
	}
}