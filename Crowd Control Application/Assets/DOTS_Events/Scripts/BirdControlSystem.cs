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
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

[UpdateAfter(typeof(BirdInputSystem))]
public class BirdControlSystem : JobComponentSystem {

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        if (HasSingleton<GameState>() && GetSingleton<GameState>().state == GameState.State.Playing) {
            float deltaTime = Time.DeltaTime;
            return Entities.WithAll<Tag_Bird>().ForEach((ref MoveSpeed moveSpeed, ref Translation translation, ref Rotation rotation) => {
                float gravity = -35f;
                moveSpeed.moveDirSpeed.y += gravity * deltaTime;
                translation.Value += moveSpeed.moveDirSpeed * deltaTime;

                float rotationDampen = 30f;
                rotation.Value = quaternion.Euler(0, 0, moveSpeed.moveDirSpeed.y / rotationDampen);
            }).Schedule(inputDeps);
        } else {
            return default;
        }
    }

}
*/