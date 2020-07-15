using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


public class PoliceUnitAuthoring : MonoBehaviour, IConvertGameObjectToEntity 
{
    public GameObject PoliceAgentGO;
    public float LineSpacing;
    public float LineWidth;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem){ 
        using (BlobAssetStore blobAssetStore = new BlobAssetStore()){
            dstManager.AddComponent<PoliceUnitComponent>(entity);
            Entity policeAgent = GameObjectConversionUtility.ConvertGameObjectHierarchy(PoliceAgentGO,
                GameObjectConversionSettings.FromWorld(dstManager.World, blobAssetStore));
            dstManager.SetName(policeAgent,"Police Agent");

            dstManager.SetName(entity,"Police Unit");
            //float3 unitPos = dstManager.GetComponentData<Translation>(entity).Value;

            //Set up the front line
            Entity line1 = dstManager.Instantiate(policeAgent);
            dstManager.SetName(line1,"Police Line 1");
            dstManager.AddComponentData<Parent>(line1, new Parent{Value = entity});
            dstManager.AddComponentData<LocalToParent>(line1, new LocalToParent());
            dstManager.SetComponentData<Translation>(line1, new Translation{Value = new float3(0f,0f,LineSpacing)});
            dstManager.AddComponent<FrontPoliceLineComponent>(line1);

            //Set up the center line
            Entity line2 = dstManager.Instantiate(policeAgent);
            dstManager.SetName(line2,"Police Line 2");
            dstManager.AddComponentData<Parent>(line2, new Parent{Value = entity});
            dstManager.AddComponentData<LocalToParent>(line2, new LocalToParent());
            dstManager.SetComponentData<Translation>(line2, new Translation{Value = new float3(0f,0f,0f)});
            dstManager.AddComponent<CenterPoliceLineComponent>(line2);

            //Set up the rear line
            Entity line3 = dstManager.Instantiate(policeAgent);
            dstManager.SetName(line3,"Police Line 3");
            dstManager.AddComponentData<Parent>(line3, new Parent{Value = entity});
            dstManager.AddComponentData<LocalToParent>(line3, new LocalToParent());
            dstManager.SetComponentData<Translation>(line3, new Translation{Value = new float3(0f,0f,-LineSpacing)});
            dstManager.AddComponent<RearPoliceLineComponent>(line3);

            dstManager.AddComponentData<To3SidedBoxFormComponent>(line1, new To3SidedBoxFormComponent{
                LineSpacing = LineSpacing,
                LineWidth = LineWidth
            });

            dstManager.AddComponentData<To3SidedBoxFormComponent>(line2, new To3SidedBoxFormComponent{
                LineSpacing = LineSpacing,
                LineWidth = LineWidth
            });

            dstManager.AddComponentData<To3SidedBoxFormComponent>(line3, new To3SidedBoxFormComponent{
                LineSpacing = LineSpacing,
                LineWidth = LineWidth
            });







            
            //dstManager.SetComponentData<Child>(entity, new Child{Value = line1});
            //DynamicBuffer<Child> unitChildren = dstManager.AddBuffer<Child>(entity);
            //unitChildren.Add(new Child{Value = line1});
            
            

            
            //Debug.Log("Pos: " + unitPos);
                    
        }
    }
        //dstManager.AddComponentData(entity, new Parent { Value = parentEntity});

        //Create a separate entity to hold the waypoint buffer

        /*Entity holder = dstManager.CreateEntity();
        dstManager.SetName(holder, "Waypoint Holder 1");
        DynamicBuffer<WayPoint> tempBuff = dstManager.AddBuffer<WayPoint>(holder);
        foreach(float3 location in positions1){ //Add the waypoints
            tempBuff.Add(new WayPoint{value = location});
        }
        dstManager.AddComponentData<FollowWayPointsStorage>(holder, new FollowWayPointsStorage {
                id = 1,
                curPointNum = 0
            });// add the FollowWayPointsAction to the crowd agent

        Entity holder2 = dstManager.CreateEntity();
        dstManager.SetName(holder2, "Waypoint Holder 2");
        DynamicBuffer<WayPoint> tempBuff2 = dstManager.AddBuffer<WayPoint>(holder2);
        tempBuff2.Add(new WayPoint{value = new float3(3,0,0)});
        dstManager.AddComponentData<FollowWayPointsStorage>(holder2, new FollowWayPointsStorage {
                id = 0,
                curPointNum = 0
            });// add the FollowWayPointsAction to the crowd agent


        
        
        
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

                    dataHolder = holder2,
                    
                }); // the Values (in the buffer) are the values in the array


        }
            


        dstManager.AddComponentData<CurrentAction>(entity, new CurrentAction{
            id = 0,
            type = ActionType.Follow_WayPoints
        });
        dstManager.AddComponentData<FollowWayPointsAction>(entity, new FollowWayPointsAction{
            id = 0,
            curPointNum = 0
        });
        DynamicBuffer<WayPoint> wpBuffer = dstManager.AddBuffer<WayPoint>(entity); // add a buffer to the entity
        wpBuffer.Add(new WayPoint {value = new float3(3,0,0)} );
        
        dstManager.AddComponentData<AddFollowWayPointsAction>(entity, new AddFollowWayPointsAction{
            id = 1,
            priority = 1,
            timeCreated = 2,
            dataHolder = holder
        });*/

        
    

}

