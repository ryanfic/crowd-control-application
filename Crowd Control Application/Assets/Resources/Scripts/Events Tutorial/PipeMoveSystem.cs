using System;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine;

//Using DOTSEvents generic class
public class PipeMoveSystem : JobComponentSystem {

    public event EventHandler OnPipePassed;

    private DOTSEvents_NextFrame<PipePassedEvent> dotsEvents;

    public struct PipePassedEvent : IComponentData {
        public double ElapsedTime; // to test that it is on the same frame
    }

    protected override void OnCreate(){
        dotsEvents = new DOTSEvents_NextFrame<PipePassedEvent>(World);
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps){
        float deltaTime = Time.DeltaTime;
        float3 moveDir = new float3(-1f, 0f, 0f); // direction that pipes move
        float moveSpeed = 4f; // how fast the pipes move

        DOTSEvents_NextFrame<PipePassedEvent>.EventTrigger eventTrigger = dotsEvents.GetEventTrigger();

        double elapsedTime = Time.ElapsedTime;

        JobHandle jobHandle = Entities.ForEach((int entityInQueryIndex, ref Translation trans, ref Pipe pipe) =>{
            //Move the pipe
            float xBefore = trans.Value.x; // get where the pipe was
            trans.Value += moveDir * moveSpeed * deltaTime;
            float xAfter = trans.Value.x;

            //Check if the pipe moved past the player (the event)
            if(pipe.isBottom && xBefore > 0 && xAfter <= 0){ //only check on one of each of the pairs of pipes
                // Passed the player
                eventTrigger.TriggerEvent(entityInQueryIndex, new PipePassedEvent {ElapsedTime = elapsedTime});
            }
        }).Schedule(inputDeps);

        dotsEvents.CaptureEvents(jobHandle, (PipePassedEvent pipeEvent) =>{
            Debug.Log("In Component: " + pipeEvent.ElapsedTime + ", In OnUpdate: " + elapsedTime);
            OnPipePassed?.Invoke(this, EventArgs.Empty);
        });

        return jobHandle;
    }
}

// Do not pause the software to capture the event, but capture the event on the next frame
/*public class PipeMoveSystem : JobComponentSystem {

    public event EventHandler OnPipePassed;

    public struct EventComponent : IComponentData {
        public double ElapsedTime; // to test that it is on the same frame
    }

    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

    protected override void OnCreate(){
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps){
        float deltaTime = Time.DeltaTime;
        float3 moveDir = new float3(-1f, 0f, 0f); // direction that pipes move
        float moveSpeed = 4f; // how fast the pipes move

        EntityCommandBuffer entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer();
        EntityCommandBuffer.Concurrent  entityCommandBufferConcurrent = entityCommandBuffer.ToConcurrent();
        EntityArchetype eventEntityArchetype = EntityManager.CreateArchetype(typeof(EventComponent));

        double elapsedTime = Time.ElapsedTime;

        JobHandle jobHandle = Entities.ForEach((int entityInQueryIndex, ref Translation trans, ref Pipe pipe) =>{
            //Move the pipe
            float xBefore = trans.Value.x; // get where the pipe was
            trans.Value += moveDir * moveSpeed * deltaTime;
            float xAfter = trans.Value.x;

            //Check if the pipe moved past the player (the event)
            if(pipe.isBottom && xBefore > 0 && xAfter <= 0){ //only check on one of each of the pairs of pipes
                // Passed the player
                Entity eventEntity = entityCommandBufferConcurrent.CreateEntity(entityInQueryIndex, eventEntityArchetype);
                entityCommandBufferConcurrent.SetComponent(entityInQueryIndex, eventEntity, new EventComponent{
                    ElapsedTime = elapsedTime
                });
            }
        }).Schedule(inputDeps);

        endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

        EntityCommandBuffer captureEventsEntityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer();

        Entities.WithoutBurst().ForEach((Entity entity, ref EventComponent eventComponent) => {
            Debug.Log("In Component: " + eventComponent.ElapsedTime + ", In OnUpdate: " + elapsedTime);
            OnPipePassed?.Invoke(this, EventArgs.Empty);
            captureEventsEntityCommandBuffer.DestroyEntity(entity);
        }).Run();

        return jobHandle;
    }
}*/

// Capture the event on the exact same frame as when the event happens, but cause the software to pause to do so
/*public class PipeMoveSystem : JobComponentSystem {

    public event EventHandler OnPipePassed;

    public struct EventComponent : IComponentData {
        public double ElapsedTime; // to test that it is on the same frame
    }

    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

    protected override void OnCreate(){
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps){
        float deltaTime = Time.DeltaTime;
        float3 moveDir = new float3(-1f, 0f, 0f); // direction that pipes move
        float moveSpeed = 4f; // how fast the pipes move

        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);
        EntityCommandBuffer.Concurrent  entityCommandBufferConcurrent = entityCommandBuffer.ToConcurrent();
        EntityArchetype eventEntityArchetype = EntityManager.CreateArchetype(typeof(EventComponent));

        double elapsedTime = Time.ElapsedTime;

        JobHandle jobHandle = Entities.ForEach((int entityInQueryIndex, ref Translation trans, ref Pipe pipe) =>{
            //Move the pipe
            float xBefore = trans.Value.x; // get where the pipe was
            trans.Value += moveDir * moveSpeed * deltaTime;
            float xAfter = trans.Value.x;

            //Check if the pipe moved past the player (the event)
            if(pipe.isBottom && xBefore > 0 && xAfter <= 0){ //only check on one of each of the pairs of pipes
                // Passed the player
                Entity eventEntity = entityCommandBufferConcurrent.CreateEntity(entityInQueryIndex, eventEntityArchetype);
                entityCommandBufferConcurrent.SetComponent(entityInQueryIndex, eventEntity, new EventComponent{
                    ElapsedTime = elapsedTime
                });
            }
        }).Schedule(inputDeps);

        jobHandle.Complete();
        entityCommandBuffer.Playback(EntityManager);
        entityCommandBuffer.Dispose();


        Entities.WithoutBurst().ForEach((ref EventComponent eventComponent) => {
            Debug.Log("In Component: " + eventComponent.ElapsedTime + ", In OnUpdate: " + elapsedTime);
            OnPipePassed?.Invoke(this, EventArgs.Empty);
        }).Run();

        EntityManager.DestroyEntity(GetEntityQuery(typeof(EventComponent)));

        return jobHandle;
    }
}*/

// Other method using NativeQueue (Bad because it uses .Complete())
/*public class PipeMoveSystem : JobComponentSystem {
    
    public event EventHandler OnPipePassed;

    public struct PipePassedEvent{
    }
    private NativeQueue<PipePassedEvent> eventQueue;

    protected override void OnCreate(){
        eventQueue = new NativeQueue<PipePassedEvent>(Allocator.Persistent);
    }

    protected override void OnDestroy(){
        eventQueue.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps){
        float deltaTime = Time.DeltaTime;
        float3 moveDir = new float3(-1f, 0f, 0f); // direction that pipes move
        float moveSpeed = 4f; // how fast the pipes move

        NativeQueue<PipePassedEvent>.ParallelWriter eventQueueParallel = eventQueue.AsParallelWriter();

        JobHandle jobHandle = Entities.ForEach((int entityInQueryIndex, ref Translation trans, ref Pipe pipe) =>{
            //Move the pipe
            float xBefore = trans.Value.x; // get where the pipe was
            trans.Value += moveDir * moveSpeed * deltaTime;
            float xAfter = trans.Value.x;

            //Check if the pipe moved past the player (the event)
            if(pipe.isBottom && xBefore > 0 && xAfter <= 0){ //only check on one of each of the pairs of pipes
                // Passed the player
                eventQueueParallel.Enqueue(new PipePassedEvent {});
            }
        }).Schedule(inputDeps);

        jobHandle.Complete();

        while(eventQueue.TryDequeue(out PipePassedEvent pipePassedEvent)){
            OnPipePassed?.Invoke(this,EventArgs.Empty);
        }

        return jobHandle;
    }
}*/
