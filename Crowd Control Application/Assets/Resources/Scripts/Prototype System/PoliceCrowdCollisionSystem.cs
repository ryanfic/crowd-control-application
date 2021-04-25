using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Jobs;
using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Physics;



[UpdateAfter(typeof(EndFramePhysicsSystem))]
public class PoliceCrowdCollisionSystem : JobComponentSystem
{
    private BuildPhysicsWorld m_BuildPhysicsWorldSystem;
    private StepPhysicsWorld m_StepPhysicsWorldSystem;

    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    private EntityQuery crowdGroup;

    protected override void OnCreate(){
        base.OnCreate();
        m_BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
        m_StepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        crowdGroup = GetEntityQuery(new EntityQueryDesc{
            All = new ComponentType[]{
                ComponentType.ReadOnly<Crowd>()
            }
        });
    }

    [BurstCompile]
    private struct CollisionEventCrowdJob : ICollisionEventsJob{
        [ReadOnly] public ComponentDataFromEntity<Crowd> crowdGroup;
        [ReadOnly] public ComponentDataFromEntity<PoliceOfficer> policeGroup;

        public EntityCommandBuffer commandBuffer;
        public void Execute(CollisionEvent collisionEvent){
            Entity entityA = collisionEvent.Entities.EntityA;
            Entity entityB = collisionEvent.Entities.EntityB;

            bool isBodyACrowd = crowdGroup.HasComponent(entityA);
            bool isBodyBCrowd = crowdGroup.HasComponent(entityB);

            bool isBodyAPolice = policeGroup.HasComponent(entityA);
            bool isBodyBPolice = policeGroup.HasComponent(entityB);

            if(isBodyACrowd && isBodyBPolice){
                commandBuffer.AddComponent<CrowdCollidedWithPolice>(entityA);
            }
            if(isBodyBCrowd && isBodyAPolice){
                commandBuffer.AddComponent<CrowdCollidedWithPolice>(entityB);
            }
            
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps){
        JobHandle jobHandle = new CollisionEventCrowdJob{
            crowdGroup = GetComponentDataFromEntity<Crowd>(true),
            policeGroup = GetComponentDataFromEntity<PoliceOfficer>(true),
            commandBuffer = commandBufferSystem.CreateCommandBuffer()
        }.Schedule(m_StepPhysicsWorldSystem.Simulation, ref m_BuildPhysicsWorldSystem.PhysicsWorld, inputDeps);

        commandBufferSystem.AddJobHandleForProducer(jobHandle); // tell the system to execute the command buffer after the job has been completed
        //jobHandle.Complete();
        return jobHandle;
    }

}
