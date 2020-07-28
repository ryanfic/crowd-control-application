/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

[DisableAutoCreation]
public class PipeMoveSystem_Empty : JobComponentSystem {

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        float deltaTime = Time.DeltaTime;
        double elapsedTime = Time.ElapsedTime;
        float3 moveDir = new float3(-1f, 0f, 0f);
        float moveSpeed = 4f;

        JobHandle jobHandle = Entities.ForEach((int entityInQueryIndex, ref Translation translation, ref Pipe pipe) => {
            float xBefore = translation.Value.x;
            translation.Value += moveDir * moveSpeed * deltaTime;
            float xAfter = translation.Value.x;

            if (pipe.isBottom && xBefore > 0 && xAfter <= 0) {
                // Passed the Player
            }
        }).Schedule(inputDeps);

        return jobHandle;
    }

}