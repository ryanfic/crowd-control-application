using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
public class GoHomeTrialAuthoring : MonoBehaviour, IConvertGameObjectToEntity 
{
    public int followWPPriority;
    public int followWPActionID;
    public float3[] wayPoints;

    public int goHomePriority;
    public int goHomeActionID;
    public float3 homePoint;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem){
        Entity wpHolder = dstManager.CreateEntity();
        dstManager.SetName(wpHolder, "Follow WP Holder");

        dstManager.AddComponentData<FollowWayPointsStorage>(wpHolder, new FollowWayPointsStorage { // add the followwaypointsstorage component to the holder
            id =  followWPActionID,
            curPointNum = 0
        }); // store the data

        
        DynamicBuffer<WayPoint> wp = dstManager.AddBuffer<WayPoint>(wpHolder); // create the list of waypoints on the holder
        foreach(float3 wayPoint in wayPoints){ // fill the list of waypoints on the holder
            wp.Add(new WayPoint{
                value = wayPoint
            });
        }
        
        dstManager.AddComponentData<AddFollowWayPointsAction>(entity, new AddFollowWayPointsAction{ //add the addfollowwaypointsaction component to the crowd agent
            id =  followWPActionID,
            priority = followWPPriority,
            timeCreated = 0f,
            dataHolder = wpHolder
        }); 
        dstManager.AddBuffer<Action>(entity);
        dstManager.AddComponentData<CurrentAction>(entity, new CurrentAction{
            id = -1,
            type = ActionType.No_Action,
            dataHolder = wpHolder
        });

        Entity homeHolder = dstManager.CreateEntity();
        dstManager.SetName(homeHolder, "Go Home Holder");

        dstManager.AddComponentData<GoHomeStorage>(homeHolder, new GoHomeStorage { // add the go home storage component to the holder
            id =  goHomeActionID,
            homePoint = homePoint
        }); // store the data

        dstManager.AddComponentData<AddGoHomeAction>(entity, new AddGoHomeAction{ //add the go home action component to the crowd agent
            id =  goHomeActionID,
            priority = goHomePriority,
            timeCreated = 0f,
            dataHolder = homeHolder
        }); 
    }
}

