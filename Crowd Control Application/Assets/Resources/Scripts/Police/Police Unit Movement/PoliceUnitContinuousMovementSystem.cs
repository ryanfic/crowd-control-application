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

[UpdateAfter(typeof(PoliceUnitRemoveMovementSystem))]

public class PoliceUnitContinuousMovementSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else
    private EntityQueryDesc addContMovementQueryDesc;

    //A job for adding the "Move forward" tag to all selected police units
    [BurstCompile]
    private struct AddMoveForwardTagJob : IJobChunk {
        public EntityCommandBuffer.ParallelWriter commandBuffer;

        [ReadOnly] public EntityTypeHandle entityType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);

            for(int i = 0; i < chunk.Count; i++){
                
                Entity entity = entityArray[i];
                commandBuffer.AddComponent<PoliceUnitMoveForward>(chunkIndex,entity,new PoliceUnitMoveForward{}); //Add location target to entity so it starts moving towards the intersection
            }
        }
    }

    //A job for adding the "Rotate" tag to all selected police units
    [BurstCompile]
    private struct AddRotateTagJob : IJobChunk {
        public EntityCommandBuffer.ParallelWriter commandBuffer;

        [ReadOnly] public EntityTypeHandle entityType;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<bool> leftTurn;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);

            for(int i = 0; i < chunk.Count; i++){
                
                Entity entity = entityArray[i];
                commandBuffer.AddComponent<PoliceUnitContinuousRotation>(chunkIndex,entity,new PoliceUnitContinuousRotation{
                    RotateLeft = leftTurn[0],
                    WaitingAtAngle = false,
                    WaitTime = 0,
                    LastWaitAngle = -1
                }); //Add location target to entity so it starts moving towards the intersection
            }
        }
    }

    protected override void OnCreate(){
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        addContMovementQueryDesc = new EntityQueryDesc{ // define query for adding constant movement components
            All = new ComponentType[]{
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<PoliceUnitComponent>(),
                ComponentType.ReadOnly<SelectedPoliceUnit>()
            }
        };

        
        
        base.OnCreate();
        this.Enabled = false;
    }
    protected override void OnUpdate(){}

    private void VoiceMoveForwardResponse(object sender, System.EventArgs eventArgs){
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
    }

    public void ConnectToVoiceController(){
        //Obtain Voice Controller - There should only be one
        PoliceUnitVoiceController[] voiceControllers = Object.FindObjectsOfType<PoliceUnitVoiceController>();
        if(voiceControllers.Length > 0){
            PoliceUnitVoiceController voiceController = voiceControllers[0]; // grab the voice controller if there is one
            voiceController.OnMoveForwardCommand += VoiceMoveForwardResponse;
            voiceController.OnRotateCommand += VoiceRotateResponse;
        }
    }
    
}
