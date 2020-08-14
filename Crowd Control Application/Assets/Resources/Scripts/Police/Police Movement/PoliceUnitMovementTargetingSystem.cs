using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;


// A system for selecting where a police unit moves
public class PoliceUnitMovementTargetingSystem : JobComponentSystem {
    private bool rightUpTriggered;

    private float3 Target;

    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    protected override void OnCreate(){
        rightUpTriggered = false;
        Target = float3.zero;
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        World.GetOrCreateSystem<UIController>().OnRightMouseUp += RightUpResponse;
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps){
        if(rightUpTriggered){  // if right mouse is released set up destination
            float3 destination = Target;
            EntityCommandBuffer.Concurrent commandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent(); // create a command buffer
            JobHandle destinationHandle = Entities
                .WithAll<PoliceUnitComponent,SelectedPoliceUnit>() // if police units are selected
                .ForEach((Entity policeUnit, int entityInQueryIndex)=>{    
                    commandBuffer.AddComponent<PoliceUnitMovementDestination>(entityInQueryIndex, policeUnit, new PoliceUnitMovementDestination{
                        Value = destination
                    }); // Add destination component
                }).Schedule(inputDeps);
            rightUpTriggered = false;

            commandBufferSystem.AddJobHandleForProducer(destinationHandle);

            return destinationHandle;
        }
        else{
            return inputDeps;
        }        
    }

    private void RightUpResponse(object sender, OnRightUpEventArgs e){
        rightUpTriggered = true;
        Target = e.Pos;
        //Debug.Log("Right Click! At " + e.Pos);
    }

}