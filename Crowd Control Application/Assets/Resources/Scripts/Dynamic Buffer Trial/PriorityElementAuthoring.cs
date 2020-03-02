using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

//Handle the conversion of a buffer holding GameObject to an Entity (Manually)
public class PriorityElementAuthoring : MonoBehaviour, IConvertGameObjectToEntity 
{
    public int[] priorites;
    public int[] messages;
    public float[] times;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem){
        DynamicBuffer<PriorityElement> dynamicBuffer = dstManager.AddBuffer<PriorityElement>(entity); // add a buffer to the entity
        
        
        //wpBuffer.Add(new WayPoint {point = float3.zero} );
        for(int i = 0; i < priorites.Length; i++){
            int curPriority = priorites[i];
            int curMessage = messages[i];
            float curTime = times[i];


            //find out where the new element should be added
            int pos = 0;
            Debug.Log(i + " try, Length is: " + dynamicBuffer.Length);

            while(pos < dynamicBuffer.Length && curPriority <= dynamicBuffer[pos].priority){
                if(curPriority == dynamicBuffer[pos].priority){ // if the current priorities are the same
                    //compare the times
                    if(curTime >= dynamicBuffer[pos].timeAdded){ // if the current elements time is greater than the other element's time, this element should go later
                        pos++;
                    }
                    else 
                        break;
                }
                else if(curPriority < dynamicBuffer[pos].priority){ // if this elements priority is smaller than the other element's priority, this element should go later
                    pos++;
                }
                else
                    break;
            }
            //add the element at that location
            dynamicBuffer.Insert( 
                pos,
                new PriorityElement {
                    priority = priorites[i],
                    message = messages[i],
                    timeAdded = times[i],
                    season = Season.Fall,
                    neato = new Neato{thing = 1}
                    
                }); // the Values (in the buffer) are the values in the array
        }
        DynamicBuffer<PriorityElement> db = dstManager.AddBuffer<PriorityElement>(entity);
        db.Add( 
                new PriorityElement {
                    priority = priorites[0],
                    message = messages[0],
                    timeAdded = times[0],
                    season = Season.Fall,
                    neato = new Neato{thing = 1},
                    WPHolder = entity
                    
                }); // the Values (in the buffer) are the values in the array
    }
}