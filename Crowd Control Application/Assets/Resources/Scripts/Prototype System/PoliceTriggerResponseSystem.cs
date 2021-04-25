using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

/*public class PoliceTriggerResponseSystem : SystemBase
{

    private bool ApproachedIntersection;
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    protected override void OnCreate(){
        ApproachedIntersection = false;
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        //World.GetOrCreateSystem<UIController>().On1Down += OneDownResponse;
        //World.GetOrCreateSystem<UIController>().On2Down += TwoDownResponse;


        ScenarioTrigManager[] sceneManagers = Object.FindObjectsOfType<ScenarioTrigManager>();
        if(sceneManagers.Length > 0){
            sceneManagers[0].OnApproachIntersectionTriggered += ApproachIntersectionTriggerResponse;
        }
        //Obtain Voice Controller - There should only be one
        /*PoliceUnitVoiceController[] voiceControllers = Object.FindObjectsOfType<PoliceUnitVoiceController>();
        if(voiceControllers.Length > 0){
            PoliceUnitVoiceController voiceController = voiceControllers[0]; // grab the voice controller if there is one
            voiceController.OnToParallelLooseCordonVoiceCommand += VoiceToParallelLooseCordonResponse;
            voiceController.OnToParallelTightCordonVoiceCommand += VoiceToParallelTightCordonResponse;
            voiceController.OnToSingleLooseCordonVoiceCommand += VoiceToSingleLooseCordonResponse;
            voiceController.OnToSingleTightCordonVoiceCommand += VoiceToSingleTightCordonResponse;
            voiceController.OnTo3SidedBoxVoiceCommand += VoiceTo3SidedBoxResponse;
            voiceController.OnToWedgeVoiceCommand += VoiceToWedgeResponse;
        }
        //Debug.Log(Object.FindObjectsOfType<Camera>().Length);
    }

    protected override void OnUpdate(){
        if(ApproachedIntersection){
            Debug.Log("It worked!!!");
            //
        }
        /*if(ToParallelLooseCordon){ 
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
            ToParallelLooseCordon = false;

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
                            }); // Add component to change to cordon
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

    private void ApproachIntersectionTriggerResponse(object sender, System.EventArgs eventArgs){
        ApproachedIntersection = true;
    }
}*/
