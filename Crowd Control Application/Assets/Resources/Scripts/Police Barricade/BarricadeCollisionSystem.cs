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
public class BarricadeCollisionSystem : JobComponentSystem
{
    private BuildPhysicsWorld m_BuildPhysicsWorldSystem;
    private StepPhysicsWorld m_StepPhysicsWorldSystem;

    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    private EntityQuery crowdGroup;
    private Unity.Mathematics.Random r;

    protected override void OnCreate(){
        base.OnCreate();

        r =  new Unity.Mathematics.Random(19);
            

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
    private struct CollisionEventCrowdJob : ITriggerEventsJob{
        [ReadOnly] public ComponentDataFromEntity<Crowd> crowdGroup;
        [ReadOnly] public ComponentDataFromEntity<PoliceBarricade> barricadeGroup;

        public EntityCommandBuffer commandBuffer;
        public Unity.Mathematics.Random r;
        public void Execute(TriggerEvent collisionEvent){
            r = new Unity.Mathematics.Random((uint)r.NextInt());
            Entity entityA = collisionEvent.EntityA;
            Entity entityB = collisionEvent.EntityB;

            bool isBodyACrowd = crowdGroup.HasComponent(entityA);
            bool isBodyBCrowd = crowdGroup.HasComponent(entityB);

            bool isBodyABarricade = barricadeGroup.HasComponent(entityA);
            bool isBodyBBarricade = barricadeGroup.HasComponent(entityB);

            if(isBodyACrowd && isBodyBBarricade){
                Debug.Log("Crowd Collided with barricade!");
                commandBuffer.AddComponent<CrowdCollidedWithBarricade>(entityA, new CrowdCollidedWithBarricade{
                    northwest = barricadeGroup[entityB].northwest
                });
                //CrowdCollideWithBarricade(entityA, barricadeGroup[entityB], commandBuffer, r);
                //commandBuffer.AddComponent<CrowdCollidedWithPolice>(entityA);
            }
            if(isBodyBCrowd && isBodyABarricade){
                commandBuffer.AddComponent<CrowdCollidedWithBarricade>(entityA, new CrowdCollidedWithBarricade{
                    northwest = barricadeGroup[entityA].northwest
                });
                //CrowdCollideWithBarricade(entityB, barricadeGroup[entityA], commandBuffer, r);
                //commandBuffer.AddComponent<CrowdCollidedWithPolice>(entityB);
                Debug.Log("Crowd Collided with barricade!!!!");
            }
            
        }
        private void CrowdCollideWithBarricade(Entity crowd, PoliceBarricade barricade, EntityCommandBuffer commandBuffer, Unity.Mathematics.Random r){
            Entity homeHolder = commandBuffer.CreateEntity();
            int goHomeActionID = 99;
            int goHomePriority = 100;

            float3 homePoint = GetDestinationPoint(barricade, r);
            
            commandBuffer.AddComponent<GoHomeStorage>(homeHolder, new GoHomeStorage { // add the go home storage component to the holder
                id =  goHomeActionID,
                homePoint = homePoint
            }); // store the data

            commandBuffer.AddComponent<AddGoHomeAction>(crowd, new AddGoHomeAction{ //add the go home action component to the crowd agent
                id =  goHomeActionID,
                priority = goHomePriority,
                timeCreated = 0f,
                dataHolder = homeHolder
            }); 
        }
        
        //Create a random destination point based on which direction the agent is heading
        private float3 GetDestinationPoint(PoliceBarricade barricade, Unity.Mathematics.Random r){
            float minX = 0;
            float maxX = 0;
            float minZ = 0;
            float maxZ = 0;

            if(barricade.northwest){
                minX = -4.5f;
                maxX = 4.5f;
                minZ = 18.5f;
                maxZ = 24.5f;
            }
            else{
                minX = 18.5f;
                maxX = 24.5f;
                minZ = -4.5f;
                maxZ = 4.5f;
            }
            float xVal = r.NextFloat(minX,maxX);
            float zVal = r.NextFloat(minZ,maxZ);
            float3 destination = new float3(xVal,0f,zVal);

            return destination;
        }
    }
    

    protected override JobHandle OnUpdate(JobHandle inputDeps){
        JobHandle jobHandle = new CollisionEventCrowdJob{
            crowdGroup = GetComponentDataFromEntity<Crowd>(true),
            barricadeGroup = GetComponentDataFromEntity<PoliceBarricade>(true),
            commandBuffer = commandBufferSystem.CreateCommandBuffer(),
            r = r
        }.Schedule(m_StepPhysicsWorldSystem.Simulation, ref m_BuildPhysicsWorldSystem.PhysicsWorld, inputDeps);

        commandBufferSystem.AddJobHandleForProducer(jobHandle); // tell the system to execute the command buffer after the job has been completed
        //jobHandle.Complete();
        return jobHandle;
    }

}


