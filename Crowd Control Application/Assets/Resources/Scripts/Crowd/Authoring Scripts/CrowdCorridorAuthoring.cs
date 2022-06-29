using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using crowd_Actions;

public class CrowdCorridorAuthoring : MonoBehaviour, IConvertGameObjectToEntity
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
    public float straightDestinationPercent = 1f;


    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        


        Entity homeHolder = dstManager.CreateEntity();


        //dstManager.SetName(homeHolder, "Go Home Holder");
        dstManager.AddBuffer<Action>(entity);
        dstManager.AddComponentData<CurrentAction>(entity, new CurrentAction
        {
            id = -1,
            type = ActionType.No_Action,
            dataHolder = homeHolder
        });

        destinationDirection = GetDestinationDirection(spawnDirection);

        float2 destinationPoint = GetDestinationPoint(destinationDirection);
        homePoint = new float3(destinationPoint.x, 0, destinationPoint.y);

        dstManager.AddComponentData<GoHomeStorage>(homeHolder, new GoHomeStorage
        { // add the go home storage component to the holder
            id = goHomeActionID,
            homePoint = homePoint
        }); // store the data

        dstManager.AddComponentData<AddGoHomeAction>(entity, new AddGoHomeAction
        { //add the go home action component to the crowd agent
            id = goHomeActionID,
            priority = goHomePriority,
            timeCreated = 0f,
            dataHolder = homeHolder
        });

        



        dstManager.AddComponent<CopyTransformToGameObject>(entity);
        //dstManager.AddComponent<CompanionLink>(entity);

        dstManager.AddComponent<Crowd>(entity); // Label the entity as a crowd agent
        dstManager.AddComponentData<MovingQuadrantEntity>(entity, new MovingQuadrantEntity
        { // Tell the quadrant system that the crowd agent will be moving
            typeEnum = MovingQuadrantEntity.TypeEnum.Crowd
        });
        dstManager.AddComponentData<PreviousMovement>(entity, new PreviousMovement
        { //add the Previous movement component to the crowd agent
            value = new float3(0f, 0f, 0f)
        });

        /*dstManager.AddComponentData<LookAtTarget>(entity, new LookAtTarget
        {
            rotationSpeed = 3f
        });*/

        dstManager.AddComponentData<FacePrevMovement>(entity, new FacePrevMovement
        {
            rotationSpeed = 1.5f
        });
    }

    

    private Direction GetDestinationDirection(Direction spawnDirection)
    {
        Direction resultDirection = Direction.East;
        float directionDecider = UnityEngine.Random.Range(0f, 1f);
        //if not left or right, is going straight
        switch (spawnDirection)
        {

            case Direction.East:
                
                resultDirection = Direction.West;
                
                break;
            case Direction.West:
                
                resultDirection = Direction.East;
                
                break;
            default:
                Debug.Log("Direction Not Found");
                break;
        }
        return resultDirection;
    }

    //Create a random destination point based on which direction the agent is heading
    private float2 GetDestinationPoint(Direction dir)
    {
        float minX = 0;
        float maxX = 0;
        float minZ = 0;
        float maxZ = 0;

        //Select the min/max value for random selection
        switch (dir)
        {
            case Direction.East:
                minX = 18.5f;
                maxX = 24.5f;
                minZ = -2.1f;
                maxZ = 2.1f;
                break;
            case Direction.West:
                minX = -18.5f;
                maxX = -24.5f;
                minZ = -2.1f;
                maxZ = 2.1f;
                break;
            default:
                Debug.Log("Direction Not Found");
                break;
        }

        float xVal = UnityEngine.Random.Range(minX, maxX);
        float zVal = UnityEngine.Random.Range(minZ, maxZ);
        float2 destination = new float2(xVal, zVal);

        return destination;
    }
}
