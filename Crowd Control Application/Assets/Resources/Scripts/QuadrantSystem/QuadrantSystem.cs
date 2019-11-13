using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using MousePositionUtil;

public class QuadrantSystem : ComponentSystem
{    private const int quadrantZMultiplier = 1000000;
    private const int quadrantYMultiplier = 1000;
    private const int quadrantCellSize = 5;
    //given a position, calculate the hashmap key
    private static int GetPositionHashMapKey(float3 position){
        return (int) (math.floor(position.x / quadrantCellSize) 
        + (quadrantYMultiplier * math.floor(position.y / quadrantCellSize)) 
        + (quadrantZMultiplier * math.floor(position.z / quadrantCellSize)));
    }

    //To visualize quadrants
    //Given a point, draw the quadrant that surrounds that point
    private static void DebugDrawQuadrant(float3 position){
        //find the bottom lower left of the cube
        Vector3 botLowerLeft = new Vector3(math.floor(position.x / quadrantCellSize) * quadrantCellSize,
                                        math.floor(position.y / quadrantCellSize) * quadrantCellSize,
                                        math.floor(position.z / quadrantCellSize) * quadrantCellSize);
        //draw the quadrant based on the bottom lower left of the cube
        Debug.DrawLine(botLowerLeft, botLowerLeft + new Vector3(1,0,0) * quadrantCellSize); //1
        Debug.DrawLine(botLowerLeft, botLowerLeft + new Vector3(0,1,0) * quadrantCellSize); //2
        Debug.DrawLine(botLowerLeft, botLowerLeft + new Vector3(0,0,1) * quadrantCellSize); //3
        Debug.DrawLine(botLowerLeft + new Vector3(1,0,0) * quadrantCellSize, botLowerLeft + new Vector3(1,1,0) * quadrantCellSize); //4
        Debug.DrawLine(botLowerLeft + new Vector3(1,0,0) * quadrantCellSize, botLowerLeft + new Vector3(1,0,1) * quadrantCellSize); //5
        Debug.DrawLine(botLowerLeft + new Vector3(0,1,0) * quadrantCellSize, botLowerLeft + new Vector3(1,1,0) * quadrantCellSize); //6
        Debug.DrawLine(botLowerLeft + new Vector3(0,1,0) * quadrantCellSize, botLowerLeft + new Vector3(0,1,1) * quadrantCellSize); //7
        Debug.DrawLine(botLowerLeft + new Vector3(0,0,1) * quadrantCellSize, botLowerLeft + new Vector3(1,0,1) * quadrantCellSize); //8
        Debug.DrawLine(botLowerLeft + new Vector3(0,0,1) * quadrantCellSize, botLowerLeft + new Vector3(0,1,1) * quadrantCellSize); //9
        Debug.DrawLine(botLowerLeft + new Vector3(1,0,1) * quadrantCellSize, botLowerLeft + new Vector3(1,1,1) * quadrantCellSize); //10
        Debug.DrawLine(botLowerLeft + new Vector3(0,1,1) * quadrantCellSize, botLowerLeft + new Vector3(1,1,1) * quadrantCellSize); //11
        Debug.DrawLine(botLowerLeft + new Vector3(1,1,0) * quadrantCellSize, botLowerLeft + new Vector3(1,1,1) * quadrantCellSize); //12
        //Debug.Log("Pos" + position);
    }

    protected override void OnUpdate(){

        //NativeMultiHashMap is for storing the quadrants
        //quadrants need multiple things (values)
        //keys are ints, and it holds Entity s
        NativeMultiHashMap<int, Entity> quadrantMultiHashMap;
        
        Debug.Log("Mouse position: " + MousePosition.GetMouseWorldPositionOnPlane(50));
        DebugDrawQuadrant(MousePosition.GetMouseWorldPositionOnPlane(50));
    }
}
