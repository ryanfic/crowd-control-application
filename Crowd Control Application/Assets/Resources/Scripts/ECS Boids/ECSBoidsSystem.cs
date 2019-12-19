using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
//using ECSBoids;
public struct CrowdData{
    public Entity entity;
    public Translation translation;
    public FlockBehaviour flockBehaviour;
}
public class ECSBoidsSystem : JobComponentSystem
{

    //[BurstCompile]
    private struct FlockBehaviourJob : IJobForEachWithEntity<Translation,FlockBehaviour> {
        [ReadOnly] public NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap; // uses information from the quadrant hash map to find nearby crowd agents

        //public NativeArray<NativeList<float3>> nearbyCrowdArray; //TRY MAKING IT A MUTLTIHASHMAP
        public NativeArray<CrowdData> crowdArray;
        //[DeallocateOnJobCompletion] public NativeList<float3> nearbyCrowdPosList; // in order to add the nearby crowd agents, need a list of nearby crowd agents

        //[DeallocateOnJobCompletion] public NativeArray<FlockBehaviour> flockBehavArray;
        // Jobs need an Execute function
        //ReadOnly on the Translation because the translation is not being altered
        public void Execute(Entity entity, int index, ref Translation trans, ref FlockBehaviour flockBehaviour){
            float3 agentPosition = trans.Value; // the position of the seeker

            NativeList<float3> nearCrowdPosList = new NativeList<float3>();
            //Entity closestTargetEntity = Entity.Null; //Since entity is a struct, it cannot simply be null, it must be Entity.Null
            //float3 closestTargetPosition = float3.zero;
            //float closestTargetDistance = float.MaxValue;
            
            int hashMapKey = QuadrantSystem.GetPositionHashMapKey(trans.Value); // Calculate the hash key of the seeker in question

            FindCrowdAgents(hashMapKey, agentPosition, ref nearCrowdPosList, ref flockBehaviour.CohesionRadius); // Seach the quadrant that the seeker is in
            FindCrowdAgents(hashMapKey + 1,agentPosition, ref nearCrowdPosList, ref flockBehaviour.CohesionRadius); // search the quadrant to the right
            FindCrowdAgents(hashMapKey - 1,agentPosition, ref nearCrowdPosList, ref flockBehaviour.CohesionRadius); // search the quadrant to the left
            FindCrowdAgents(hashMapKey + QuadrantSystem.quadrantYMultiplier,agentPosition, ref nearCrowdPosList, ref flockBehaviour.CohesionRadius); // quadrant above
            FindCrowdAgents(hashMapKey - QuadrantSystem.quadrantYMultiplier,agentPosition, ref nearCrowdPosList, ref flockBehaviour.CohesionRadius); // quadrant below
            FindCrowdAgents(hashMapKey + 1 + QuadrantSystem.quadrantYMultiplier,agentPosition, ref nearCrowdPosList, ref flockBehaviour.CohesionRadius); // up right
            FindCrowdAgents(hashMapKey - 1 + QuadrantSystem.quadrantYMultiplier,agentPosition, ref nearCrowdPosList, ref flockBehaviour.CohesionRadius); // up left
            FindCrowdAgents(hashMapKey + 1 - QuadrantSystem.quadrantYMultiplier,agentPosition, ref nearCrowdPosList, ref flockBehaviour.CohesionRadius); // down right
            FindCrowdAgents(hashMapKey -1 - QuadrantSystem.quadrantYMultiplier,agentPosition, ref nearCrowdPosList, ref flockBehaviour.CohesionRadius); // down left

            // once all nearby crowd agents found, add them to the array of nearby crowd agent lists
            //nearbyCrowdArray[index] = nearCrowdPosList;
            crowdArray[index] = new CrowdData{
                entity = entity,
                translation = trans,
                flockBehaviour = flockBehaviour
                };

            //ApplyFlockingBehaviour(ref trans, nearCrowdPosList, flockBehaviour);
        }

        private void FindCrowdAgents(int hashMapKey, float3 agentPosition, ref NativeList<float3> nearbyCrowdPosList, ref float cohesionRadius){
            // Get the data from the quadrant that the seeker belongs to
            QuadrantData quadData;
            NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
            if(quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadData, out nativeMultiHashMapIterator)){ // try to get the first element in the hashmap
                do{ //if there is at least one thing in the quadrant, try getting more
                    if(QuadrantEntity.TypeEnum.Crowd == quadData.quadrantEntity.typeEnum){ // make sure the other entity is a crowd agent
                        if(math.distancesq(agentPosition, quadData.position) < cohesionRadius)
                        {
                            //this target is within cohesion radius
                            nearbyCrowdPosList.Add(quadData.position);
                        }
                    }
                } while(quadrantMultiHashMap.TryGetNextValue(out quadData, ref nativeMultiHashMapIterator));
            }
        }

        private void ApplyFlockingBehaviour(ref Translation agentTranslation, NativeList<float3> nearbyCrowdPosList, FlockBehaviour flockBehaviour){
            // Calculate Avoidance first
            float3 avoidance = CalculateAvoidance(agentTranslation.Value,nearbyCrowdPosList, flockBehaviour);
            float3 cohesion;
        }

        private float3 CalculateAvoidance(float3 agentPosition, NativeList<float3> context, FlockBehaviour flockBehaviour)
        {
            //if no neighbours, return no adjustment
            if(context.Length == 0)
            {
                return float3.zero;
            }
            //add all points together and average
            float3 avoidanceMove = float3.zero;
            int nAvoid = 0; //number of neighbours to avoid
            /*foreach (float3 item in context)
            {
                //if the item is within the avoidance radius
                if(math.distancesq(item,agentPosition) < flockBehaviour.AlignmentRadius)
                {
                    avoidanceMove += (agentPosition- item);//add the vector from item's position to the agent into the average
                                                                                //this makes it relative to the agent as well
                    nAvoid++; //increment the number of neighbours to avoid
                }
                
            }
            if(nAvoid > 0)
            {
                avoidanceMove /= nAvoid;
            }*/
            return avoidanceMove;
        }
    }
    
    private struct ApplyFlockingBehaviourJob : IJobParallelFor{
        //[DeallocateOnJobCompletion] public NativeArray<NativeList<float3>> nearbyCrowdArray;
        [DeallocateOnJobCompletion] public NativeArray<CrowdData> crowdArray;

        public void Execute(int index){
            //float3 avoidance = CalculateAvoidance(crowdArray[index].translation.Value,nearbyCrowdArray[index], crowdArray[index].flockBehaviour);
        }
        private float3 CalculateAvoidance(float3 agentPosition, NativeList<float3> context, FlockBehaviour flockBehaviour)
        {
            //if no neighbours, return no adjustment
            if(context.Length == 0)
            {
                return float3.zero;
            }
            //add all points together and average
            float3 avoidanceMove = float3.zero;
            int nAvoid = 0; //number of neighbours to avoid
            foreach (float3 item in context)
            {
                //if the item is within the avoidance radius
                if(math.distancesq(item,agentPosition) < flockBehaviour.AlignmentRadius)
                {
                    avoidanceMove += (agentPosition- item);//add the vector from item's position to the agent into the average
                                                                                //this makes it relative to the agent as well
                    nAvoid++; //increment the number of neighbours to avoid
                }
                
            }
            if(nAvoid > 0)
            {
                avoidanceMove /= nAvoid;
            }
            return avoidanceMove;
        }

    }
    
    protected override JobHandle OnUpdate(JobHandle inputDeps){
        EntityQuery crowdQuery = GetEntityQuery(typeof(Crowd)); // Need the number of crowd agents to make nearby crowd array
        int queryLength = crowdQuery.CalculateLength();
        //NativeArray<NativeList<float3>> nearbyCrowdArray = new NativeArray<NativeList<float3>>(queryLength, Allocator.TempJob);
        NativeArray<CrowdData> crowdArray = new NativeArray<CrowdData>(queryLength, Allocator.TempJob);
        FlockBehaviourJob flockBehaviourJob = new FlockBehaviourJob{ // creates the "find nearby crowd" job
            quadrantMultiHashMap = QuadrantSystem.quadrantMultiHashMap,
            //nearbyCrowdArray = nearbyCrowdArray,
            crowdArray = crowdArray
        };
        JobHandle jobHandle = flockBehaviourJob.Schedule(this, inputDeps);

        
        ApplyFlockingBehaviourJob applyFlockingBehaviourJob = new ApplyFlockingBehaviourJob{
            //nearbyCrowdArray = nearbyCrowdArray,
            crowdArray = crowdArray
        };
        
        jobHandle = applyFlockingBehaviourJob.Schedule(queryLength, 100, jobHandle);

        return jobHandle;
    }
}
