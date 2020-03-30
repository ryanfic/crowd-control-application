using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

public struct TQuadData{
    public Entity entity;
    //public float3 position;
    //public QuadrantEntity quadrantEntity;
}
public class MockSuggestionSystem : JobComponentSystem {
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else
    private NativeMultiHashMap<int, TQuadData> quadrantMultiHashMap;
    private NativeHashMap<int,TQuadData> addHashMap;
   
    private struct SuggestFollowWayPointsJob : IJob {
        public EntityCommandBuffer entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        [ReadOnly] public NativeMultiHashMap<int, TQuadData> qMultiHashMap; // uses information from the quadrant hash map to find nearby crowd agents
        public NativeHashMap<int, TQuadData> writeMap;
        public NativeArray<Entity> suggesters;
        public float time;
        public void Execute(){

            // Get the data from the quadrant that the seeker belongs to
            TQuadData quadData;
            NativeMultiHashMapIterator<int> iterator;
            foreach(Entity ent in suggesters){
                if(qMultiHashMap.TryGetFirstValue(1, out quadData, out iterator)){ // try to get the first element in the hashmap
                    do{ //if there is at least one thing in the quadrant, try getting more
                        writeMap.TryAdd(2,new TQuadData{entity = quadData.entity});
                    } while(qMultiHashMap.TryGetNextValue(out quadData, ref iterator));
                }
            }


            
            //if(time - suggestion.lastSuggestionTime > suggestion.frequency){ // if it has been longer than the frequency since the last suggestion
                //writeMap.TryAdd(entity,new TQuadData{ entity = entity});
                //Suggest
                //Find all nearby crowd agents
                //For each agent that is nearby:
                    // get a random float (between 0 & 100), if it is bigger than (100 - Opinion), the agent is affected
                    //If affected, check if action (by id) is in queue or is being added
                        //if not in queue/ being added, add the follow waypoints action to the agent's actions queue
                        //if in queue/being added, increase the priority of the action

            //}
        }
    }
    protected override void OnCreate() {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        quadrantMultiHashMap = new NativeMultiHashMap<int, TQuadData>(1,Allocator.Persistent); // Instantiate the hashmap
        Entity ent = EntityManager.CreateEntity();//Create an entity for the multihash
        EntityManager.AddBuffer<WayPoint>(ent);
        EntityManager.SetName(ent, "Bob");
        quadrantMultiHashMap.Add(1,new TQuadData{entity = ent}); //add TQuadData to hashmap
        addHashMap = new NativeHashMap<int, TQuadData>(1, Allocator.Persistent);
        base.OnCreate();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps){
        addHashMap.Clear();

        EntityQuery query = GetEntityQuery(typeof(MockSuggestionTag));

        NativeArray<Entity> sug = query.ToEntityArray(Allocator.TempJob);

        SuggestFollowWayPointsJob suggestJob = new SuggestFollowWayPointsJob{ // creates the change action job
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer(),
            qMultiHashMap = quadrantMultiHashMap,
            writeMap = addHashMap,
            suggesters = sug,
            time = Time.time
        };
        JobHandle jobHandle = suggestJob.Schedule(inputDeps);
        jobHandle.Complete();

        
        commandBufferSystem.AddJobHandleForProducer(jobHandle); // tell the system to execute the command buffer after the job has been completed


        TQuadData quadData;
        
        if(addHashMap.TryGetValue(2, out quadData)){ // try to get the first element in the hashmap
            
                Debug.Log("Entity: " + quadData.entity);

        }

        sug.Dispose();
       
        return jobHandle;
    }

}
