using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class ReynoldsNearbyFlockPosAuthoring : MonoBehaviour, IConvertGameObjectToEntity 
{
    
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem){
        DynamicBuffer<ReynoldsNearbyFlockPos> dynamicBuffer = dstManager.AddBuffer<ReynoldsNearbyFlockPos>(entity); // add a buffer to the entity
    }
}
