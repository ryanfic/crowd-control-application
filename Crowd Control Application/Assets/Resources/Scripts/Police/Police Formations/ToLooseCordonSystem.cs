using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;


// A system for changing the formation of a police unit to a 3 sided box
public class ToLooseCordonSystem : SystemBase {
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else
    private static float tolerance = 0.1f;
    
    private EntityQueryDesc toCordonFrontQueryDec;
    private EntityQueryDesc toCordonCenterQueryDec;
    private EntityQueryDesc toCordonRearQueryDec;

    // the job used when it is the front police line
    private struct ToLooseCordonFrontJob : IJobChunk {
        public EntityCommandBuffer.ParallelWriter entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        
        [ReadOnly] public EntityTypeHandle entityType;
        public ComponentTypeHandle<Translation> translationType;
        public ComponentTypeHandle<Rotation> rotationType;
        [ReadOnly] public ComponentTypeHandle<ToLooseCordonFormComponent> toLooseCordonType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            NativeArray<Translation> transArray = chunk.GetNativeArray(translationType);
            NativeArray<Rotation> rotArray = chunk.GetNativeArray(rotationType);
            NativeArray<ToLooseCordonFormComponent> toLooseCordonArray = chunk.GetNativeArray(toLooseCordonType);

            for(int i = 0; i < chunk.Count; i++){
                Entity entity = entityArray[i];  
                Translation trans = transArray[i];
                Rotation rot = rotArray[i];
                ToLooseCordonFormComponent toCordon = toLooseCordonArray[i];
        
                float3 destination = new float3(0f,0f,toCordon.LineSpacing); // the front line should move ahead of the center point of the unit by half the width of a police line
                if(math.distance(trans.Value, destination) < tolerance){ // if the entity is within tolerance of the destination
                    entityCommandBuffer.RemoveComponent<ToLooseCordonFormComponent>(chunkIndex, entity); // remove the formation change component from the police line
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
    private struct ToLooseCordonCenterJob : IJobChunk {
        public EntityCommandBuffer.ParallelWriter entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        
        [ReadOnly] public EntityTypeHandle entityType;
        public ComponentTypeHandle<Translation> translationType;
        public ComponentTypeHandle<Rotation> rotationType;
        [ReadOnly] public ComponentTypeHandle<ToLooseCordonFormComponent> toLooseCordonType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            NativeArray<Translation> transArray = chunk.GetNativeArray(translationType);
            NativeArray<Rotation> rotArray = chunk.GetNativeArray(rotationType);
            NativeArray<ToLooseCordonFormComponent> toLooseCordonArray = chunk.GetNativeArray(toLooseCordonType);

            for(int i = 0; i < chunk.Count; i++){
                Entity entity = entityArray[i];  
                Translation trans = transArray[i];
                Rotation rot = rotArray[i];
                ToLooseCordonFormComponent toCordon = toLooseCordonArray[i];
        
                float3 destination = new float3(0f,0f,0f); // the front line should move ahead of the center point of the unit by half the width of a police line
                if(math.distance(trans.Value, destination) < tolerance){ // if the entity is within tolerance of the destination
                    entityCommandBuffer.RemoveComponent<ToLooseCordonFormComponent>(chunkIndex, entity); // remove the formation change component from the police line
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
    // the job used when it is the front police line
    private struct ToLooseCordonRearJob : IJobChunk {
        public EntityCommandBuffer.ParallelWriter entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        
        [ReadOnly] public EntityTypeHandle entityType;
        public ComponentTypeHandle<Translation> translationType;
        public ComponentTypeHandle<Rotation> rotationType;
        [ReadOnly] public ComponentTypeHandle<ToLooseCordonFormComponent> toLooseCordonType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            NativeArray<Translation> transArray = chunk.GetNativeArray(translationType);
            NativeArray<Rotation> rotArray = chunk.GetNativeArray(rotationType);
            NativeArray<ToLooseCordonFormComponent> toLooseCordonArray = chunk.GetNativeArray(toLooseCordonType);

            for(int i = 0; i < chunk.Count; i++){
                Entity entity = entityArray[i];  
                Translation trans = transArray[i];
                Rotation rot = rotArray[i];
                ToLooseCordonFormComponent toCordon = toLooseCordonArray[i];
        
                float3 destination = new float3(0f,0f,-toCordon.LineSpacing); // the front line should move ahead of the center point of the unit by half the width of a police line
                if(math.distance(trans.Value, destination) < tolerance){ // if the entity is within tolerance of the destination
                    entityCommandBuffer.RemoveComponent<ToLooseCordonFormComponent>(chunkIndex, entity); // remove the formation change component from the police line
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


    protected override void OnCreate() {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        toCordonFrontQueryDec = new EntityQueryDesc{
            All = new ComponentType[]{
                ComponentType.ReadOnly<ToLooseCordonFormComponent>(),
                ComponentType.ReadOnly<FrontPoliceLineComponent>(),
                typeof(Translation),
                typeof(Rotation)
            }
        };

        toCordonCenterQueryDec = new EntityQueryDesc{
            All = new ComponentType[]{
                ComponentType.ReadOnly<ToLooseCordonFormComponent>(),
                ComponentType.ReadOnly<CenterPoliceLineComponent>(),
                typeof(Translation),
                typeof(Rotation)
            }
        };
        

        toCordonRearQueryDec = new EntityQueryDesc{
            All = new ComponentType[]{
                ComponentType.ReadOnly<ToLooseCordonFormComponent>(),
                ComponentType.ReadOnly<RearPoliceLineComponent>(),
                typeof(Translation),
                typeof(Rotation)  
            }
        };

        base.OnCreate();
    }
    protected override void OnUpdate(){
        EntityQuery frontQuery = GetEntityQuery(toCordonFrontQueryDec); // query the entities
        EntityQuery centerQuery = GetEntityQuery(toCordonCenterQueryDec); // query the entities
        EntityQuery rearQuery = GetEntityQuery(toCordonRearQueryDec); // query the entities

        ToLooseCordonFrontJob frontJob = new ToLooseCordonFrontJob{ // creates the to loose cordon front job
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            entityType =  GetEntityTypeHandle(),
            translationType = GetComponentTypeHandle<Translation>(),
            rotationType = GetComponentTypeHandle<Rotation>(),
            toLooseCordonType = GetComponentTypeHandle<ToLooseCordonFormComponent>(true)
        };
        JobHandle frontJobHandle = frontJob.Schedule(frontQuery, this.Dependency);
        ToLooseCordonCenterJob centerJob = new ToLooseCordonCenterJob{
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            entityType =  GetEntityTypeHandle(),
            translationType = GetComponentTypeHandle<Translation>(),
            rotationType = GetComponentTypeHandle<Rotation>(),
            toLooseCordonType = GetComponentTypeHandle<ToLooseCordonFormComponent>(true)
        };
        JobHandle centerJobHandle = centerJob.Schedule(centerQuery, frontJobHandle);
        ToLooseCordonRearJob rearJob = new ToLooseCordonRearJob{
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            entityType =  GetEntityTypeHandle(),
            translationType = GetComponentTypeHandle<Translation>(),
            rotationType = GetComponentTypeHandle<Rotation>(),
            toLooseCordonType = GetComponentTypeHandle<ToLooseCordonFormComponent>(true)
        };
        JobHandle rearJobHandle = rearJob.Schedule(rearQuery, centerJobHandle);
        

        commandBufferSystem.AddJobHandleForProducer(rearJobHandle); // tell the system to execute the command buffer after the job has been completed


        this.Dependency = rearJobHandle;
    }
}

