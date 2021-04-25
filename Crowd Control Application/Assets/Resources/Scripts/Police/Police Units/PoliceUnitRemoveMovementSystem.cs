using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;



// A system for removing any movement components on selected police units
// Acts in response to any calls to start doing a movement
public class PoliceUnitRemoveMovementSystem : SystemBase {
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else
    private EntityQueryDesc removeMovementQueryDesc;  // description for removing every movement type
    private EntityQueryDesc removeMovementNotFwdQueryDesc; // description for removing every movement type except moving forward
    private EntityQueryDesc removeMovementNotRotQueryDesc; // description for removing every movement type except rotating

    private bool removeNotFwd;
    private bool removeNotRot;

    // Remove movement components job
    private struct RemoveMovementComponentsJob : IJobChunk {
        public EntityCommandBuffer.ParallelWriter entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        
        [ReadOnly] public EntityTypeHandle entityType;
        [ReadOnly] public ComponentTypeHandle<PoliceUnitMovementDestination> moveDstnType;
        [ReadOnly] public ComponentTypeHandle<PoliceUnitMoveForward> moveFwdType;
        [ReadOnly] public ComponentTypeHandle<PoliceUnitContinuousRotation> moveRotType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            if(chunk.Has(moveDstnType)){
                Debug.Log("Removing Destination");
                for(int i = 0; i < chunk.Count; i++){
                    entityCommandBuffer.RemoveComponent<PoliceUnitMovementDestination>(chunkIndex, entityArray[i]); // remove the movement destination from the police unit
                }
            }
            if(chunk.Has(moveFwdType)){
                Debug.Log("Removing Fwd");
                for(int i = 0; i < chunk.Count; i++){
                    entityCommandBuffer.RemoveComponent<PoliceUnitMoveForward>(chunkIndex, entityArray[i]); // remove the move forward component from the police unit
                }
            }
            if(chunk.Has(moveRotType)){
                Debug.Log("Removing Rot");
                for(int i = 0; i < chunk.Count; i++){
                    entityCommandBuffer.RemoveComponent<PoliceUnitContinuousRotation>(chunkIndex, entityArray[i]); // remove the rotation component from the police unit
                }
            }
        }
    }
    
    protected override void OnCreate(){
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        removeMovementNotFwdQueryDesc = new EntityQueryDesc{ // define query for removing everyting but the move forward
            All = new ComponentType[]{
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<PoliceUnitComponent>(),
                ComponentType.ReadOnly<SelectedPoliceUnit>()
            },
            Any = new ComponentType[]{
                ComponentType.ReadOnly<PoliceUnitMovementDestination>(),
                ComponentType.ReadOnly<PoliceUnitContinuousRotation>()
            },
            None = new ComponentType[]{
                ComponentType.ReadOnly<PoliceUnitMoveForward>()
            }
        };
        removeMovementNotRotQueryDesc = new EntityQueryDesc{ // define query for removing everyting but the rotation
            All = new ComponentType[]{
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<PoliceUnitComponent>(),
                ComponentType.ReadOnly<SelectedPoliceUnit>()
            },
            Any = new ComponentType[]{
                ComponentType.ReadOnly<PoliceUnitMovementDestination>(),
                ComponentType.ReadOnly<PoliceUnitMoveForward>()                
            },
            None = new ComponentType[]{
                ComponentType.ReadOnly<PoliceUnitContinuousRotation>()
            }
        };
        removeMovementQueryDesc = new EntityQueryDesc{ //define query for removing everyting but the move to intersection
            All = new ComponentType[]{
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<PoliceUnitComponent>(),
                ComponentType.ReadOnly<SelectedPoliceUnit>()
            },
            Any = new ComponentType[]{
                ComponentType.ReadOnly<PoliceUnitMovementDestination>(),
                ComponentType.ReadOnly<PoliceUnitMoveForward>(),
                ComponentType.ReadOnly<PoliceUnitContinuousRotation>()
            }
        };

        
        

        removeNotFwd = false;
        removeNotRot = false;
        
        base.OnCreate();
    }

    protected override void OnUpdate(){
        //Define the job first
        RemoveMovementComponentsJob removalJob = new RemoveMovementComponentsJob{ // creates the remove movement job
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            entityType =  GetEntityTypeHandle(),
            moveDstnType = GetComponentTypeHandle<PoliceUnitMovementDestination>(true),
            moveFwdType = GetComponentTypeHandle<PoliceUnitMoveForward>(true),
            moveRotType = GetComponentTypeHandle<PoliceUnitContinuousRotation>(true)
        };
        
        if(removeNotFwd){ //if removing everything except the forward movement
            EntityQuery removeNotFwdQuery = GetEntityQuery(removeMovementNotFwdQueryDesc); // query the entities

            JobHandle removeNotFwdJobHandle = removalJob.Schedule(removeNotFwdQuery, this.Dependency); // schedule the job
            commandBufferSystem.AddJobHandleForProducer(removeNotFwdJobHandle); // tell the system to execute the command buffer after the job has been completed
            this.Dependency = removeNotFwdJobHandle; // update the dependencies
            removeNotFwd = false;
        }
        else if(removeNotRot){  //if removing everything except the rotation movement
            EntityQuery removeNotRotQuery = GetEntityQuery(removeMovementNotRotQueryDesc); // query the entities

            JobHandle removeNotRotJobHandle = removalJob.Schedule(removeNotRotQuery, this.Dependency);
            commandBufferSystem.AddJobHandleForProducer(removeNotRotJobHandle); // tell the system to execute the command buffer after the job has been completed
            this.Dependency = removeNotRotJobHandle; // update the dependencies
            removeNotRot = false;
        }
        else{ //if removing everything except the move to intersection
            EntityQuery removeQuery = GetEntityQuery(removeMovementQueryDesc); // query the entities

            JobHandle removeJobHandle = removalJob.Schedule(removeQuery, this.Dependency);
            commandBufferSystem.AddJobHandleForProducer(removeJobHandle); // tell the system to execute the command buffer after the job has been completed
            this.Dependency = removeJobHandle; // update the dependencies
        }       

        this.Enabled = false; // turn off the system until an event turns it back on
    }

    private void VoiceMovementCommandResponse(object sender, System.EventArgs eventArgs){
        EntityQuery removeQuery = GetEntityQuery(removeMovementQueryDesc); // query the entities
        if(removeQuery.CalculateEntityCount()>0){//check if there are any entities that match the query
            this.Enabled = true; // do the removal
        }
    }

    private void VoiceMoveForwardResponse(object sender, System.EventArgs eventArgs){
        EntityQuery removeNotFwdQuery = GetEntityQuery(removeMovementNotFwdQueryDesc); // query the entities

        if(removeNotFwdQuery.CalculateEntityCount()>0){//check if there are any entities that match the query
            removeNotFwd = true;
            this.Enabled = true; // do the removal
        }
    }
    private void VoiceRotateResponse(object sender, OnRotateEventArgs eventArgs){
        EntityQuery removeNotRotQuery = GetEntityQuery(removeMovementNotRotQueryDesc); // query the entities

        if(removeNotRotQuery.CalculateEntityCount()>0){//check if there are any entities that match the query
            removeNotRot = true;
            this.Enabled = true; // do the removal
        }
    }    

    public void ConnectToVoiceController(){
        //Obtain Voice Controller - There should only be one
        PoliceUnitVoiceController[] voiceControllers = Object.FindObjectsOfType<PoliceUnitVoiceController>();
        if(voiceControllers.Length > 0){
            PoliceUnitVoiceController voiceController = voiceControllers[0]; // grab the voice controller if there is one
            voiceController.OnMoveToIntersectionCommand += VoiceMovementCommandResponse;
            voiceController.OnMoveForwardCommand += VoiceMoveForwardResponse;
            voiceController.OnHaltCommand += VoiceMovementCommandResponse;
            voiceController.OnRotateCommand += VoiceRotateResponse;
        }
    }

    public void ConnectToPrototypeManager(){
        ScenarioTrigManager[] sceneManagers = Object.FindObjectsOfType<ScenarioTrigManager>();
        if(sceneManagers.Length > 0){
            sceneManagers[0].HaltPoliceEvent += VoiceMovementCommandResponse;
        }
    }
}
