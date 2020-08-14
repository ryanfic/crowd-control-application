using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;


// A system for changing the formation of a police unit
// Pressing 1 changes the unit(s) to cordon
// Pressing 2 changes the unit9s) to 3-sided box
public class PoliceFormationChangeSystem : JobComponentSystem {
    private bool OneDown;
    private bool TwoDown;

    private float LineSpacing;
    private float LineWidth;

    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    protected override void OnCreate(){
        OneDown = false;
        TwoDown = false;
        LineSpacing = 2f;
        LineWidth = 5f;
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        World.GetOrCreateSystem<UIController>().On1Down += OneDownResponse;
        World.GetOrCreateSystem<UIController>().On2Down += TwoDownResponse;
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps){
        if(OneDown){  // if one is pressed, change units to cordon
            float spacing = LineSpacing;
            float width = LineWidth;
            EntityCommandBuffer.Concurrent commandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent(); // create a command buffer
            JobHandle cordonHandle = Entities
                .WithAll<PoliceUnitComponent,SelectedPoliceUnit>() // if police units are selected
                .ForEach((Entity policeUnit, int entityInQueryIndex, in DynamicBuffer<Child> children)=>{
                        for(int i = 0; i < children.Length; i++){
                            //Debug.Log("Changing : " + i +"!");
                            commandBuffer.AddComponent<ToCordonFormComponent>(entityInQueryIndex, children[i].Value, new ToCordonFormComponent{
                                LineSpacing = spacing,
                                LineWidth = width
                            }); // Add component to change to cordon
                        }
                }).Schedule(inputDeps);
            OneDown = false;

            commandBufferSystem.AddJobHandleForProducer(cordonHandle);

            return cordonHandle;
        }
        else if(TwoDown){ // if two is pressed, change units to 3 sided box
            float spacing = LineSpacing;
            float width = LineWidth;
            EntityCommandBuffer.Concurrent commandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent(); // create a command buffer
            JobHandle boxHandle = Entities
                .WithAll<PoliceUnitComponent,SelectedPoliceUnit>() // if police units are selected
                .ForEach((Entity policeUnit, int entityInQueryIndex, in DynamicBuffer<Child> children)=>{
                        for(int i = 0; i < children.Length; i++){
                            Debug.Log("Changing : " + i +"!");
                            commandBuffer.AddComponent<To3SidedBoxFormComponent>(entityInQueryIndex, children[i].Value, new To3SidedBoxFormComponent {
                                LineSpacing = spacing,
                                LineWidth = width
                            }); // Add component to change to cordon
                        }
                }).Schedule(inputDeps);
            TwoDown = false;

            commandBufferSystem.AddJobHandleForProducer(boxHandle);

            return boxHandle;
        }
        else{
            return inputDeps;
        }        
    }

    private void OneDownResponse(object sender, System.EventArgs e){
        OneDown = true;
        //Debug.Log("1 Pressed!");
    }

    private void TwoDownResponse(object sender, System.EventArgs eventArgs){
        TwoDown = true;
        //Debug.Log("2 Pressed!");
    }

}

