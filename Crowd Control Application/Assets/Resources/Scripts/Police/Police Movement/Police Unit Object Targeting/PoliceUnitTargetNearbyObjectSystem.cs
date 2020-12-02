using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;


public class PoliceUnitTargetNearbyObjectSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    protected override void OnCreate(){
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }

    [BurstCompile]
    private struct FindNearestIntersectionJob : IJobForEachWithEntity<Translation,PoliceUnitComponent,PoliceUnitTargetNearestIntersection> {
        [ReadOnly] public NativeMultiHashMap<int, StationaryQuadrantData> stationaryQuadrantMultiHashMap; // uses information from the stationary quadrant hash map to find nearby crowd agents
        public EntityCommandBuffer.Concurrent commandBuffer;

        //ReadOnly on certain components as they are not being altered
        public void Execute(Entity entity, int index, [ReadOnly] ref Translation trans, [ReadOnly] ref PoliceUnitComponent policeUnitComponent, [ReadOnly] ref PoliceUnitTargetNearestIntersection targetIntersection){
            float3 agentPosition = trans.Value; // the position of the seeker

            float nearestDst = float.PositiveInfinity; // set the closest distance to the largest possible number
            float3 nearestPos = new float3(0,0,0); // a holder for now
            
            int hashMapKey = StationaryQuadrantSystem.GetPositionHashMapKey(trans.Value); // Calculate the hash key of the seeker in question

            FindNearestIntersection(hashMapKey, agentPosition, ref nearestPos, ref nearestDst); // Seach the quadrant that the seeker is in
            FindNearestIntersection(hashMapKey + 1,agentPosition, ref nearestPos, ref nearestDst); // search the quadrant to the right
            FindNearestIntersection(hashMapKey - 1,agentPosition, ref nearestPos, ref nearestDst); // search the quadrant to the left
            FindNearestIntersection(hashMapKey + StationaryQuadrantSystem.quadrantYMultiplier,agentPosition, ref nearestPos, ref nearestDst); // quadrant above
            FindNearestIntersection(hashMapKey - StationaryQuadrantSystem.quadrantYMultiplier,agentPosition, ref nearestPos, ref nearestDst); // quadrant below
            FindNearestIntersection(hashMapKey + 1 + StationaryQuadrantSystem.quadrantYMultiplier,agentPosition, ref nearestPos, ref nearestDst); // up right
            FindNearestIntersection(hashMapKey - 1 + StationaryQuadrantSystem.quadrantYMultiplier,agentPosition, ref nearestPos, ref nearestDst); // up left
            FindNearestIntersection(hashMapKey + 1 - StationaryQuadrantSystem.quadrantYMultiplier,agentPosition, ref nearestPos, ref nearestDst); // down right
            FindNearestIntersection(hashMapKey -1 - StationaryQuadrantSystem.quadrantYMultiplier,agentPosition, ref nearestPos, ref nearestDst); // down left

            if(!float.IsInfinity(nearestDst)){//if the distance to nearest intersection is not still infinity
                commandBuffer.AddComponent<PoliceUnitMovementDestination>(index,entity,new PoliceUnitMovementDestination{
                    Value = nearestPos
                }); //Add location target to entity so it starts moving towards the intersection
            }            
            
            commandBuffer.RemoveComponent<PoliceUnitTargetNearestIntersection>(index,entity); //Remove the "find nearest intersection" component
        }



        // Find nearest intersection
        private void FindNearestIntersection(int hashMapKey, float3 agentPosition, ref float3 nearestPos, ref float nearestDst){
            // Get the data from the quadrant that the seeker belongs to
            StationaryQuadrantData quadData;
            NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
            if(stationaryQuadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadData, out nativeMultiHashMapIterator)){ // try to get the first element in the hashmap
                do{ //if there is at least one thing in the quadrant, try getting more
                    if(StationaryQuadrantEntity.TypeEnum.Intersection == quadData.quadrantEntity.typeEnum){ // make sure the other entity is an intersection
                        float dist = math.distance(agentPosition, quadData.position); // get the distance
                        if(dist < nearestDst  && dist > 0.01f)  // check if it's the closest, if so, update info
                        {
                            nearestDst = dist;
                            nearestPos = quadData.position;
                        }
                    }
                } while(stationaryQuadrantMultiHashMap.TryGetNextValue(out quadData, ref nativeMultiHashMapIterator));
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps){
        EntityCommandBuffer.Concurrent commandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent(); // create a command buffer

        FindNearestIntersectionJob nearestIntersectionJob = new FindNearestIntersectionJob{ 
            stationaryQuadrantMultiHashMap = StationaryQuadrantSystem.quadrantMultiHashMap,
            commandBuffer = commandBuffer
        };
        JobHandle jobHandle = nearestIntersectionJob.Schedule(this, inputDeps); //schedule the job

        commandBufferSystem.AddJobHandleForProducer(jobHandle); // make sure the components get added/removed for the job


        return jobHandle;
    }
}

