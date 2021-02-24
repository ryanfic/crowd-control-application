using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;

/*public class FindTargetSystem : ComponentSystem
{
    protected override void OnUpdate(){
        Entities.WithNone<HasTarget>().WithAll<Seeker>().ForEach((Entity seeker, ref Translation seekerTranslation) => {
            //Code running on all Seekers
            //Debug.Log(seeker);
            
            float3 seekerPosition = seekerTranslation.Value;

            Entity closestTargetEntity = Entity.Null; //Since entity is a struct, it cannot simply be null, it must be Entity.Null
            float3 closestTargetPosition = float3.zero;


            Entities.WithAll<Target>().ForEach((Entity targetEntity, ref Translation targetTranslation) => {
                //Cycling through all entities with Target tag
                //Debug.Log(targetEntity);

                if(closestTargetEntity == Entity.Null){ //if there was no closest target entity
                    //No target
                    closestTargetEntity = targetEntity;
                    closestTargetPosition = targetTranslation.Value;
                }
                else{
                    if(math.distance(seekerPosition, targetTranslation.Value) < math.distance(seekerPosition, closestTargetPosition))
                    {
                        //this target is closer
                        closestTargetEntity = targetEntity;
                        closestTargetPosition = targetTranslation.Value;
                    }
                }
            });

            //Closest target
            if(closestTargetEntity != Entity.Null){ //if there is a closest entity
                PostUpdateCommands.AddComponent(seeker, new HasTarget{targetEntity = closestTargetEntity});
            }

        });
    }
}*/

/*
    Uses Jobs to find closest target (for multithreading purposes)
*/
public class FindTargetJobSystem : JobComponentSystem{

    private struct EntityWithPosition{
        public Entity entity;
        public float3 position;
    }

    [RequireComponentTag(typeof(Seeker))] // The Job will only run on entities that are seekers
    [ExcludeComponent(typeof(HasTarget))] // The Job will NOT run on entities that have a target
    //[BurstCompile]
    //Needs a job to work
    private struct FindTargetJob : IJobForEachWithEntity<Translation> {
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<EntityWithPosition> targetArray; // To look for target, we need an array of all targets (and their positions)
                                                                                                    //DeallocateOnJobCompletion means the nativearray gets removed when the job is done

        //Jobs cannot use static variables, so the Entity Command buffer needs to be passed in
        public EntityCommandBuffer.ParallelWriter entityCommandBuffer; // Holds commands that will be executed at a later time
                                                                    //Concurrent for concurrent writing

        // Jobs need an Execute function
        //ReadOnly on the Translation because the translation is not being altered
        public void Execute(Entity entity, int index, [ReadOnly] ref Translation trans){
            float3 seekerPosition = trans.Value; // the position of the seeker

            Entity closestTargetEntity = Entity.Null; //Since entity is a struct, it cannot simply be null, it must be Entity.Null
            float3 closestTargetPosition = float3.zero;

            for(int i=0; i<targetArray.Length; i++){
                //Cycle through all target entities
                EntityWithPosition targetEntityWithPosition = targetArray[i];

                if(closestTargetEntity == Entity.Null){ //if there was no closest target entity
                    //No target
                    closestTargetEntity = targetEntityWithPosition.entity;
                    closestTargetPosition = targetEntityWithPosition.position;
                }
                else{
                    if(math.distance(seekerPosition, targetEntityWithPosition.position) < math.distance(seekerPosition, closestTargetPosition))
                    {
                        //this target is closer
                        closestTargetEntity = targetEntityWithPosition.entity;
                        closestTargetPosition = targetEntityWithPosition.position;
                    }
                }
            }

            //Closest target
            if(closestTargetEntity != Entity.Null){ //if there is a closest entity
                entityCommandBuffer.AddComponent(index, entity, new HasTarget{targetEntity = closestTargetEntity});
            }
        }
    }

    /*
        Separated the Find Target Job into two parts in order to allow for BurstCompile to work
        This Job finds all closest targets for each seeker
        This job uses burstcompile
    */
    [RequireComponentTag(typeof(Seeker))] // The Job will only run on entities that are seekers
    [ExcludeComponent(typeof(HasTarget))] // The Job will NOT run on entities that have a target
    [BurstCompile]
    //Needs a job to work
    private struct FindTargetBurstJob : IJobForEachWithEntity<Translation> {
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<EntityWithPosition> targetArray; // To look for target, we need an array of all targets (and their positions)
                                                                                                    //DeallocateOnJobCompletion means the nativearray gets removed when the job is done

        public NativeArray<Entity> closestTargetEntityArray; // in order to add the closest target (in the other job), need an array of closest targets


        // Jobs need an Execute function
        //ReadOnly on the Translation because the translation is not being altered
        public void Execute(Entity entity, int index, [ReadOnly] ref Translation trans){
            float3 seekerPosition = trans.Value; // the position of the seeker

            Entity closestTargetEntity = Entity.Null; //Since entity is a struct, it cannot simply be null, it must be Entity.Null
            float3 closestTargetPosition = float3.zero;

            for(int i=0; i<targetArray.Length; i++){
                //Cycle through all target entities
                EntityWithPosition targetEntityWithPosition = targetArray[i];

                if(closestTargetEntity == Entity.Null){ //if there was no closest target entity
                    //No target
                    closestTargetEntity = targetEntityWithPosition.entity;
                    closestTargetPosition = targetEntityWithPosition.position;
                }
                else{
                    if(math.distance(seekerPosition, targetEntityWithPosition.position) < math.distance(seekerPosition, closestTargetPosition))
                    {
                        //this target is closer
                        closestTargetEntity = targetEntityWithPosition.entity;
                        closestTargetPosition = targetEntityWithPosition.position;
                    }
                }
            }

            // once found closest target, add the closest target to the closest target array
            closestTargetEntityArray[index] = closestTargetEntity;

        }
    }

    /*
        Separated the Find Target Job into two parts in order to allow for BurstCompile to work
        This Job assigns all closest targets to each seeker
        This job does not use burstcompile
    */
    [RequireComponentTag(typeof(Seeker))] // The Job will only run on entities that are seekers
    [ExcludeComponent(typeof(HasTarget))] // The Job will NOT run on entities that have a target
    private struct AddComponentJob : IJobForEachWithEntity<Translation>{
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> closestTargetEntityArray; // Used to assign all closest targets to seekers
        public EntityCommandBuffer.ParallelWriter entityCommandBuffer;
        public void Execute(Entity entity, int index, [ReadOnly] ref Translation trans){
            if(closestTargetEntityArray[index] != Entity.Null){ // If there is a closest target for this seeker
                entityCommandBuffer.AddComponent(index, entity, new HasTarget{ targetEntity = closestTargetEntityArray[index]});
            }
        }
    }

    [RequireComponentTag(typeof(Seeker))] // The Job will only run on entities that are seekers
    [ExcludeComponent(typeof(HasTarget))] // The Job will NOT run on entities that have a target
    [BurstCompile]
    /*Find Targets using the Quadrant system (instead of iterating through all targets)*/
    private struct FindTargetQuadrantSystemJob : IJobForEachWithEntity<Translation> {
        [ReadOnly] public NativeMultiHashMap<int, MovingQuadrantData> quadrantMultiHashMap; // uses information from the quadrant hash map to find nearby targets

        public NativeArray<Entity> closestTargetEntityArray; // in order to add the closest target (in the other job), need an array of closest targets


        // Jobs need an Execute function
        //ReadOnly on the Translation because the translation is not being altered
        public void Execute(Entity entity, int index, [ReadOnly] ref Translation trans/*, [ReadOnly] ref QuadrantEntity quadrantEntity*/){
            float3 seekerPosition = trans.Value; // the position of the seeker

            Entity closestTargetEntity = Entity.Null; //Since entity is a struct, it cannot simply be null, it must be Entity.Null
            //float3 closestTargetPosition = float3.zero;
            float closestTargetDistance = float.MaxValue;
            int hashMapKey = MovingQuadrantSystem.GetPositionHashMapKey(trans.Value); // Calculate the hash key of the seeker in question

            FindTarget(hashMapKey,seekerPosition, ref closestTargetEntity, ref closestTargetDistance); // Seach the quadrant that the seeker is in
            FindTarget(hashMapKey + 1,seekerPosition, ref closestTargetEntity, ref closestTargetDistance); // search the quadrant to the right
            FindTarget(hashMapKey - 1,seekerPosition, ref closestTargetEntity, ref closestTargetDistance); // search the quadrant to the left
            FindTarget(hashMapKey + MovingQuadrantSystem.quadrantYMultiplier,seekerPosition, ref closestTargetEntity, ref closestTargetDistance); // quadrant above
            FindTarget(hashMapKey - MovingQuadrantSystem.quadrantYMultiplier,seekerPosition, ref closestTargetEntity, ref closestTargetDistance); // quadrant below
            FindTarget(hashMapKey + 1 + MovingQuadrantSystem.quadrantYMultiplier,seekerPosition, ref closestTargetEntity, ref closestTargetDistance); // up right
            FindTarget(hashMapKey - 1 + MovingQuadrantSystem.quadrantYMultiplier,seekerPosition, ref closestTargetEntity, ref closestTargetDistance); // up left
            FindTarget(hashMapKey + 1 - MovingQuadrantSystem.quadrantYMultiplier,seekerPosition, ref closestTargetEntity, ref closestTargetDistance); // down right
            FindTarget(hashMapKey -1 - MovingQuadrantSystem.quadrantYMultiplier,seekerPosition, ref closestTargetEntity, ref closestTargetDistance); // down left

            // once found closest target, add the closest target to the closest target array
            closestTargetEntityArray[index] = closestTargetEntity;

        }

        private void FindTarget(int hashMapKey, float3 seekerPosition, ref Entity closestTargetEntity, ref float closestTargetDistance){
            // Get the data from the quadrant that the seeker belongs to
            MovingQuadrantData quadData;
            NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
            if(quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadData, out nativeMultiHashMapIterator)){ // try to get the first element in the hashmap
                do{ //if there is at least one thing in the quadrant, try getting more
                    if(MovingQuadrantEntity.TypeEnum.Target == quadData.quadrantEntity.typeEnum){ // make sure the other entity is not the same type (is a target and seeker combo)
                        if(closestTargetEntity == Entity.Null){ //if there was no closest target entity
                            //No target
                            closestTargetEntity = quadData.entity;
                            closestTargetDistance = math.distancesq(seekerPosition, quadData.position);
                        }
                        else{
                            if(math.distancesq(seekerPosition, quadData.position) < closestTargetDistance)
                            {
                                //this target is closer
                                closestTargetEntity = quadData.entity;
                                closestTargetDistance = math.distancesq(seekerPosition, quadData.position);
                            }
                        }
                    }
                } while(quadrantMultiHashMap.TryGetNextValue(out quadData, ref nativeMultiHashMapIterator));
            }
        }
    }

    

    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;
    protected override void OnCreate(){
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps){
        /*EntityQuery targetQuery = GetEntityQuery(typeof(Target),ComponentType.ReadOnly<Translation>()); // To get the amount of entities we need to store in the target array
                                                    // Targets need to have Target label, but they also need Translation
                                                    // Won't be altering the Translation of the target, so have it be Read only!
        NativeArray<Entity> targetEntityArray = targetQuery.ToEntityArray(Allocator.TempJob); //Create an array of all the target entities
        NativeArray<Translation> targetTranslationArray = targetQuery.ToComponentDataArray<Translation>(Allocator.TempJob); // Create an array of all the target's translations

        NativeArray<EntityWithPosition> tarArray = new NativeArray<EntityWithPosition>(targetEntityArray.Length, Allocator.TempJob);
        
        for(int i = 0; i < targetEntityArray.Length; i++){ // cycle through and create the target array
            tarArray[i] = new EntityWithPosition{ // fill the position with the appropriate information from the parallel arrays
                entity = targetEntityArray[i],
                position = targetTranslationArray[i].Value,
            };
        }
        
        // Dispose of the parallel arrays, they've served their purpose
        targetEntityArray.Dispose();
        targetTranslationArray.Dispose();*/

        EntityQuery seekerQuery = GetEntityQuery(typeof(Seeker),ComponentType.Exclude<HasTarget>()); // Need number of seekers to make closest target array, so query all seekers that do not have targets
        NativeArray<Entity> closestTargetEntityArray = new NativeArray<Entity>(seekerQuery.CalculateEntityCount(), Allocator.TempJob); // Filled in the burst job, then used in the other job to assign closest targets
        /*
        FindTargetJob findTargetJob = new FindTargetJob{ // Create the job
            targetArray = tarArray,
            entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(), // Create a command buffer for the job
        };
        */
        /*FindTargetBurstJob findTargetBurstJob = new FindTargetBurstJob{
            targetArray = tarArray,
            closestTargetEntityArray = closestTargetEntityArray
        };

        JobHandle jobHandle = findTargetBurstJob.Schedule(this, inputDeps); // Schedule the job*/
        FindTargetQuadrantSystemJob findTargetQuadrantSystemJob = new FindTargetQuadrantSystemJob { // create the "find targets" job
            quadrantMultiHashMap = MovingQuadrantSystem.quadrantMultiHashMap, // the job needs the hashmap
            closestTargetEntityArray = closestTargetEntityArray
        };
        JobHandle jobHandle = findTargetQuadrantSystemJob.Schedule(this, inputDeps);


        AddComponentJob addComponentJob = new AddComponentJob{
            closestTargetEntityArray = closestTargetEntityArray,
            entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(), // Create a command buffer for the job
        };
        jobHandle = addComponentJob.Schedule(this, jobHandle); // second arg is to say that it has to wait until the other job is done
        
        endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle); // Execute the command buffer after the job is completed
    
        return jobHandle;
    }
}
