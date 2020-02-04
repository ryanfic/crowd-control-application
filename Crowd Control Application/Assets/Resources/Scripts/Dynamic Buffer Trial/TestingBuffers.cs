using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class TestingBuffers : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager; // get EM reference
        Entity entity = entityManager.CreateEntity(); //create an entity to play with
        DynamicBuffer<IntBufferElement> dynamicBuffer = entityManager.AddBuffer<IntBufferElement>(entity); //add a buffer to the entity
        dynamicBuffer.Add(new IntBufferElement{Value = 1}); //instantiate buffer elements
        dynamicBuffer.Add(new IntBufferElement{Value = 2});
        dynamicBuffer.Add(new IntBufferElement{Value = 3});

        DynamicBuffer<int> intDynamicBuffer = dynamicBuffer.Reinterpret<int>(); //reinterpret so we don't have to keep instantiating IntBufferElements
        intDynamicBuffer[1] = 5; // you can alter something already in the buffer
        intDynamicBuffer.Add(1); // you can add things to the buffer, and it affects the original buffer

        for(int i = 0; i < intDynamicBuffer.Length; i++){
            intDynamicBuffer[i]++;
        }

        DynamicBuffer<IntBufferElement> dynBuff //You put what type the buffer holds in the <>
            = entityManager.GetBuffer<IntBufferElement>(entity); //you pass the entity that has the buffer attached to it
    }

}
