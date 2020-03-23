using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

// Removes an action from the action queue
public class RemoveActionSystem : JobComponentSystem {
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    private struct RemoveActionJob : IJobForEachWithEntity_EBC<Action,RemoveAction> {
        public EntityCommandBuffer.Concurrent entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        public void Execute(Entity entity, int index, DynamicBuffer<Action> actions, ref RemoveAction removal){
            if(actions.Length > 0){ //if there are actions
                int i = 0;
                while(i < actions.Length && actions[index].id != removal.id){ // find the index of the action
                    i++;
                }
                if(index != actions.Length){ // if the action was found before the end of the buffer
                    Debug.Log("Removing Action " + actions[i].id + "!");
                    ActionType aType = actions[i].type; // get the type of the action that was removed
                    entityCommandBuffer.DestroyEntity(index, actions[i].dataHolder); // delete the data holder for the action
                    actions.RemoveAt(i); //remove the action
                    entityCommandBuffer.AddComponent<ChangeAction>(index,entity, new ChangeAction{}); // tell the system that the current action should be changed
                }
            }
            entityCommandBuffer.RemoveComponent<RemoveAction>(index,entity); // remove this component
        }
    }

    protected override void OnCreate() {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps){

        RemoveActionJob removeJob = new RemoveActionJob{ // creates the "follow waypoints" job
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent()
        };
        JobHandle jobHandle = removeJob.Schedule(this, inputDeps);

        commandBufferSystem.AddJobHandleForProducer(jobHandle); // tell the system to execute the command buffer after the job has been completed

        return jobHandle;
    }
}
