using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

public class SuggestFollowWayPointsSystem : JobComponentSystem {
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else
   
    private struct SuggestFollowWayPointsJob : IJobForEachWithEntity_ECC<Translation,SuggestFollowWayPointsAction> {
        public EntityCommandBuffer.Concurrent entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        [ReadOnly] public NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap; // uses information from the quadrant hash map to find nearby crowd agents
        public float time;
        public void Execute(Entity entity, int index, ref Translation trans, ref SuggestFollowWayPointsAction suggestion){
            if(time - suggestion.lastSuggestionTime > suggestion.frequency){ // if it has been longer than the frequency since the last suggestion
                //Suggest
                //Find all nearby crowd agents
                //For each agent that is nearby:
                    // get a random float (between 0 & 100), if it is bigger than (100 - Opinion), the agent is affected
                    //If affected, check if action (by id) is in queue or is being added
                        //if not in queue/ being added, add the follow waypoints action to the agent's actions queue
                        //if in queue/being added, increase the priority of the action

            }
        }
    }

    protected override void OnCreate() {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps){

        SuggestFollowWayPointsJob suggestJob = new SuggestFollowWayPointsJob{ // creates the change action job
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent(),
            quadrantMultiHashMap = QuadrantSystem.quadrantMultiHashMap,
            time = Time.time
        };
        JobHandle jobHandle = suggestJob.Schedule(this, inputDeps);

        commandBufferSystem.AddJobHandleForProducer(jobHandle); // tell the system to execute the command buffer after the job has been completed

        return jobHandle;
    }
}

