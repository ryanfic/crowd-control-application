using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using crowd_Actions;

public class GoToAndWaitSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else
    private static float tolerance = 10f;

    private EntityQueryDesc gTAWQueryDec;

    private struct GoToAndWaitJob : IJobChunk {
        public EntityCommandBuffer.ParallelWriter entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        public float time;

        [ReadOnly] public EntityTypeHandle entityType;
        [ReadOnly] public ComponentTypeHandle<HasReynoldsSeekTargetPos> reynoldsType;
        public ComponentTypeHandle<GoToAndWaitAction> gTAWType;
        [ReadOnly] public ComponentTypeHandle<Translation> translationType;
        [ReadOnly] public BufferTypeHandle<Action> actionBufferType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            NativeArray<HasReynoldsSeekTargetPos> reynoldsArray = chunk.GetNativeArray(reynoldsType);
            NativeArray<GoToAndWaitAction> gTAWArray = chunk.GetNativeArray(gTAWType);
            NativeArray<Translation> transArray = chunk.GetNativeArray(translationType);
            BufferAccessor<Action> actionBuffers = chunk.GetBufferAccessor<Action>(actionBufferType);

            for(int i = 0; i < chunk.Count; i++){   
                Entity entity = entityArray[i];
                DynamicBuffer<Action> actions = actionBuffers[i];
                HasReynoldsSeekTargetPos seek = reynoldsArray[i];
                GoToAndWaitAction data = gTAWArray[i];
                Translation trans = transArray[i];

                if(actions.Length > 0){ //if there are actions
                    if(actions[0].id == data.id){ //if the current action is the same as the action in the first position 
                        if(math.distance(trans.Value, seek.targetPos) < tolerance){ // if the entity is within tolerance of the target
                            //Increment the time waited (must be done by altering the original array)                            
                            gTAWArray[i] = new GoToAndWaitAction {
                                id = data.id, // the id of the action
                                timeWaited = data.timeWaited + time,
                                timeToWait = data.timeToWait,
                                position = data.position
                            };

                            //Debug.Log("timeWaited (After adding) = " + data.timeWaited);

                            if(data.timeToWait <= gTAWArray[i].timeWaited){ // if the agent has waited the correct amount of time
                                //Debug.Log("Done waiting");
                                entityCommandBuffer.AddComponent<RemoveAction>(chunkIndex, entity, new RemoveAction { // add a component that tells the system to remove the action from the queue
                                    id = data.id
                                }); 
                                entityCommandBuffer.RemoveComponent<HasReynoldsSeekTargetPos>(chunkIndex, entity);
                                entityCommandBuffer.RemoveComponent<GoToAndWaitAction>(chunkIndex, entity);
                            }
                        }
                    }
                    else{ // if there are actions but this action is not the right one
                    Debug.Log("Gotta change from waiting!");
                        //If there were data to store, this would be the point to do it
                        entityCommandBuffer.RemoveComponent<HasReynoldsSeekTargetPos>(chunkIndex, entity); // remove the seek target pos from the crowd agent
                        entityCommandBuffer.RemoveComponent<GoToAndWaitAction>(chunkIndex, entity); // remove the waiting action from the crowd agent
                        entityCommandBuffer.AddComponent<ChangeAction>(chunkIndex, entity, new ChangeAction {}); //signify that the action should be changed
                        
                    }
                }
                else{ // if there are no actions in the action queue
                    Debug.Log("Nothin left!");
                    entityCommandBuffer.RemoveComponent<HasReynoldsSeekTargetPos>(chunkIndex, entity); // remove the seek target pos from the crowd agent
                    entityCommandBuffer.RemoveComponent<GoToAndWaitAction>(chunkIndex, entity); // remove the going  action from the crowd agent
                    entityCommandBuffer.AddComponent<ChangeAction>(chunkIndex, entity, new ChangeAction {}); //signify that the action should be changed (will remove action)
                        
                }
            }
        }
    }

    protected override void OnCreate() {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        gTAWQueryDec = new EntityQueryDesc{
            All = new ComponentType[]{
                ComponentType.ReadOnly<Action>(),
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<GoToAndWaitAction>(),
                ComponentType.ReadOnly<HasReynoldsSeekTargetPos>()
            },
            None = new ComponentType[]{ // don't go to and wait if the information is being stored/changed
                typeof(StoreWayPoints),
                typeof(ChangeAction),
                typeof(RemoveAction)
            }
        };
        base.OnCreate();
    }
    protected override void OnUpdate(){
        float curTime = Time.DeltaTime;

        EntityQuery gTAWQuery = GetEntityQuery(gTAWQueryDec); // query the entities

        GoToAndWaitJob gtawJob = new GoToAndWaitJob{ // creates the "go to and wait" job
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            time = curTime,
            entityType =  GetEntityTypeHandle(),
            reynoldsType = GetComponentTypeHandle<HasReynoldsSeekTargetPos>(true),
            gTAWType = GetComponentTypeHandle<GoToAndWaitAction>(),
            translationType = GetComponentTypeHandle<Translation>(true),
            actionBufferType = GetBufferTypeHandle<Action>(true)
        };
        JobHandle jobHandle = gtawJob.Schedule(gTAWQuery, this.Dependency);

        commandBufferSystem.AddJobHandleForProducer(jobHandle); // tell the system to execute the command buffer after the job has been completed

        this.Dependency = jobHandle;
    }
}
