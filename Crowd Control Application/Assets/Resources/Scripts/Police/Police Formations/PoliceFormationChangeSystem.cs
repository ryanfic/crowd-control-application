﻿using UnityEngine;
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
    private bool To3SidedBox;

    private static readonly float LooseCordonOfficerSpacing = 0.5f;
    private static readonly float threeSidedBoxOfficerSpacing = 0f;

    //private float LineSpacing;
    //private float LineWidth;

    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    protected override void OnCreate(){
        ToParallelLooseCordon = false;
        To3SidedBox = false;
        //LineSpacing = 2f;
        //LineWidth = 5f;
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        World.GetOrCreateSystem<UIController>().On1Down += OneDownResponse;
        World.GetOrCreateSystem<UIController>().On2Down += TwoDownResponse;

        //Obtain Voice Controller - There should only be one
        PoliceUnitVoiceController[] voiceControllers = Object.FindObjectsOfType<PoliceUnitVoiceController>();
        if(voiceControllers.Length > 0){
            PoliceUnitVoiceController voiceController = voiceControllers[0]; // grab the voice controller if there is one
            voiceController.OnToLooseCordonVoiceCommand += VoiceToParallelLooseCordonResponse;
            voiceController.OnTo3SidedBoxVoiceCommand += VoiceTo3SidedBoxResponse;
        }
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

    private void VoiceTo3SidedBoxResponse(object sender, OnTo3SidedBoxEventArgs eventArgs){
        To3SidedBox = true;
    }

}

