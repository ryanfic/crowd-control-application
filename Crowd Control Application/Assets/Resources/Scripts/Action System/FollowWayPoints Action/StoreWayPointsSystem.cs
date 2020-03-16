using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;


public class StoreWayPointsSystem : ComponentSystem
{
    protected override void OnUpdate(){
        Entities.ForEach((Entity crowdEntity, ref StoreWayPoints store, DynamicBuffer<WayPoint> buffer) => {  
            Entity wayPointHolder = store.dataHolder; // get the entity that holds the waypoints
            /*FollowWayPointsStorage toStore = new FollowWayPointsStorage(); // create the new storage
            toStore.id = store.id; // set the storage data
            toStore.curPointNum = store.curPointNum; // set the storage data*/

            buffer.Clear();
            EntityManager.AddComponentData<FollowWayPointsStorage>(wayPointHolder, new FollowWayPointsStorage {
                id = store.id,
                curPointNum = store.curPointNum
            }); // store the data
            EntityManager.RemoveComponent<HasReynoldsSeekTargetPos>(crowdEntity);
            EntityManager.RemoveComponent<StoreWayPoints>(crowdEntity);
            EntityManager.RemoveComponent<FollowWayPointsAction>(crowdEntity);
            
        });
    }
}
