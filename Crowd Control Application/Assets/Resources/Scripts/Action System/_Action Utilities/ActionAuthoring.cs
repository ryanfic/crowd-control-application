﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using crowd_Actions;
public class ActionAuthoring : MonoBehaviour, IConvertGameObjectToEntity 
{
    //public float3[] positions;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem){
        DynamicBuffer<Action> tempBuff = dstManager.AddBuffer<Action>(entity);
        /*foreach(float3 location in positions){ //Add the waypoints
            tempBuff.Add(new WayPoint{value = location});
        }*/
    }
}

