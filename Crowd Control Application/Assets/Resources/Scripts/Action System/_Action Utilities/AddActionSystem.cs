using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

// Adds an action to the action queue
public class AddActionSystem : JobComponentSystem {
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else
    private struct AddFollowWayPointsActionJob : IJobForEachWithEntity_EBC<Action,AddFollowWayPointsAction> {
        public EntityCommandBuffer.Concurrent entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        public void Execute(Entity entity, int index, DynamicBuffer<Action> actions, ref AddFollowWayPointsAction toAdd){
            int pos = 0; // where the action should be added
             // find the index where the action should be added
            while(pos < actions.Length && toAdd.priority <= actions[pos].priority){
                if(toAdd.priority == actions[pos].priority){ // if the priorities are the same
                    //compare the times
                    if(toAdd.timeCreated >= actions[pos].timeCreated){ // if the current action time is greater than the other action's time, this action should go later
                        pos++;
                    }
                    else 
                        break;
                }
                else if(toAdd.priority < actions[pos].priority){ // if this action's priority is smaller than the other action's priority, this action should go later
                    pos++;
                }
                else
                    break;
            }
            if(pos == 0){ // if the action was added at the start of the buffer
                Debug.Log("Added to start!");
                entityCommandBuffer.AddComponent<ChangeAction>(index,entity, new ChangeAction{}); // tell the system that the current action should be changed
            }
            else{ //
                Debug.Log("Added after start");
            }
            // Add the action at the correct position
            actions.Insert( 
                    pos,
                    new Action {
                        id = toAdd.id,
                        priority = toAdd.priority,
                        type = ActionType.Follow_WayPoints,
                        timeCreated = toAdd.timeCreated,
                        dataHolder = toAdd.dataHolder
                    });
            
            entityCommandBuffer.RemoveComponent<AddFollowWayPointsAction>(index,entity); // remove this component
        }
    }

    protected override void OnCreate() {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps){
        //Will have to go through all the possible actions that can be added and schedule them all (none of them are dependent on eachother)
        //Will have to combine all the JobHandles into one jobhandle to be returned


        AddFollowWayPointsActionJob addFollowWPJob = new AddFollowWayPointsActionJob{ // creates the "follow waypoints" job
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent()
        };
        JobHandle jobHandle = addFollowWPJob.Schedule(this, inputDeps);

        commandBufferSystem.AddJobHandleForProducer(jobHandle); // tell the system to execute the command buffer after the job has been completed

        return jobHandle;
    }
}
