using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine;
using System;

public class OnPoliceUnitDeletedWithNameArgs : EventArgs{
    public string PoliceUnitName;
}
public class PoliceUnitToDeleteSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else
    private EntityQueryDesc nameQueryDesc;

    public event EventHandler<OnPoliceUnitDeletedWithNameArgs> OnPoliceUnitDeletedWithName; 

    //private DOTSEvents_NextFrame<NameToAddEvent> dotsEvents;
    private struct NameToRemoveEvent : IComponentData{
        public FixedString64 Name;
    }
    private NativeQueue<NameToRemoveEvent> eventQueue;

    //A job for removing the name of a police unit that is to be deleted to the voice controller
    [BurstCompile]
    private struct RemoveNameToVoiceControllerJob : IJobChunk {
        public EntityCommandBuffer.ParallelWriter commandBuffer;

        [ReadOnly] public EntityTypeHandle entityType;
        [ReadOnly] public ComponentTypeHandle<PoliceUnitName> nameType;

        //public DOTSEvents_NextFrame<NameToAddEvent>.EventTrigger eventTrigger;
        public NativeQueue<NameToRemoveEvent>.ParallelWriter eventQueueParallel;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            NativeArray<PoliceUnitName> nameArray = chunk.GetNativeArray(nameType);

            for(int i = 0; i < chunk.Count; i++){
                
                Entity entity = entityArray[i];
                PoliceUnitName policeUnitName = nameArray[i];

                /*eventTrigger.TriggerEvent(chunkIndex, new NameToAddEvent{
                    Name = policeUnitName.String
                });*/
                eventQueueParallel.Enqueue(new NameToRemoveEvent{
                    Name = policeUnitName.String
                });

                
                commandBuffer.DestroyEntity(chunkIndex,entity); //remove the police unit
            }
        }
    }

    protected override void OnCreate(){
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        nameQueryDesc = new EntityQueryDesc{ // define query for adding constant movement components
            All = new ComponentType[]{
                ComponentType.ReadOnly<PoliceUnitComponent>(),
                ComponentType.ReadOnly<PoliceUnitToDelete>(),
                ComponentType.ReadOnly<PoliceUnitName>()
            }
        };

        eventQueue = new NativeQueue<NameToRemoveEvent>(Allocator.Persistent); //create the event queue
        
        base.OnCreate();
    }

    protected override void OnDestroy(){
        eventQueue.Dispose();
        base.OnDestroy();
    }
    protected override void OnUpdate(){
        EntityCommandBuffer.ParallelWriter commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(); // create a command buffer
        EntityQuery nameQuery = GetEntityQuery(nameQueryDesc); // query the entities
            //Add label job
        
        NativeQueue<NameToRemoveEvent>.ParallelWriter eventQueueParallel = eventQueue.AsParallelWriter();
        
        RemoveNameToVoiceControllerJob removeNameJob = new RemoveNameToVoiceControllerJob{ 
            commandBuffer = commandBuffer,
            entityType =  GetEntityTypeHandle(),
            nameType = GetComponentTypeHandle<PoliceUnitName>(true),
            eventQueueParallel = eventQueueParallel
        };
        JobHandle removeNameJobHandle = removeNameJob.Schedule(nameQuery, this.Dependency); //schedule the job
        commandBufferSystem.AddJobHandleForProducer(removeNameJobHandle); // make sure the components get added/removed for the job

        removeNameJobHandle.Complete();

        while(eventQueue.TryDequeue(out NameToRemoveEvent nameEvent)){
            OnPoliceUnitDeletedWithName?.Invoke(this, new OnPoliceUnitDeletedWithNameArgs{
                PoliceUnitName = nameEvent.Name.ToString()
            });

        }
         
    }

}


