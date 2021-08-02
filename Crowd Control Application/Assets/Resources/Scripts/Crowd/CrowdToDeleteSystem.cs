using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine;
using crowd_Actions;


public class CrowdToDeleteSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else
    private EntityQueryDesc crowdDeleteQueryDesc;
    private EntityQueryDesc crowdDeleteWithTransformQueryDesc;

    //A job for removing the name of a police unit that is to be deleted to the voice controller
    [BurstCompile]
    private struct DeleteCrowdEntityJob : IJobChunk {
        public EntityCommandBuffer.ParallelWriter commandBuffer;

        [ReadOnly] public EntityTypeHandle entityType;
        [ReadOnly] public BufferTypeHandle<Child> childBufferType;
        [ReadOnly] public BufferTypeHandle<Action> actionBufferType;
        

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            BufferAccessor<Action> actionBuffers = chunk.GetBufferAccessor<Action>(actionBufferType);

            for(int i = 0; i < chunk.Count; i++){                
                Entity entity = entityArray[i];
                DynamicBuffer<Action> actions = actionBuffers[i];

                // loop through all of the actions in the agent's list, and destroy all of the data holder entities
                for(int j = 0; j < actions.Length; j++){
                    commandBuffer.DestroyEntity(chunkIndex,actions[j].dataHolder);
                }  
                if(chunk.Has<Child>(childBufferType)){
                    BufferAccessor<Child> childBuffers = chunk.GetBufferAccessor<Child>(childBufferType);
                    DynamicBuffer<Child> children = childBuffers[i];
                    for(int j = 0; j < children.Length; j++){
                        commandBuffer.DestroyEntity(chunkIndex,children[j].Value); //remove the child
                    }
                }
                //)
                
                commandBuffer.DestroyEntity(chunkIndex,entity); //remove the crowd agent
            }
        }
    }

    protected override void OnCreate(){
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        crowdDeleteWithTransformQueryDesc = new EntityQueryDesc{ // define query for adding constant movement components
            All = new ComponentType[]{
                ComponentType.ReadOnly<Crowd>(),
                ComponentType.ReadOnly<CrowdToDelete>(),
                ComponentType.ReadOnly<Action>(),
                ComponentType.ReadOnly<Transform>()
            }
        };

        crowdDeleteQueryDesc = new EntityQueryDesc{ // define query for adding constant movement components
            All = new ComponentType[]{
                ComponentType.ReadOnly<Crowd>(),
                ComponentType.ReadOnly<CrowdToDelete>(),
                ComponentType.ReadOnly<Action>()
            }
        };
        
        base.OnCreate();
    }

    protected override void OnUpdate(){
        EntityCommandBuffer.ParallelWriter commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(); // create a command buffer
        
        NativeArray<Entity> entitiesWithGO = GetEntityQuery(crowdDeleteWithTransformQueryDesc).ToEntityArray(Allocator.Temp);
        for(int i = 0; i < entitiesWithGO.Length; i++){
            EntityManager.GetComponentObject<Transform>(entitiesWithGO[i]).gameObject.Destroy();
        } 
        
        entitiesWithGO.Dispose();


        EntityQuery deleteQuery = GetEntityQuery(crowdDeleteQueryDesc); // query the entities
            //Add label job
        

        
        DeleteCrowdEntityJob deleteCrowdJob = new DeleteCrowdEntityJob{ 
            commandBuffer = commandBuffer,
            entityType =  GetEntityTypeHandle(),
            childBufferType = GetBufferTypeHandle<Child>(true),
            actionBufferType = GetBufferTypeHandle<Action>(true)
        };

        JobHandle deleteCrowdJobHandle = deleteCrowdJob.Schedule(deleteQuery, this.Dependency); //schedule the job
        commandBufferSystem.AddJobHandleForProducer(deleteCrowdJobHandle); // make sure the components get added/removed for the job
        this.Dependency = deleteCrowdJobHandle;  
    }
}


