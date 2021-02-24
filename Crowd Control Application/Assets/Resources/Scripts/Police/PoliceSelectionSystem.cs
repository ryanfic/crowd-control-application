/*using System.Collections;
using System.Collections.Generic;*/
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;

public class PoliceSelectionSystem : SystemBase
{
    //Variables for selecting police units with mouse button presses
    private bool mouseEventTriggered;
    private float minX;
    private float maxX;
    private float minZ;
    private float maxZ;

    //Variables for selecting police units with voice commands
    private bool voiceSelectUnitEventTriggered;
    private NativeArray<FixedString64> selectedUnitName;
    private EntityQueryDesc selectSingleUnitQueryDesc;

    private bool voiceSelectAllUnitsEventTriggered;
    private EntityQueryDesc selectAllUnitsQueryDesc;

    private bool voiceDeselectAllUnitsEventTriggered;
    private EntityQueryDesc deselectAllUnitsQueryDesc;


    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    //Jobs

    //A job for selecting a particular police unit
    [BurstCompile]
    private struct SelectSingleUnitJob : IJobChunk {
        public EntityCommandBuffer.ParallelWriter commandBuffer;

        [ReadOnly] public EntityTypeHandle entityType;
        [ReadOnly] public ComponentTypeHandle<PoliceUnitName> nameType;
        [ReadOnly] public ComponentTypeHandle<SelectedPoliceUnit> selectedType;

        [ReadOnly] public NativeArray<FixedString64> selectedUnitName;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            NativeArray<PoliceUnitName> nameArray = chunk.GetNativeArray(nameType);

            if(chunk.Has(selectedType)){ //if the chunk has police units that are selected
                for(int i = 0; i < chunk.Count; i++){
                    if(nameArray[i].String != selectedUnitName[0]){ //if the name of the police unit that was selected is not the same as the police unit in question
                        commandBuffer.RemoveComponent<SelectedPoliceUnit>(chunkIndex,entityArray[i]); //deselect the police unit
                    }
                }
            }
            else{ // if the chunk does not have selected police units
                for(int i = 0; i < chunk.Count; i++){
                    if(nameArray[i].String == selectedUnitName[0]){ //if the name of the police unit that was selected is the same as the police unit in question
                        commandBuffer.AddComponent<SelectedPoliceUnit>(chunkIndex,entityArray[i]); //select the police unit
                    }
                }
            }
        }
    }

    //A job for selecting all police units
    [BurstCompile]
    private struct SelectAllUnitsJob : IJobChunk {
        public EntityCommandBuffer.ParallelWriter commandBuffer;

        [ReadOnly] public EntityTypeHandle entityType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            for(int i = 0; i < chunk.Count; i++){
                commandBuffer.AddComponent<SelectedPoliceUnit>(chunkIndex,entityArray[i]); //select the police unit
            }
        }
    }

    //A job for deselecting all police units
    [BurstCompile]
    private struct DeselectAllUnitsJob : IJobChunk {
        public EntityCommandBuffer.ParallelWriter commandBuffer;

        [ReadOnly] public EntityTypeHandle entityType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            for(int i = 0; i < chunk.Count; i++){
                commandBuffer.RemoveComponent<SelectedPoliceUnit>(chunkIndex,entityArray[i]); //deselect the police unit
            }
        }
    }






    protected override void OnCreate(){
        //Mouse click controls set up
        mouseEventTriggered = false;
        minX = 0;
        maxX = 0;
        minZ = 0;
        maxZ = 0;
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        World.GetOrCreateSystem<UIController>().OnLeftMouseClick += LeftClickResponse;


        //Voice controller set up
        voiceSelectUnitEventTriggered = false;
        selectedUnitName = new NativeArray<FixedString64>(1,Allocator.Persistent);

        voiceSelectAllUnitsEventTriggered = false;

        voiceDeselectAllUnitsEventTriggered = false;

        //Obtain Voice Controller - There should only be one
        PoliceUnitVoiceController[] voiceControllers = UnityEngine.Object.FindObjectsOfType<PoliceUnitVoiceController>();
        if(voiceControllers.Length > 0){
            PoliceUnitVoiceController voiceController = voiceControllers[0]; // grab the voice controller if there is one
            voiceController.OnPoliceUnitSelectionCommand += VoicePoliceSelectionResponse;
            voiceController.OnDeselectPoliceUnitsCommand += VoicePoliceDeselectionResponse;
        }

        selectSingleUnitQueryDesc = new EntityQueryDesc{ // define query for selecting a single police unit
            All = new ComponentType[]{
                ComponentType.ReadOnly<PoliceUnitComponent>(),
                ComponentType.ReadOnly<PoliceUnitName>()
            }
        };

        selectAllUnitsQueryDesc = new EntityQueryDesc{ // define query for selecting a single police unit
            All = new ComponentType[]{
                ComponentType.ReadOnly<PoliceUnitComponent>()
            },
            None = new ComponentType[]{
                ComponentType.ReadOnly<SelectedPoliceUnit>()
            }
        };

        deselectAllUnitsQueryDesc = new EntityQueryDesc{ // define query for selecting a single police unit
            All = new ComponentType[]{
                ComponentType.ReadOnly<PoliceUnitComponent>(),
                ComponentType.ReadOnly<SelectedPoliceUnit>()
            }
        };

        //base.OnCreate();
    }

    protected override void OnDestroy(){
        selectedUnitName.Dispose();
        base.OnDestroy();
    }

    protected override void OnUpdate(){
        if(mouseEventTriggered){
            EntityCommandBuffer.ParallelWriter commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(); // create a command buffer
            //ForEach needs local variables, assign to local variables
            float Xmin = minX;
            float Xmax = maxX;
            float Zmin = minZ;
            float Zmax = maxZ;
            JobHandle selectionHandle = Entities
                .WithAll<PoliceUnitComponent>()
                .WithNone<SelectedPoliceUnit>()
                .ForEach((Entity policeUnit, int entityInQueryIndex, in Translation transl)=>{
                    if(transl.Value.x >= Xmin && transl.Value.x <= Xmax && transl.Value.z >= Zmin && transl.Value.z <= Zmax){ // If the translation is within the min/max of X and Z
                        //Debug.Log("SELECTED");
                        //Debug.Log("X: " + transl.Value.x + ", Z: " + transl.Value.z);
                        commandBuffer.AddComponent<SelectedPoliceUnit>(entityInQueryIndex, policeUnit); // Add component
                    }         
                }).Schedule(this.Dependency);
            JobHandle deselectionHandle = Entities
                .WithAll<PoliceUnitComponent,SelectedPoliceUnit>()
                .ForEach((Entity policeUnit, int entityInQueryIndex, in Translation transl)=>{
                    if(transl.Value.x < Xmin || transl.Value.x > Xmax || transl.Value.z < Zmin || transl.Value.z > Zmax){ // If the translation is without the min/max of X and Z
                        //Debug.Log("DESELECTED");
                        commandBuffer.RemoveComponent<SelectedPoliceUnit>(entityInQueryIndex, policeUnit); // Remove component
                    }           
                }).Schedule(selectionHandle);
            mouseEventTriggered = false;

            commandBufferSystem.AddJobHandleForProducer(selectionHandle);
            commandBufferSystem.AddJobHandleForProducer(deselectionHandle);

            this.Dependency = deselectionHandle;
        }
        else if(voiceSelectUnitEventTriggered){
            Debug.Log("Selected " + selectedUnitName[0] + " in selection system");

            EntityCommandBuffer.ParallelWriter commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(); // create a command buffer
            EntityQuery selectionQuery = GetEntityQuery(selectSingleUnitQueryDesc); // query the entities

            SelectSingleUnitJob selectUnitJob = new SelectSingleUnitJob{ 
                commandBuffer = commandBuffer,
                entityType =  GetEntityTypeHandle(),
                nameType = GetComponentTypeHandle<PoliceUnitName>(true),
                selectedType = GetComponentTypeHandle<SelectedPoliceUnit>(true),
                selectedUnitName = selectedUnitName
            };
            JobHandle selectUnitJobHandle = selectUnitJob.Schedule(selectionQuery, this.Dependency); //schedule the job
            commandBufferSystem.AddJobHandleForProducer(selectUnitJobHandle); // make sure the components get added/removed for the job

            this.Dependency = selectUnitJobHandle;

            voiceSelectUnitEventTriggered = false;
        }
        else if(voiceSelectAllUnitsEventTriggered){
            Debug.Log("Selected all units in selection system");

            EntityCommandBuffer.ParallelWriter commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(); // create a command buffer
            EntityQuery selectionQuery = GetEntityQuery(selectAllUnitsQueryDesc); // query the entities

            SelectAllUnitsJob selectAllUnitJob = new SelectAllUnitsJob{ 
                commandBuffer = commandBuffer,
                entityType =  GetEntityTypeHandle()
            };
            JobHandle selectAllUnitJobHandle = selectAllUnitJob.Schedule(selectionQuery, this.Dependency); //schedule the job
            commandBufferSystem.AddJobHandleForProducer(selectAllUnitJobHandle); // make sure the components get added/removed for the job

            this.Dependency = selectAllUnitJobHandle;

            voiceSelectAllUnitsEventTriggered = false;
        }
        else if(voiceDeselectAllUnitsEventTriggered){
            Debug.Log("Deselected all units in selection system");

            EntityCommandBuffer.ParallelWriter commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(); // create a command buffer
            EntityQuery deselectionQuery = GetEntityQuery(deselectAllUnitsQueryDesc); // query the entities

            DeselectAllUnitsJob deselectAllUnitJob = new DeselectAllUnitsJob{ 
                commandBuffer = commandBuffer,
                entityType =  GetEntityTypeHandle()
            };
            JobHandle deselectAllUnitJobHandle = deselectAllUnitJob.Schedule(deselectionQuery, this.Dependency); //schedule the job
            commandBufferSystem.AddJobHandleForProducer(deselectAllUnitJobHandle); // make sure the components get added/removed for the job

            this.Dependency = deselectAllUnitJobHandle;

            voiceDeselectAllUnitsEventTriggered = false;
        }
    }

    private void LeftClickResponse(object sender, OnLeftClickEventArgs e){
        if(e.FromPos.x <= e.ToPos.x){ // if the x in the FromPos position is smaller than in the ToPos
            minX = e.FromPos.x; // min x is frompos
            maxX = e.ToPos.x; // max x is topos
        }
        else{
            minX = e.ToPos.x; // min x is topos
            maxX = e.FromPos.x; // max x is frompos
        }
        if(e.FromPos.z <= e.ToPos.z){ // if the z in the FromPos position is smaller than in the ToPos
            minZ = e.FromPos.z; // min z is frompos
            maxZ = e.ToPos.z; // max z is topos
        }
        else{
            minZ = e.ToPos.z; // min z is topos
            maxZ = e.FromPos.z; // max z is frompos
        }

        mouseEventTriggered = true;
    }

    private void VoicePoliceSelectionResponse(object sender, OnPoliceUnitSelectionEventArgs args){
        if(args.UnitName == PoliceUnitVoiceController.AllUnitSelectName){ // if all police units were selected
            voiceSelectAllUnitsEventTriggered = true; // set flag to say all police units should be selected
        }
        else{ // if not all police units are selected, only select one unit
            voiceSelectUnitEventTriggered = true;
            selectedUnitName[0] = args.UnitName;
        }
    }

    private void VoicePoliceDeselectionResponse(object sender, System.EventArgs eventArgs){
        voiceDeselectAllUnitsEventTriggered = true; // set flag to say that all police units should be deselected
    }
}
