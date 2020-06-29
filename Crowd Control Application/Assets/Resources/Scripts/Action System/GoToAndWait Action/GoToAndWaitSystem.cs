using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using crowd_Actions;

public class GoToAndWaitSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else
    private static float tolerance = 10f;

    [ExcludeComponent(typeof(StoreWayPoints),typeof(ChangeAction),typeof(RemoveAction))] // don't go home if the information is being stored/changed
    private struct GoHomeJob : IJobForEachWithEntity_EBCCC<Action,Translation,GoToAndWaitAction,HasReynoldsSeekTargetPos> {
        public EntityCommandBuffer.Concurrent entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        public float time;
        public void Execute(Entity entity, int index, DynamicBuffer<Action> actions, [ReadOnly] ref Translation trans, ref GoToAndWaitAction data, ref HasReynoldsSeekTargetPos seek){
            if(actions.Length > 0){ //if there are actions
                if(actions[0].id == data.id){ //if the current action is the same as the action in the first position 
                    if(math.distance(trans.Value, seek.targetPos) < tolerance){ // if the entity is within tolerance of the target
                        //Increment the time waited
                        data.timeWaited += time;

                        if(data.timeToWait <= data.timeWaited){ // if the agent has waited the correct amount of time
                            Debug.Log("Done waiting");
                            entityCommandBuffer.AddComponent<RemoveAction>(index, entity, new RemoveAction { // add a component that tells the system to remove the action from the queue
                                id = data.id
                            }); 
                            entityCommandBuffer.RemoveComponent<HasReynoldsSeekTargetPos>(index, entity);
                            entityCommandBuffer.RemoveComponent<GoToAndWaitAction>(index, entity);
                        }
                    }
                }
                else{ // if there are actions but this action is not the right one
                Debug.Log("Gotta change from waiting!");
                    //If there were data to store, this would be the point to do it
                    entityCommandBuffer.RemoveComponent<HasReynoldsSeekTargetPos>(index, entity); // remove the seek target pos from the crowd agent
                    entityCommandBuffer.RemoveComponent<GoToAndWaitAction>(index, entity); // remove the waiting action from the crowd agent
                    entityCommandBuffer.AddComponent<ChangeAction>(index, entity, new ChangeAction {}); //signify that the action should be changed
                    
                }
            }
            else{ // if there are no actions in the action queue
                Debug.Log("Nothin left!");
                entityCommandBuffer.RemoveComponent<HasReynoldsSeekTargetPos>(index, entity); // remove the seek target pos from the crowd agent
                entityCommandBuffer.RemoveComponent<GoToAndWaitAction>(index, entity); // remove the going  action from the crowd agent
                entityCommandBuffer.AddComponent<ChangeAction>(index, entity, new ChangeAction {}); //signify that the action should be changed (will remove action)
                    
            }
        }
    }

    protected override void OnCreate() {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps){
        float curTime = Time.DeltaTime;

        GoHomeJob homeJob = new GoHomeJob{ // creates the "go home" job
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent(),
            time = curTime
        };
        JobHandle jobHandle = homeJob.Schedule(this, inputDeps);

        commandBufferSystem.AddJobHandleForProducer(jobHandle); // tell the system to execute the command buffer after the job has been completed

        return jobHandle;
    }
}
