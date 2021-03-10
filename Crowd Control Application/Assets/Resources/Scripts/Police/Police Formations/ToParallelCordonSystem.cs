using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;


// A system that assigns the proper translation and rotation to each police officer so that the unit will form a parallel cordon
public class ToParallelCordonSystem : SystemBase {
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    private EntityQueryDesc toParallelCordonQueryDesc;

    // The job that calculates the correct translation and rotation for each police officer
    private struct ToParallelCordonJob : IJobChunk {
        public EntityCommandBuffer.ParallelWriter commandBuffer; //Entity command buffer to allow adding/removing components inside the job
        
        [ReadOnly] public EntityTypeHandle entityType;
        [ReadOnly] public ComponentTypeHandle<ToParallelCordonFormComponent> pCordonType;
        [ReadOnly] public ComponentTypeHandle<PoliceOfficerNumber> officerNumType;
        [ReadOnly] public ComponentTypeHandle<PoliceOfficerPoliceLineNumber> lineNumType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            NativeArray<ToParallelCordonFormComponent> pCordonArray = chunk.GetNativeArray(pCordonType);
            NativeArray<PoliceOfficerNumber> officerNumArray = chunk.GetNativeArray(officerNumType);
            NativeArray<PoliceOfficerPoliceLineNumber> lineNumArray = chunk.GetNativeArray(lineNumType);

            for(int i = 0; i < chunk.Count; i++){
                Entity entity = entityArray[i];  
                ToParallelCordonFormComponent pCordon = pCordonArray[i];
                PoliceOfficerNumber officerNumber = officerNumArray[i];
                PoliceOfficerPoliceLineNumber lineNumber = lineNumArray[i];

                int officersInLine;
                if(lineNumber.Value == 0){
                    officersInLine = pCordon.NumOfficersInLine1;
                }
                else if(lineNumber.Value == 1){
                    officersInLine = pCordon.NumOfficersInLine2;
                }
                else {
                    officersInLine = pCordon.NumOfficersInLine3;
                }

                float xLocation;
                float leftside;
                float xOffset;
                
                if(officersInLine%2 == 1){ // if there are an odd number of police officers
                    leftside = -((officersInLine/2) * pCordon.OfficerSpacing + (officersInLine/2) * pCordon.OfficerWidth);        
                }
                else{ // if there are an even number of police officers
                    leftside = -(((officersInLine/2)-0.5f) * pCordon.OfficerSpacing + (officersInLine/2) * pCordon.OfficerWidth);      
                }
                xOffset = (officerNumber.Value + 0.5f) * pCordon.OfficerWidth + officerNumber.Value * pCordon.OfficerSpacing;
                xLocation = leftside + xOffset;

                float topPosition = 1.5f * pCordon.OfficerLength + pCordon.LineSpacing;
                float zOffset = -(lineNumber.Value * pCordon.LineSpacing + (0.5f + lineNumber.Value) * pCordon.OfficerLength);
                float zLocation = topPosition + zOffset;

                commandBuffer.AddComponent<FormationLocation>(chunkIndex,entity, new FormationLocation{
                    Value = new float3(xLocation, 0f, zLocation)
                });
                commandBuffer.AddComponent<FormationRotation>(chunkIndex,entity, new FormationRotation{
                    Value = quaternion.RotateY(math.radians(0)) // rotate to forward
                });
                commandBuffer.AddComponent<PoliceOfficerOutOfFormation>(chunkIndex,entity, new PoliceOfficerOutOfFormation{});
                commandBuffer.RemoveComponent<ToParallelCordonFormComponent>(chunkIndex,entity);
            }
        }
    }



    protected override void OnCreate() {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        toParallelCordonQueryDesc = new EntityQueryDesc{
            All = new ComponentType[]{
                ComponentType.ReadOnly<ToParallelCordonFormComponent>(),
                ComponentType.ReadOnly<PoliceOfficer>(),
                ComponentType.ReadOnly<PoliceOfficerNumber>(),
                ComponentType.ReadOnly<PoliceOfficerPoliceLineNumber>()

            }
        };

        base.OnCreate();
    }
    protected override void OnUpdate(){
        EntityQuery pCordonQuery = GetEntityQuery(toParallelCordonQueryDesc); // query the entities

        ToParallelCordonJob pCordonJob = new ToParallelCordonJob{ // creates the job
            commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            entityType =  GetEntityTypeHandle(),
            pCordonType = GetComponentTypeHandle<ToParallelCordonFormComponent>(true),
            officerNumType = GetComponentTypeHandle<PoliceOfficerNumber>(true),
            lineNumType = GetComponentTypeHandle<PoliceOfficerPoliceLineNumber>(true)
        };
        JobHandle pCordonJobHandle = pCordonJob.Schedule(pCordonQuery, this.Dependency);        

        commandBufferSystem.AddJobHandleForProducer(pCordonJobHandle); // tell the system to execute the command buffer after the job has been completed

        this.Dependency = pCordonJobHandle;
    }
}

