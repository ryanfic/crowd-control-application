using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

public class GoHomeSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else
    private static float tolerance = 0.1f;

    [ExcludeComponent(typeof(StoreWayPoints),typeof(ChangeAction),typeof(RemoveAction))] // don't go home if the information is being stored/changed
    private struct GoHomeJob : IJobForEachWithEntity_EBCCC<Action,Translation,GoHomeAction,HasReynoldsSeekTargetPos> {
        public EntityCommandBuffer.Concurrent entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        public void Execute(Entity entity, int index, DynamicBuffer<Action> actions, [ReadOnly] ref Translation trans, ref GoHomeAction data, ref HasReynoldsSeekTargetPos seek){
            if(actions.Length > 0){ //if there are actions
                if(actions[0].id == data.id){ //if the current action is the same as the action in the first position 
                    if(math.distance(trans.Value, seek.targetPos) < tolerance){ // if the entity is within tolerance of the home point
                        //Remove the entity from the simulation (the crowd agent is going home)
                        Debug.Log("Going home!");
                        // loop through all of the actions in the agent's list, and destroy all of the data holder entities
                        for(int i = 0; i < actions.Length; i++){
                            entityCommandBuffer.DestroyEntity(index,actions[i].dataHolder);
                        }  
                        entityCommandBuffer.DestroyEntity(index, entity); // remove the crowd agent
                    }
                }
                else{ // if there are actions but this action is not the right one
                Debug.Log("Gotta change from going home!");
                    //If there were data to store, this would be the point to do it
                    entityCommandBuffer.RemoveComponent<HasReynoldsSeekTargetPos>(index, entity); // remove the seek target pos from the crowd agent
                    entityCommandBuffer.RemoveComponent<GoHomeAction>(index, entity); // remove the going home action from the crowd agent
                    entityCommandBuffer.AddComponent<ChangeAction>(index, entity, new ChangeAction {}); //signify that the action should be changed
                    
                }
            }
            else{ // if there are no actions in the action queue
                Debug.Log("Nothin left!");
                entityCommandBuffer.RemoveComponent<HasReynoldsSeekTargetPos>(index, entity); // remove the seek target pos from the crowd agent
                entityCommandBuffer.RemoveComponent<GoHomeAction>(index, entity); // remove the going home action from the crowd agent
                entityCommandBuffer.AddComponent<ChangeAction>(index, entity, new ChangeAction {}); //signify that the action should be changed (will remove action)
                    
            }
        }
    }

    protected override void OnCreate() {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps){

        GoHomeJob homeJob = new GoHomeJob{ // creates the "go home" job
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent()
        };
        JobHandle jobHandle = homeJob.Schedule(this, inputDeps);

        commandBufferSystem.AddJobHandleForProducer(jobHandle); // tell the system to execute the command buffer after the job has been completed

        return jobHandle;
    }
}
