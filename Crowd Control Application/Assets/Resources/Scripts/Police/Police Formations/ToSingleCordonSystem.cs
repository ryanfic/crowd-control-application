using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;


// A system that assigns the proper translation and rotation to each police officer so that the unit will form a single line cordon
public class ToSingleCordonSystem : SystemBase {
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    private EntityQueryDesc toSingleCordonQueryDesc;

    // The job that calculates the correct translation and rotation for each police officer
    private struct ToSingleCordonJob : IJobChunk {
        public EntityCommandBuffer.ParallelWriter commandBuffer; //Entity command buffer to allow adding/removing components inside the job
        
        [ReadOnly] public EntityTypeHandle entityType;
        [ReadOnly] public ComponentTypeHandle<ToSingleCordonFormComponent> sCordonType;
        [ReadOnly] public ComponentTypeHandle<PoliceOfficerNumber> officerNumType;
        [ReadOnly] public ComponentTypeHandle<PoliceOfficerPoliceLineNumber> lineNumType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            NativeArray<ToSingleCordonFormComponent> sCordonArray = chunk.GetNativeArray(sCordonType);
            NativeArray<PoliceOfficerNumber> officerNumArray = chunk.GetNativeArray(officerNumType);
            NativeArray<PoliceOfficerPoliceLineNumber> lineNumArray = chunk.GetNativeArray(lineNumType);

            for(int i = 0; i < chunk.Count; i++){
                Entity entity = entityArray[i];  
                ToSingleCordonFormComponent sCordon = sCordonArray[i];
                PoliceOfficerNumber officerNumber = officerNumArray[i];
                PoliceOfficerPoliceLineNumber lineNumber = lineNumArray[i];


                int offNumInUnit = officerNumber.Value; 

                if(lineNumber.Value >= 1){
                    offNumInUnit += sCordon.NumOfficersInLine1;
                }
                if(lineNumber.Value == 2){
                    offNumInUnit += sCordon.NumOfficersInLine2;
                }

                float leftPos = (sCordon.NumOfficersInUnit - 1 ) * 0.5f * (sCordon.OfficerWidth + sCordon.OfficerSpacing);
                float xOffset = offNumInUnit * (sCordon.OfficerWidth + sCordon.OfficerSpacing);
                float xLocation = - leftPos + xOffset;


                commandBuffer.AddComponent<FormationLocation>(chunkIndex,entity, new FormationLocation{
                    Value = new float3(xLocation, 0f, 0f)
                });
                commandBuffer.AddComponent<FormationRotation>(chunkIndex,entity, new FormationRotation{
                    Value = quaternion.RotateY(math.radians(0)) // rotate to forward
                });
                commandBuffer.AddComponent<PoliceOfficerOutOfFormation>(chunkIndex,entity, new PoliceOfficerOutOfFormation{});
                commandBuffer.RemoveComponent<ToSingleCordonFormComponent>(chunkIndex,entity);
            }
        }
    }



    protected override void OnCreate() {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        toSingleCordonQueryDesc = new EntityQueryDesc{
            All = new ComponentType[]{
                ComponentType.ReadOnly<ToSingleCordonFormComponent>(),
                ComponentType.ReadOnly<PoliceOfficer>(),
                ComponentType.ReadOnly<PoliceOfficerNumber>(),
                ComponentType.ReadOnly<PoliceOfficerPoliceLineNumber>()

            }
        };

        base.OnCreate();
    }
    protected override void OnUpdate(){
        EntityQuery sCordonQuery = GetEntityQuery(toSingleCordonQueryDesc); // query the entities

        ToSingleCordonJob sCordonJob = new ToSingleCordonJob{ // creates the job
            commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            entityType =  GetEntityTypeHandle(),
            sCordonType = GetComponentTypeHandle<ToSingleCordonFormComponent>(true),
            officerNumType = GetComponentTypeHandle<PoliceOfficerNumber>(true),
            lineNumType = GetComponentTypeHandle<PoliceOfficerPoliceLineNumber>(true)
        };
        JobHandle sCordonJobHandle = sCordonJob.Schedule(sCordonQuery, this.Dependency);        

        commandBufferSystem.AddJobHandleForProducer(sCordonJobHandle); // tell the system to execute the command buffer after the job has been completed

        this.Dependency = sCordonJobHandle;
    }
}

