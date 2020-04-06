using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

public class SuggestFollowWayPointsSystem : JobComponentSystem {
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else
   
    //[BurstCompile]
    private struct SuggestFollowWayPointsJob : IJob {
        public EntityCommandBuffer entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        public NativeArray<Entity> suggesterArray; // Entity array of entities with SuggestWayPointsAction & Translation
        public NativeArray<SuggestFollowWayPointsAction> suggestActionArray; // Array of all of the SuggestFollowWayPointsAction components;
        public NativeArray<Translation> translationArray; // Array of all of the translations of the suggestors
        //public BufferFromEntity<SuggestNearbyCrowd> nearbyCrowdArrays;

        
        [ReadOnly] public NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap; // uses information from the quadrant hash map to find nearby crowd agents
        public BufferFromEntity<Action> actionBufferArray; // an array of all action buffers
        public BufferFromEntity<WayPoint> wayPointBufferArray; // an array of all waypoint buffers
        public float time;
        public void Execute(){
            for(int i = 0; i < suggesterArray.Length; i++){ // for all entities that have the suggestion action
                if(time - suggestActionArray[i].lastSuggestionTime > suggestActionArray[i].frequency){ // if it has been longer than the frequency since the last suggestion
                    Debug.Log(suggesterArray[i] + " suggested Action " + suggestActionArray[i].id + " at " + time);
                    //Suggest

                    //Find all nearby crowd agents
                    Entity policeAgent = suggesterArray[i]; // get the police agent reference
                    //DynamicBuffer<SuggestNearbyCrowd> nearCrowdList = nearbyCrowdArrays[policeAgent]; // get the nearby crowd buffer of the police agent in question 
                    //nearCrowdList.Clear(); // clear the list of nearby crowd agents
                    
                    int hashMapKey = QuadrantSystem.GetPositionHashMapKey(translationArray[i].Value); // Calculate the hash key of the police agent in question

                    SuggestInQuadrant(hashMapKey, i); // Search the quadrant that the police agent is in
                    SuggestInQuadrant(hashMapKey + 1,i); // search the quadrant to the right
                    SuggestInQuadrant(hashMapKey - 1,i); // search the quadrant to the left
                    SuggestInQuadrant(hashMapKey + QuadrantSystem.quadrantYMultiplier,i); // quadrant above
                    SuggestInQuadrant(hashMapKey - QuadrantSystem.quadrantYMultiplier,i); // quadrant below
                    SuggestInQuadrant(hashMapKey + 1 + QuadrantSystem.quadrantYMultiplier,i); // up right
                    SuggestInQuadrant(hashMapKey - 1 + QuadrantSystem.quadrantYMultiplier,i); // up left
                    SuggestInQuadrant(hashMapKey + 1 - QuadrantSystem.quadrantYMultiplier,i); // down right
                    SuggestInQuadrant(hashMapKey -1 - QuadrantSystem.quadrantYMultiplier,i); // down left
                    
                    entityCommandBuffer.SetComponent<SuggestFollowWayPointsAction>(suggesterArray[i],new SuggestFollowWayPointsAction{
                        id = suggestActionArray[i].id,
                        frequency = suggestActionArray[i].frequency, 
                        lastSuggestionTime = time,
                        radius = suggestActionArray[i].radius,
                        waypointHolder = suggestActionArray[i].waypointHolder
                    }); // update when the last suggestion was made
                }
            }
            
        }
        // Suggest to nearby crowd agents
        private void SuggestInQuadrant(int hashMapKey, int policeIndex){
            float3 policePosition = translationArray[policeIndex].Value; // get the police agent's position
            float suggestionRadius = suggestActionArray[policeIndex].radius; // get the suggestion radius
            // Get the data from the quadrant that the seeker belongs to
            QuadrantData quadData;
            NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
            if(quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadData, out nativeMultiHashMapIterator)){ // try to get the first element in the hashmap
                do{ //if there is at least one thing in the quadrant, try getting more
                    if(QuadrantEntity.TypeEnum.Crowd == quadData.quadrantEntity.typeEnum){ // make sure the other entity is a crowd agent
                        float dist = math.distance(policePosition, quadData.position);
                        if(dist < suggestionRadius  && dist > 0.01f)  // if the crowd agent is within the suggestion radius
                        {
                            Suggest(policeIndex, quadData.entity);
                        }
                    }
                } while(quadrantMultiHashMap.TryGetNextValue(out quadData, ref nativeMultiHashMapIterator));
            }
        }
        private void Suggest(int policeIndex, Entity crowdAgent){
            int followActionID = suggestActionArray[policeIndex].id;
            DynamicBuffer<Action> actionBuffer = actionBufferArray[crowdAgent]; // the array of actions for the crowd agent
            int actionPriority = CalculateActionPriority(crowdAgent);

            //Search through the actions array and see if the follow waypoints action is already in there / where a new follow waypoints action should go
            bool foundOldActionIndex = false;
            //bool foundNewActionIndex = false;
            int oldActionIndex = 0; // the position of the Follow waypoints action already in the action 
            //int newActionIndex = 0; // where a new follow waypoints action should be added
            int index = 0; //current position in the action list
            while(index < actionBuffer.Length  // while you are still in the array of actions
                && !foundOldActionIndex){ // while you have not found  the old action index
                if(actionBuffer[index].id == followActionID){ // if the action ids match, then the follow waypoints action was found
                    foundOldActionIndex = true;
                    oldActionIndex = index;
                }
                index++;
            }

            //if the action was already found in the action list
            if(foundOldActionIndex){
                //Do something to the old action
            }
            else{ // if the action was not in the action list, a new action must be added
                Entity PoliceWPHolder = suggestActionArray[policeIndex].waypointHolder;
                DynamicBuffer<WayPoint> policeWP = wayPointBufferArray[PoliceWPHolder];
                Entity holder = entityCommandBuffer.CreateEntity();
                //entityCommandBuffer.SetName(holder, "Follow WayPoint Action Data Holder");

                entityCommandBuffer.AddComponent<FollowWayPointsStorage>(holder, new FollowWayPointsStorage { // add the followwaypointsstorage component to the holder
                    id =  suggestActionArray[policeIndex].id,
                    curPointNum = 0
                }); // store the data

                
                DynamicBuffer<WayPoint> wp = entityCommandBuffer.AddBuffer<WayPoint>(holder); // create the list of waypoints on the holder
                foreach(WayPoint wayPoint in policeWP){ // fill the list of waypoints on the holder
                    wp.Add(new WayPoint{
                        value = wayPoint.value
                    });
                }
                
                entityCommandBuffer.AddComponent<AddFollowWayPointsAction>(crowdAgent, new AddFollowWayPointsAction{ //add the addfollowwaypointsaction component to the crowd agent
                    id =  suggestActionArray[policeIndex].id,
                    priority = actionPriority,
                    timeCreated = time,
                    dataHolder = holder
                }); 
            }
        }

        //Calculates the priority of the follow waypoints action
        private int CalculateActionPriority(Entity crowdAgent){
            return 1;
        }
    }

    protected override void OnCreate() {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps){
        //Find all entities that have the SuggestFollowWayPointsAction and translation components
        EntityQuery query = GetEntityQuery(typeof(SuggestFollowWayPointsAction), typeof(Translation), typeof(SuggestNearbyCrowd));

        NativeArray<Entity> entityArray = query.ToEntityArray(Allocator.TempJob); // Get the arrays corresponding to the entities queried
        NativeArray<SuggestFollowWayPointsAction> suggestionArray = query.ToComponentDataArray<SuggestFollowWayPointsAction>(Allocator.TempJob);
        NativeArray<Translation> transArray = query.ToComponentDataArray<Translation>(Allocator.TempJob);



        SuggestFollowWayPointsJob suggestJob = new SuggestFollowWayPointsJob{ // creates the change action job
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer(),
            suggesterArray = entityArray,
            suggestActionArray = suggestionArray,
            translationArray = transArray,
            //nearbyCrowdArrays = GetBufferFromEntity<SuggestNearbyCrowd>(),
            quadrantMultiHashMap = QuadrantSystem.quadrantMultiHashMap,
            actionBufferArray = GetBufferFromEntity<Action>(),
            wayPointBufferArray = GetBufferFromEntity<WayPoint>(),
            time = Time.time
        };
        JobHandle jobHandle = suggestJob.Schedule(inputDeps);
        jobHandle.Complete();

        commandBufferSystem.AddJobHandleForProducer(jobHandle); // tell the system to execute the command buffer after the job has been completed

        entityArray.Dispose(); // Dispose of the arrays
        suggestionArray.Dispose();
        transArray.Dispose();

        return jobHandle;
    }
}

