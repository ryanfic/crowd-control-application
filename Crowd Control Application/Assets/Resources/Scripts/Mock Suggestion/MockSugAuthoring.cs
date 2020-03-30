using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
public class MockSugAuthoring : MonoBehaviour, IConvertGameObjectToEntity 
{
    public float3[] positions;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem){
        DynamicBuffer<WayPoint> tempBuff = dstManager.AddBuffer<WayPoint>(entity);
        foreach(float3 location in positions){ //Add the waypoints
            tempBuff.Add(new WayPoint{value = location});
        }
    }
}
