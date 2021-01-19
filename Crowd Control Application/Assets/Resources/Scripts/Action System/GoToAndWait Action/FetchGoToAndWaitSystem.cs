using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

public class FetchGoToAndWaitSystem : SystemBase
{

    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else
    
    private EntityQueryDesc fetchGTAWQueryDec;

    private struct FetchGTAWJob : IJob {
        public EntityCommandBuffer entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job

        [DeallocateOnJobCompletion] public NativeArray<Entity> entityArray;
        [DeallocateOnJobCompletion] public NativeArray<FetchGoToAndWaitData> fetchGTAWArray;

        public ComponentDataFromEntity<GoToAndWaitStorage> storageComponents;

        public void Execute(){
            for(int i = 0; i < entityArray.Length; i++){
                Entity entity = entityArray[i];
                FetchGoToAndWaitData fetch = fetchGTAWArray[i];

                GoToAndWaitStorage waitData = storageComponents[fetch.dataHolder]; // get the action data from the entity that holds the data
            
                entityCommandBuffer.AddComponent<GoToAndWaitAction>(entity, new GoToAndWaitAction {
                    id = waitData.id,
                    timeWaited = waitData.timeWaited,
                    timeToWait = waitData.timeToWait,
                    position = waitData.position
                });// add the GoToAndWaitAction to the crowd agent

                entityCommandBuffer.AddComponent<HasReynoldsSeekTargetPos>(entity, new HasReynoldsSeekTargetPos { // add the Target position to the crowd agent so it moves
                    targetPos = waitData.position
                });
                entityCommandBuffer.RemoveComponent<FetchGoToAndWaitData>(entity);
            }
        }
    }

    protected override void OnCreate() {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        fetchGTAWQueryDec = new EntityQueryDesc{
            All = new ComponentType[]{
                ComponentType.ReadOnly<FetchGoToAndWaitData>()
            }
        };
        base.OnCreate();
    }
    
    protected override void OnUpdate(){
        EntityQuery fetchGTAWQuery = GetEntityQuery(fetchGTAWQueryDec); // query the entities
        ComponentDataFromEntity<GoToAndWaitStorage> storageComponents = GetComponentDataFromEntity<GoToAndWaitStorage>();

        NativeArray<Entity> fetchGTAWEntityArray = fetchGTAWQuery.ToEntityArray(Allocator.TempJob);//get the array of entities
        NativeArray<FetchGoToAndWaitData> fetchGTAWArray = fetchGTAWQuery.ToComponentDataArray<FetchGoToAndWaitData>(Allocator.TempJob);

        FetchGTAWJob fetchJob = new FetchGTAWJob{ // creates the "fetch go to and wait" job
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer(),
            entityArray = fetchGTAWEntityArray,
            fetchGTAWArray = fetchGTAWArray,
            storageComponents = storageComponents
        };
        JobHandle jobHandle = IJobExtensions.Schedule(fetchJob,this.Dependency);

        commandBufferSystem.AddJobHandleForProducer(jobHandle); // tell the system to execute the command buffer after the job has been completed

        this.Dependency = jobHandle;
    }
}