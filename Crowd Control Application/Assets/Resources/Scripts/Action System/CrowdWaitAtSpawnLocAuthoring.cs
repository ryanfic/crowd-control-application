using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using crowd_Actions;

public class CrowdWaitAtSpawnLocAuthoring : MonoBehaviour, IConvertGameObjectToEntity 
{
    public int goHomePriority;
    public int goHomeActionID;
    public float3 homePoint;

    public int goToAndWaitPriority;
    public int goToAndWaitActionID;
    public float timeToWait;
    //public float3 waitPoint;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem){
        Entity homeHolder = dstManager.CreateEntity();
        //dstManager.SetName(homeHolder, "Go Home Holder");
        dstManager.AddBuffer<Action>(entity);
        dstManager.AddComponentData<CurrentAction>(entity, new CurrentAction{
            id = -1,
            type = ActionType.No_Action,
            dataHolder = homeHolder
        });

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
        Entity waitHolder = dstManager.CreateEntity();
        //dstManager.SetName(waitHolder, "Go To And Wait Holder");

        float3 waitPoint = new float3(gameObject.transform.localPosition.x,gameObject.transform.localPosition.y,gameObject.transform.localPosition.z);
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

        //dstManager.AddComponent<CopyTransformToGameObject>(entity);


        
        dstManager.AddComponentData<PreviousMovement>(entity, new PreviousMovement{ //add the Previous movement component to the crowd agent
            value = new float3(0f,0f,0f)
        }); 
    }
}