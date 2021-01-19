using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

public class FetchGoHomeSystem : SystemBase
{

    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else
    
    private EntityQueryDesc fetchGoHomeQueryDec;

    private struct FetchGoHomeJob : IJob {
        public EntityCommandBuffer entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job

        [DeallocateOnJobCompletion] public NativeArray<Entity> entityArray;
        [DeallocateOnJobCompletion] public NativeArray<FetchGoHomeData> fetchGoHomeArray;

        public ComponentDataFromEntity<GoHomeStorage> storageComponents;

        public void Execute(){
            for(int i = 0; i < entityArray.Length; i++){
                Entity entity = entityArray[i];
                FetchGoHomeData fetch = fetchGoHomeArray[i];

                GoHomeStorage homeData = storageComponents[fetch.dataHolder]; // get the action data from the entity that holds the data
            
                entityCommandBuffer.AddComponent<GoHomeAction>(entity, new GoHomeAction {
                    id = homeData.id,
                });// add the GoHomeAction to the crowd agent

                entityCommandBuffer.AddComponent<HasReynoldsSeekTargetPos>(entity, new HasReynoldsSeekTargetPos { // add the Target position to the crowd agent so it moves
                    targetPos = homeData.homePoint
                });
                entityCommandBuffer.RemoveComponent<FetchGoHomeData>(entity);
            }
        }
    }

    protected override void OnCreate() {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        fetchGoHomeQueryDec = new EntityQueryDesc{
            All = new ComponentType[]{
                ComponentType.ReadOnly<FetchGoHomeData>()
            }
        };
        base.OnCreate();
    }
    
    protected override void OnUpdate(){
        EntityQuery fetchGoHomeQuery = GetEntityQuery(fetchGoHomeQueryDec); // query the entities
        ComponentDataFromEntity<GoHomeStorage> storageComponents = GetComponentDataFromEntity<GoHomeStorage>();

        NativeArray<Entity> fetchGoHomeEntityArray = fetchGoHomeQuery.ToEntityArray(Allocator.TempJob);//get the array of entities
        NativeArray<FetchGoHomeData> fetchGoHomeArray = fetchGoHomeQuery.ToComponentDataArray<FetchGoHomeData>(Allocator.TempJob);

        FetchGoHomeJob fetchJob = new FetchGoHomeJob{ // creates the "follow waypoints" job
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer(),
            entityArray = fetchGoHomeEntityArray,
            fetchGoHomeArray = fetchGoHomeArray,
            storageComponents = storageComponents

        };
        JobHandle jobHandle = IJobExtensions.Schedule(fetchJob,this.Dependency);

        commandBufferSystem.AddJobHandleForProducer(jobHandle); // tell the system to execute the command buffer after the job has been completed

        this.Dependency = jobHandle;
    }
}

