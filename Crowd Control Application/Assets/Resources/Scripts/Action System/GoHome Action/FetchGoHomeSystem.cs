using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

public class FetchGoHomeSystem : ComponentSystem
{
    
    protected override void OnUpdate(){
        Entities.ForEach((Entity crowdEntity, ref FetchGoHomeData fetch) => {  
            GoHomeStorage homeData = EntityManager.GetComponentData<GoHomeStorage>(fetch.dataHolder); // get the action data from the entity that holds the data
            
            EntityManager.AddComponentData<GoHomeAction>(crowdEntity, new GoHomeAction {
                id = homeData.id,
            });// add the GoHomeAction to the crowd agent

            EntityManager.AddComponentData<HasReynoldsSeekTargetPos>(crowdEntity, new HasReynoldsSeekTargetPos { // add the Target position to the crowd agent so it moves
                targetPos = homeData.homePoint
            });
            EntityManager.RemoveComponent<FetchGoHomeData>(crowdEntity);
        });
    }
}
