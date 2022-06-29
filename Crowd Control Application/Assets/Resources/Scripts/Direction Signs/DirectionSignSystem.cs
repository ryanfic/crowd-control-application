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
public class DirectionSignSystem : JobComponentSystem
{
    private BuildPhysicsWorld m_BuildPhysicsWorldSystem;
    private StepPhysicsWorld m_StepPhysicsWorldSystem;

    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    private EntityQuery crowdGroup;

    private Unity.Mathematics.Random r;

    protected override void OnCreate()
    {
        base.OnCreate();

        r = new Unity.Mathematics.Random(19);

        m_BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
        m_StepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        crowdGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[]{
                ComponentType.ReadOnly<Crowd>()
            }
        });
    }

    [BurstCompile]
    private struct CollisionEventCrowdJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentDataFromEntity<Crowd> crowdGroup;
        [ReadOnly] public ComponentDataFromEntity<DirectionSignTrigger> triggerGroup;
        [ReadOnly] public ComponentDataFromEntity<SawDirectionSign> seenGroup;

        public EntityCommandBuffer commandBuffer;
        public Unity.Mathematics.Random r;
        public void Execute(TriggerEvent collisionEvent)
        {
            Entity entityA = collisionEvent.EntityA;
            Entity entityB = collisionEvent.EntityB;

            bool isBodyACrowd = crowdGroup.HasComponent(entityA); // HasComponent
            bool isBodyBCrowd = crowdGroup.HasComponent(entityB);

            bool isBodyATrigger = triggerGroup.HasComponent(entityA);
            bool isBodyBTrigger = triggerGroup.HasComponent(entityB);

            bool hasBodyASeenSign = seenGroup.HasComponent(entityA);
            bool hasBodyBSeenSign = seenGroup.HasComponent(entityB);

            if (isBodyACrowd && isBodyBTrigger && !hasBodyASeenSign)
            {
                Debug.Log("Crowd Saw A Sign!");
                commandBuffer.AddComponent<SawDirectionSign>(entityA, new SawDirectionSign
                {
                    seen = true
                });
                CrowdCollideWithTrigger(entityA, triggerGroup[entityB], commandBuffer, r);
            }
            if (isBodyBCrowd && isBodyATrigger && !hasBodyBSeenSign)
            {
                Debug.Log("Crowd Saw A Sign!!!");
                commandBuffer.AddComponent<SawDirectionSign>(entityB, new SawDirectionSign
                {
                    seen = true
                });
                CrowdCollideWithTrigger(entityA, triggerGroup[entityA], commandBuffer, r);
            }

        }
        private void CrowdCollideWithTrigger(Entity crowd, DirectionSignTrigger sign, EntityCommandBuffer commandBuffer, Unity.Mathematics.Random r)
        {
            Entity goToHolder = commandBuffer.CreateEntity();
            int goToActionID = 99;
            int goToPriority = 100;

            float3 wayPoint = GetDestinationPoint(sign, r);

            commandBuffer.AddComponent<GoToAndWaitStorage>(goToHolder, new GoToAndWaitStorage
            { // add the go to and wait storage component to the holder
                id = goToActionID,
                timeWaited = 0,
                timeToWait = 0,
                position = wayPoint
            }); // store the data

            commandBuffer.AddComponent<AddGoToAndWaitAction>(crowd, new AddGoToAndWaitAction
            { //add the go to and wait action component to the crowd agent
                id = goToActionID,
                priority = goToPriority,
                timeCreated = 0f,
                dataHolder = goToHolder
            });
        }

        //Create a random destination point based on which direction the agent is heading
        private float3 GetDestinationPoint(DirectionSignTrigger sign, Unity.Mathematics.Random r)
        {
            float minX = 0;
            float maxX = 0;
            float minZ = 0;
            float maxZ = 0;

            if (sign.west)
            {
                minX = -17f;
                maxX = -6f;
                minZ = 3.5f;
                maxZ = 4.5f;
            }
            else
            {
                minX = 6f;
                maxX = 17f;
                minZ = -4.5f;
                maxZ = -3.5f;
            }
            float xVal = r.NextFloat(minX, maxX);
            float zVal = r.NextFloat(minZ, maxZ);
            float3 destination = new float3(xVal, 0f, zVal);

            return destination;
        }


    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        JobHandle jobHandle = new CollisionEventCrowdJob
        {
            crowdGroup = GetComponentDataFromEntity<Crowd>(true),
            triggerGroup = GetComponentDataFromEntity<DirectionSignTrigger>(true),
            seenGroup = GetComponentDataFromEntity<SawDirectionSign>(true),
            commandBuffer = commandBufferSystem.CreateCommandBuffer(),
            r = r
        }.Schedule(m_StepPhysicsWorldSystem.Simulation, ref m_BuildPhysicsWorldSystem.PhysicsWorld, inputDeps);

        commandBufferSystem.AddJobHandleForProducer(jobHandle); // tell the system to execute the command buffer after the job has been completed
        //jobHandle.Complete();
        return jobHandle;
    }

}