using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;


// A system for changing the formation of a police unit to a wedge

public class ToWedgeSystem : SystemBase {
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    private EntityQueryDesc toWedgeQueryDesc;

    // The job that calculates the correct translation and rotation for each police officer
    private struct ToWedgeJob : IJobChunk {
        public EntityCommandBuffer.ParallelWriter commandBuffer; //Entity command buffer to allow adding/removing components inside the job
        
        [ReadOnly] public EntityTypeHandle entityType;
        [ReadOnly] public ComponentTypeHandle<ToWedgeFormComponent> wedgeType;
        [ReadOnly] public ComponentTypeHandle<PoliceOfficerNumber> officerNumType;
        [ReadOnly] public ComponentTypeHandle<PoliceOfficerPoliceLineNumber> lineNumType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            NativeArray<ToWedgeFormComponent> wedgeArray = chunk.GetNativeArray(wedgeType);
            NativeArray<PoliceOfficerNumber> officerNumArray = chunk.GetNativeArray(officerNumType);
            NativeArray<PoliceOfficerPoliceLineNumber> lineNumArray = chunk.GetNativeArray(lineNumType);

            for(int i = 0; i < chunk.Count; i++){
                Entity entity = entityArray[i];  
                ToWedgeFormComponent wedge = wedgeArray[i];
                PoliceOfficerNumber officerNumber = officerNumArray[i];
                PoliceOfficerPoliceLineNumber lineNumber = lineNumArray[i];

                int officerNumInUnit = 1;

                if(lineNumber.Value == 0){ // if in the first line
                    officerNumInUnit += officerNumber.Value;
                }
                else if(lineNumber.Value == 1){ // if in the second line
                    officerNumInUnit += officerNumber.Value + wedge.NumOfficersInLine1;
                }
                else { // if in the third line
                    officerNumInUnit += officerNumber.Value + wedge.NumOfficersInLine1 + wedge.NumOfficersInLine2;
                }

                int relNumToFront = officerNumInUnit - wedge.MiddleOfficerNum;

                float frontOfficerZPos = ((float)wedge.MiddleOfficerNum / 2) * wedge.OfficerLength;
                float zOffset = math.abs(relNumToFront) * wedge.OfficerLength;
                float zLocation = frontOfficerZPos - zOffset;  

                float xLocation = relNumToFront * wedge.OfficerLength * math.tan(math.radians(wedge.Angle/2));
                              
                quaternion formRot = quaternion.RotateY(math.radians(0));



                float3 formLocation = new float3(xLocation,0f,zLocation);

                commandBuffer.AddComponent<FormationLocation>(chunkIndex,entity, new FormationLocation{
                    Value = formLocation
                });
                commandBuffer.AddComponent<FormationRotation>(chunkIndex,entity, new FormationRotation{
                    Value = formRot
                });
                commandBuffer.AddComponent<PoliceOfficerOutOfFormation>(chunkIndex,entity, new PoliceOfficerOutOfFormation{});
                commandBuffer.RemoveComponent<ToWedgeFormComponent>(chunkIndex,entity);
            }
        }
    }



    protected override void OnCreate() {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        toWedgeQueryDesc = new EntityQueryDesc{
            All = new ComponentType[]{
                ComponentType.ReadOnly<ToWedgeFormComponent>(),
                ComponentType.ReadOnly<PoliceOfficer>(),
                ComponentType.ReadOnly<PoliceOfficerNumber>(),
                ComponentType.ReadOnly<PoliceOfficerPoliceLineNumber>()
            }
        };

        base.OnCreate();
    }
    protected override void OnUpdate(){
        EntityQuery wedgeQuery = GetEntityQuery(toWedgeQueryDesc); // query the entities

        ToWedgeJob wedgeJob = new ToWedgeJob{ // creates the job
            commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            entityType =  GetEntityTypeHandle(),
            wedgeType = GetComponentTypeHandle<ToWedgeFormComponent>(true),
            officerNumType = GetComponentTypeHandle<PoliceOfficerNumber>(true),
            lineNumType = GetComponentTypeHandle<PoliceOfficerPoliceLineNumber>(true)
        };
        JobHandle wedgeJobHandle = wedgeJob.Schedule(wedgeQuery, this.Dependency);        

        commandBufferSystem.AddJobHandleForProducer(wedgeJobHandle); // tell the system to execute the command buffer after the job has been completed

        this.Dependency = wedgeJobHandle;
    }
}


