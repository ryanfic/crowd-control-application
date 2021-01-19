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
    private static float tolerance = 0.1f;

    private EntityQueryDesc toBoxFrontQueryDec;
    private EntityQueryDesc toBoxCenterQueryDec;
    private EntityQueryDesc toBoxRearQueryDec;

    // the job used when it is the front police line
    private struct ToBoxFrontJob : IJobChunk {
        public EntityCommandBuffer.Concurrent entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        
        [ReadOnly] public ArchetypeChunkEntityType entityType;
        public ArchetypeChunkComponentType<Translation> translationType;
        public ArchetypeChunkComponentType<Rotation> rotationType;
        [ReadOnly] public ArchetypeChunkComponentType<To3SidedBoxFormComponent> toBoxType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            NativeArray<Translation> transArray = chunk.GetNativeArray(translationType);
            NativeArray<Rotation> rotArray = chunk.GetNativeArray(rotationType);
            NativeArray<To3SidedBoxFormComponent> toBoxArray = chunk.GetNativeArray(toBoxType);

            for(int i = 0; i < chunk.Count; i++){
                Entity entity = entityArray[i];  
                Translation trans = transArray[i];
                Rotation rot = rotArray[i];
                To3SidedBoxFormComponent toBox = toBoxArray[i];

                float3 destination = new float3(0f,0f,(toBox.LineLength/2)+(toBox.LineWidth/2)); // the front line should move ahead of the center point of the unit by half the width of a police line
                if(math.distance(trans.Value, destination) < tolerance){ // if the entity is within tolerance of the destination
                    entityCommandBuffer.RemoveComponent<To3SidedBoxFormComponent>(chunkIndex, entity); // remove the formation change component from the police line
                }
                else{ // if not at the destination
                    transArray[i] = new Translation {
                        Value = destination // move to the destination
                    };
                    rotArray[i] = new Rotation {
                        Value = quaternion.RotateY(math.radians(0)) // rotate to forward
                    };
                }
            }
        }
    }
    // the job used when it is the center police line
    private struct ToBoxCenterJob : IJobChunk {
        public EntityCommandBuffer.Concurrent entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        
        [ReadOnly] public ArchetypeChunkEntityType entityType;
        public ArchetypeChunkComponentType<Translation> translationType;
        public ArchetypeChunkComponentType<Rotation> rotationType;
        [ReadOnly] public ArchetypeChunkComponentType<To3SidedBoxFormComponent> toBoxType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            NativeArray<Translation> transArray = chunk.GetNativeArray(translationType);
            NativeArray<Rotation> rotArray = chunk.GetNativeArray(rotationType);
            NativeArray<To3SidedBoxFormComponent> toBoxArray = chunk.GetNativeArray(toBoxType);

            for(int i = 0; i < chunk.Count; i++){  
                Entity entity = entityArray[i]; 
                Translation trans = transArray[i];
                Rotation rot = rotArray[i];
                To3SidedBoxFormComponent toBox = toBoxArray[i];

                float3 destination = new float3((toBox.LineWidth-toBox.LineLength)/2,0f,0f); // the front line should move ahead of the center point of the unit by half the width of a police line
                if(math.distance(trans.Value, destination) < tolerance){ // if the entity is within tolerance of the destination
                    entityCommandBuffer.RemoveComponent<To3SidedBoxFormComponent>(chunkIndex, entity); // remove the formation change component from the police line
                }
                else{ // if not at the destination
                    transArray[i] = new Translation {
                        Value = destination // move to the destination
                    };
                    rotArray[i] = new Rotation {
                        Value = quaternion.RotateY(math.radians(-90)) // rotate to the left
                    };
                }
            }
        }
    }
    // the job used when it is the front police line
    private struct ToBoxRearJob : IJobChunk {
        public EntityCommandBuffer.Concurrent entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job

        [ReadOnly] public ArchetypeChunkEntityType entityType;
        public ArchetypeChunkComponentType<Translation> translationType;
        public ArchetypeChunkComponentType<Rotation> rotationType;
        [ReadOnly] public ArchetypeChunkComponentType<To3SidedBoxFormComponent> toBoxType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            NativeArray<Translation> transArray = chunk.GetNativeArray(translationType);
            NativeArray<Rotation> rotArray = chunk.GetNativeArray(rotationType);
            NativeArray<To3SidedBoxFormComponent> toBoxArray = chunk.GetNativeArray(toBoxType);

            for(int i = 0; i < chunk.Count; i++){   
                Entity entity = entityArray[i];
                Translation trans = transArray[i];
                Rotation rot = rotArray[i];
                To3SidedBoxFormComponent toBox = toBoxArray[i];
        
                float3 destination = new float3((toBox.LineLength-toBox.LineWidth)/2,0f,0f); // the front line should move ahead of the center point of the unit by half the width of a police line
                if(math.distance(trans.Value, destination) < tolerance){ // if the entity is within tolerance of the destination
                    entityCommandBuffer.RemoveComponent<To3SidedBoxFormComponent>(chunkIndex, entity); // remove the formation change component from the police line
                }
                else{ // if not at the destination
                    transArray[i] = new Translation {
                        Value = destination // move to the destination
                    };
                    rotArray[i] = new Rotation {
                        Value = quaternion.RotateY(math.radians(90)) // rotate to the right
                    };
                }
            }
        }
    }


    protected override void OnCreate() {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        toBoxFrontQueryDec = new EntityQueryDesc{
            All = new ComponentType[]{
                ComponentType.ReadOnly<To3SidedBoxFormComponent>(),
                ComponentType.ReadOnly<FrontPoliceLineComponent>(),
                typeof(Translation),
                typeof(Rotation)
            }
        };

        toBoxCenterQueryDec = new EntityQueryDesc{
            All = new ComponentType[]{
                ComponentType.ReadOnly<To3SidedBoxFormComponent>(),
                ComponentType.ReadOnly<CenterPoliceLineComponent>(),
                typeof(Translation),
                typeof(Rotation)
            }
        };
        

        toBoxRearQueryDec = new EntityQueryDesc{
            All = new ComponentType[]{
                ComponentType.ReadOnly<To3SidedBoxFormComponent>(),
                ComponentType.ReadOnly<RearPoliceLineComponent>(),
                typeof(Translation),
                typeof(Rotation)  
            }
        };

        base.OnCreate();
    }
    protected override void OnUpdate(){
        EntityQuery frontQuery = GetEntityQuery(toBoxFrontQueryDec); // query the entities
        EntityQuery centerQuery = GetEntityQuery(toBoxCenterQueryDec); // query the entities
        EntityQuery rearQuery = GetEntityQuery(toBoxRearQueryDec); // query the entities

        ToBoxFrontJob frontJob = new ToBoxFrontJob{ // creates the to 3 sided box front job
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent(),
            entityType =  GetArchetypeChunkEntityType(),
            translationType = GetArchetypeChunkComponentType<Translation>(),
            rotationType = GetArchetypeChunkComponentType<Rotation>(),
            toBoxType = GetArchetypeChunkComponentType<To3SidedBoxFormComponent>(true)
        };
        JobHandle frontJobHandle = frontJob.Schedule(frontQuery, this.Dependency);
        ToBoxCenterJob centerJob = new ToBoxCenterJob{
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent(),
            entityType =  GetArchetypeChunkEntityType(),
            translationType = GetArchetypeChunkComponentType<Translation>(),
            rotationType = GetArchetypeChunkComponentType<Rotation>(),
            toBoxType = GetArchetypeChunkComponentType<To3SidedBoxFormComponent>(true)
        };
        JobHandle centerJobHandle = centerJob.Schedule(centerQuery, frontJobHandle);
        ToBoxRearJob rearJob = new ToBoxRearJob{
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent(),
            entityType =  GetArchetypeChunkEntityType(),
            translationType = GetArchetypeChunkComponentType<Translation>(),
            rotationType = GetArchetypeChunkComponentType<Rotation>(),
            toBoxType = GetArchetypeChunkComponentType<To3SidedBoxFormComponent>(true)
        };
        JobHandle rearJobHandle = rearJob.Schedule(rearQuery, centerJobHandle);

        commandBufferSystem.AddJobHandleForProducer(rearJobHandle); // tell the system to execute the command buffer after the job has been completed

        this.Dependency = rearJobHandle;
    }
}
