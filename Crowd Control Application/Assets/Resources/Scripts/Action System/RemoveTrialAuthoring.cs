using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using crowd_Actions;


//Handle the conversion of a buffer holding GameObject to an Entity (Manually)
public class RemoveTrialAuthoring : MonoBehaviour, IConvertGameObjectToEntity 
{

    public int[] priorites;
    public float[] times;
    public float3[] positions1;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem){
        
        //Create a separate entity to hold the waypoint buffer

        Entity holder = dstManager.CreateEntity();
        dstManager.SetName(holder, "Waypoint Holder 1");
        DynamicBuffer<WayPoint> tempBuff = dstManager.AddBuffer<WayPoint>(holder);
        foreach(float3 location in positions1){ //Add the waypoints
            tempBuff.Add(new WayPoint{value = location});
        }
        dstManager.AddComponentData<FollowWayPointsStorage>(holder, new FollowWayPointsStorage {
                id = 0,
                curPointNum = 0
            });// add the FollowWayPointsAction to the crowd agent

        /*Entity holder2 = dstManager.CreateEntity();
        dstManager.SetName(holder2, "Waypoint Holder 2");
        DynamicBuffer<WayPoint> tempBuff2 = dstManager.AddBuffer<WayPoint>(holder);
        tempBuff2.Add(new WayPoint{value = new float3(0,0,0)});
        dstManager.AddComponentData<FollowWayPointsStorage>(holder2, new FollowWayPointsStorage {
                id = 1,
                curPointNum = 0
            });// add the FollowWayPointsAction to the crowd agent*/


        
        
        
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

                    dataHolder = holder,
                    
                }); // the Values (in the buffer) are the values in the array
        }


        dstManager.AddComponentData<FetchWayPoints>(entity, new FetchWayPoints {
                id = 0,
                dataHolder = holder
            });// add the FollowWayPointsAction to the crowd agent

        
    }

}
