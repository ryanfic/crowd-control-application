/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Mathematics;
using Unity.Collections;

public class PipeHitSystem : JobComponentSystem {

    private struct PipeTrigger : ITriggerEventsJob {

        [ReadOnly] public ComponentDataFromEntity<Pipe> tagPipeComponentDataFromEntity;
        [ReadOnly] public ComponentDataFromEntity<Tag_Wall> tagWallComponentDataFromEntity;
        [ReadOnly] public ComponentDataFromEntity<Tag_Bird> tagBirdComponentDataFromEntity;
        public EntityCommandBuffer entityCommandBuffer;

        public void Execute(TriggerEvent triggerEvent) {
            Entity entityA = triggerEvent.Entities.EntityA;
            Entity entityB = triggerEvent.Entities.EntityB;

            Entity birdEntity = Entity.Null;
            Entity pipeEntity = Entity.Null;
            Entity wallEntity = Entity.Null;

            if (tagBirdComponentDataFromEntity.HasComponent(entityA)) birdEntity = entityA;
            if (tagBirdComponentDataFromEntity.HasComponent(entityB)) birdEntity = entityB;

            if (tagPipeComponentDataFromEntity.HasComponent(entityA)) pipeEntity = entityA;
            if (tagPipeComponentDataFromEntity.HasComponent(entityB)) pipeEntity = entityB;

            if (tagWallComponentDataFromEntity.HasComponent(entityA)) wallEntity = entityA;
            if (tagWallComponentDataFromEntity.HasComponent(entityB)) wallEntity = entityB;

            if ((birdEntity != Entity.Null && pipeEntity != Entity.Null) ||
                (birdEntity != Entity.Null && wallEntity != Entity.Null)) {
                // Collision between Bird and Pipe or Bird and Wall
                entityCommandBuffer.AddComponent(birdEntity, new Tag_GameOver());
            }

            //UnityEngine.Debug.Log(entityA + " " + entityB);
        }

    }

    private struct PipeCollision : ICollisionEventsJob {

        public void Execute(CollisionEvent collisionEvent) {
            UnityEngine.Debug.Log("Collision: " );
        }
    }

    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

    protected override void OnCreate() {
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        EntityCommandBuffer entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer();

        JobHandle jobHandle = new PipeTrigger {
            tagBirdComponentDataFromEntity = GetComponentDataFromEntity<Tag_Bird>(),
            tagPipeComponentDataFromEntity = GetComponentDataFromEntity<Pipe>(),
            tagWallComponentDataFromEntity = GetComponentDataFromEntity<Tag_Wall>(),
            entityCommandBuffer = entityCommandBuffer,
        }.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, inputDeps);

        endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

        /*I just noticed I had a system where I was using a command buffer to add a component inside a job and I forgot to call AddJobHandleForProducer(); but everything still worked correctly.
Is that call no longer needed? Or did I just get lucky with the order of the Systems? Does it depend on which buffer system I'm using, in this case EndSimulationEntityCommandBufferSystem?
         * */

        /*
        Entities.WithAll<Tag_GameOver>().ForEach((Entity entity) => {
            UnityEngine.Debug.Log("Game Over!");
        }).Run();
        //*/

        return jobHandle;
    }

}
