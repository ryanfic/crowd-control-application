using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine;

[UpdateAfter(typeof(PoliceUnitRemoveMovementSystem))]
public class PoliceUnitTargetNearbyObjectSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else
    private EntityQueryDesc nearbyIntersectionQueryDesc;
    private EntityQueryDesc addFindIntersectionQueryDesc;

    private bool toldToFindIntersection;

    protected override void OnCreate(){
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        nearbyIntersectionQueryDesc = new EntityQueryDesc{ // define query for the 'find nearest intersection'
            All = new ComponentType[]{
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<PoliceUnitComponent>(),
                ComponentType.ReadOnly<PoliceUnitTargetNearestIntersection>(),
            }
        };

        addFindIntersectionQueryDesc = new EntityQueryDesc{ // define query for the 'find nearest intersection'
            All = new ComponentType[]{
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<PoliceUnitComponent>(),
                ComponentType.ReadOnly<SelectedPoliceUnit>(),
            }
        };

        toldToFindIntersection = false;

        //Obtain Voice Controller - There should only be one
        PoliceUnitVoiceController[] voiceControllers = Object.FindObjectsOfType<PoliceUnitVoiceController>();
        if(voiceControllers.Length > 0){
            PoliceUnitVoiceController voiceController = voiceControllers[0]; // grab the voice controller if there is one
            voiceController.OnMoveToIntersectionCommand += VoiceFindIntersectionResponse;
        }
        
        base.OnCreate();
    }

    [BurstCompile]
    private struct FindNearestIntersectionJob : IJobChunk {
        [ReadOnly] public NativeMultiHashMap<int, StationaryQuadrantData> stationaryQuadrantMultiHashMap; // uses information from the stationary quadrant hash map to find nearby crowd agents
        public EntityCommandBuffer.ParallelWriter commandBuffer;

        [ReadOnly] public EntityTypeHandle entityType;
        [ReadOnly] public ComponentTypeHandle<Translation> translationType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            NativeArray<Translation> transArray = chunk.GetNativeArray(translationType);

            for(int i = 0; i < chunk.Count; i++){
                Entity entity = entityArray[i];
                Translation trans = transArray[i];


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

                if(!float.IsInfinity(nearestDst)){//if the distance to nearest intersection is not still infinity (thus we found something)
                    commandBuffer.AddComponent<PoliceUnitMovementDestination>(chunkIndex,entity,new PoliceUnitMovementDestination{
                        Value = nearestPos
                    }); //Add location target to entity so it starts moving towards the intersection
                }            
                
                commandBuffer.RemoveComponent<PoliceUnitTargetNearestIntersection>(chunkIndex,entity); //Remove the "find nearest intersection" component
            }
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

    //A job for adding the "find nearest intersection" tag to all selected police units
    [BurstCompile]
    private struct AddFindNearestIntersectionTagJob : IJobChunk {
        public EntityCommandBuffer.ParallelWriter commandBuffer;

        [ReadOnly] public EntityTypeHandle entityType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);

            for(int i = 0; i < chunk.Count; i++){
                Entity entity = entityArray[i];
                commandBuffer.AddComponent<PoliceUnitTargetNearestIntersection>(chunkIndex,entity,new PoliceUnitTargetNearestIntersection{}); //Add location target to entity so it starts moving towards the intersection
            }
        }
    }

    protected override void OnUpdate(){
        EntityCommandBuffer.ParallelWriter commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(); // create a command buffer

        EntityQuery nearbyIntersectionQuery = GetEntityQuery(nearbyIntersectionQueryDesc); // query the entities

        FindNearestIntersectionJob nearestIntersectionJob = new FindNearestIntersectionJob{ 
            stationaryQuadrantMultiHashMap = StationaryQuadrantSystem.quadrantMultiHashMap,
            commandBuffer = commandBuffer,
            entityType =  GetEntityTypeHandle(),
            translationType = GetComponentTypeHandle<Translation>(true)
        };
        JobHandle findIntersectionJobHandle = nearestIntersectionJob.Schedule(nearbyIntersectionQuery, this.Dependency); //schedule the job

        commandBufferSystem.AddJobHandleForProducer(findIntersectionJobHandle); // make sure the components get added/removed for the job


        this.Dependency = findIntersectionJobHandle;

        /*if(toldToFindIntersection){
            
            EntityQuery addFindIntersectionTagQuery = GetEntityQuery(addFindIntersectionQueryDesc); // query the entities
            //Add label job
            AddFindNearestIntersectionTagJob addFindIntersectionTagJob = new AddFindNearestIntersectionTagJob{ 
                commandBuffer = commandBuffer,
                entityType =  GetEntityTypeHandle()
            };
            JobHandle intersectionTagJobHandle = addFindIntersectionTagJob.Schedule(addFindIntersectionTagQuery, this.Dependency ); //schedule the job
            commandBufferSystem.AddJobHandleForProducer(intersectionTagJobHandle); // make sure the components get added/removed for the job
            this.Dependency = intersectionTagJobHandle;
            toldToFindIntersection = false;
        }*/
        
    }

    private void VoiceFindIntersectionResponse(object sender, System.EventArgs eventArgs){
        //toldToFindIntersection = true;
        //this.OnUpdate();

        EntityCommandBuffer.ParallelWriter commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(); // create a command buffer
        EntityQuery addFindIntersectionTagQuery = GetEntityQuery(addFindIntersectionQueryDesc); // query the entities
            //Add label job
        AddFindNearestIntersectionTagJob addFindIntersectionTagJob = new AddFindNearestIntersectionTagJob{ 
            commandBuffer = commandBuffer,
            entityType =  GetEntityTypeHandle()
        };
        JobHandle intersectionTagJobHandle = addFindIntersectionTagJob.Schedule(addFindIntersectionTagQuery, this.Dependency /*findIntersectionJobHandle*/); //schedule the job
        commandBufferSystem.AddJobHandleForProducer(intersectionTagJobHandle); // make sure the components get added/removed for the job
        this.Dependency = intersectionTagJobHandle;
        //toldToFindIntersection = false;
        
    }
}

