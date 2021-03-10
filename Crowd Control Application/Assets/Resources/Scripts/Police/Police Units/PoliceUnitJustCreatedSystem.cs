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

public class OnPoliceUnitCreatedWithNameArgs : EventArgs{
    public string PoliceUnitName;
}
public class PoliceUnitJustCreatedSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else
    private EntityQueryDesc nameQueryDesc;

    public event EventHandler<OnPoliceUnitCreatedWithNameArgs> OnPoliceUnitCreatedWithName;
    //private DOTSEvents_NextFrame<NameToAddEvent> dotsEvents;
    private struct NameToAddEvent : IComponentData{
        public FixedString64 Name;
    }
    private NativeQueue<NameToAddEvent> eventQueue;

    //A job for adding the name of a newly created police unit to the voice controller
    [BurstCompile]
    private struct AddNameToVoiceControllerJob : IJobChunk {
        public EntityCommandBuffer.ParallelWriter commandBuffer;

        [ReadOnly] public EntityTypeHandle entityType;
        [ReadOnly] public ComponentTypeHandle<PoliceUnitName> nameType;

        //public DOTSEvents_NextFrame<NameToAddEvent>.EventTrigger eventTrigger;
        public NativeQueue<NameToAddEvent>.ParallelWriter eventQueueParallel;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            NativeArray<PoliceUnitName> nameArray = chunk.GetNativeArray(nameType);

            for(int i = 0; i < chunk.Count; i++){
                
                Entity entity = entityArray[i];
                PoliceUnitName policeUnitName = nameArray[i];

                /*eventTrigger.TriggerEvent(chunkIndex, new NameToAddEvent{
                    Name = policeUnitName.String
                });*/
                eventQueueParallel.Enqueue(new NameToAddEvent{
                    Name = policeUnitName.String
                });

                
                commandBuffer.RemoveComponent<PoliceUnitJustCreated>(chunkIndex,entity); //remove the 'just created' label from the police unit
            }
        }
    }

    protected override void OnCreate(){
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        nameQueryDesc = new EntityQueryDesc{ // define query for adding constant movement components
            All = new ComponentType[]{
                ComponentType.ReadOnly<PoliceUnitComponent>(),
                ComponentType.ReadOnly<PoliceUnitJustCreated>(),
                ComponentType.ReadOnly<PoliceUnitName>()
            }
        };

        //dotsEvents = new DOTSEvents_NextFrame<NameToAddEvent>(World);
        eventQueue = new NativeQueue<NameToAddEvent>(Allocator.Persistent); //create the event queue

        //Obtain Voice Controller - There should only be one
        /*PoliceUnitVoiceController[] voiceControllers = UnityEngine.Object.FindObjectsOfType<PoliceUnitVoiceController>();
        if(voiceControllers.Length > 0){
            PoliceUnitVoiceController voiceController = voiceControllers[0]; // grab the voice controller if there is one
            voiceController.OnMoveForwardCommand += VoiceMoveForwardResponse;
            voiceController.OnRotateCommand += VoiceRotateResponse;
        }*/
        
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
        
        //DOTSEvents_NextFrame<NameToAddEvent>.EventTrigger eventTrigger = dotsEvents.GetEventTrigger();
        NativeQueue<NameToAddEvent>.ParallelWriter eventQueueParallel = eventQueue.AsParallelWriter();
        
        AddNameToVoiceControllerJob addNameJob = new AddNameToVoiceControllerJob{ 
            commandBuffer = commandBuffer,
            entityType =  GetEntityTypeHandle(),
            nameType = GetComponentTypeHandle<PoliceUnitName>(true),
            //eventTrigger = eventTrigger
            eventQueueParallel = eventQueueParallel
        };
        JobHandle addNameJobHandle = addNameJob.Schedule(nameQuery, this.Dependency); //schedule the job
        commandBufferSystem.AddJobHandleForProducer(addNameJobHandle); // make sure the components get added/removed for the job

        addNameJobHandle.Complete();

        while(eventQueue.TryDequeue(out NameToAddEvent nameEvent)){
            OnPoliceUnitCreatedWithName?.Invoke(this, new OnPoliceUnitCreatedWithNameArgs{
                PoliceUnitName = nameEvent.Name.ToString()
            });

        }
        
        /*dotsEvents.CaptureEvents(addNameJobHandle, (NameToAddEvent addNameEvent)=>{
            Debug.Log("Capturing the event!!");
            OnPoliceUnitCreatedWithName?.Invoke(this, new OnPoliceUnitCreatedWithNameArgs{
                PoliceUnitName = addNameEvent.Name.ToString()
            });
        });*/
        
        //this.Dependency = addNameJobHandle;  
    }

    /*private void VoiceMoveForwardResponse(object sender, System.EventArgs eventArgs){
        EntityCommandBuffer.ParallelWriter commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(); // create a command buffer
        EntityQuery addFwdMvmntTagQuery = GetEntityQuery(addContMovementQueryDesc); // query the entities
            //Add label job
        AddMoveForwardTagJob addMoveForwardTagJob = new AddMoveForwardTagJob{ 
            commandBuffer = commandBuffer,
            entityType =  GetEntityTypeHandle()
        };
        JobHandle fwdTagJobHandle = addMoveForwardTagJob.Schedule(addFwdMvmntTagQuery, this.Dependency); //schedule the job
        commandBufferSystem.AddJobHandleForProducer(fwdTagJobHandle); // make sure the components get added/removed for the job
        this.Dependency = fwdTagJobHandle;     
    }

    private void VoiceRotateResponse(object sender, OnRotateEventArgs eventArgs){
        EntityCommandBuffer.ParallelWriter commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(); // create a command buffer
        EntityQuery addRotateTagQuery = GetEntityQuery(addContMovementQueryDesc); // query the entities
        NativeArray<bool> rotateLeft = new NativeArray<bool>(1,Allocator.TempJob);
        rotateLeft[0] = eventArgs.RotateLeft;
        Debug.Log("Left: " + rotateLeft[0]);
            //Add label job
        AddRotateTagJob addRotateTagJob = new AddRotateTagJob{ 
            commandBuffer = commandBuffer,
            entityType =  GetEntityTypeHandle(),
            leftTurn = rotateLeft
        };
        JobHandle rotateTagJobHandle = addRotateTagJob.Schedule(addRotateTagQuery, this.Dependency); //schedule the job
        commandBufferSystem.AddJobHandleForProducer(rotateTagJobHandle); // make sure the components get added/removed for the job
        this.Dependency = rotateTagJobHandle;     
    }*/
}

