using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

//Handle the conversion of a buffer holding GameObject to an Entity (Manually)
public class IntBufferElementAuthoring : MonoBehaviour, IConvertGameObjectToEntity 
{
    public int[] valueArray;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem){
        DynamicBuffer<IntBufferElement> dynamicBuffer = dstManager.AddBuffer<IntBufferElement>(entity); // add a buffer to the entity
        foreach(int value in valueArray){
            dynamicBuffer.Add( // add all values in the array to the buffer on the entity
                new IntBufferElement {Value = value}); // the Values (in the buffer) are the values in the array
        }
    }
}
