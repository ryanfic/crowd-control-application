/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */
/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Jobs;

public class BirdInputSystem : JobComponentSystem {
    
    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        bool jumpInputDown = Input.GetKeyDown(KeyCode.Space);

        if (jumpInputDown) {
            if (HasSingleton<GameState>()) {
                GameState gameState = GetSingleton<GameState>();
                if (gameState.state == GameState.State.WaitingToStart) {
                    gameState.state = GameState.State.Playing;
                    SetSingleton(gameState);

                    World.GetExistingSystem<PipeHitSystem>().Enabled = true;
                    World.GetExistingSystem<PipeMoveSystem_Done>().Enabled = true;
                    World.GetExistingSystem<PipeDestroySystem>().Enabled = true;
                    //World.GetExistingSystem<PipeSpawnerSystem>().Enabled = true;
                    World.GetExistingSystem<BirdControlSystem>().Enabled = true;
                }
            }
        }

        return Entities.WithAll<Tag_Bird>().ForEach((ref MoveSpeed moveSpeed) => {
            if (jumpInputDown) {
                moveSpeed.moveDirSpeed.y = 8f;
            }
        }).Schedule(inputDeps);
    }
}
*/