using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;


// A system for changing the formation of a police unit to a 3 sided box

public class To3SidedBoxSystem : SystemBase {
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    private EntityQueryDesc toBoxQueryDesc;

    // The job that calculates the correct translation and rotation for each police officer
    private struct ToBoxJob : IJobChunk {
        public EntityCommandBuffer.ParallelWriter commandBuffer; //Entity command buffer to allow adding/removing components inside the job
        
        [ReadOnly] public EntityTypeHandle entityType;
        [ReadOnly] public ComponentTypeHandle<To3SidedBoxFormComponent> boxType;
        [ReadOnly] public ComponentTypeHandle<PoliceOfficerNumber> officerNumType;
        [ReadOnly] public ComponentTypeHandle<PoliceOfficerPoliceLineNumber> lineNumType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            NativeArray<To3SidedBoxFormComponent> boxArray = chunk.GetNativeArray(boxType);
            NativeArray<PoliceOfficerNumber> officerNumArray = chunk.GetNativeArray(officerNumType);
            NativeArray<PoliceOfficerPoliceLineNumber> lineNumArray = chunk.GetNativeArray(lineNumType);

            for(int i = 0; i < chunk.Count; i++){
                Entity entity = entityArray[i];  
                To3SidedBoxFormComponent box = boxArray[i];
                PoliceOfficerNumber officerNumber = officerNumArray[i];
                PoliceOfficerPoliceLineNumber lineNumber = lineNumArray[i];


                int largerNumOfficers; // find the side line that has the bigger number of officers in it
                if(box.NumOfficersInLine1 > box.NumOfficersInLine3){
                    largerNumOfficers = box.NumOfficersInLine1;
                }
                else{
                    largerNumOfficers = box.NumOfficersInLine3;
                }

                float backOfFrontLine = ((largerNumOfficers + 1f)/2f) * box.OfficerSpacing + (largerNumOfficers/2f) * box.OfficerWidth;

                float xLocation;
                float zLocation;
                int officersInLine;
                float3 formLocation;
                quaternion formRot;

                if(lineNumber.Value == 0){ // if on the left side of the box
                    officersInLine = box.NumOfficersInLine1;
                    //xLocation = -backOfFrontLine + (officerNumber.Value+1) * box.OfficerSpacing + (officerNumber.Value + 0.5f) * box.OfficerWidth;
                    zLocation = backOfFrontLine - ((box.NumOfficersInLine1 - officerNumber.Value) *box.OfficerSpacing + (box.NumOfficersInLine1 - 1 - officerNumber.Value + 0.5f) * box.OfficerWidth);
                    xLocation = -(((box.NumOfficersInLine2 - 1f)/2f) * (box.OfficerWidth + box.OfficerSpacing));
                    formLocation = new float3(xLocation,0f,zLocation);
                    formRot = quaternion.RotateY(math.radians(-90));
                    //formLocation = math.mul(formRot,formLocation);
                }
                else if(lineNumber.Value == 1){ // if on the front side of the box
                    officersInLine = box.NumOfficersInLine2;
                    zLocation = backOfFrontLine + box.OfficerLength/2f;
                    float leftside = -(((officersInLine-1f)/2f) * box.OfficerWidth + ((officersInLine-1f)/2f) * box.OfficerSpacing);
                    xLocation = leftside + officerNumber.Value * box.OfficerWidth + officerNumber.Value * box.OfficerSpacing;
                    formLocation = new float3(xLocation,0f,zLocation);
                    formRot = quaternion.RotateY(math.radians(0));
                }
                else { // if on the right side of the box
                    officersInLine = box.NumOfficersInLine3;
                    //xLocation = backOfFrontLine - ((box.NumOfficersInLine1 - officerNumber.Value + 1) *box.OfficerSpacing + (box.NumOfficersInLine1 - officerNumber.Value + 0.5f) * box.OfficerWidth);
                    
                    zLocation = backOfFrontLine - (officerNumber.Value+1f) * box.OfficerSpacing - (officerNumber.Value + 0.5f) * box.OfficerWidth;
                    xLocation = ((box.NumOfficersInLine2 - 1f)/2f) * (box.OfficerWidth + box.OfficerSpacing);
                    formLocation = new float3(xLocation,0f,zLocation);
                    formRot = quaternion.RotateY(math.radians(90));
                    //formLocation = math.mul(formRot,formLocation);
                }

                commandBuffer.AddComponent<FormationLocation>(chunkIndex,entity, new FormationLocation{
                    Value = formLocation
                });
                commandBuffer.AddComponent<FormationRotation>(chunkIndex,entity, new FormationRotation{
                    Value = formRot
                });
                commandBuffer.AddComponent<PoliceOfficerOutOfFormation>(chunkIndex,entity, new PoliceOfficerOutOfFormation{});
                commandBuffer.RemoveComponent<To3SidedBoxFormComponent>(chunkIndex,entity);
            }
        }
    }



    protected override void OnCreate() {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        toBoxQueryDesc = new EntityQueryDesc{
            All = new ComponentType[]{
                ComponentType.ReadOnly<To3SidedBoxFormComponent>(),
                ComponentType.ReadOnly<PoliceOfficer>(),
                ComponentType.ReadOnly<PoliceOfficerNumber>(),
                ComponentType.ReadOnly<PoliceOfficerPoliceLineNumber>()

            }
        };

        base.OnCreate();
    }
    protected override void OnUpdate(){
        EntityQuery boxQuery = GetEntityQuery(toBoxQueryDesc); // query the entities

        ToBoxJob pCordonJob = new ToBoxJob{ // creates the job
            commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            entityType =  GetEntityTypeHandle(),
            boxType = GetComponentTypeHandle<To3SidedBoxFormComponent>(true),
            officerNumType = GetComponentTypeHandle<PoliceOfficerNumber>(true),
            lineNumType = GetComponentTypeHandle<PoliceOfficerPoliceLineNumber>(true)
        };
        JobHandle boxJobHandle = pCordonJob.Schedule(boxQuery, this.Dependency);        

        commandBufferSystem.AddJobHandleForProducer(boxJobHandle); // tell the system to execute the command buffer after the job has been completed

        this.Dependency = boxJobHandle;
    }
}



