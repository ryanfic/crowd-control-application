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
{   
    private const int quadrantDimMax = 1000; // The maximum amount of quadrants you would expect
                                            // used in the hashMap
    private const int quadrantZMultiplier = quadrantDimMax*quadrantDimMax;
    private const int quadrantYMultiplier = quadrantDimMax;
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
        
    }

    /*
    * Given a specific key, find out how many entities are in that quadrant (in the hashmap)
    */
    private static int GetEntityCountInHashMap(NativeMultiHashMap<int, Entity> quadrantMultiHashMap, int hashMapKey){
        Entity entity; //out for the function below
        NativeMultiHashMapIterator<int> nativeMultiHashMapIterator; //another out for the function below
        int count = 0;
        //try to get to get the first value for the given key
        if(quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out entity, out nativeMultiHashMapIterator)){
            //if there is a value, try to get more (must go through once since there was at least one entity)
            do{
                count++; //there was an entity, let's count it!
            } while(quadrantMultiHashMap.TryGetNextValue(out entity, ref nativeMultiHashMapIterator));
        }
        return count;
    }

    private struct SetQuadrantDataHashMapJob : IJobForEachWithEntity<Translation>{
        public NativeMultiHashMap<int, Entity>.Concurrent quadrantMultiHashMap; //job is about putting things into the hashmap, so we need
                                                                    //a reference to the hashmap in question
        public void Execute(Entity entity, int index, ref Translation translation){

        }
    }
    protected override void OnUpdate(){
        //calculate the number of entities we have to store (entities with translation component)
        EntityQuery entityQuery = GetEntityQuery(typeof(Translation));

        //NativeMultiHashMap is for storing the quadrants
        //quadrants need multiple things (values)
        //keys are ints, and it holds Entity s
        //the length is calculated from above
        NativeMultiHashMap<int, Entity> quadrantMultiHashMap = new NativeMultiHashMap<int, Entity>(entityQuery.CalculateLength(),Allocator.TempJob);

        //Cycle through all entities and get their positions
        //selects all entities with a translation component
        Entities.ForEach((Entity entity, ref Translation translation) =>{
            int hashMapKey = GetPositionHashMapKey(translation.Value);
            quadrantMultiHashMap.Add(hashMapKey, entity);
        });
        
        //Debug.Log(GetPositionHashMapKey(MousePosition.GetMouseWorldPositionOnPlane(50)) + " Mouse position: " + MousePosition.GetMouseWorldPositionOnPlane(50));
        DebugDrawQuadrant(MousePosition.GetMouseWorldPositionOnPlane(50));
        Debug.Log(GetEntityCountInHashMap(quadrantMultiHashMap,GetPositionHashMapKey(MousePosition.GetMouseWorldPositionOnPlane(50))));
        
        quadrantMultiHashMap.Dispose();
    }
}
