using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


public class CrowdAreaCounterAuthoring : MonoBehaviour, IConvertGameObjectToEntity 
{
    //public float frequency;
    private float3 center;
    private float minX,maxX,minZ,maxZ;

    public void Awake(){
        Transform trans = GetComponent<Transform>();
        center = trans.localPosition;
        //Debug.Log("Center: "+center);
        minX = center.x - trans.localScale.x/2;
        maxX = center.x + trans.localScale.x/2;
        minZ = center.z - trans.localScale.z/2;
        maxZ = center.z + trans.localScale.z/2;
        //Debug.Log("minX: "+minX+" maxX: "+maxX+" minZ: "+minZ+" maxZ: "+maxZ);

    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem){ 
            dstManager.AddComponentData<CrowdAreaCounter>(entity, new CrowdAreaCounter{
                //frequency = frequency,
                //lastCount = 0,
                minX = minX,
                maxX = maxX,
                minZ = minZ,
                maxZ = maxZ
            });
            //dstManager.SetName(entity,"Crowd Area Counter");
    }
}