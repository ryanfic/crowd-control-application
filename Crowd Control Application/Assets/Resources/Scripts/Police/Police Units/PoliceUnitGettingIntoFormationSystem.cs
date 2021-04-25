using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using System;


public class PoliceUnitGettingIntoFormationSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else
    private EntityQueryDesc formationCheckQueryDesc;


    //A job for checking if all the officers in a police unit are in formation
    //if the officers are all in formation, remove the 'getting into formation' component on the police unit
    [BurstCompile]
    private struct InFormationCheckJob : IJobChunk {
        public EntityCommandBuffer.ParallelWriter commandBuffer;

        [ReadOnly] public EntityTypeHandle entityType;
        [ReadOnly] public BufferTypeHandle<OfficerInFormation> officerInFormationType;
        [ReadOnly] public BufferTypeHandle<OfficerInPoliceUnit> officerInUnitType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            BufferAccessor<OfficerInFormation> officerInFormationArray = chunk.GetBufferAccessor<OfficerInFormation>(officerInFormationType);
            BufferAccessor<OfficerInPoliceUnit> officerInUnitArray = chunk.GetBufferAccessor<OfficerInPoliceUnit>(officerInUnitType);

            for(int i = 0; i < chunk.Count; i++){
                
                Entity entity = entityArray[i];
                DynamicBuffer<OfficerInFormation> officersInFormation = officerInFormationArray[i];
                DynamicBuffer<OfficerInPoliceUnit> officersInUnit = officerInUnitArray[i];

                if(officersInFormation.Length >= officersInUnit.Length){ // if all (or more) of the officers in the police unit are in the appropriate formation
                    if(officersInFormation.Length > officersInUnit.Length){ // if there are more than the number of officers in the police unit that are in formation, some error occurred
                        //Debug.Log("Somehow " + officersInFormation.Length + " out of " + officersInUnit.Length + " are in formation. That's an error alright.");
                    }
                    commandBuffer.RemoveComponent<PoliceUnitGettingIntoFormation>(chunkIndex,entity); //remove the 'Getting into formation' label from the police unit
                }
                /*else{
                    Debug.Log((officersInUnit.Length - officersInFormation.Length) + " officers are still getting into formation.");
                }*/
            }
        }
    }

    protected override void OnCreate(){
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        formationCheckQueryDesc = new EntityQueryDesc{ // define query for adding constant movement components
            All = new ComponentType[]{
                ComponentType.ReadOnly<PoliceUnitComponent>(),
                ComponentType.ReadOnly<PoliceUnitGettingIntoFormation>(),
                ComponentType.ReadOnly<OfficerInFormation>(),
                ComponentType.ReadOnly<OfficerInPoliceUnit>()
            }
        };
        
        base.OnCreate();
    }

    protected override void OnUpdate(){
        EntityCommandBuffer.ParallelWriter commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(); // create a command buffer
        EntityQuery formationCheckQuery = GetEntityQuery(formationCheckQueryDesc); // query the entities
        
        InFormationCheckJob formationCheckJob = new InFormationCheckJob{ 
            commandBuffer = commandBuffer,
            entityType =  GetEntityTypeHandle(),
            officerInFormationType = GetBufferTypeHandle<OfficerInFormation>(true),
            officerInUnitType = GetBufferTypeHandle<OfficerInPoliceUnit>(true)
        };
        JobHandle formationCheckJobHandle = formationCheckJob.Schedule(formationCheckQuery, this.Dependency); //schedule the job
        commandBufferSystem.AddJobHandleForProducer(formationCheckJobHandle); // make sure the components get added/removed for the job
    
        this.Dependency = formationCheckJobHandle;  
    }

}

