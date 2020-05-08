using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using crowd_Actions;
public class SimulationCrowdAuthoring : MonoBehaviour, IConvertGameObjectToEntity 
{
    public int goHomePriority;
    public int goHomeActionID;
    public float3 homePoint;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem){
        Entity homeHolder = dstManager.CreateEntity();
        dstManager.SetName(homeHolder, "Go Home Holder");
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
    }
}
