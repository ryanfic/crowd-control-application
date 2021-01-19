using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


public class StoreWayPointsSystem : SystemBase{
    protected override void OnUpdate(){
        Entities
            .WithoutBurst()
            .ForEach((Entity crowdEntity, ref StoreWayPoints store, ref DynamicBuffer<WayPoint> buffer) => {  
                Debug.Log("HEY WE ARE HERE; Store Way Points System");
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
                
        }).Run();
    }
}
