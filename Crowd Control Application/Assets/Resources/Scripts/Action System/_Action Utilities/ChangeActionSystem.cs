using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using crowd_Actions;


// A system for changing the current action done by an agent
public class ChangeActionSystem : SystemBase {
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else
    
    private EntityQueryDesc changeActionQueryDec;

    private struct ChangeActionJob : IJobChunk{
        public EntityCommandBuffer.Concurrent entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        
        [ReadOnly] public ArchetypeChunkEntityType entityType;
        public ArchetypeChunkComponentType<CurrentAction> curActionType;
        public ArchetypeChunkBufferType<Action> actionBufferType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            NativeArray<CurrentAction> curActionArray = chunk.GetNativeArray(curActionType);
            BufferAccessor<Action> buffers = chunk.GetBufferAccessor<Action>(actionBufferType);

            for(int i = 0; i < chunk.Count; i++){
                Entity entity = entityArray[i];
                DynamicBuffer<Action> actions = buffers[i];
                CurrentAction current = curActionArray[i];

                if(actions.Length > 0){ //if there are actions, add another action
                    Entity holder = actions[0].dataHolder;
                    current.id = actions[0].id;
                    current.dataHolder = holder;
                    switch (actions[0].type){ // add a component based on what action has the highest priority
                        case ActionType.Follow_WayPoints: // if the action to add is a Follow waypoints action
                            entityCommandBuffer.AddComponent<FetchWayPoints>(chunkIndex, entity, new FetchWayPoints{ // tell the system to fetch the waypoints for the action
                                id = actions[0].id,
                                dataHolder = holder
                            });
                            current.type = ActionType.Follow_WayPoints;
                            Debug.Log("Changing to Action " + actions[0].id + "!");
                            break;
                        case ActionType.Go_Home:
                            entityCommandBuffer.AddComponent<FetchGoHomeData>(chunkIndex, entity, new FetchGoHomeData{ // tell the system to fetch the go home data for the action
                                id = actions[0].id,
                                dataHolder = holder
                            });
                            current.type = ActionType.Go_Home;
                            Debug.Log("Changing to Action " + actions[0].id + "!");
                            break;
                        case ActionType.Go_And_Wait:
                            entityCommandBuffer.AddComponent<FetchGoToAndWaitData>(chunkIndex, entity, new FetchGoToAndWaitData{ // tell the system to fetch the go home data for the action
                                id = actions[0].id,
                                dataHolder = holder
                            });
                            current.type = ActionType.Go_And_Wait;
                            Debug.Log("Changing to Action " + actions[0].id + "!");
                            break;
                    }
                }
                else { // if there are no more actions to do
                    current.id = -1; // set id to a nonsense value
                    current.type = ActionType.No_Action; // Tell the system that there is no current action

                }
                entityCommandBuffer.RemoveComponent<ChangeAction>(chunkIndex,entity); // remove this component to show that there is no need to change the action anymore
            }

        }
    }

    protected override void OnCreate() {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        changeActionQueryDec = new EntityQueryDesc{
            All = new ComponentType[]{
                typeof(Action),
                typeof(CurrentAction),
                ComponentType.ReadOnly<ChangeAction>()
            }
        };
        base.OnCreate();
    }
    protected override void OnUpdate(){
        EntityQuery changeActionQuery = GetEntityQuery(changeActionQueryDec); // query the entities

        ChangeActionJob changeJob = new ChangeActionJob{ // creates the change action job
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent(),
            entityType =  GetArchetypeChunkEntityType(),
            curActionType = GetArchetypeChunkComponentType<CurrentAction>(),
            actionBufferType = GetArchetypeChunkBufferType<Action>()
        };
        JobHandle jobHandle = changeJob.Schedule(changeActionQuery, this.Dependency);

        commandBufferSystem.AddJobHandleForProducer(jobHandle); // tell the system to execute the command buffer after the job has been completed


        this.Dependency = jobHandle;
    }
}
