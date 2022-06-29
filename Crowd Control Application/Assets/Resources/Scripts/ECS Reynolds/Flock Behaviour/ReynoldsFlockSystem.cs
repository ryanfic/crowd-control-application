using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;

public struct CrowdData{
    public Entity entity;
    public Translation translation;
    public ReynoldsFlockBehaviour flockBehaviour;
}

//[UpdateAfter(typeof(MovingQuadrantSystem))]
public class ReynoldsFlockSystem : SystemBase
{
    private EntityQueryDesc flockQueryDec;

    //[BurstCompile]
    private struct FlockBehaviourJob : IJobChunk {
        // Find a list of nearby agents
        // Do the calculation for flocking
        // Change the translation of the agent

        [ReadOnly] public NativeMultiHashMap<int, MovingQuadrantData> quadrantMultiHashMap; // uses information from the quadrant hash map to find nearby crowd agents

        public EntityCommandBuffer.ParallelWriter entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job

        // Jobs need an Execute function
        //ReadOnly on the Translation because the translation is not being altered
        [ReadOnly] public EntityTypeHandle entityType;
        public BufferTypeHandle<ReynoldsNearbyFlockPos> nearbyPosBufferType;
        public BufferTypeHandle<ReynoldsNearbyFlockVel> nearbyVelBufferType;
        [ReadOnly] public ComponentTypeHandle<Translation> translationType;
        [ReadOnly] public ComponentTypeHandle<ReynoldsFlockBehaviour> flockType;
        [ReadOnly] public ComponentTypeHandle<PreviousMovement> preVelType;
        public ComponentTypeHandle<ReynoldsMovementValues> reynoldsMovementValuesType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            BufferAccessor<ReynoldsNearbyFlockPos> nearbyPosBuffers = chunk.GetBufferAccessor<ReynoldsNearbyFlockPos>(nearbyPosBufferType);
            BufferAccessor<ReynoldsNearbyFlockVel> nearbyVelBuffers = chunk.GetBufferAccessor<ReynoldsNearbyFlockVel>(nearbyVelBufferType);
            NativeArray<Translation> transArray = chunk.GetNativeArray(translationType);
            NativeArray<ReynoldsFlockBehaviour> flockArray = chunk.GetNativeArray(flockType);
            NativeArray<PreviousMovement> preVelArray = chunk.GetNativeArray(preVelType);
            NativeArray<ReynoldsMovementValues> movementArray = chunk.GetNativeArray(reynoldsMovementValuesType);

            for(int i = 0; i < chunk.Count; i++){
                Entity entity = entityArray[i];
                DynamicBuffer<ReynoldsNearbyFlockPos> posBuffer = nearbyPosBuffers[i];
                DynamicBuffer<ReynoldsNearbyFlockVel> velBuffer = nearbyVelBuffers[i];
                Translation trans = transArray[i];
                ReynoldsFlockBehaviour flockBehaviour = flockArray[i];
                PreviousMovement preVel = preVelArray[i];
                ReynoldsMovementValues movement = movementArray[i];

                float3 agentPosition = trans.Value; // the position of the seeker

                DynamicBuffer<float3> nearCrowdPosList = posBuffer.Reinterpret<float3>(); // reinterpret the buffer so that it is used like a buffer of float3s
                nearCrowdPosList.Clear();

                DynamicBuffer<float3> nearCrowdVelList = velBuffer.Reinterpret<float3>(); // reinterpret the buffer so that it is used like a buffer of float3s
                nearCrowdVelList.Clear();


                float searchRadius = math.max(flockBehaviour.CohesionRadius,flockBehaviour.AvoidanceRadius); // Choose the farther radius
                
                int hashMapKey = MovingQuadrantSystem.GetPositionHashMapKey(trans.Value); // Calculate the hash key of the seeker in question

                FindCrowdAgents(hashMapKey, agentPosition, entity, ref nearCrowdPosList, ref nearCrowdVelList, ref searchRadius); // Seach the quadrant that the seeker is in
                FindCrowdAgents(hashMapKey + 1,agentPosition, entity, ref nearCrowdPosList, ref nearCrowdVelList, ref searchRadius); // search the quadrant to the right
                FindCrowdAgents(hashMapKey - 1,agentPosition, entity, ref nearCrowdPosList, ref nearCrowdVelList, ref searchRadius); // search the quadrant to the left
                FindCrowdAgents(hashMapKey + MovingQuadrantSystem.quadrantZMultiplier,agentPosition, entity, ref nearCrowdPosList, ref nearCrowdVelList, ref searchRadius); // quadrant above
                FindCrowdAgents(hashMapKey - MovingQuadrantSystem.quadrantZMultiplier,agentPosition, entity, ref nearCrowdPosList, ref nearCrowdVelList, ref searchRadius); // quadrant below
                FindCrowdAgents(hashMapKey + 1 + MovingQuadrantSystem.quadrantZMultiplier,agentPosition, entity, ref nearCrowdPosList, ref nearCrowdVelList, ref searchRadius); // up right
                FindCrowdAgents(hashMapKey - 1 + MovingQuadrantSystem.quadrantZMultiplier,agentPosition, entity, ref nearCrowdPosList, ref nearCrowdVelList, ref searchRadius); // up left
                FindCrowdAgents(hashMapKey + 1 - MovingQuadrantSystem.quadrantZMultiplier,agentPosition, entity, ref nearCrowdPosList, ref nearCrowdVelList, ref searchRadius); // down right
                FindCrowdAgents(hashMapKey -1 - MovingQuadrantSystem.quadrantZMultiplier,agentPosition, entity, ref nearCrowdPosList, ref nearCrowdVelList, ref searchRadius); // down left

                movementArray[i] = GetFlockingBehaviour(ref trans, ref preVel.value, ref nearCrowdPosList, ref nearCrowdVelList, ref flockBehaviour, movement);
            }
        }



        // Find nearby crowd agents using a buffer
        private void FindCrowdAgents(int hashMapKey, float3 agentPosition, Entity agentEntity , ref DynamicBuffer<float3> nearbyCrowdPosList,ref DynamicBuffer<float3> nearbyCrowdVelList, ref float nearbyRadius){
            // Get the data from the quadrant that the seeker belongs to
            MovingQuadrantData quadData;
            NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
            if(quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadData, out nativeMultiHashMapIterator)){ // try to get the first element in the hashmap
                do{ //if there is at least one thing in the quadrant, try getting more
                    if(MovingQuadrantEntity.TypeEnum.Crowd == quadData.quadrantEntity.typeEnum){ // make sure the other entity is a crowd agent
                        float dist = math.distance(agentPosition, quadData.position);
                        if(dist < nearbyRadius  && agentEntity != quadData.entity) 
                        {
                            
                            //this target is within cohesion radius
                            nearbyCrowdPosList.Add(quadData.position);
                            nearbyCrowdVelList.Add(quadData.velocity);
                        }
                    }
                } while(quadrantMultiHashMap.TryGetNextValue(out quadData, ref nativeMultiHashMapIterator));
            }
        }

        // Apply the flocking behaviour using a buffer
        private ReynoldsMovementValues GetFlockingBehaviour(ref Translation agentTranslation, ref float3 preVelocity, ref DynamicBuffer<float3> nearbyCrowdPosList, ref DynamicBuffer<float3> nearbyCrowdVelList,  ref ReynoldsFlockBehaviour flockBehaviour, ReynoldsMovementValues movement){
            float3 move = float3.zero; // where the agent will move
            // Calculate Avoidance first
            float3 avoidance = CalculateAvoidance(ref agentTranslation.Value, ref nearbyCrowdPosList, ref flockBehaviour);
            //if the avoidance is not zero
            if(!avoidance.Equals(float3.zero))
            {

                avoidance = math.normalize(avoidance);
                avoidance *= flockBehaviour.AvoidanceWeight;

                move += avoidance;
                //Debug.Log("Move after avoid: " +move);
            }

            // Calculate cohesion
            float3 cohesion = CalculateCohesion(ref agentTranslation.Value, ref preVelocity, ref nearbyCrowdPosList, ref nearbyCrowdVelList, ref flockBehaviour);
            if(!cohesion.Equals(float3.zero))
            {
                //Debug.Log("Cohes Dst Sq: " +math.distancesq(float3.zero,cohesion));
                //if the avoidance is not normalized  (The square of the length of the position is greater than the square of the weights)
                if(math.distancesq(float3.zero,cohesion) > flockBehaviour.CohesionWeight * flockBehaviour.CohesionWeight)
                {
                    //normalize the movement vector
                    cohesion = math.normalize(cohesion);
                    cohesion *= flockBehaviour.CohesionWeight;
                }

                move += cohesion;
                //Debug.Log("Move after cohes: " +move);
            }
            // May want to calculate alignment after, but not at time of writing this


            ReynoldsMovementValues movementValues = new ReynoldsMovementValues{
                flockMovement = move,
                seekMovement = movement.seekMovement,
                fleeMovement = movement.fleeMovement,
                obstacleAvoidanceMovement = movement.obstacleAvoidanceMovement
            };

            //Debug.Log("Flock stored: " +movementValues.flockMovement);
            return movementValues;
            //movement.flockMovement = move;


            //float moveSpeed = 5f; //movement speed
            //agentTranslation.Value += move * moveSpeed * Time.deltaTime; //add movement to the translation
            //return move;
        }

        // Calculate the avoidance using a buffer
        private float3 CalculateAvoidance(ref float3 agentPosition, ref DynamicBuffer<float3> context, /*[ReadOnly]*/ ref ReynoldsFlockBehaviour flockBehaviour)
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
                if(math.distancesq(item,agentPosition) < flockBehaviour.AvoidanceRadius * flockBehaviour.AvoidanceRadius)
                {
                    avoidanceMove += (agentPosition - item);
                                                            //add the vector from the projected position to item's position into the average
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

        // Calculate the cohesion using a buffer
        private float3 CalculateCohesion(ref float3 agentPosition, ref float3 preVelocity, ref DynamicBuffer<float3> context, ref DynamicBuffer<float3> contextVelocity,  ref ReynoldsFlockBehaviour flockBehaviour){
            //if no neighbours, return no adjustment
            if(context.Length == 0)
            {
                return float3.zero;
            }
            int count = 0;
            //add all points together and average
            float3 cohesionMove = float3.zero;
            for(int i = 0; i < context.Length; i++){
                float3 item = context[i];
                float3 velocity = contextVelocity[i];

                float angle = math.degrees(math.acos(math.dot(velocity, math.normalize(preVelocity))));

                //Debug.Log("Angle: " + angle);
                //Debug.Log("Is a number? " + (angle == angle));

                if(angle == angle){     
                    if(math.distancesq(item,agentPosition) < flockBehaviour.CohesionRadius * flockBehaviour.CohesionRadius && angle < 45){
                        cohesionMove += item;
                        count++;
                    }
                }                
            }
                
            if(count > 0){
                cohesionMove /= count;
            }
        
            //create offset from agent position
            cohesionMove -= agentPosition;
            return cohesionMove;
        }
    }

    protected override void OnCreate() {
        flockQueryDec = new EntityQueryDesc{
            All = new ComponentType[]{
                typeof(ReynoldsNearbyFlockPos),
                typeof(ReynoldsNearbyFlockVel),
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<ReynoldsFlockBehaviour>(),
                ComponentType.ReadOnly<PreviousMovement>(),
                typeof(ReynoldsMovementValues)
            }
        };
        base.OnCreate();
    }

    
    protected override void OnUpdate(){
        EntityQuery flockQuery = GetEntityQuery(flockQueryDec); // query the entities

        FlockBehaviourJob flockBehaviourJob = new FlockBehaviourJob{ // creates the "find nearby crowd" job
            quadrantMultiHashMap = MovingQuadrantSystem.quadrantMultiHashMap,
            entityType = GetEntityTypeHandle(),
            nearbyPosBufferType = GetBufferTypeHandle<ReynoldsNearbyFlockPos>(),
            nearbyVelBufferType = GetBufferTypeHandle<ReynoldsNearbyFlockVel>(),
            translationType = GetComponentTypeHandle<Translation>(true),
            flockType = GetComponentTypeHandle<ReynoldsFlockBehaviour>(true),
            preVelType = GetComponentTypeHandle<PreviousMovement>(true),
            reynoldsMovementValuesType = GetComponentTypeHandle<ReynoldsMovementValues>()
        };
        JobHandle jobHandle = flockBehaviourJob.Schedule(flockQuery, this.Dependency);
        jobHandle.Complete();
        //this.Dependency = jobHandle;
    }
}
