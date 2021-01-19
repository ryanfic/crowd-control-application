using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using crowd_Actions;

// Removes an action from the action queue
public class RemoveActionSystem : SystemBase {
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    EntityQueryDesc removeActionQueryDec;

    private struct RemoveActionJob : IJobChunk {
        public EntityCommandBuffer.Concurrent entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job

        [ReadOnly] public ArchetypeChunkEntityType entityType;
        public ArchetypeChunkComponentType<RemoveAction> removeActionType;
        public ArchetypeChunkBufferType<Action> actionBufferType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            NativeArray<RemoveAction> removeActionArray = chunk.GetNativeArray(removeActionType);
            BufferAccessor<Action> buffers = chunk.GetBufferAccessor<Action>(actionBufferType);

            for(int i = 0; i < chunk.Count; i++){
                Entity entity = entityArray[i];
                DynamicBuffer<Action> actions = buffers[i];
                RemoveAction removal = removeActionArray[i];

                if(actions.Length > 0){ //if there are actions
                    int j = 0;
                    while(j < actions.Length && actions[j].id != removal.id){ // find the index of the action
                        j++;
                    }
                    if(j < actions.Length){ // if the action was found before the end of the buffer
                        Debug.Log("Removing Action " + "!");
                        ActionType aType = actions[j].type; // get the type of the action that was removed
                        entityCommandBuffer.DestroyEntity(chunkIndex, actions[j].dataHolder); // delete the data holder for the action
                        actions.RemoveAt(j); //remove the action
                        entityCommandBuffer.AddComponent<ChangeAction>(chunkIndex,entity, new ChangeAction{}); // tell the system that the current action should be changed
                    }
                    else{
                        Debug.Log("Failed to remove " + "!");
                    }
                }
                entityCommandBuffer.RemoveComponent<RemoveAction>(chunkIndex,entity); // remove this component
            }
        }
    }

    protected override void OnCreate() {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        removeActionQueryDec = new EntityQueryDesc{
            All = new ComponentType[]{
                typeof(Action),
                ComponentType.ReadOnly<RemoveAction>()
            }
        };
        base.OnCreate();
    }

    protected override void OnUpdate(){
        EntityQuery removeActionQuery = GetEntityQuery(removeActionQueryDec); // query the entities

        RemoveActionJob removeJob = new RemoveActionJob{ // creates the "follow waypoints" job
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent(),
            entityType =  GetArchetypeChunkEntityType(),
            removeActionType = GetArchetypeChunkComponentType<RemoveAction>(),
            actionBufferType = GetArchetypeChunkBufferType<Action>()

        };
        JobHandle jobHandle = removeJob.Schedule(removeActionQuery, this.Dependency);

        commandBufferSystem.AddJobHandleForProducer(jobHandle); // tell the system to execute the command buffer after the job has been completed

        this.Dependency = jobHandle;
    }
}
