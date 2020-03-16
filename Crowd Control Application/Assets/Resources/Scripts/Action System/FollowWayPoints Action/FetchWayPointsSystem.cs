using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

public class FetchWayPointsSystem : ComponentSystem
{
    
    protected override void OnUpdate(){
        //BufferFromEntity<WayPoint> buffers = GetBufferFromEntity<WayPoint>(); // used to access things with waypoint buffer

        //For entities without the hasReynoldsSeekTargetPos component
        Entities.WithNone<StoreWayPoints>().ForEach((Entity crowdEntity, ref FetchWayPoints fetch) => {  
            Entity wayPointHolder = fetch.dataHolder; // get the entity that holds the waypoints
            FollowWayPointsStorage wpData = EntityManager.GetComponentData<FollowWayPointsStorage>(fetch.dataHolder); // get the action data from the entity that holds the data
            DynamicBuffer<WayPoint> wayPoints = EntityManager.GetBuffer<WayPoint>(fetch.dataHolder); // get the waypoints from the entity that holds the data
            float3[] points = new float3[wayPoints.Length]; // store the waypoints as an array of points
            for(int i = 0; i < wayPoints.Length; i++){
                points[i] = wayPoints[i].value;
            }

            EntityManager.AddComponentData<FollowWayPointsAction>(crowdEntity, new FollowWayPointsAction {
                id = wpData.id,
                curPointNum = wpData.curPointNum
            });// add the FollowWayPointsAction to the crowd agent

            DynamicBuffer<WayPoint> wpAdded = EntityManager.AddBuffer<WayPoint>(crowdEntity); // add the waypoint buffer to the crowd agent
            for(int i = 0; i < points.Length; i++){
                wpAdded.Add(new WayPoint{value = points[i]});
            }

            EntityManager.AddComponentData<HasReynoldsSeekTargetPos>(crowdEntity, new HasReynoldsSeekTargetPos { // add the Target position to the crowd agent so it moves
                targetPos = points[wpData.curPointNum]
            });
            EntityManager.RemoveComponent<FetchWayPoints>(crowdEntity);
        });
    }
}


/*public class FetchWayPointsSystem : JobComponentSystem {
    private struct EntityWithStorageAndWP{
        public Entity entity;
        public FollowWayPointsStorage storage;
        public DynamicBuffer<WayPoint> wayPoints;
    }
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else
    
    [ExcludeComponent(typeof(StoreWayPoints))] // don't fetch the data (and possible overwrite) if the current data needs to be stored
    private struct FetchWayPointsJob : IJobForEachWithEntity<FetchWayPoints> {
        public EntityCommandBuffer entityCommandBuffer; // for adding / removing components
        [DeallocateOnJobCompletion] public NativeArray<EntityWithStorageAndWP> holderArray;
        public void Execute(Entity entity, int index, ref FetchWayPoints fetch){
            Entity wayPointHolder = fetch.dataHolder; // get the entity that holds the waypoints
            FollowWayPointsStorage wpData = EntityManager.GetComponentData<FollowWayPointsStorage>(fetch.dataHolder); // get the action data from the entity that holds the data
            DynamicBuffer<WayPoint> wayPoints = EntityManager.GetBuffer<WayPoint>(fetch.dataHolder); // get the waypoints from the entity that holds the data
            float3[] points = new float3[wayPoints.Length]; // store the waypoints as an array of points
            for(int i = 0; i < wayPoints.Length; i++){
                points[i] = wayPoints[i].value;
            }

            EntityManager.AddComponentData<FollowWayPointsAction>(crowdEntity, new FollowWayPointsAction {
                id = wpData.id,
                curPointNum = wpData.curPointNum
            });// add the FollowWayPointsAction to the crowd agent

            DynamicBuffer<WayPoint> wpAdded = EntityManager.AddBuffer<WayPoint>(crowdEntity); // add the waypoint buffer to the crowd agent
            for(int i = 0; i < points.Length; i++){
                wpAdded.Add(new WayPoint{value = points[i]});
            }
            EntityManager.RemoveComponent<FetchWayPoints>(crowdEntity);
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        EntityQuery holderQuery = GetEntityQuery(typeof(FollowWayPointsStorage),typeof(WayPoint)); // get all entities with FollowWayPointsStorage & have waypoints
        NativeArray<Entity> holderEntityArray = holderQuery.ToEntityArray(Allocator.TempJob); // get an array of all the entities from query
        NativeArray<FollowWayPointsStorage> holderStorageArray = holderQuery.ToComponentDataArray<FollowWayPointsStorage>(Allocator.TempJob); // get an array of all the storage from the query
        //NativeArray<WayPoint> holderWPArray = holderQuery.To

        NativeArray<EntityWithStorageAndWP> holderArray = new NativeArray<EntityWithStorageAndWP>(holderEntityArray.Length, Allocator.TempJob);
        
        FetchWayPointsJob fetchJob = new FetchWayPointsJob{ // make the job
    
        };
    }
}*/
