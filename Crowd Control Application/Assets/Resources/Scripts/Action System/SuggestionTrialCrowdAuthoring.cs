using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
public class SuggestionTrialCrowdAuthoring : MonoBehaviour, IConvertGameObjectToEntity 
{
    public int priority;
    public int actionID;
    public float3[] wayPoints;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem){
        Entity holder = dstManager.CreateEntity();
        dstManager.SetName(holder, "Follow WP Holder");

        dstManager.AddComponentData<FollowWayPointsStorage>(holder, new FollowWayPointsStorage { // add the followwaypointsstorage component to the holder
            id =  actionID,
            curPointNum = 0
        }); // store the data

        
        DynamicBuffer<WayPoint> wp = dstManager.AddBuffer<WayPoint>(holder); // create the list of waypoints on the holder
        foreach(float3 wayPoint in wayPoints){ // fill the list of waypoints on the holder
            wp.Add(new WayPoint{
                value = wayPoint
            });
        }
        
        dstManager.AddComponentData<AddFollowWayPointsAction>(entity, new AddFollowWayPointsAction{ //add the addfollowwaypointsaction component to the crowd agent
            id =  actionID,
            priority = priority,
            timeCreated = 0f,
            dataHolder = holder
        }); 
        dstManager.AddBuffer<Action>(entity);
        dstManager.AddComponentData<CurrentAction>(entity, new CurrentAction{
            id = -1,
            type = ActionType.No_Action,
            dataHolder = holder
        });
    }
}

