using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;



// A system that adds the 'count until leave' component to crowd agents that collide with police
public class CrowdCollidedWithBarricadeSystem : SystemBase {
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    private EntityQueryDesc collisionQueryDesc;
    private Unity.Mathematics.Random r;

    // The job that adds the 'count until leave' component to crowd agent
    private struct CollisionJob : IJobChunk {
        public EntityCommandBuffer.ParallelWriter commandBuffer; //Entity command buffer to allow adding/removing components inside the job
        
        [ReadOnly] public EntityTypeHandle entityType;
        public ComponentTypeHandle<CrowdCollidedWithBarricade> collisionType;
        public Unity.Mathematics.Random r;


        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            NativeArray<CrowdCollidedWithBarricade> collisionArray = chunk.GetNativeArray(collisionType);

            for(int i = 0; i < chunk.Count; i++){
                
                Entity entity = entityArray[i];  
                CrowdCollidedWithBarricade collision = collisionArray[i];

                Entity homeHolder = commandBuffer.CreateEntity(chunkIndex);

                CrowdCollideWithBarricade(entity,collision,commandBuffer,chunkIndex, r);
                commandBuffer.AddComponent<BarricadeCollisionRectified>(chunkIndex,entity);
            }
        }
        private void CrowdCollideWithBarricade(Entity crowd, CrowdCollidedWithBarricade barricade, EntityCommandBuffer.ParallelWriter commandBuffer,int chunkIndex, Unity.Mathematics.Random r){
            Entity homeHolder = commandBuffer.CreateEntity(chunkIndex);
            int goHomeActionID = 99;
            int goHomePriority = 100;

            float3 homePoint = GetDestinationPoint(barricade, r);
            
            commandBuffer.AddComponent<GoHomeStorage>(chunkIndex, homeHolder, new GoHomeStorage { // add the go home storage component to the holder
                id =  goHomeActionID,
                homePoint = homePoint
            }); // store the data

            commandBuffer.AddComponent<AddGoHomeAction>(chunkIndex, crowd, new AddGoHomeAction{ //add the go home action component to the crowd agent
                id =  goHomeActionID,
                priority = goHomePriority,
                timeCreated = 0f,
                dataHolder = homeHolder
            }); 
        }
        
        //Create a random destination point based on which direction the agent is heading
        private float3 GetDestinationPoint(CrowdCollidedWithBarricade barricade, Unity.Mathematics.Random r){
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



    protected override void OnCreate() {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        collisionQueryDesc = new EntityQueryDesc{
            All = new ComponentType[]{
                ComponentType.ReadOnly<CrowdCollidedWithBarricade>(),
                ComponentType.ReadOnly<Crowd>(),              
            },
            None = new ComponentType[]{
                ComponentType.ReadOnly<BarricadeCollisionRectified>()
            },
        };

        r = new Unity.Mathematics.Random(19);

        base.OnCreate();
    }
    protected override void OnUpdate(){
        EntityQuery cQuery = GetEntityQuery(collisionQueryDesc); // query the entities

        r = new Unity.Mathematics.Random(r.NextUInt());
        CollisionJob cJob = new CollisionJob{ // creates the job
            commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            entityType =  GetEntityTypeHandle(),
            collisionType = GetComponentTypeHandle<CrowdCollidedWithBarricade>(),
            r = r
        };
        JobHandle collisionJobHandle = cJob.Schedule(cQuery, this.Dependency);        

        commandBufferSystem.AddJobHandleForProducer(collisionJobHandle); // tell the system to execute the command buffer after the job has been completed

        this.Dependency = collisionJobHandle;
    }
}

