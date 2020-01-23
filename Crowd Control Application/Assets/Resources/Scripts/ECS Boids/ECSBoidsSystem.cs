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
public struct BoidsMovement : IComponentData{
    public float3 movement;
}
public class ECSBoidsSystem : JobComponentSystem
{
    //[BurstCompile]
    private struct FlockBehaviourJob : IJobForEachWithEntity<Translation,FlockBehaviour,BoidsMovement> {
        // Find a list of nearby agents
        // Do the calculation for flocking
        // Change the translation of the agent

        [ReadOnly] public NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap; // uses information from the quadrant hash map to find nearby crowd agents
        // Jobs need an Execute function
        //ReadOnly on the Translation because the translation is not being altered
        public void Execute(Entity entity, int index, ref Translation trans, ref FlockBehaviour flockBehaviour, ref BoidsMovement boidsMovement){
            float3 agentPosition = trans.Value; // the position of the seeker
            List<float3> nearCrowdPosList = new List<float3>();
            float searchRadius = math.max(flockBehaviour.CohesionRadius,flockBehaviour.AvoidanceRadius);
            
            int hashMapKey = QuadrantSystem.GetPositionHashMapKey(trans.Value); // Calculate the hash key of the seeker in question

            // Cohesion Radius is used because it is larger than the separation radius, but if other behaviours need things that are farther from the agent, may need to use other distance
            FindCrowdAgents(hashMapKey, agentPosition, ref nearCrowdPosList, ref searchRadius); // Seach the quadrant that the seeker is in
            FindCrowdAgents(hashMapKey + 1,agentPosition, ref nearCrowdPosList, ref searchRadius); // search the quadrant to the right
            FindCrowdAgents(hashMapKey - 1,agentPosition, ref nearCrowdPosList, ref searchRadius); // search the quadrant to the left
            FindCrowdAgents(hashMapKey + QuadrantSystem.quadrantYMultiplier,agentPosition, ref nearCrowdPosList, ref searchRadius); // quadrant above
            FindCrowdAgents(hashMapKey - QuadrantSystem.quadrantYMultiplier,agentPosition, ref nearCrowdPosList, ref searchRadius); // quadrant below
            FindCrowdAgents(hashMapKey + 1 + QuadrantSystem.quadrantYMultiplier,agentPosition, ref nearCrowdPosList, ref searchRadius); // up right
            FindCrowdAgents(hashMapKey - 1 + QuadrantSystem.quadrantYMultiplier,agentPosition, ref nearCrowdPosList, ref searchRadius); // up left
            FindCrowdAgents(hashMapKey + 1 - QuadrantSystem.quadrantYMultiplier,agentPosition, ref nearCrowdPosList, ref searchRadius); // down right
            FindCrowdAgents(hashMapKey -1 - QuadrantSystem.quadrantYMultiplier,agentPosition, ref nearCrowdPosList, ref searchRadius); // down left

            ApplyFlockingBehaviour(ref trans, ref nearCrowdPosList, ref flockBehaviour, ref boidsMovement);

            /*FindCrowdAgents(hashMapKey, agentPosition, ref nearCrowdPosList, ref count, ref flockBehaviour.CohesionRadius); // Seach the quadrant that the seeker is in
            FindCrowdAgents(hashMapKey + 1,agentPosition, ref nearCrowdPosList, ref count, ref flockBehaviour.CohesionRadius); // search the quadrant to the right
            FindCrowdAgents(hashMapKey - 1,agentPosition, ref nearCrowdPosList, ref count, ref flockBehaviour.CohesionRadius); // search the quadrant to the left
            FindCrowdAgents(hashMapKey + QuadrantSystem.quadrantYMultiplier,agentPosition, ref nearCrowdPosList, ref count, ref flockBehaviour.CohesionRadius); // quadrant above
            FindCrowdAgents(hashMapKey - QuadrantSystem.quadrantYMultiplier,agentPosition, ref nearCrowdPosList, ref count, ref flockBehaviour.CohesionRadius); // quadrant below
            FindCrowdAgents(hashMapKey + 1 + QuadrantSystem.quadrantYMultiplier,agentPosition, ref nearCrowdPosList, ref count, ref flockBehaviour.CohesionRadius); // up right
            FindCrowdAgents(hashMapKey - 1 + QuadrantSystem.quadrantYMultiplier,agentPosition, ref nearCrowdPosList, ref count, ref flockBehaviour.CohesionRadius); // up left
            FindCrowdAgents(hashMapKey + 1 - QuadrantSystem.quadrantYMultiplier,agentPosition, ref nearCrowdPosList, ref count, ref flockBehaviour.CohesionRadius); // down right
            FindCrowdAgents(hashMapKey -1 - QuadrantSystem.quadrantYMultiplier,agentPosition, ref nearCrowdPosList, ref count, ref flockBehaviour.CohesionRadius); // down left

            ApplyFlockingBehaviour(ref trans, ref nearCrowdPosList, ref count, ref flockBehaviour, ref boidsMovement);*/
            // once all nearby crowd agents found, add them to the array of nearby crowd agent lists
            //nearbyCrowdArray[index] = nearCrowdPosList;
            /*crowdArray[index] = new CrowdData{
                entity = entity,
                translation = trans,
                flockBehaviour = flockBehaviour
                };*/
            
            //crowdMovementArray[index] = CalculateFlockingBehaviour(ref trans, ref nearCrowdPosList, ref flockBehaviour);
        }

        private void FindCrowdAgents(int hashMapKey, float3 agentPosition, ref List<float3> nearbyCrowdPosList, ref float nearbyRadius){
            // Get the data from the quadrant that the seeker belongs to
            QuadrantData quadData;
            NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
            if(quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadData, out nativeMultiHashMapIterator)){ // try to get the first element in the hashmap
                do{ //if there is at least one thing in the quadrant, try getting more
                    if(QuadrantEntity.TypeEnum.Crowd == quadData.quadrantEntity.typeEnum){ // make sure the other entity is a crowd agent
                        if(math.distancesq(agentPosition, quadData.position) < nearbyRadius * nearbyRadius)
                        {
                            //this target is within cohesion radius
                            nearbyCrowdPosList.Add(quadData.position);
                        }
                    }
                } while(quadrantMultiHashMap.TryGetNextValue(out quadData, ref nativeMultiHashMapIterator));
            }
        }

        private void FindCrowdAgents(int hashMapKey, float3 agentPosition, ref float3[] nearbyCrowdPosList, ref int count, ref float nearbyRadius){
            //NativeList<float3> nearbyCrowdPosList = new NativeList<float3>(Allocator.TempJob); // create a list to add to
            // Get the data from the quadrant that the seeker belongs to
            QuadrantData quadData;
            NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
            if(quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadData, out nativeMultiHashMapIterator)){ // try to get the first element in the hashmap
                do{ //if there is at least one thing in the quadrant, try getting more
                    if(QuadrantEntity.TypeEnum.Crowd == quadData.quadrantEntity.typeEnum){ // make sure the other entity is a crowd agent
                        if(math.distancesq(agentPosition, quadData.position) < nearbyRadius * nearbyRadius)
                        {
                            //float3 loc = float3.zero;
                            //copyIt(ref loc,quadData.position);
                            //nearbyCrowdPosList.Add(loc);

                            //float3 loc = quadData.position;

                            //this target is within cohesion radius
                            nearbyCrowdPosList[count] = quadData.position;

                            //nearbyCrowdPosList.Add(float3.zero);
                        }
                        else{
                            nearbyCrowdPosList[count] = new float3(-999f,-999f,-999f);
                        }
                        count++;
                    }
                } while(quadrantMultiHashMap.TryGetNextValue(out quadData, ref nativeMultiHashMapIterator));
            }
            //NativeArray<float3> result = nearbyCrowdPosList.ToArray();
            //nearbyCrowdPosList.Dispose();
            //return result;
        }
        /*private void copyIt(ref float3 to, float3 val){
            to = val;
            //return to;
        }*/

        private void ApplyFlockingBehaviour(ref Translation agentTranslation, ref List<float3> nearbyCrowdPosList,  ref FlockBehaviour flockBehaviour, ref BoidsMovement boidsMovement){
            float3 move = float3.zero; // where the agent will move
            // Calculate Avoidance first
            float3 avoidance = CalculateAvoidance(ref agentTranslation.Value, ref nearbyCrowdPosList, ref flockBehaviour);
            //if the avoidance is not zero
            if(!avoidance.Equals(float3.zero))
            {
                //if the avoidance is not  (The square of the length of the position is greater than the square of the weights)
                if(math.distancesq(float3.zero,avoidance) > flockBehaviour.AvoidanceWeight * flockBehaviour.AvoidanceWeight)
                {
                    //normalize the movement vector
                    avoidance = math.normalize(avoidance);
                    avoidance *= flockBehaviour.AvoidanceWeight;
                }

                move += avoidance;
            }

            // Calculate cohesion
            float3 cohesion = CalculateCohesion(ref agentTranslation.Value, ref nearbyCrowdPosList, ref flockBehaviour);
            if(!cohesion.Equals(float3.zero))
            {
                //if the avoidance is not normalized  (The square of the length of the position is greater than the square of the weights)
                if(math.distancesq(float3.zero,cohesion) > flockBehaviour.CohesionWeight * flockBehaviour.CohesionWeight)
                {
                    //normalize the movement vector
                    cohesion = math.normalize(cohesion);
                    cohesion *= flockBehaviour.CohesionWeight;
                }

                move += cohesion;
            }
            // May want to calculate alignment after, but not at time of writing this

            boidsMovement.movement = move;
            //float moveSpeed = 5f; //movement speed
            //agentTranslation.Value += move * moveSpeed * Time.deltaTime; //add movement to the translation
            //return move;
        }

        private void ApplyFlockingBehaviour(ref Translation agentTranslation, ref float3[] nearbyCrowdPosList, ref int count, ref FlockBehaviour flockBehaviour, ref BoidsMovement boidsMovement){
            float3 move = float3.zero; // where the agent will move
            // Calculate Avoidance first
            float3 avoidance = CalculateAvoidance(ref agentTranslation.Value, ref nearbyCrowdPosList, ref count, ref flockBehaviour);
            //if the avoidance is not zero
            if(!avoidance.Equals(float3.zero))
            {
                //if the avoidance is not  (The square of the length of the position is greater than the square of the weights)
                if(math.distancesq(float3.zero,avoidance) > flockBehaviour.AvoidanceWeight * flockBehaviour.AvoidanceWeight)
                {
                    //normalize the movement vector
                    avoidance = math.normalize(avoidance);
                    avoidance *= flockBehaviour.AvoidanceWeight;
                }

                move += avoidance;
            }

            // Calculate cohesion
            float3 cohesion = CalculateCohesion(ref agentTranslation.Value, ref nearbyCrowdPosList, ref count, ref flockBehaviour);
            if(!cohesion.Equals(float3.zero))
            {
                //if the avoidance is not normalized  (The square of the length of the position is greater than the square of the weights)
                if(math.distancesq(float3.zero,cohesion) > flockBehaviour.CohesionWeight * flockBehaviour.CohesionWeight)
                {
                    //normalize the movement vector
                    cohesion = math.normalize(cohesion);
                    cohesion *= flockBehaviour.CohesionWeight;
                }

                move += cohesion;
            }
            // May want to calculate alignment after, but not at time of writing this

            boidsMovement.movement = move;
            //float moveSpeed = 5f; //movement speed
            //agentTranslation.Value += move * moveSpeed * Time.deltaTime; //add movement to the translation
            //return move;
        }

        private float3 CalculateAvoidance(ref float3 agentPosition, ref /*Native*/List<float3> context, /*[ReadOnly]*/ ref FlockBehaviour flockBehaviour)
        {
            //if no neighbours, return no adjustment
            if(context.Count == 0)
            {
                return float3.zero;
            }
            //add all points together and average
            float3 avoidanceMove = float3.zero;
            int nAvoid = 0; //number of neighbours to avoid
            foreach (float3 item in context)
            {
                //if the item is within the avoidance radius
                if(math.distancesq(item,agentPosition) < flockBehaviour.AvoidanceRadius * flockBehaviour.AvoidanceRadius)
                {
                    avoidanceMove += (agentPosition - item);//add the vector from item's position to the agent into the average
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
        private float3 CalculateAvoidance(ref float3 agentPosition, ref float3[] context, ref int count, ref FlockBehaviour flockBehaviour)
        {
            //if no neighbours, return no adjustment
            if(count == 0)
            {
                return float3.zero;
            }
            //add all points together and average
            float3 avoidanceMove = float3.zero;
            int nAvoid = 0; //number of neighbours to avoid
            foreach (float3 item in context)
            {
                //if the item is within the avoidance radius
                if(math.distancesq(item,agentPosition) < flockBehaviour.AvoidanceRadius * flockBehaviour.AvoidanceRadius)
                {
                    avoidanceMove += (agentPosition - item);//add the vector from item's position to the agent into the average
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

        private float3 CalculateCohesion(ref float3 agentPosition, ref List<float3> context,  ref FlockBehaviour flockBehaviour){
            //if no neighbours, return no adjustment
            if(context.Count == 0)
            {
                return float3.zero;
            }
            int count = 0;
            //add all points together and average
            float3 cohesionMove = float3.zero;
            foreach (float3 item in context)
            {
                if(math.distancesq(item,agentPosition) < flockBehaviour.CohesionRadius * flockBehaviour.CohesionRadius)
                {
                    cohesionMove += item;
                    count++;
                }
            }
            cohesionMove /= count;

            //create offset from agent position
            cohesionMove -= agentPosition;
            return cohesionMove;
        }
        private float3 CalculateCohesion(ref float3 agentPosition, ref float3[] context, ref int length, ref FlockBehaviour flockBehaviour){
            //if no neighbours, return no adjustment
            if(length == 0)
            {
                return float3.zero;
            }
            int count = 0;
            //add all points together and average
            float3 cohesionMove = float3.zero;
            foreach (float3 item in context)
            {
                if(item.x==-999f&&item.y==-999f&&item.z==-999f){}
                else if(math.distancesq(item,agentPosition) < flockBehaviour.CohesionRadius * flockBehaviour.CohesionRadius)
                {
                    cohesionMove += item;
                    count++;
                }
            }
            cohesionMove /= count;

            //create offset from agent position
            cohesionMove -= agentPosition;
            return cohesionMove;
        }
    }

    [RequireComponentTag(typeof(Crowd))] // The Job will only run on entities that are seekers
    private struct AddComponentJob : IJobForEachWithEntity<Translation>{
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float3> crowdMovementArray; // Used to assign all closest targets to seekers
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        public void Execute(Entity entity, int index, [ReadOnly] ref Translation trans){
            entityCommandBuffer.AddComponent(index, entity, new BoidsMovement{ movement = crowdMovementArray[index]});
        }
    }
    
    /*private struct ApplyFlockingBehaviourJob : IJobParallelFor{
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

    }*/
    
    protected override JobHandle OnUpdate(JobHandle inputDeps){
        //EntityQuery crowdQuery = GetEntityQuery(typeof(Crowd)); // Need the number of crowd agents to make nearby crowd array
        //int queryLength = crowdQuery.CalculateLength();
        //int[] num = {queryLength};
        //NativeArray<int> numAgents = new NativeArray<int>(num,Allocator.TempJob);
        //NativeArray<float3> crowdMovementArray = new NativeArray<float3>(queryLength, Allocator.TempJob);
        //NativeArray<CrowdData> crowdArray = new NativeArray<CrowdData>(queryLength, Allocator.TempJob);
        FlockBehaviourJob flockBehaviourJob = new FlockBehaviourJob{ // creates the "find nearby crowd" job
            quadrantMultiHashMap = QuadrantSystem.quadrantMultiHashMap//,
            //numAgents = queryLength//numAgents
            //nearCrowdPosList = new NativeList<float3>(Allocator.TempJob)
            //nearbyCrowdArray = nearbyCrowdArray,
            //crowdArray = crowdArray
        };
        JobHandle jobHandle = flockBehaviourJob.Schedule(this, inputDeps);
        int index = 0;
        /*Entities.ForEach((Entity seeker, ref Translation transl)=>{
            
            //Translation targetTranslation = World.Active.EntityManager.GetComponentData<Translation>(hasTar.targetEntity);
    
            float3 targetDir = crowdMovementArray[index];//math.normalize(targetTranslation.Value - transl.Value); //the direction for movement
            float moveSpeed = 5f; //movement speed
            transl.Value += targetDir * moveSpeed * Time.deltaTime; //add movement to the translation 
            index++;
        });*/
        
        /*ApplyFlockingBehaviourJob applyFlockingBehaviourJob = new ApplyFlockingBehaviourJob{
            //nearbyCrowdArray = nearbyCrowdArray,
            crowdArray = crowdArray
        };
        
        jobHandle = applyFlockingBehaviourJob.Schedule(queryLength, 100, jobHandle);*/

        return jobHandle;
    }
}
