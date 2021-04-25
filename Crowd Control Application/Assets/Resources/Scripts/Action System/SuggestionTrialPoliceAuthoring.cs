using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
public class SuggestionTrialPoliceAuthoring : MonoBehaviour, IConvertGameObjectToEntity 
{
    public float suggestFrequency;
    public float suggestRadius;
    public int actionID;
    public float3[] wayPoints;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem){
        DynamicBuffer<SuggestNearbyCrowd> tempBuff = dstManager.AddBuffer<SuggestNearbyCrowd>(entity);
        Entity holder = dstManager.CreateEntity();
        //dstManager.SetName(holder, "Suggestion Data Holder For " + dstManager.GetName(entity));
        DynamicBuffer<WayPoint> wp = dstManager.AddBuffer<WayPoint>(holder);
        foreach(float3 pos in wayPoints){
            wp.Add(new WayPoint{
                value = pos
            });
        }
        dstManager.AddComponentData<SuggestFollowWayPointsAction>(entity, new SuggestFollowWayPointsAction{
            id = actionID,
            frequency = suggestFrequency, 
            lastSuggestionTime = 0f,
            radius = suggestRadius,
            waypointHolder = holder
        });
    }
}
