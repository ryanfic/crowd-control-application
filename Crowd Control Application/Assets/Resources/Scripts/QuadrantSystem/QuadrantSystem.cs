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

/*public struct QuadrantEntity : IComponentData{ // Quadrant System only works with entities with this component
    //empty component works fine, but we can add info too
    public TypeEnum typeEnum;

    public enum TypeEnum{
        Seeker,
        Target,
        Crowd
    }
}*/

public struct QuadrantData{
    public Entity entity;
    public float3 position;
    public QuadrantEntity quadrantEntity;
}


public class QuadrantSystem : ComponentSystem
{   
    //NativeMultiHashMap is for storing the quadrants
    //quadrants need multiple things (values)
    //keys are ints, and it holds Entity s
    public static NativeMultiHashMap <int, QuadrantData> quadrantMultiHashMap;

    private const int quadrantDimMax = 1000; // The maximum amount of quadrants you would expect
                                            // used in the hashMap
    public const int quadrantZMultiplier = quadrantDimMax*quadrantDimMax;
    public const int quadrantYMultiplier = quadrantDimMax;
    private const int quadrantCellSize = 5;
    //given a position, calculate the hashmap key
    public static int GetPositionHashMapKey(float3 position){
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
    private static int GetEntityCountInHashMap(NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap, int hashMapKey){
        QuadrantData quadrantData; //out for the function below
        NativeMultiHashMapIterator<int> nativeMultiHashMapIterator; //another out for the function below
        int count = 0;
        //try to get to get the first value for the given key
        if(quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadrantData, out nativeMultiHashMapIterator)){
            //if there is a value, try to get more (must go through once since there was at least one entity)
            do{
                count++; //there was an entity, let's count it!
            } while(quadrantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
        }
        return count;
    }

    [BurstCompile]
    private struct SetQuadrantDataHashMapJob : IJobForEachWithEntity<Translation, QuadrantEntity>{
        public NativeMultiHashMap<int, QuadrantData>.ParallelWriter quadrantMultiHashMap; //job is about putting things into the hashmap, so we need
                                                                    //a reference to the hashmap in question
        public void Execute(Entity entity, int index, ref Translation translation, ref QuadrantEntity quadrantEnt){
            int hashMapKey = GetPositionHashMapKey(translation.Value); //get the entity's hashmap key
            quadrantMultiHashMap.Add(hashMapKey, new QuadrantData{
                    entity = entity,
                    position = translation.Value,
                    quadrantEntity = quadrantEnt
                }); //add the entity and its relevant values to the hashmap
        }
    }




    /*
        When the Quadrant System is created
    */
    protected override void OnCreate(){
        quadrantMultiHashMap = new NativeMultiHashMap<int, QuadrantData>(0,Allocator.Persistent); // Instantiate the hashmap
        base.OnCreate();
    }

    protected override void OnDestroy(){
        quadrantMultiHashMap.Dispose(); // Remove the hashmap
        base.OnDestroy();
    }

    protected override void OnUpdate(){
        //calculate the number of entities we have to store (entities with translation component and QuadrantEntity component)
        EntityQuery entityQuery = GetEntityQuery(typeof(Translation), typeof(QuadrantEntity));


        
        //the length is calculated from above
        //NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap = new NativeMultiHashMap<int, QuadrantData>(entityQuery.CalculateLength(),Allocator.TempJob);

        quadrantMultiHashMap.Clear(); // clear the hashmap

        // if the amount of stuff to add to the hashmap is larger than the capacity of the hashmap
        if(entityQuery.CalculateEntityCount() > quadrantMultiHashMap.Capacity){
            quadrantMultiHashMap.Capacity = entityQuery.CalculateEntityCount(); //Increase the hashmap to hold everything
        }

        //using jobs
        //Cycle through all entities and get their positions
        //selects all entities with a translation component and adds them to the hashmap
        SetQuadrantDataHashMapJob setQuadrantDataHashMapJob = new SetQuadrantDataHashMapJob{
            quadrantMultiHashMap = quadrantMultiHashMap.AsParallelWriter(), //ToConcurrent used to allow for concurrent writing
        };
        JobHandle jobHandle = JobForEachExtensions.Schedule(setQuadrantDataHashMapJob, entityQuery);
        jobHandle.Complete();




        //Cycle through all entities and get their positions
        //selects all entities with a translation component
        //without jobs
        /*Entities.ForEach((Entity entity, ref Translation translation) =>{
            int hashMapKey = GetPositionHashMapKey(translation.Value);
            quadrantMultiHashMap.Add(hashMapKey, entity);
        });*/
        
        //Debug.Log(GetPositionHashMapKey(MousePosition.GetMouseWorldPositionOnPlane(50)) + " Mouse position: " + MousePosition.GetMouseWorldPositionOnPlane(50));
        DebugDrawQuadrant(MousePosition.GetMouseWorldPositionOnPlane(50));
        //Debug.Log(GetEntityCountInHashMap(quadrantMultiHashMap,GetPositionHashMapKey(MousePosition.GetMouseWorldPositionOnPlane(50))));
        
        //quadrantMultiHashMap.Dispose();
    }
}
