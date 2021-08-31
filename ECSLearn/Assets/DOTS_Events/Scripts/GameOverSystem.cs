/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Entities;

public class GameOverSystem : JobComponentSystem {

    public event System.EventHandler OnGameOver;

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        Entities.WithAll<Tag_GameOver>().WithoutBurst().WithStructuralChanges().ForEach((Entity entity) => {
            EntityManager.RemoveComponent<Tag_GameOver>(entity);
            GameOver();
        }).Run();
        return default;
    }

    private void GameOver() {
        //UnityEngine.Debug.Log("Game Over!");
        World.GetExistingSystem<PipeHitSystem>().Enabled = false;
        World.GetExistingSystem<PipeMoveSystem_Done>().Enabled = false;
        World.GetExistingSystem<PipeDestroySystem>().Enabled = false;
        World.GetExistingSystem<PipeSpawnerSystem>().Enabled = false;
        World.GetExistingSystem<BirdControlSystem>().Enabled = false;

        if (HasSingleton<GameState>()) {
            GameState gameState = GetSingleton<GameState>();
            gameState.state = GameState.State.Dead;
            SetSingleton(gameState);
        }

        //OnGameOver?.Invoke(this, System.EventArgs.Empty);
    }

}
