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

public class PipeMoveSystem_Done : JobComponentSystem {

    public event EventHandler OnPipePassed;

    private DOTSEvents_NextFrame<PipePassedEvent> dotsEvents;
    public struct PipePassedEvent : IComponentData {
        public double Value;
    }

    protected override void OnCreate() {
        dotsEvents = new DOTSEvents_NextFrame<PipePassedEvent>(World);
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        float deltaTime = Time.DeltaTime;
        double elapsedTime = Time.ElapsedTime;
        float3 moveDir = new float3(-1f, 0f, 0f);
        float moveSpeed = 4f;

        DOTSEvents_NextFrame<PipePassedEvent>.EventTrigger eventTrigger = dotsEvents.GetEventTrigger();

        JobHandle jobHandle = Entities.ForEach((int entityInQueryIndex, ref Translation translation, ref Pipe pipe) => {
            float xBefore = translation.Value.x;
            translation.Value += moveDir * moveSpeed * deltaTime;
            float xAfter = translation.Value.x;

            if (pipe.isBottom && xBefore > 0 && xAfter <= 0) {
                // Passed the Player
                eventTrigger.TriggerEvent(entityInQueryIndex, new PipePassedEvent { Value = elapsedTime });
            }
        }).Schedule(inputDeps);

        dotsEvents.CaptureEvents(jobHandle, (PipePassedEvent basicEvent) => {
            Debug.Log(basicEvent.Value + " ###### " + elapsedTime);
            OnPipePassed?.Invoke(this, EventArgs.Empty);
        });
        return jobHandle;
    }


}