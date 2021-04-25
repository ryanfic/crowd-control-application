using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;



// A system that adds the 'count until leave' component to crowd agents that collide with police
public class CrowdCountToLeaveSystem : SystemBase {
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    private EntityQueryDesc countQueryDesc;

    // The job that adds the 'count until leave' component to crowd agent
    private struct CountJob : IJobChunk {
        public EntityCommandBuffer.ParallelWriter commandBuffer; //Entity command buffer to allow adding/removing components inside the job
        
        [ReadOnly] public EntityTypeHandle entityType;
        public ComponentTypeHandle<CountUntilLeave> countType;

        public float time;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            NativeArray<CountUntilLeave> countArray = chunk.GetNativeArray(countType);

            for(int i = 0; i < chunk.Count; i++){
                Entity entity = entityArray[i];  
                CountUntilLeave count = countArray[i];

                countArray[i] = new CountUntilLeave {
                                timeWaited = count.timeWaited + time,
                                timeUntilLeave = count.timeUntilLeave
                            };

                if(count.timeWaited + time > count.timeUntilLeave){
                    Entity homeHolder = commandBuffer.CreateEntity(chunkIndex);
    
                    commandBuffer.AddComponent<GoHomeStorage>(chunkIndex,homeHolder, new GoHomeStorage { // add the go home storage component to the holder
                        id =  11,
                        homePoint = new float3(0f,0f,5f)
                    }); // store the data

                    commandBuffer.AddComponent<AddGoHomeAction>(chunkIndex,entity, new AddGoHomeAction{ //add the go home action component to the crowd agent
                        id =  11,
                        priority = 100,
                        timeCreated = 0f,
                        dataHolder = homeHolder
                    }); 
                }
            }
        }
    }



    protected override void OnCreate() {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        countQueryDesc = new EntityQueryDesc{
            All = new ComponentType[]{
                ComponentType.ReadOnly<CrowdCollidedWithPolice>(),
                ComponentType.ReadOnly<Crowd>(),
                typeof(CountUntilLeave),
                
            },
            None = new ComponentType[]{
                ComponentType.ReadOnly<GoHomeAction>()
            },
        };

        base.OnCreate();
    }
    protected override void OnUpdate(){
        float time = (float)Time.DeltaTime;
        EntityQuery cQuery = GetEntityQuery(countQueryDesc); // query the entities

        CountJob cJob = new CountJob{ // creates the job
            commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            entityType =  GetEntityTypeHandle(),
            countType = GetComponentTypeHandle<CountUntilLeave>(),
            time = time
        };
        JobHandle countJobHandle = cJob.Schedule(cQuery, this.Dependency);        

        commandBufferSystem.AddJobHandleForProducer(countJobHandle); // tell the system to execute the command buffer after the job has been completed

        this.Dependency = countJobHandle;
    }
}
