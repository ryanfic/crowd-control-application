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


public struct StationaryQuadrantData{
    public Entity entity;
    public float3 position;
    public StationaryQuadrantEntity quadrantEntity;
}


public class StationaryQuadrantSystem : SystemBase
{   
    //NativeMultiHashMap is for storing the quadrants
    //quadrants need multiple things (values)
    //keys are ints, and it holds Entity s
    public static NativeMultiHashMap <int, StationaryQuadrantData> quadrantMultiHashMap;

    private const int quadrantDimMax = 1000; // The maximum amount of quadrants you would expect
                                            // used in the hashMap
    public const int quadrantZMultiplier = quadrantDimMax*quadrantDimMax;
    public const int quadrantYMultiplier = quadrantDimMax;
    private const int quadrantCellSize = 10;

    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    private EntityQueryDesc addQueryDesc;

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
    private static int GetEntityCountInHashMap(NativeMultiHashMap<int, StationaryQuadrantData> quadrantMultiHashMap, int hashMapKey){
        StationaryQuadrantData quadrantData; //out for the function below
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

    /*
        Job to add an entity to the stationary quadrant if the entity has the 'add me' tag
    */
    [BurstCompile]
    private struct AddStationaryQuadrantEntityToMapJob : IJobParallelFor {
        public NativeMultiHashMap<int, StationaryQuadrantData>.ParallelWriter quadrantMultiHashMap; //job is about putting things into the hashmap, so we need
                                                                    //a reference to the hashmap in question
        [DeallocateOnJobCompletion] public NativeArray<Entity> entities;
        [DeallocateOnJobCompletion] public NativeArray<Translation> translations; // where the entity is
        [DeallocateOnJobCompletion] public NativeArray<StationaryQuadrantEntity> quadEntTypes; // what type of stationary quadrant entity the entity is

        public EntityCommandBuffer.ParallelWriter entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        public void Execute(int i){
            int hashMapKey = GetPositionHashMapKey(translations[i].Value); //get the entity's hashmap key
            quadrantMultiHashMap.Add(hashMapKey, new StationaryQuadrantData{
                    entity = entities[i],
                    position = translations[i].Value,
                    quadrantEntity = quadEntTypes[i]
                }); //add the entity and its relevant values to the hashmap
            entityCommandBuffer.RemoveComponent<AddStationaryQuadrantEntity>(i,entities[i]); //remove the tag
            Debug.Log("Added!");
        }
    }
    
    /*[BurstCompile]
    private struct AddStationaryQuadrantEntityToMapJob : IJobForEachWithEntity<Translation, StationaryQuadrantEntity, AddStationaryQuadrantEntity>{
        public NativeMultiHashMap<int, StationaryQuadrantData>.ParallelWriter quadrantMultiHashMap; //job is about putting things into the hashmap, so we need
                                                                    //a reference to the hashmap in question
        public EntityCommandBuffer.ParallelWriter entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation, [ReadOnly] ref StationaryQuadrantEntity quadrantEnt, [ReadOnly] ref AddStationaryQuadrantEntity addEntityComponent){
            int hashMapKey = GetPositionHashMapKey(translation.Value); //get the entity's hashmap key
            quadrantMultiHashMap.Add(hashMapKey, new StationaryQuadrantData{
                    entity = entity,
                    position = translation.Value,
                    quadrantEntity = quadrantEnt
                }); //add the entity and its relevant values to the hashmap
            entityCommandBuffer.RemoveComponent<AddStationaryQuadrantEntity>(index,entity); //remove the tag
            Debug.Log("Added!");
        }
    }*/

    /*
        Job to remove an entity to the stationary quadrant if the entity has the 'remove me' tag
    */
    /*private struct RemoveStationaryQuadrantEntityFromMapJob : IJobForEachWithEntity<Translation, StationaryQuadrantEntity, RemoveStationaryQuadrantEntity>{
        public NativeMultiHashMap<int, StationaryQuadrantData> quadrantMultiHashMap; //job is about putting things into the hashmap, so we need
                                                                    //a reference to the hashmap in question

        public EntityCommandBuffer entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        public void Execute(Entity entity, int index, ref Translation translation, ref StationaryQuadrantEntity quadrantEnt, [ReadOnly] ref RemoveStationaryQuadrantEntity removeEntityComponent){
            StationaryQuadrantData quadrantData; //Where the data is stored for the job
            NativeMultiHashMapIterator<int> nativeMultiHashMapIterator; //another out for the function below
            int hashMapKey = GetPositionHashMapKey(translation.Value); //get the entity's hashmap key
            bool found = false; // if the entity to be removed has been found
            
            if(quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadrantData, out nativeMultiHashMapIterator)){
                //if there is a value, try to get more (must go through once since there was at least one entity)
                do{
                    if(quadrantData.entity == entity){//check if it's the right entity, if not, iterate
                        found = true;
                    }
                } while(!found && quadrantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator)); //specifically check if it was found first so we don't iterate when it's found
            }
            
            if(found){
                quadrantMultiHashMap.Remove(nativeMultiHashMapIterator); //remove the entity if it was found
                entityCommandBuffer.RemoveComponent<RemoveStationaryQuadrantEntity>(entity); //remove the tag
            }
            //Possibly do something if it was not found
        }        
    }*/


    /*
        When the Stationary Quadrant System is created
    */
    protected override void OnCreate(){
        quadrantMultiHashMap = new NativeMultiHashMap<int, StationaryQuadrantData>(0,Allocator.Persistent); // Instantiate the hashmap
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>(); // get the command buffer system
        addQueryDesc = new EntityQueryDesc{
            All = new ComponentType[]{
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<StationaryQuadrantEntity>(),
                ComponentType.ReadOnly<AddStationaryQuadrantEntity>()
            }
        }; // define what we are looking for in the add job

        base.OnCreate();
    }

    protected override void OnDestroy(){
        quadrantMultiHashMap.Dispose(); // Remove the hashmap
        base.OnDestroy();
    }

    protected override void OnUpdate(){
        //calculate the number of entities we have to store (entities with translation component and QuadrantEntity component)
        EntityQuery entityQuery = GetEntityQuery(typeof(Translation), typeof(StationaryQuadrantEntity));
        /*EntityQuery addEntityQuery = GetEntityQuery(typeof(Translation), typeof(StationaryQuadrantEntity), typeof(AddStationaryQuadrantEntity));
        EntityQuery removeEntityQuery = GetEntityQuery(typeof(Translation), typeof(StationaryQuadrantEntity), typeof(RemoveStationaryQuadrantEntity));*/
    
        
        EntityQuery addEntityQuery = GetEntityQuery(addQueryDesc); // do the query
        NativeArray<Entity> addEntityArray = addEntityQuery.ToEntityArray(Allocator.TempJob);// create the entity array
        NativeArray<Translation> addTransArray = addEntityQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        NativeArray<StationaryQuadrantEntity> addStatQuadEntArray = addEntityQuery.ToComponentDataArray<StationaryQuadrantEntity>(Allocator.TempJob);// create the stationary quadrant entities array


        // if the number of stationary stuff has grown larger than the capacity of the hashmap
        if(entityQuery.CalculateEntityCount() > quadrantMultiHashMap.Capacity){
            quadrantMultiHashMap.Capacity = entityQuery.CalculateEntityCount(); //Increase the hashmap to hold everything
        }

        //selects all entities with a translation component and the 'add to stationary quadrant tag' and adds them to the hashmap
        /*RemoveStationaryQuadrantEntityFromMapJob removeEntityFromMapJob = new RemoveStationaryQuadrantEntityFromMapJob{
            quadrantMultiHashMap = quadrantMultiHashMap,
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer()
        };*/

        //selects all entities with a translation component and the 'add to stationary quadrant tag' and adds them to the hashmap
        AddStationaryQuadrantEntityToMapJob addEntityToMapJob = new AddStationaryQuadrantEntityToMapJob{
            quadrantMultiHashMap = quadrantMultiHashMap.AsParallelWriter(), //Asparallelwriter used to allow for concurrent writing
            entities = addEntityArray,
            translations = addTransArray,
            quadEntTypes = addStatQuadEntArray,
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter()
        };
        /*JobHandle removeJobHandle = JobForEachExtensions.Schedule(removeEntityFromMapJob, removeEntityQuery);
        commandBufferSystem.AddJobHandleForProducer(removeJobHandle); // tell the system to execute the command buffer after the job has been completed*/

        JobHandle addJobHandle = IJobParallelForExtensions.Schedule(addEntityToMapJob, addEntityArray.Length, 32, this.Dependency); //JobForEachExtensions.Schedule(addEntityToMapJob, addEntityQuery, this.Dependency/*, removeJobHandle*/);
        
        commandBufferSystem.AddJobHandleForProducer(addJobHandle); // tell the system to execute the command buffer after the job has been completed
        addJobHandle.Complete();
    }
}


/*public class StationaryQuadrantSystem : ComponentSystem
{   
    //NativeMultiHashMap is for storing the quadrants
    //quadrants need multiple things (values)
    //keys are ints, and it holds Entity s
    public static NativeMultiHashMap <int, StationaryQuadrantData> quadrantMultiHashMap;

    private const int quadrantDimMax = 1000; // The maximum amount of quadrants you would expect
                                            // used in the hashMap
    public const int quadrantZMultiplier = quadrantDimMax*quadrantDimMax;
    public const int quadrantYMultiplier = quadrantDimMax;
    private const int quadrantCellSize = 10;

    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

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
/*    private static int GetEntityCountInHashMap(NativeMultiHashMap<int, StationaryQuadrantData> quadrantMultiHashMap, int hashMapKey){
        StationaryQuadrantData quadrantData; //out for the function below
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

    /*
        Job to add an entity to the stationary quadrant if the entity has the 'add me' tag
    */
/*    [BurstCompile]
    private struct AddStationaryQuadrantEntityToMapJob : IJobForEachWithEntity<Translation, StationaryQuadrantEntity, AddStationaryQuadrantEntity>{
        public NativeMultiHashMap<int, StationaryQuadrantData>.ParallelWriter quadrantMultiHashMap; //job is about putting things into the hashmap, so we need
                                                                    //a reference to the hashmap in question
        public EntityCommandBuffer.ParallelWriter entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        public void Execute(Entity entity, int index, ref Translation translation, ref StationaryQuadrantEntity quadrantEnt, [ReadOnly] ref AddStationaryQuadrantEntity addEntityComponent){
            int hashMapKey = GetPositionHashMapKey(translation.Value); //get the entity's hashmap key
            quadrantMultiHashMap.Add(hashMapKey, new StationaryQuadrantData{
                    entity = entity,
                    position = translation.Value,
                    quadrantEntity = quadrantEnt
                }); //add the entity and its relevant values to the hashmap
            entityCommandBuffer.RemoveComponent<AddStationaryQuadrantEntity>(index,entity); //remove the tag
            Debug.Log("Added!");
        }
    }

    /*
        Job to remove an entity to the stationary quadrant if the entity has the 'remove me' tag
    */
    /*private struct RemoveStationaryQuadrantEntityFromMapJob : IJobForEachWithEntity<Translation, StationaryQuadrantEntity, RemoveStationaryQuadrantEntity>{
        public NativeMultiHashMap<int, StationaryQuadrantData> quadrantMultiHashMap; //job is about putting things into the hashmap, so we need
                                                                    //a reference to the hashmap in question

        public EntityCommandBuffer entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        public void Execute(Entity entity, int index, ref Translation translation, ref StationaryQuadrantEntity quadrantEnt, [ReadOnly] ref RemoveStationaryQuadrantEntity removeEntityComponent){
            StationaryQuadrantData quadrantData; //Where the data is stored for the job
            NativeMultiHashMapIterator<int> nativeMultiHashMapIterator; //another out for the function below
            int hashMapKey = GetPositionHashMapKey(translation.Value); //get the entity's hashmap key
            bool found = false; // if the entity to be removed has been found
            
            if(quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadrantData, out nativeMultiHashMapIterator)){
                //if there is a value, try to get more (must go through once since there was at least one entity)
                do{
                    if(quadrantData.entity == entity){//check if it's the right entity, if not, iterate
                        found = true;
                    }
                } while(!found && quadrantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator)); //specifically check if it was found first so we don't iterate when it's found
            }
            
            if(found){
                quadrantMultiHashMap.Remove(nativeMultiHashMapIterator); //remove the entity if it was found
                entityCommandBuffer.RemoveComponent<RemoveStationaryQuadrantEntity>(entity); //remove the tag
            }
            //Possibly do something if it was not found
        }        
    }*/


    /*
        When the Stationary Quadrant System is created
    */
/*    protected override void OnCreate(){
        quadrantMultiHashMap = new NativeMultiHashMap<int, StationaryQuadrantData>(0,Allocator.Persistent); // Instantiate the hashmap
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>(); // get the command buffer system
        base.OnCreate();
    }

    protected override void OnDestroy(){
        quadrantMultiHashMap.Dispose(); // Remove the hashmap
        base.OnDestroy();
    }

    protected override void OnUpdate(){
        //calculate the number of entities we have to store (entities with translation component and QuadrantEntity component)
        EntityQuery entityQuery = GetEntityQuery(typeof(Translation), typeof(StationaryQuadrantEntity));
        EntityQuery addEntityQuery = GetEntityQuery(typeof(Translation), typeof(StationaryQuadrantEntity), typeof(AddStationaryQuadrantEntity));
        EntityQuery removeEntityQuery = GetEntityQuery(typeof(Translation), typeof(StationaryQuadrantEntity), typeof(RemoveStationaryQuadrantEntity));
    

        // if the number of stationary stuff has grown larger than the capacity of the hashmap
        if(entityQuery.CalculateEntityCount() > quadrantMultiHashMap.Capacity){
            quadrantMultiHashMap.Capacity = entityQuery.CalculateEntityCount(); //Increase the hashmap to hold everything
        }

        //selects all entities with a translation component and the 'add to stationary quadrant tag' and adds them to the hashmap
        /*RemoveStationaryQuadrantEntityFromMapJob removeEntityFromMapJob = new RemoveStationaryQuadrantEntityFromMapJob{
            quadrantMultiHashMap = quadrantMultiHashMap,
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer()
        };*/

        //selects all entities with a translation component and the 'add to stationary quadrant tag' and adds them to the hashmap
/*        AddStationaryQuadrantEntityToMapJob addEntityToMapJob = new AddStationaryQuadrantEntityToMapJob{
            quadrantMultiHashMap = quadrantMultiHashMap.AsParallelWriter(), //Asparallelwriter used to allow for concurrent writing
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter()
        };
        /*JobHandle removeJobHandle = JobForEachExtensions.Schedule(removeEntityFromMapJob, removeEntityQuery);
        commandBufferSystem.AddJobHandleForProducer(removeJobHandle); // tell the system to execute the command buffer after the job has been completed*/

/*        JobHandle addJobHandle = JobForEachExtensions.Schedule(addEntityToMapJob, addEntityQuery/*, removeJobHandle*//*);*/
/*        commandBufferSystem.AddJobHandleForProducer(addJobHandle); // tell the system to execute the command buffer after the job has been completed
        addJobHandle.Complete();
    }
}*/