using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using crowd_Actions;

//Handle the conversion of a buffer holding GameObject to an Entity (Manually)
public class TrialActionAuthoring : MonoBehaviour, IConvertGameObjectToEntity 
{
    public int[] priorites;
    //public int[] messages;
    public float[] times;
    public float3[] positions1;
    public float3[] positions2;
    public float3[] positions3;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem){
        
        Entity[] holders = new Entity[3];


        //Create a separate entity to hold the waypoint buffer
        for(int i = 0; i < 3; i++){
            holders[i] = dstManager.CreateEntity();
            dstManager.SetName(holders[i], "Waypoint Holder " + i);
            DynamicBuffer<WayPoint> tempBuff = dstManager.AddBuffer<WayPoint>(holders[i]);
            if(i == 0){ // if is the first entity, access the first position list
                foreach(float3 location in positions1){ //Add the waypoints
                    tempBuff.Add(new WayPoint{value = location});
                }
                
            }
            else if (i == 1){ // if is the second entity, access the second position list
                foreach(float3 location in positions2){ //Add the waypoints
                    tempBuff.Add(new WayPoint{value = location});
                }
            }
            else{ // if is the third entity, access the third position list
                foreach(float3 location in positions3){ //Add the waypoints
                    tempBuff.Add(new WayPoint{value = location});
                }
            }
            
        }
        
        
        
        DynamicBuffer<Action> dynamicBuffer = dstManager.AddBuffer<Action>(entity); // add a buffer to the entity
        //wpBuffer.Add(new WayPoint {point = float3.zero} );
        for(int i = 0; i < priorites.Length; i++){
            int curPriority = priorites[i];
            //int curMessage = messages[i];
            float curTime = times[i];


            //find out where the new element should be added
            int pos = 0;
            //Debug.Log(i + " try, Length is: " + dynamicBuffer.Length);

            while(pos < dynamicBuffer.Length && curPriority <= dynamicBuffer[pos].priority){
                if(curPriority == dynamicBuffer[pos].priority){ // if the current priorities are the same
                    //compare the times
                    if(curTime >= dynamicBuffer[pos].timeCreated){ // if the current elements time is greater than the other element's time, this element should go later
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
                new Action {
                    id = i,
                    priority = priorites[i],
                    type = ActionType.Follow_WayPoints,
                    timeCreated = times[i],

                    dataHolder = holders[i],
                    
                }); // the Values (in the buffer) are the values in the array
        }
        
    }
}