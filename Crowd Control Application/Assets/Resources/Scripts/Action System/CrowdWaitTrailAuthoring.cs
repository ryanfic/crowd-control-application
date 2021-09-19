using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using crowd_Actions;

public enum Direction{
    North,
    South,
    East,
    West
}
public class CrowdWaitTrailAuthoring : MonoBehaviour, IConvertGameObjectToEntity 
{
    public int goHomePriority;
    public int goHomeActionID;
    private float3 homePoint;

    private int goToAndWaitPriority;
    private int goToAndWaitActionID;
    private float timeToWait;
    private float3 waitPoint;
    public Direction spawnDirection;
    
    private Direction destinationDirection;
    public float leftDestinationPercent = 0f;
    public float rightDestinationPercent = 0f;
    public float straightDestinationPercent = 1f;
    

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem){
        Entity homeHolder = dstManager.CreateEntity();
        //dstManager.SetName(homeHolder, "Go Home Holder");
        dstManager.AddBuffer<Action>(entity);
        dstManager.AddComponentData<CurrentAction>(entity, new CurrentAction{
            id = -1,
            type = ActionType.No_Action,
            dataHolder = homeHolder
        });

        destinationDirection = GetDestinationDirection(spawnDirection);

        float2 destinationPoint = GetDestinationPoint(destinationDirection);
        homePoint = new float3(destinationPoint.x, 0, destinationPoint.y);

        dstManager.AddComponentData<GoHomeStorage>(homeHolder, new GoHomeStorage { // add the go home storage component to the holder
            id =  goHomeActionID,
            homePoint = homePoint
        }); // store the data

        dstManager.AddComponentData<AddGoHomeAction>(entity, new AddGoHomeAction{ //add the go home action component to the crowd agent
            id =  goHomeActionID,
            priority = goHomePriority,
            timeCreated = 0f,
            dataHolder = homeHolder
        }); 

        if(!OppositeDirections(spawnDirection,destinationDirection)){
            goToAndWaitActionID = goHomeActionID+1;
            goToAndWaitPriority = goHomePriority+1;
            timeToWait = 0f;
            float2 waitPoint2D = GetWaypointLocation(spawnDirection,destinationDirection);
            waitPoint = new float3(waitPoint2D.x,0,waitPoint2D.y);

            Entity waitHolder = dstManager.CreateEntity();
            //dstManager.SetName(waitHolder, "Go To And Wait Holder");
            dstManager.AddComponentData<GoToAndWaitStorage>(waitHolder, new GoToAndWaitStorage { // add the go to and wait storage component to the holder
                id =  goToAndWaitActionID,
                timeToWait = timeToWait,
                timeWaited = 0f,
                position = waitPoint
            }); // store the data

            dstManager.AddComponentData<AddGoToAndWaitAction>(entity, new AddGoToAndWaitAction{ //add the go to and wait action component to the crowd agent
                id =  goToAndWaitActionID,
                priority = goToAndWaitPriority,
                timeCreated = 0f,
                dataHolder = waitHolder
            }); 
        }

        

        dstManager.AddComponent<CopyTransformToGameObject>(entity);
        //dstManager.AddComponent<CompanionLink>(entity);

        
        dstManager.AddComponentData<PreviousMovement>(entity, new PreviousMovement{ //add the Previous movement component to the crowd agent
            value = new float3(0f,0f,0f)
        }); 
    }

    private bool OppositeDirections(Direction dir1, Direction dir2){
        bool opposite = false;
        switch(spawnDirection){
            case Direction.North:
                if(dir2 == Direction.South){
                    opposite = true;
                }
                break;
            case Direction.South:
                if(dir2 == Direction.North){
                    opposite = true;
                }
                break;
            case Direction.East:
                if(dir2 == Direction.West){
                    opposite = true;
                }
                break;
            case Direction.West:
                if(dir2 == Direction.East){
                    opposite = true;
                }
                break;
            default:
                break;
        }
        return opposite;
    }
    private bool DirectionToLeft(Direction spawnDir, Direction destinationDir){
        bool left = false;
        switch(spawnDir){
            case Direction.North:
                if(destinationDir == Direction.East){ // east is to the left of north
                    left = true;
                } 
                break;
            case Direction.South:
                if(destinationDir == Direction.West){ // west is to the left of south
                    left = true;
                } 
                break;
            case Direction.East:
                if(destinationDir == Direction.South){ // south is to the left of east
                    left = true;
                } 
                break;
            case Direction.West:
                if(destinationDir == Direction.North){ // north is to the left of west
                    left = true;
                } 
                break;
            default:
                Debug.Log("Direction Not Found");
                break;
        }
        return left;
    }

    private float2 GetWaypointLocation(Direction spawnDir, Direction destinationDir){
        float minX = 0;
        float maxX = 0;
        float minZ = 0;
        float maxZ = 0;

        //Select the min/max value for random selection
        switch(spawnDir){
            case Direction.North:
                if(DirectionToLeft(spawnDir,destinationDir)){ //if moving to the left, going from north to east
                    //use values of the top right side of the intersection
                    minX = 2.5f;
                    maxX = 5f;
                    minZ = 2.5f;
                    maxZ = 5f;
                }
                else{ //if moving to the right, going from north to west
                    //use values of the top left side of the intersection
                    minX = -5f;
                    maxX = -2.5f;
                    minZ = 2.5f;
                    maxZ = 5f;
                }
                    
                break;
            case Direction.South:
                if(DirectionToLeft(spawnDir,destinationDir)){//if moving to the left, going from south to west
                    //use values of the bottom left side of the intersection
                    minX = -5f;
                    maxX = -2.5f;
                    minZ = -5f;
                    maxZ = -2.5f;
                }
                else{ //if moving to the right, going from south to east
                    //use values of the bottom right side of the intersection
                    minX = 2.5f;
                    maxX = 5f;
                    minZ = -5f;
                    maxZ = -2.5f;
                }
                break;
            case Direction.East:
                if(DirectionToLeft(spawnDir,destinationDir)){//if moving to the left, going from east to south
                    //use values of the bottom right side of the intersection
                    minX = 2.5f;
                    maxX = 5f;
                    minZ = -5f;
                    maxZ = -2.5f;
                }
                else{ //if moving to the right, going from east to north
                    //use values of the top right side of the intersection
                    minX = 2.5f;
                    maxX = 5f;
                    minZ = 2.5f;
                    maxZ = 5f;
                }
                break;
            case Direction.West:
                if(DirectionToLeft(spawnDir,destinationDir)){//if moving to the left, going from west to north
                    //use values of the top left side of the intersection
                    minX =-5f;
                    maxX = -2.5f;
                    minZ = 2.5f;
                    maxZ = 5f;
                }
                else{ //if moving to the right, going from west to south
                    //use values of the bottom left side of the intersection
                    minX = -5f;
                    maxX = -2.5f;
                    minZ = -5f;
                    maxZ = -2.5f;
                }
                break;
            default:
                Debug.Log("Direction Not Found");
                break;
        }

        float xVal = UnityEngine.Random.Range(minX,maxX);
        float zVal = UnityEngine.Random.Range(minZ,maxZ);
        float2 destination = new float2(xVal,zVal);

        return destination;
    }

    private Direction GetDestinationDirection(Direction spawnDirection){
        Direction resultDirection = Direction.North;
        bool left = false;
        bool right = false;
        float directionDecider =  UnityEngine.Random.Range(0f,1f);
        if(directionDecider < leftDestinationPercent){
            left = true;
        }
        else if(directionDecider < leftDestinationPercent+rightDestinationPercent){
            right = true;
        }
        //if not left or right, is going straight
        switch(spawnDirection){
            case Direction.North:
                if(left){
                    resultDirection = Direction.East;
                }
                else if(right){
                    resultDirection = Direction.West;
                }
                else{
                    resultDirection = Direction.South;
                }
                break;
            case Direction.South:
                if(left){
                    resultDirection = Direction.West;
                }
                else if(right){
                    resultDirection = Direction.East;
                }
                else{
                    resultDirection = Direction.North;
                }
                break;
            case Direction.East:
                if(left){
                    resultDirection = Direction.South;
                }
                else if(right){
                    resultDirection = Direction.North;
                }
                else{
                    resultDirection = Direction.West;
                }
                break;
            case Direction.West:
                if(left){
                    resultDirection = Direction.North;
                }
                else if(right){
                    resultDirection = Direction.South;
                }
                else{
                    resultDirection = Direction.East;
                }
                break;
            default:
                Debug.Log("Direction Not Found");
                break;
        }
        return resultDirection;
    }

    //Create a random destination point based on which direction the agent is heading
    private float2 GetDestinationPoint(Direction dir){
        float minX = 0;
        float maxX = 0;
        float minZ = 0;
        float maxZ = 0;

        //Select the min/max value for random selection
        switch(dir){
            case Direction.North:
                minX = -4.5f;
                maxX = 4.5f;
                minZ = 18.5f;
                maxZ = 24.5f;
                break;
            case Direction.South:
                minX = -4.5f;
                maxX = 4.5f;
                minZ = -24.5f;
                maxZ = -18.5f;
                break;
            case Direction.East:
                minX = 18.5f;
                maxX = 24.5f;
                minZ = -4.5f;
                maxZ = 4.5f;
                break;
            case Direction.West:
                minX = -18.5f;
                maxX = -24.5f;
                minZ = -4.5f;
                maxZ = 4.5f;
                break;
            default:
                Debug.Log("Direction Not Found");
                break;
        }

        float xVal = UnityEngine.Random.Range(minX,maxX);
        float zVal = UnityEngine.Random.Range(minZ,maxZ);
        float2 destination = new float2(xVal,zVal);

        return destination;
    }
}
