using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using crowd_Actions;
public class CrowdAuthoring : MonoBehaviour, IConvertGameObjectToEntity 
{
    public bool hasGoHomeAction;
    public int goHomePriority;
    public int goHomeActionID;
    public float3 homePoint;

    public bool hasGoToAndWaitAction;
    public int goToAndWaitPriority;
    public int goToAndWaitActionID;
    public float timeToWait;
    public float3 waitPoint;

    public bool hasFollowWayPointsAction;
    public int followWayPointsPriority;
    public int followWayPointsID;
    public float3[] wayPoints;


    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem){
        dstManager.AddComponent<Crowd>(entity); // Label the entity as a crowd agent
        dstManager.AddComponentData<MovingQuadrantEntity>(entity, new MovingQuadrantEntity{ // Tell the quadrant system that the crowd agent will be moving
            typeEnum = MovingQuadrantEntity.TypeEnum.Crowd
        });
        dstManager.AddComponentData<PreviousMovement>(entity, new PreviousMovement{ //add the Previous movement component to the crowd agent
            value = new float3(0f,0f,0f)
        }); 

        Entity mootHolder = dstManager.CreateEntity(); // we need a holder even if there isn't an action
        //dstManager.SetName(homeHolder, "Moot Point Holder");
        dstManager.AddBuffer<Action>(entity);
        dstManager.AddComponentData<CurrentAction>(entity, new CurrentAction{ // set the current action to no action
            id = -1,
            type = ActionType.No_Action,
            dataHolder = mootHolder
        });

        if(hasGoHomeAction){ // if the crowd agent has a go home action, set up the go home action
            Entity homeHolder = dstManager.CreateEntity();
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

        if(hasGoToAndWaitAction){ // if the crowd agent has a go to and wait action, set the action up (along with the data holder)
            Entity waitHolder = dstManager.CreateEntity();
            //dstManager.SetName(waitHolder, "Go To And Wait Holder");
            dstManager.AddComponentData<GoToAndWaitStorage>(waitHolder, new GoToAndWaitStorage { // add the go to and wait storage component to the holder
                id =  goToAndWaitActionID,
                timeToWait = timeToWait,
                timeWaited = 0f,
                position = waitPoint
            }); // store the data

            dstManager.AddComponentData<AddGoToAndWaitAction>(entity, new AddGoToAndWaitAction{ //add the go to and wait action component to the crowd agent
                id =  goToAndWaitActionID,
                priority = goToAndWaitPriority,
                timeCreated = 0f,
                dataHolder = waitHolder
            }); 
        }

        if(hasFollowWayPointsAction){ // if the crowd agent has a go to and wait action, set the action up (along with the data holder)
            Entity wpHolder = dstManager.CreateEntity();
            //entityCommandBuffer.SetName(holder, "Follow WayPoint Action Data Holder");

            dstManager.AddComponentData<FollowWayPointsStorage>(wpHolder, new FollowWayPointsStorage { // add the followwaypointsstorage component to the holder
                id =  followWayPointsID,
                curPointNum = 0
            }); // store the data

            DynamicBuffer<WayPoint> wp = dstManager.AddBuffer<WayPoint>(wpHolder); // create the list of waypoints on the holder
            foreach(float3 wayPoint in wayPoints){ // fill the list of waypoints on the holder
                wp.Add(new WayPoint{
                    value = wayPoint
                });
            }
        
            dstManager.AddComponentData<AddFollowWayPointsAction>(entity, new AddFollowWayPointsAction{ //add the addfollowwaypointsaction component to the crowd agent
                id = followWayPointsID,
                priority = followWayPointsPriority,
                timeCreated = 0f,
                dataHolder = wpHolder
            }); 
        }

        /*dstManager.AddComponentData<LookAtTarget>(entity, new LookAtTarget
        {
            rotationSpeed = 15f
        });*/

        dstManager.AddComponentData<FacePrevMovement>(entity, new FacePrevMovement
        {
            rotationSpeed = 3f
        });

    }
}
