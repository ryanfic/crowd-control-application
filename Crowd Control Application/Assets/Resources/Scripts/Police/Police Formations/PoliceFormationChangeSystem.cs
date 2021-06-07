using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;


// A system for changing the formation of a police unit
// Pressing 1 changes the unit(s) to loose cordon
// Pressing 2 changes the unit9s) to 3-sided box
public class PoliceFormationChangeSystem : SystemBase {
    //private bool OneDown;
    //private bool TwoDown;

    private bool ToParallelLooseCordon;
    private bool ToParallelTightCordon;
    private bool ToSingleLooseCordon;
    private bool ToSingleTightCordon;
    private bool To3SidedBox;
    private bool ToWedge;

    private static readonly float LooseCordonOfficerSpacing = 0.5f;
    private static readonly float threeSidedBoxOfficerSpacing = 0f;//0.4f;
    private static readonly float wedgeAngle = 60f; // Angle of wedge, in degrees
                                                    // Wedge angle should be between min (2*arctan(0.5 * officer width / officer length)) and max (2 * arctan(officer width / officer length))
                                                    // for equal officer length and width, min = 53.13, max = 90

    //private float LineSpacing;
    //private float LineWidth;

    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    protected override void OnCreate(){
        ToParallelLooseCordon = false;
        ToParallelTightCordon = false;
        ToSingleLooseCordon = false;
        ToSingleTightCordon = false;
        To3SidedBox = false;
        ToWedge = false;
        //LineSpacing = 2f;
        //LineWidth = 5f;
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        World.GetOrCreateSystem<UIController>().On1Down += OneDownResponse;
        World.GetOrCreateSystem<UIController>().On2Down += TwoDownResponse;


        Debug.Log("Let's see if this displays");
        //Obtain Voice Controller - There should only be one
        /*PoliceUnitVoiceController[] voiceControllers = Object.FindObjectsOfType<PoliceUnitVoiceController>();
        if(voiceControllers.Length > 0){
            Debug.Log("Found a voice controller");
            PoliceUnitVoiceController voiceController = voiceControllers[0]; // grab the voice controller if there is one
            voiceController.OnToParallelLooseCordonVoiceCommand += VoiceToParallelLooseCordonResponse;
            voiceController.OnToParallelTightCordonVoiceCommand += VoiceToParallelTightCordonResponse;
            voiceController.OnToSingleLooseCordonVoiceCommand += VoiceToSingleLooseCordonResponse;
            voiceController.OnToSingleTightCordonVoiceCommand += VoiceToSingleTightCordonResponse;
            voiceController.OnTo3SidedBoxVoiceCommand += VoiceTo3SidedBoxResponse;
            voiceController.OnToWedgeVoiceCommand += VoiceToWedgeResponse;
        }*/
        //Debug.Log(Object.FindObjectsOfType<Camera>().Length);
    }

    protected override void OnUpdate(){
        if(ToParallelLooseCordon){ 
            //float spacing = LineSpacing;
            //float width = LineWidth;
            EntityCommandBuffer.ParallelWriter commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(); // create a command buffer
            JobHandle cordonHandle = Entities
                .WithAll<PoliceUnitComponent,SelectedPoliceUnit>() // if police units are selected
                .ForEach((Entity policeUnit, int entityInQueryIndex, ref DynamicBuffer<OfficerInFormation> inFormation, in PoliceUnitDimensions dimensions, in DynamicBuffer<OfficerInPoliceUnit> officers)=>{
                        for(int i = 0; i < officers.Length; i++){
                            //Debug.Log("Changing : " + i +"!");
                            commandBuffer.AddComponent<ToParallelCordonFormComponent>(entityInQueryIndex, officers[i].officer, new ToParallelCordonFormComponent{ 
                                LineSpacing = dimensions.LineSpacing,
                                OfficerLength = dimensions.OfficerLength,
                                OfficerWidth = dimensions.OfficerWidth, 
                                OfficerSpacing = LooseCordonOfficerSpacing,
                                NumOfficersInLine1 = dimensions.NumOfficersInLine1,
                                NumOfficersInLine2 = dimensions.NumOfficersInLine2,
                                NumOfficersInLine3 = dimensions.NumOfficersInLine3
                            }); // Add component to change to cordon
                        }
                        inFormation.Clear();
                        commandBuffer.AddComponent<PoliceUnitGettingIntoFormation>(entityInQueryIndex, policeUnit, new PoliceUnitGettingIntoFormation{});
                }).Schedule(this.Dependency);
            //OneDown = false;
            ToParallelLooseCordon = false;

            commandBufferSystem.AddJobHandleForProducer(cordonHandle);

            this.Dependency = cordonHandle;
        }
        else if(ToParallelTightCordon){ 
            //float spacing = LineSpacing;
            //float width = LineWidth;
            EntityCommandBuffer.ParallelWriter commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(); // create a command buffer
            JobHandle cordonHandle = Entities
                .WithAll<PoliceUnitComponent,SelectedPoliceUnit>() // if police units are selected
                .ForEach((Entity policeUnit, int entityInQueryIndex, ref DynamicBuffer<OfficerInFormation> inFormation, in PoliceUnitDimensions dimensions, in DynamicBuffer<OfficerInPoliceUnit> officers)=>{
                        for(int i = 0; i < officers.Length; i++){
                            //Debug.Log("Changing : " + i +"!");
                            commandBuffer.AddComponent<ToParallelCordonFormComponent>(entityInQueryIndex, officers[i].officer, new ToParallelCordonFormComponent{ 
                                LineSpacing = dimensions.LineSpacing,
                                OfficerLength = dimensions.OfficerLength,
                                OfficerWidth = dimensions.OfficerWidth, 
                                OfficerSpacing = 0f,
                                NumOfficersInLine1 = dimensions.NumOfficersInLine1,
                                NumOfficersInLine2 = dimensions.NumOfficersInLine2,
                                NumOfficersInLine3 = dimensions.NumOfficersInLine3
                            }); // Add component to change to cordon
                        }
                        inFormation.Clear();
                        commandBuffer.AddComponent<PoliceUnitGettingIntoFormation>(entityInQueryIndex, policeUnit, new PoliceUnitGettingIntoFormation{});
                }).Schedule(this.Dependency);
            //OneDown = false;
            ToParallelTightCordon = false;

            commandBufferSystem.AddJobHandleForProducer(cordonHandle);

            this.Dependency = cordonHandle;
        }
        else if(ToSingleLooseCordon){ 
            //float spacing = LineSpacing;
            //float width = LineWidth;
            EntityCommandBuffer.ParallelWriter commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(); // create a command buffer
            JobHandle cordonHandle = Entities
                .WithAll<PoliceUnitComponent,SelectedPoliceUnit>() // if police units are selected
                .ForEach((Entity policeUnit, int entityInQueryIndex,  ref DynamicBuffer<OfficerInFormation> inFormation, in PoliceUnitDimensions dimensions, in DynamicBuffer<OfficerInPoliceUnit> officers)=>{
                        for(int i = 0; i < officers.Length; i++){
                            //Debug.Log("Changing : " + i +"!");
                            commandBuffer.AddComponent<ToSingleCordonFormComponent>(entityInQueryIndex, officers[i].officer, new ToSingleCordonFormComponent {
                                OfficerWidth = dimensions.OfficerWidth, 
                                OfficerSpacing = LooseCordonOfficerSpacing,
                                NumOfficersInLine1 = dimensions.NumOfficersInLine1,
                                NumOfficersInLine2 = dimensions.NumOfficersInLine2,
                                NumOfficersInUnit = dimensions.NumOfficersInLine1 + dimensions.NumOfficersInLine2 + dimensions.NumOfficersInLine3
                            }); // Add component to change to cordon
                        }
                        inFormation.Clear();
                        commandBuffer.AddComponent<PoliceUnitGettingIntoFormation>(entityInQueryIndex, policeUnit, new PoliceUnitGettingIntoFormation{});
                }).Schedule(this.Dependency);
            ToSingleLooseCordon = false;

            commandBufferSystem.AddJobHandleForProducer(cordonHandle);

            this.Dependency = cordonHandle;
        } 
        else if(ToSingleTightCordon){ 
            //float spacing = LineSpacing;
            //float width = LineWidth;
            EntityCommandBuffer.ParallelWriter commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(); // create a command buffer
            JobHandle cordonHandle = Entities
                .WithAll<PoliceUnitComponent,SelectedPoliceUnit>() // if police units are selected
                .ForEach((Entity policeUnit, int entityInQueryIndex,  ref DynamicBuffer<OfficerInFormation> inFormation, in PoliceUnitDimensions dimensions, in DynamicBuffer<OfficerInPoliceUnit> officers)=>{
                        for(int i = 0; i < officers.Length; i++){
                            //Debug.Log("Changing : " + i +"!");
                            commandBuffer.AddComponent<ToSingleCordonFormComponent>(entityInQueryIndex, officers[i].officer, new ToSingleCordonFormComponent {
                                OfficerWidth = dimensions.OfficerWidth, 
                                OfficerSpacing = 0f,
                                NumOfficersInLine1 = dimensions.NumOfficersInLine1,
                                NumOfficersInLine2 = dimensions.NumOfficersInLine2,
                                NumOfficersInUnit = dimensions.NumOfficersInLine1 + dimensions.NumOfficersInLine2 + dimensions.NumOfficersInLine3
                            }); // Add component to change to cordon
                        }
                        inFormation.Clear();
                        commandBuffer.AddComponent<PoliceUnitGettingIntoFormation>(entityInQueryIndex, policeUnit, new PoliceUnitGettingIntoFormation{});
                }).Schedule(this.Dependency);
            ToSingleTightCordon = false;

            commandBufferSystem.AddJobHandleForProducer(cordonHandle);

            this.Dependency = cordonHandle;
        } 
        else if(To3SidedBox){ 
            //float spacing = LineSpacing;
            //float width = LineWidth;
            Debug.Log("Called inside onupdate");
            EntityCommandBuffer.ParallelWriter commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(); // create a command buffer
            JobHandle boxHandle = Entities
                .WithAll<PoliceUnitComponent,SelectedPoliceUnit>() // if police units are selected
                .ForEach((Entity policeUnit, int entityInQueryIndex,  ref DynamicBuffer<OfficerInFormation> inFormation, in PoliceUnitDimensions dimensions, in DynamicBuffer<OfficerInPoliceUnit> officers)=>{
                        for(int i = 0; i < officers.Length; i++){
                            //Debug.Log("Changing : " + i +"!");
                            commandBuffer.AddComponent<To3SidedBoxFormComponent>(entityInQueryIndex, officers[i].officer, new To3SidedBoxFormComponent {
                                LineSpacing = dimensions.LineSpacing,
                                OfficerLength = dimensions.OfficerLength,
                                OfficerWidth = dimensions.OfficerWidth, 
                                OfficerSpacing = threeSidedBoxOfficerSpacing,
                                NumOfficersInLine1 = dimensions.NumOfficersInLine1,
                                NumOfficersInLine2 = dimensions.NumOfficersInLine2,
                                NumOfficersInLine3 = dimensions.NumOfficersInLine3
                            }); // Add component to change to Three sided box
                        }
                        inFormation.Clear();
                        commandBuffer.AddComponent<PoliceUnitGettingIntoFormation>(entityInQueryIndex, policeUnit, new PoliceUnitGettingIntoFormation{});
                }).Schedule(this.Dependency);
            To3SidedBox = false;

            commandBufferSystem.AddJobHandleForProducer(boxHandle);

            this.Dependency = boxHandle;
        } 
        else if(ToWedge){ 
            EntityCommandBuffer.ParallelWriter commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(); // create a command buffer
            JobHandle wedgeHandle = Entities
                .WithAll<PoliceUnitComponent,SelectedPoliceUnit>() // if police units are selected
                .ForEach((Entity policeUnit, int entityInQueryIndex,  ref DynamicBuffer<OfficerInFormation> inFormation, in PoliceUnitDimensions dimensions, in DynamicBuffer<OfficerInPoliceUnit> officers)=>{
                        int totalOfficers = dimensions.NumOfficersInLine1 + dimensions.NumOfficersInLine2 + dimensions.NumOfficersInLine3;
                        int remainder = totalOfficers % 2;
                        int midOfficerNum = (totalOfficers + remainder)/2;
                        for(int i = 0; i < officers.Length; i++){
                            commandBuffer.AddComponent<ToWedgeFormComponent>(entityInQueryIndex, officers[i].officer, new ToWedgeFormComponent {
                                Angle = wedgeAngle,
                                OfficerLength = dimensions.OfficerLength,
                                OfficerWidth = dimensions.OfficerWidth, 
                                NumOfficersInLine1 = dimensions.NumOfficersInLine1,
                                NumOfficersInLine2 = dimensions.NumOfficersInLine2,
                                NumOfficersInLine3 = dimensions.NumOfficersInLine3,
                                MiddleOfficerNum = midOfficerNum,
                                TotalOfficers = totalOfficers
                            }); // Add component to change to wedge
                        }
                        inFormation.Clear();
                        commandBuffer.AddComponent<PoliceUnitGettingIntoFormation>(entityInQueryIndex, policeUnit, new PoliceUnitGettingIntoFormation{});
                }).Schedule(this.Dependency);
            ToWedge = false;

            commandBufferSystem.AddJobHandleForProducer(wedgeHandle);

            this.Dependency = wedgeHandle;
        }        
    }

    private void OneDownResponse(object sender, System.EventArgs e){
        // if one is pressed, change units to loose cordon
        ToParallelLooseCordon = true;
        //Debug.Log("1 Pressed!");
    }

    private void TwoDownResponse(object sender, System.EventArgs eventArgs){
        // if two is pressed, change units to 3 sided box
        To3SidedBox = true;
        //Debug.Log("2 Pressed!");
    }

    private void VoiceToParallelLooseCordonResponse(object sender, System.EventArgs eventArgs){
        ToParallelLooseCordon = true;
    }

    private void VoiceToParallelTightCordonResponse(object sender, System.EventArgs eventArgs){
        ToParallelTightCordon = true;
    }

    private void VoiceToSingleLooseCordonResponse(object sender, System.EventArgs eventArgs){
        ToSingleLooseCordon = true;
    }

    private void VoiceToSingleTightCordonResponse(object sender, System.EventArgs eventArgs){
        ToSingleTightCordon = true;
    }

    private void VoiceTo3SidedBoxResponse(object sender, OnTo3SidedBoxEventArgs eventArgs){
        To3SidedBox = true;
        Debug.Log("Called inside response");
    }

    private void VoiceToWedgeResponse(object sender, System.EventArgs eventArgs){
        ToWedge = true;
    }
    public void ConnectToVoiceController(){
        PoliceUnitVoiceController[] voiceControllers = Object.FindObjectsOfType<PoliceUnitVoiceController>();
        if(voiceControllers.Length > 0){
            Debug.Log("Found a voice controller");
            PoliceUnitVoiceController voiceController = voiceControllers[0]; // grab the voice controller if there is one
            voiceController.OnToParallelLooseCordonVoiceCommand += VoiceToParallelLooseCordonResponse;
            voiceController.OnToParallelTightCordonVoiceCommand += VoiceToParallelTightCordonResponse;
            voiceController.OnToSingleLooseCordonVoiceCommand += VoiceToSingleLooseCordonResponse;
            voiceController.OnToSingleTightCordonVoiceCommand += VoiceToSingleTightCordonResponse;
            voiceController.OnTo3SidedBoxVoiceCommand += VoiceTo3SidedBoxResponse;
            voiceController.OnToWedgeVoiceCommand += VoiceToWedgeResponse;
        }
    }

    public void ConnectToFormationHandler(){
        EISICFormationHandler[] handlers = Object.FindObjectsOfType<EISICFormationHandler>();
        if(handlers.Length > 0){
            Debug.Log("Found a Formation handler");
            EISICFormationHandler handler = handlers[0]; // grab the formation handler if there is one
            handler.OnToParallelTightCordonCommand += VoiceToParallelTightCordonResponse;
            handler.OnTo3SidedBoxCommand += VoiceTo3SidedBoxResponse;
            handler.OnToWedgeCommand += VoiceToWedgeResponse;
        }
    }
}

