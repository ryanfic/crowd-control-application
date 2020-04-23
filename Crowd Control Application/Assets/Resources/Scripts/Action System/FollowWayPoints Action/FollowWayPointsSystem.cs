using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using crowd_Actions;

public class FollowWayPointsSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else
    private static float tolerance = 0.1f;

    [ExcludeComponent(typeof(StoreWayPoints),typeof(ChangeAction),typeof(RemoveAction))] // don't follow waypoints if the information is being stored/changed
    private struct FollowWayPointsJob : IJobForEachWithEntity_EBBCCC<WayPoint,Action,Translation,FollowWayPointsAction,HasReynoldsSeekTargetPos> {
        public EntityCommandBuffer.Concurrent entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        public void Execute(Entity entity, int index, DynamicBuffer<WayPoint> wayPoints, DynamicBuffer<Action> actions, [ReadOnly] ref Translation trans, ref FollowWayPointsAction data, ref HasReynoldsSeekTargetPos seek){
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
                            entityCommandBuffer.AddComponent<RemoveAction>(index, entity, new RemoveAction { // add a component that tells the system to remove the action from the queue
                                id = data.id
                            }); 
                            
                            entityCommandBuffer.DestroyEntity(index,actions[0].dataHolder);
                            entityCommandBuffer.RemoveComponent<HasReynoldsSeekTargetPos>(index, entity);
                            entityCommandBuffer.RemoveComponent<FollowWayPointsAction>(index, entity);
                            //remove the current follow waypoints action
                        }      
                    }
                }
                else{ // if there are actions but this action is not the right one
                Debug.Log("Gotta change!");
                    int i = 0;
                    while(i < actions.Length && actions[i].id != data.id){ // find the location of the action in the actions queue
                        i++;
                    }
                    if(i < actions.Length){ // if the action was found in the queue
                        entityCommandBuffer.AddComponent<StoreWayPoints>(index, entity, new StoreWayPoints{ // store the data
                            id = data.id,
                            dataHolder = actions[i].dataHolder,
                            curPointNum = data.curPointNum
                        });
                    }
                    entityCommandBuffer.AddComponent<ChangeAction>(index, entity, new ChangeAction {}); //signify that the action should be changed
                    
                }
            }
            else{ // if there are no actions in the action queue
                Debug.Log("Nothin left!");
                entityCommandBuffer.RemoveComponent<HasReynoldsSeekTargetPos>(index, entity);
                entityCommandBuffer.RemoveComponent<FollowWayPointsAction>(index, entity);
                entityCommandBuffer.AddComponent<ChangeAction>(index, entity, new ChangeAction {}); //signify that the action should be changed (will remove action)
                    
            }
        }
    }

    protected override void OnCreate() {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps){

        FollowWayPointsJob followJob = new FollowWayPointsJob{ // creates the "follow waypoints" job
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent()
        };
        JobHandle jobHandle = followJob.Schedule(this, inputDeps);

        commandBufferSystem.AddJobHandleForProducer(jobHandle); // tell the system to execute the command buffer after the job has been completed

        return jobHandle;
    }
}
