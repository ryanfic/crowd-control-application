using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;


// A system that adds the 'count until leave' component to crowd agents that collide with police
public class CrowdBeginCountToLeaveSystem : SystemBase {
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    private EntityQueryDesc beginCountQueryDesc;

    // The job that adds the 'count until leave' component to crowd agent
    private struct BeginCountJob : IJobChunk {
        public EntityCommandBuffer.ParallelWriter commandBuffer; //Entity command buffer to allow adding/removing components inside the job
        
        [ReadOnly] public EntityTypeHandle entityType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);

            for(int i = 0; i < chunk.Count; i++){
                Entity entity = entityArray[i];  
                commandBuffer.AddComponent<CountUntilLeave>(chunkIndex,entity, new CountUntilLeave{
                    timeWaited = 0f,
                    timeUntilLeave = 10f
                });
            }
        }
    }



    protected override void OnCreate() {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        beginCountQueryDesc = new EntityQueryDesc{
            All = new ComponentType[]{
                ComponentType.ReadOnly<CrowdCollidedWithPolice>(),
                ComponentType.ReadOnly<Crowd>()
            },
            None = new ComponentType[]{
                ComponentType.ReadOnly<CountUntilLeave>(),
                ComponentType.ReadOnly<GoHomeAction>()
            },
        };

        base.OnCreate();
    }
    protected override void OnUpdate(){
        EntityQuery beginCDQuery = GetEntityQuery(beginCountQueryDesc); // query the entities

        BeginCountJob beginCDJob = new BeginCountJob{ // creates the job
            commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            entityType =  GetEntityTypeHandle(),
        };
        JobHandle beginCDJobHandle = beginCDJob.Schedule(beginCDQuery, this.Dependency);        

        commandBufferSystem.AddJobHandleForProducer(beginCDJobHandle); // tell the system to execute the command buffer after the job has been completed

        this.Dependency = beginCDJobHandle;
    }
}
