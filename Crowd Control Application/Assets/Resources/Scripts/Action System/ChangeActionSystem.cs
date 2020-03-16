using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

public class ChangeActionSystem : JobComponentSystem {
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    private struct ChangeActionJob : IJobForEachWithEntity_EBC<Action,ChangeAction> {
        public EntityCommandBuffer.Concurrent entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        public void Execute(Entity entity, int index, DynamicBuffer<Action> actions, ref ChangeAction change){
            switch (change.fromType){ // remove the component based on what we are changing from
                case ActionType.Follow_WayPoints:
                    changeFromFollowWayPoints(entity, index, actions, ref change, ref entityCommandBuffer);
                    break;
                case ActionType.Go_Home:
                    // Do something based on going home
                    break;
            }
            if(actions.Length > 0){ //if there are actions, add another action
                Entity holder = actions[0].dataHolder;
                switch (actions[0].type){ // add a component based on what action has the highest priority
                    case ActionType.Follow_WayPoints: // if the action to add is a Follow waypoints action
                        entityCommandBuffer.AddComponent<FetchWayPoints>(index, entity, new FetchWayPoints{ // tell the system to fetch the waypoints for the action
                            id = actions[0].id,
                            dataHolder = holder
                        });
                        break;
                    case ActionType.Go_Home:
                        // Do something based on going home
                        break;
                }
                

            }
            entityCommandBuffer.RemoveComponent<ChangeAction>(index,entity); // remove this component to show that there is no need to change the action anymore
        }
        private void changeFromFollowWayPoints(Entity entity, int index, DynamicBuffer<Action> actions, ref ChangeAction change, ref EntityCommandBuffer.Concurrent eCommandBuffer){
            eCommandBuffer.RemoveComponent<FollowWayPointsAction>(index, entity); // remove the follow way points action from the agent
            eCommandBuffer.RemoveComponent<HasReynoldsSeekTargetPos>(index,entity); // make it so the agent doesn't move
            DynamicBuffer<WayPoint> wp = eCommandBuffer.SetBuffer<WayPoint>(index, entity);
            wp.Clear();
        }
    }


    protected override void OnCreate() {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps){

        ChangeActionJob changeJob = new ChangeActionJob{ // creates the change action job
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent()
        };
        JobHandle jobHandle = changeJob.Schedule(this, inputDeps);

        commandBufferSystem.AddJobHandleForProducer(jobHandle); // tell the system to execute the command buffer after the job has been completed

        return jobHandle;
    }
}
