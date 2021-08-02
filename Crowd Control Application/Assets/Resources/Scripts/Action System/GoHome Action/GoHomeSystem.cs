using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using crowd_Actions;

public class GoHomeSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else
    private static float tolerance = 2f;

    private EntityQueryDesc goHomeQueryDec;

    private struct GoHomeJob : IJobChunk {
        public EntityCommandBuffer.ParallelWriter entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job

        [ReadOnly] public EntityTypeHandle entityType;
        [ReadOnly] public ComponentTypeHandle<HasReynoldsSeekTargetPos> reynoldsType;
        [ReadOnly] public ComponentTypeHandle<GoHomeAction> goHomeType;
        [ReadOnly] public ComponentTypeHandle<Translation> translationType;
        [ReadOnly] public BufferTypeHandle<Action> actionBufferType;
        

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            NativeArray<HasReynoldsSeekTargetPos> reynoldsArray = chunk.GetNativeArray(reynoldsType);
            NativeArray<GoHomeAction> goHomeArray = chunk.GetNativeArray(goHomeType);
            NativeArray<Translation> transArray = chunk.GetNativeArray(translationType);
            BufferAccessor<Action> actionBuffers = chunk.GetBufferAccessor<Action>(actionBufferType);

            

            for(int i = 0; i < chunk.Count; i++){   
                Entity entity = entityArray[i];
                DynamicBuffer<Action> actions = actionBuffers[i];
                HasReynoldsSeekTargetPos seek = reynoldsArray[i];
                GoHomeAction data = goHomeArray[i];
                Translation trans = transArray[i];
                
                //Debug.Log("Something? " + copy.Value);
                

                if(actions.Length > 0){ //if there are actions
                    if(actions[0].id == data.id){ //if the current action is the same as the action in the first position 
                        if(math.distance(trans.Value, seek.targetPos) < tolerance){ // if the entity is within tolerance of the home point
                            //Remove the entity from the simulation (the crowd agent is going home)
                            Debug.Log("Going home!");
                            // loop through all of the actions in the agent's list, and destroy all of the data holder entities
                            /*for(int j = 0; j < actions.Length; j++){
                                entityCommandBuffer.DestroyEntity(chunkIndex,actions[j].dataHolder);
                            }  
                            entityCommandBuffer.DestroyEntity(chunkIndex, entity); // remove the crowd agent*/
                            entityCommandBuffer.AddComponent<CrowdToDelete>(chunkIndex,entity, new CrowdToDelete{});
                        }
                    }
                    else{ // if there are actions but this action is not the right one
                    Debug.Log("Gotta change from going home!");
                        //If there were data to store, this would be the point to do it
                        entityCommandBuffer.RemoveComponent<HasReynoldsSeekTargetPos>(chunkIndex, entity); // remove the seek target pos from the crowd agent
                        entityCommandBuffer.RemoveComponent<GoHomeAction>(chunkIndex, entity); // remove the going home action from the crowd agent
                        entityCommandBuffer.AddComponent<ChangeAction>(chunkIndex, entity, new ChangeAction {}); //signify that the action should be changed
                        
                    }
                }
                else{ // if there are no actions in the action queue
                    Debug.Log("Nothin left!");
                    entityCommandBuffer.RemoveComponent<HasReynoldsSeekTargetPos>(chunkIndex, entity); // remove the seek target pos from the crowd agent
                    entityCommandBuffer.RemoveComponent<GoHomeAction>(chunkIndex, entity); // remove the going home action from the crowd agent
                    entityCommandBuffer.AddComponent<ChangeAction>(chunkIndex, entity, new ChangeAction {}); //signify that the action should be changed (will remove action)
                        
                }
            }
        }
    }

    protected override void OnCreate() {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        goHomeQueryDec = new EntityQueryDesc{
            All = new ComponentType[]{
                ComponentType.ReadOnly<Action>(),
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<GoHomeAction>(),
                ComponentType.ReadOnly<HasReynoldsSeekTargetPos>()
            },
            None = new ComponentType[]{ // don't go home if the information is being stored/changed
                typeof(StoreWayPoints),
                typeof(ChangeAction),
                typeof(RemoveAction)
            }
        };
        base.OnCreate();
    }
    protected override void OnUpdate(){
        EntityQuery goHomeQuery = GetEntityQuery(goHomeQueryDec); // query the entities

        //this.EntityManager.GetComponentGameObject<Transform>();

        GoHomeJob homeJob = new GoHomeJob{ // creates the "go home" job
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            entityType =  GetEntityTypeHandle(),
            reynoldsType = GetComponentTypeHandle<HasReynoldsSeekTargetPos>(true),
            goHomeType = GetComponentTypeHandle<GoHomeAction>(true),
            translationType = GetComponentTypeHandle<Translation>(true),
            actionBufferType = GetBufferTypeHandle<Action>(true)
            
        };
        JobHandle jobHandle = homeJob.Schedule(goHomeQuery, this.Dependency);

        commandBufferSystem.AddJobHandleForProducer(jobHandle); // tell the system to execute the command buffer after the job has been completed

        this.Dependency = jobHandle;
    }
}
