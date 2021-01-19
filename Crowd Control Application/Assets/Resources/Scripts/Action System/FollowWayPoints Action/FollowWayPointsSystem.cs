using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using crowd_Actions;


// A system to allow an entity to follow a set of points (aka waypoints)
public class FollowWayPointsSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else
    private static float tolerance = 10f;

    private EntityQueryDesc followWPQueryDec;

    private struct FollowWayPointsJob : IJobChunk {
        public EntityCommandBuffer.Concurrent entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job

        [ReadOnly] public ArchetypeChunkEntityType entityType;
        public ArchetypeChunkComponentType<HasReynoldsSeekTargetPos> reynoldsType;
        public ArchetypeChunkComponentType<FollowWayPointsAction> followWPType;
        [ReadOnly]public ArchetypeChunkComponentType<Translation> translationType;
        public ArchetypeChunkBufferType<Action> actionBufferType;
        public ArchetypeChunkBufferType<WayPoint> wayPointBufferType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            NativeArray<HasReynoldsSeekTargetPos> reynoldsArray = chunk.GetNativeArray(reynoldsType);
            NativeArray<FollowWayPointsAction> followWPArray = chunk.GetNativeArray(followWPType);
            NativeArray<Translation> transArray = chunk.GetNativeArray(translationType);
            BufferAccessor<Action> actionBuffers = chunk.GetBufferAccessor<Action>(actionBufferType);
            BufferAccessor<WayPoint> wpBuffers = chunk.GetBufferAccessor<WayPoint>(wayPointBufferType);

            for(int i = 0; i < chunk.Count; i++){   
                Entity entity = entityArray[i];
                DynamicBuffer<Action> actions = actionBuffers[i];
                DynamicBuffer<WayPoint> wayPoints = wpBuffers[i];
                HasReynoldsSeekTargetPos seek = reynoldsArray[i];
                FollowWayPointsAction data = followWPArray[i];
                Translation trans = transArray[i];


                if(actions.Length > 0){ //if there are actions
                    if(actions[0].id == data.id){ //if the current action is the same as the action in the first position 
                        if(math.distance(trans.Value, wayPoints[data.curPointNum].value) < tolerance){ // if the entity is within tolerance of the waypoint
                            if(data.curPointNum < wayPoints.Length - 1){ // if the entity has at least one more waypoint to follow
                                data.curPointNum++; // the index increases by one
                                seek.targetPos = wayPoints[data.curPointNum].value;// move to the next point
                            }
                            else { // if there are no more waypoints
                                //entityCommandBuffer.DestroyEntity(actions[0].dataHolder);//demolish the storage entity
                                Debug.Log("Got to last point");
                                entityCommandBuffer.AddComponent<RemoveAction>(chunkIndex, entity, new RemoveAction { // add a component that tells the system to remove the action from the queue
                                    id = data.id
                                }); 
                                entityCommandBuffer.RemoveComponent<HasReynoldsSeekTargetPos>(chunkIndex, entity);
                                entityCommandBuffer.RemoveComponent<FollowWayPointsAction>(chunkIndex, entity);
                                //remove the current follow waypoints action
                            }      
                        }
                    }
                    else{ // if there are actions but this action is not the right one
                    Debug.Log("Gotta change!");
                        int j = 0;
                        while(j < actions.Length && actions[j].id != data.id){ // find the location of the action in the actions queue
                            j++;
                        }
                        if(j < actions.Length){ // if the action was found in the queue
                            entityCommandBuffer.AddComponent<StoreWayPoints>(chunkIndex, entity, new StoreWayPoints{ // store the data
                                id = data.id,
                                dataHolder = actions[j].dataHolder,
                                curPointNum = data.curPointNum
                            });
                        }
                        entityCommandBuffer.AddComponent<ChangeAction>(chunkIndex, entity, new ChangeAction {}); //signify that the action should be changed
                        
                    }
                }
                else{ // if there are no actions in the action queue
                    Debug.Log("Nothin left!");
                    entityCommandBuffer.RemoveComponent<HasReynoldsSeekTargetPos>(chunkIndex, entity);
                    entityCommandBuffer.RemoveComponent<FollowWayPointsAction>(chunkIndex, entity);
                    entityCommandBuffer.AddComponent<ChangeAction>(chunkIndex, entity, new ChangeAction {}); //signify that the action should be changed (will remove action)
                        
                }
            }
        }
    }

    protected override void OnCreate() {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        followWPQueryDec = new EntityQueryDesc{
            All = new ComponentType[]{
                typeof(WayPoint),
                typeof(Action),
                typeof(FollowWayPointsAction),
                typeof(HasReynoldsSeekTargetPos),
                ComponentType.ReadOnly<Translation>(),

            },
            None = new ComponentType[]{
                typeof(StoreWayPoints),
                typeof(ChangeAction),
                typeof(RemoveAction)
            }
        };
        base.OnCreate();
    }
    protected override void OnUpdate(){
        EntityQuery followWPQuery = GetEntityQuery(followWPQueryDec); // query the entities

        FollowWayPointsJob followJob = new FollowWayPointsJob{ // creates the "follow waypoints" job
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent(),
            entityType =  GetArchetypeChunkEntityType(),
            reynoldsType = GetArchetypeChunkComponentType<HasReynoldsSeekTargetPos>(),
            followWPType = GetArchetypeChunkComponentType<FollowWayPointsAction>(),
            translationType = GetArchetypeChunkComponentType<Translation>(true),
            actionBufferType = GetArchetypeChunkBufferType<Action>(),
            wayPointBufferType = GetArchetypeChunkBufferType<WayPoint>()
        };
        JobHandle jobHandle = followJob.Schedule(followWPQuery, this.Dependency);

        commandBufferSystem.AddJobHandleForProducer(jobHandle); // tell the system to execute the command buffer after the job has been completed

        this.Dependency = jobHandle;
        }
}