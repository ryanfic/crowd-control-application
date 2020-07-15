using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;


// A system for changing the formation of a police unit to a 3 sided box
public class To3SidedBoxSystem : JobComponentSystem {
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else
    private static float tolerance = 0.1f;

    // the job used when it is the front police line
    private struct ToBoxFrontJob : IJobForEachWithEntity<To3SidedBoxFormComponent,FrontPoliceLineComponent,Translation> {
        public EntityCommandBuffer.Concurrent entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        
        public void Execute(Entity entity, int index, [ReadOnly] ref To3SidedBoxFormComponent toBox, [ReadOnly] ref FrontPoliceLineComponent frontLine, ref Translation trans){
            float3 destination = new float3(0f,0f,toBox.LineWidth/2); // the front line should move ahead of the center point of the unit by half the width of a police line
            if(math.distance(trans.Value, destination) < tolerance){ // if the entity is within tolerance of the destination
                entityCommandBuffer.RemoveComponent<To3SidedBoxFormComponent>(index, entity); // remove the formation change component from the police line
            }
            else{ // if not at the destination
                trans.Value = destination; // move to the destination
            }
        }
    }
    // the job used when it is the center police line
    private struct ToBoxCenterJob : IJobForEachWithEntity<To3SidedBoxFormComponent,CenterPoliceLineComponent,Translation,Rotation> {
        public EntityCommandBuffer.Concurrent entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        
        public void Execute(Entity entity, int index, [ReadOnly] ref To3SidedBoxFormComponent toBox, [ReadOnly] ref CenterPoliceLineComponent centerLine, ref Translation trans, ref Rotation rot){
            float3 destination = new float3(-toBox.LineWidth/2,0f,0f); // the front line should move ahead of the center point of the unit by half the width of a police line
            if(math.distance(trans.Value, destination) < tolerance){ // if the entity is within tolerance of the destination
                entityCommandBuffer.RemoveComponent<To3SidedBoxFormComponent>(index, entity); // remove the formation change component from the police line
            }
            else{ // if not at the destination
                trans.Value = destination; // move to the destination
                rot.Value = quaternion.RotateY(math.radians(-90)); // rotate to the left
            }
        }
    }
    // the job used when it is the front police line
    private struct ToBoxRearJob : IJobForEachWithEntity<To3SidedBoxFormComponent,RearPoliceLineComponent,Translation,Rotation> {
        public EntityCommandBuffer.Concurrent entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        
        public void Execute(Entity entity, int index, [ReadOnly] ref To3SidedBoxFormComponent toBox, [ReadOnly] ref RearPoliceLineComponent rearLine, ref Translation trans, ref Rotation rot){
            float3 destination = new float3(toBox.LineWidth/2,0f,0f); // the front line should move ahead of the center point of the unit by half the width of a police line
            if(math.distance(trans.Value, destination) < tolerance){ // if the entity is within tolerance of the destination
                entityCommandBuffer.RemoveComponent<To3SidedBoxFormComponent>(index, entity); // remove the formation change component from the police line
            }
            else{ // if not at the destination
                trans.Value = destination; // move to the destination
                rot.Value = quaternion.RotateY(math.radians(90)); // rotate to the right
            }
        }
    }


    protected override void OnCreate() {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps){
        ToBoxFrontJob frontJob = new ToBoxFrontJob{ // creates the to 3 sided box front job
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent()
        };
        JobHandle jobHandle1 = frontJob.Schedule(this, inputDeps);
        ToBoxCenterJob centerJob = new ToBoxCenterJob{
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent()
        };
        JobHandle jobHandle2 = centerJob.Schedule(this, jobHandle1);
        ToBoxRearJob rearJob = new ToBoxRearJob{
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent()
        };
        JobHandle jobHandle3 = rearJob.Schedule(this, jobHandle2);
        

        commandBufferSystem.AddJobHandleForProducer(jobHandle3); // tell the system to execute the command buffer after the job has been completed


        return jobHandle3;
    }
}
