using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;


// A system for changing the formation of a police unit to a 3 sided box
public class ToCordonSystem : JobComponentSystem {
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else
    private static float tolerance = 0.1f;

    // the job used when it is the front police line
    private struct ToCordonFrontJob : IJobForEachWithEntity<ToCordonFormComponent,FrontPoliceLineComponent,Translation> {
        public EntityCommandBuffer.Concurrent entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        
        public void Execute(Entity entity, int index, [ReadOnly] ref ToCordonFormComponent toCordon, [ReadOnly] ref FrontPoliceLineComponent frontLine, ref Translation trans){
            float3 destination = new float3(0f,0f,toCordon.LineSpacing); // the front line should move ahead of the center point of the unit by half the width of a police line
            if(math.distance(trans.Value, destination) < tolerance){ // if the entity is within tolerance of the destination
                entityCommandBuffer.RemoveComponent<ToCordonFormComponent>(index, entity); // remove the formation change component from the police line
            }
            else{ // if not at the destination
                trans.Value = destination; // move to the destination
            }
        }
    }
    // the job used when it is the center police line
    private struct ToCordonCenterJob : IJobForEachWithEntity<ToCordonFormComponent,CenterPoliceLineComponent,Translation,Rotation> {
        public EntityCommandBuffer.Concurrent entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        
        public void Execute(Entity entity, int index, [ReadOnly] ref ToCordonFormComponent toCordon, [ReadOnly] ref CenterPoliceLineComponent centerLine, ref Translation trans, ref Rotation rot){
            float3 destination = new float3(0f,0f,0f); // the front line should move ahead of the center point of the unit by half the width of a police line
            if(math.distance(trans.Value, destination) < tolerance){ // if the entity is within tolerance of the destination
                entityCommandBuffer.RemoveComponent<ToCordonFormComponent>(index, entity); // remove the formation change component from the police line
            }
            else{ // if not at the destination
                trans.Value = destination; // move to the destination
                rot.Value = quaternion.RotateY(math.radians(0)); // rotate to forward
            }
        }
    }
    // the job used when it is the front police line
    private struct ToCordonRearJob : IJobForEachWithEntity<ToCordonFormComponent,RearPoliceLineComponent,Translation,Rotation> {
        public EntityCommandBuffer.Concurrent entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        
        public void Execute(Entity entity, int index, [ReadOnly] ref ToCordonFormComponent toCordon, [ReadOnly] ref RearPoliceLineComponent rearLine, ref Translation trans, ref Rotation rot){
            float3 destination = new float3(0f,0f,-toCordon.LineSpacing); // the front line should move ahead of the center point of the unit by half the width of a police line
            if(math.distance(trans.Value, destination) < tolerance){ // if the entity is within tolerance of the destination
                entityCommandBuffer.RemoveComponent<ToCordonFormComponent>(index, entity); // remove the formation change component from the police line
            }
            else{ // if not at the destination
                trans.Value = destination; // move to the destination
                rot.Value = quaternion.RotateY(math.radians(0)); // rotate to forward
            }
        }
    }


    protected override void OnCreate() {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps){
        ToCordonFrontJob frontJob = new ToCordonFrontJob{ // creates the to 3 sided box front job
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent()
        };
        JobHandle frontJobHandle = frontJob.Schedule(this, inputDeps);
        ToCordonCenterJob centerJob = new ToCordonCenterJob{
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent()
        };
        JobHandle centerJobHandle = centerJob.Schedule(this, frontJobHandle);
        ToCordonRearJob rearJob = new ToCordonRearJob{
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent()
        };
        JobHandle rearJobHandle = rearJob.Schedule(this, centerJobHandle);
        

        commandBufferSystem.AddJobHandleForProducer(rearJobHandle); // tell the system to execute the command buffer after the job has been completed


        return rearJobHandle;
    }
}

