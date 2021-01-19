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

    private bool ToLooseCordon;
    private bool To3SidedBox;

    //private float LineSpacing;
    //private float LineWidth;

    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    protected override void OnCreate(){
        ToLooseCordon = false;
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
            voiceController.OnToLooseCordonVoiceCommand += VoiceToLooseCordonResponse;
            voiceController.OnTo3SidedBoxVoiceCommand += VoiceTo3SidedBoxResponse;
        }
        //Debug.Log(Object.FindObjectsOfType<Camera>().Length);
    }

    protected override void OnUpdate(){
        if(ToLooseCordon){ 
            //float spacing = LineSpacing;
            //float width = LineWidth;
            EntityCommandBuffer.Concurrent commandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent(); // create a command buffer
            JobHandle cordonHandle = Entities
                .WithAll<PoliceUnitComponent,SelectedPoliceUnit>() // if police units are selected
                .ForEach((Entity policeUnit, int entityInQueryIndex, in PoliceUnitDimensions dimensions , in DynamicBuffer<Child> children)=>{
                        for(int i = 0; i < children.Length; i++){
                            //Debug.Log("Changing : " + i +"!");
                            commandBuffer.AddComponent<ToLooseCordonFormComponent>(entityInQueryIndex, children[i].Value, new ToLooseCordonFormComponent{
                                LineSpacing = dimensions.LineSpacing,
                                LineLength = dimensions.LineLength,
                                LineWidth = dimensions.LineWidth
                            }); // Add component to change to cordon
                        }
                }).Schedule(this.Dependency);
            //OneDown = false;
            ToLooseCordon = false;

            commandBufferSystem.AddJobHandleForProducer(cordonHandle);

            this.Dependency = cordonHandle;
        }
        else if(To3SidedBox){ 
            //float spacing = LineSpacing;
            //float width = LineWidth;
            EntityCommandBuffer.Concurrent commandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent(); // create a command buffer
            JobHandle boxHandle = Entities
                .WithAll<PoliceUnitComponent,SelectedPoliceUnit>() // if police units are selected
                .ForEach((Entity policeUnit, int entityInQueryIndex, in PoliceUnitDimensions dimensions, in DynamicBuffer<Child> children)=>{
                        for(int i = 0; i < children.Length; i++){
                            //Debug.Log("Changing : " + i +"!");
                            commandBuffer.AddComponent<To3SidedBoxFormComponent>(entityInQueryIndex, children[i].Value, new To3SidedBoxFormComponent {
                                LineSpacing = dimensions.LineSpacing,
                                LineLength = dimensions.LineLength,
                                LineWidth = dimensions.LineWidth
                            }); // Add component to change to cordon
                        }
                }).Schedule(this.Dependency);
            To3SidedBox = false;

            commandBufferSystem.AddJobHandleForProducer(boxHandle);

            this.Dependency = boxHandle;
        }        
    }

    private void OneDownResponse(object sender, System.EventArgs e){
        // if one is pressed, change units to loose cordon
        ToLooseCordon = true;
        //Debug.Log("1 Pressed!");
    }

    private void TwoDownResponse(object sender, System.EventArgs eventArgs){
        // if two is pressed, change units to 3 sided box
        To3SidedBox = true;
        //Debug.Log("2 Pressed!");
    }

    private void VoiceToLooseCordonResponse(object sender, System.EventArgs eventArgs){
        ToLooseCordon = true;
    }

    private void VoiceTo3SidedBoxResponse(object sender, OnTo3SidedBoxEventArgs eventArgs){
        To3SidedBox = true;
    }

}

