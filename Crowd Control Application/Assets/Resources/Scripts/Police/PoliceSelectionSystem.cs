/*using System.Collections;
using System.Collections.Generic;*/
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

public class PoliceSelectionSystem : SystemBase
{
    private bool mouseEventTriggered;
    private float minX;
    private float maxX;
    private float minZ;
    private float maxZ;
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    protected override void OnCreate(){
        mouseEventTriggered = false;
        minX = 0;
        maxX = 0;
        minZ = 0;
        maxZ = 0;
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        World.GetOrCreateSystem<UIController>().OnLeftMouseClick += LeftClickResponse;
        //base.OnCreate();
    }

    protected override void OnUpdate(){
        if(mouseEventTriggered){
            EntityCommandBuffer.Concurrent commandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent(); // create a command buffer
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
}
