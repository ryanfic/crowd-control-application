using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

public class FetchGoToAndWaitSystem : ComponentSystem
{
    
    protected override void OnUpdate(){
        Entities.ForEach((Entity crowdEntity, ref FetchGoToAndWaitData fetch) => {  
            GoToAndWaitStorage waitData = EntityManager.GetComponentData<GoToAndWaitStorage>(fetch.dataHolder); // get the action data from the entity that holds the data
            
            EntityManager.AddComponentData<GoToAndWaitAction>(crowdEntity, new GoToAndWaitAction {
                id = waitData.id,
                timeWaited = waitData.timeWaited,
                timeToWait = waitData.timeToWait,
                position = waitData.position
            });// add the GoToAndWaitAction to the crowd agent

            EntityManager.AddComponentData<HasReynoldsSeekTargetPos>(crowdEntity, new HasReynoldsSeekTargetPos { // add the Target position to the crowd agent so it moves
                targetPos = waitData.position
            });
            EntityManager.RemoveComponent<FetchGoToAndWaitData>(crowdEntity);
        });
    }
}