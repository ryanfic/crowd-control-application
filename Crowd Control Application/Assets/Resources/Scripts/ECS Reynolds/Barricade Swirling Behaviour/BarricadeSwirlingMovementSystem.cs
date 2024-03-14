using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;

public class BarricadeSwirlingMovementSystem : SystemBase
{
    private static readonly float anglePercent = 0.5f;
    private static readonly float exitAngle = 10;
    private EntityQueryDesc swirlQueryDec;

    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    //[BurstCompile]
    private struct SwirlBehaviourJob : IJobChunk
    {
        [ReadOnly] public EntityTypeHandle entityType;
        [ReadOnly] public ComponentTypeHandle<Translation> translationType;
        public ComponentTypeHandle<ReynoldsMovementValues> reynoldsMovementValuesType;
        //[ReadOnly] public ComponentTypeHandle<HasReynoldsSeekTargetPos> seekType;
        [ReadOnly] public ComponentTypeHandle<FinalDestinationComponent> destinationType;
        [ReadOnly] public ComponentTypeHandle<BarricadeSwirlingMovementComponent> swirlType;
        public EntityCommandBuffer.ParallelWriter commandBuffer;
        //public NativeArray<float3> posMarkerAngle;// = new NativeArray<float>(1, Allocator.Temp);
        public double curTime;


        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            NativeArray<Translation> transArray = chunk.GetNativeArray(translationType);
            NativeArray<ReynoldsMovementValues> movementArray = chunk.GetNativeArray(reynoldsMovementValuesType);
            //NativeArray<HasReynoldsSeekTargetPos> seekTargetArray = chunk.GetNativeArray(seekType);
            NativeArray<FinalDestinationComponent> destinationArray = chunk.GetNativeArray(destinationType);
            NativeArray<BarricadeSwirlingMovementComponent> swirlDataArray = chunk.GetNativeArray(swirlType);
            /*if (!posMarkerAngle.IsCreated)
            {
                posMarkerAngle = new NativeArray<float3>(1, Allocator.TempJob);
            }*/
            
            
            for (int i = 0; i < chunk.Count; i++)
            {
                Entity entity = entityArray[i];
                Translation trans = transArray[i];
                ReynoldsMovementValues movement = movementArray[i];
                //HasReynoldsSeekTargetPos targetPos = seekTargetArray[i];
                FinalDestinationComponent targetPos = destinationArray[i];
                BarricadeSwirlingMovementComponent swirlData = swirlDataArray[i];


                float3 finalGoalVec = targetPos.destination - swirlData.aoeCenter; // Calculate Vector from AOE center to real goal
                finalGoalVec.y = 0;
                float3 crowdVec = trans.Value - swirlData.aoeCenter; // Calculate vector from AOE center to agent
                crowdVec.y = 0;


                // angle is relative to +x
                // so (x,y,z) (10,0,0) is 0 degrees
                float goalAngle = math.atan2(finalGoalVec.z, finalGoalVec.x);
                //Debug.Log("Goal Angle (Rad): " + goalAngle + " (Deg): " + goalAngle * 180/math.PI);

                float absGoalAngle = goalAngle + math.PI;
                //Debug.Log("Shifted Goal Angle (Rad): " + absGoalAngle + " (Deg): " + absGoalAngle * 180 / math.PI);

                float crowdAngle = math.atan2(crowdVec.z, crowdVec.x);
                
                //Debug.Log("Crowd Angle (Rad): " + crowdAngle + " (Deg): " + crowdAngle * 180/math.PI);

                float absCrowdAngle = crowdAngle + math.PI;
                //Debug.Log("Shifted Crowd Angle (Rad): " + absCrowdAngle + " (Deg): " + absCrowdAngle * 180 / math.PI);


                float betweenAngle = (goalAngle + crowdAngle) * anglePercent;
                
                // We want angle < 180 if the agent is on the left (+z direction)
                //if (crowdAngle < goalAngle && goalAngle -  crowdAngle < math.PI)

                if (betweenAngle < goalAngle // this only happens if the target angle has passed into positives from negatives
                    || betweenAngle > crowdAngle) // this means the between angle is ccw from crowd agent
                {
                    //Debug.Log("Target Ang: " + betweenAngle * 180 / math.PI); // this is the appropriate state, do nothing
                }
                else // this is the scenario where the target angle is cw, and we want to actually have it be ccw, so -180 degrees from it
                {
                    //Debug.Log("Flipped! Target Ang: " + betweenAngle * 180 / math.PI);
                    betweenAngle = (goalAngle + crowdAngle) * (1 - anglePercent);
                    betweenAngle = betweenAngle - math.PI;
                }
                quaternion swirlAngle = quaternion.RotateY(-betweenAngle);
                
                float absHalfAngle = (absGoalAngle + absCrowdAngle) / 2;

                //Debug.Log("Halfway Angle: " + betweenAngle);
                //Debug.Log("Absolute halfway angle: " + (absHalfAngle));

                //quaternion swirlAngle = quaternion.RotateY(-betweenAngle);
                float3 swirlGoalPos = new float3(1, 0, 0);

                //Debug.Log("Goal Before Rotation: " + swirlGoalPos);

                swirlGoalPos = math.mul(swirlAngle, swirlGoalPos);

                //Debug.Log("Goal After Rotation: " + swirlGoalPos);


                swirlGoalPos = swirlGoalPos * swirlData.halfRadius;

                swirlGoalPos.y = 0;
                //posMarkerAngle[0] = swirlGoalPos;
                //Debug.Log("Goal After Rescale: " + swirlGoalPos);


                // TODO: REPURPOSE FOR MOVEMENT LATER
                float3 move = swirlGoalPos - trans.Value;

                movementArray[i] = new ReynoldsMovementValues
                {
                    flockMovement = movement.flockMovement,
                    seekMovement = move,
                    fleeMovement = movement.fleeMovement,
                    obstacleAvoidanceMovement = movement.obstacleAvoidanceMovement
                };

                

                float goalAngDeg = goalAngle * 180 / math.PI;

                //Debug.Log("Objective Angle  (Deg): " + goalAngDeg);
                
                //Debug.Log("Crowd Angle (Deg): " + crowdAngle * 180 / math.PI);
                //Debug.Log("Original Crowd Angle (Deg): " + originalCA * 180 / math.PI);

                if (goalAngle * 180 / math.PI <= (-180+exitAngle))
                {
                    if ((crowdAngle * 180 / math.PI > -exitAngle + goalAngDeg && crowdAngle * 180 / math.PI < exitAngle + goalAngDeg) // agent is within +/- exit angle
                        || (crowdAngle * 180 / math.PI > -exitAngle + (180+goalAngDeg) && crowdAngle * 180 / math.PI < exitAngle + goalAngDeg))
                    {
                        if (crowdAngle * 180 / math.PI > -exitAngle + goalAngDeg && crowdAngle * 180 / math.PI < exitAngle + goalAngDeg){
                            //Debug.Log("From within normal angle");
                        }
                        if((crowdAngle * 180 / math.PI > -exitAngle + (180 + goalAngDeg) && crowdAngle * 180 / math.PI < exitAngle + goalAngDeg))
                        {
                            //Debug.Log("On the 'opposite side' of 180, which is within " + (180 + goalAngDeg) + " of 180");
                        }
                        //Debug.Log("(Goal < -170) Crowd Angle  (Deg): " + crowdAngle * 180 / math.PI + " is > " + exitAngle + " and < " + exitAngle + ", so the agent should exit!");
                        LeaveBarricadeAOEComponent component = new LeaveBarricadeAOEComponent
                        {
                            lastTriggerCollision = curTime
                        };
                        commandBuffer.AddComponent<LeaveBarricadeAOEComponent>(chunkIndex, entity, component);
                    }
                }
                else if (goalAngle * 180 / math.PI >= (180-exitAngle))
                {
                    if ((crowdAngle * 180 / math.PI > -exitAngle + goalAngDeg && crowdAngle * 180 / math.PI < exitAngle + goalAngDeg) // agent is within +/- exit angle
                        || (crowdAngle * 180 / math.PI > -exitAngle + goalAngDeg && crowdAngle * 180 / math.PI < exitAngle + (-180+goalAngDeg)))
                    {
                        if (crowdAngle * 180 / math.PI > -exitAngle + goalAngDeg && crowdAngle * 180 / math.PI < exitAngle + goalAngDeg)
                        {
                            //Debug.Log("From within normal angle");
                        }
                        if ((crowdAngle * 180 / math.PI > -exitAngle + goalAngDeg && crowdAngle * 180 / math.PI < exitAngle + (-180 + goalAngDeg)))
                        {
                            //Debug.Log("On the 'opposite side' of 180, which is within " + (-180 + goalAngDeg) + " of 180");
                        }
                        //Debug.Log("(Goal < -170) Crowd Angle  (Deg): " + crowdAngle * 180 / math.PI + " is > " + exitAngle + " and < " + exitAngle + ", so the agent should exit!");
                        LeaveBarricadeAOEComponent component = new LeaveBarricadeAOEComponent
                        {
                            lastTriggerCollision = curTime
                        };
                        commandBuffer.AddComponent<LeaveBarricadeAOEComponent>(chunkIndex, entity, component);
                    }
                }
                else
                {
                    if (crowdAngle * 180 / math.PI > -exitAngle + goalAngDeg && crowdAngle * 180 / math.PI < exitAngle + goalAngDeg)
                    {
                        //Debug.Log("Crowd Angle  (Deg): " + crowdAngle * 180 / math.PI + " is > " + (exitAngle + goalAngDeg) + " and < " + (exitAngle + goalAngDeg) + ", so the agent should exit!");
                        LeaveBarricadeAOEComponent component = new LeaveBarricadeAOEComponent
                        {
                            lastTriggerCollision = curTime
                        };
                        commandBuffer.AddComponent<LeaveBarricadeAOEComponent>(chunkIndex, entity, component);
                    }
                }
                
            }
        }
    }

    protected override void OnCreate()
    {
        swirlQueryDec = new EntityQueryDesc
        {
            All = new ComponentType[]{
                ComponentType.ReadOnly<Translation>(),
                typeof(ReynoldsMovementValues),
                ComponentType.ReadOnly<FinalDestinationComponent>(),
                //ComponentType.ReadOnly<HasReynoldsSeekTargetPos>(),
                ComponentType.ReadOnly<BarricadeSwirlingMovementComponent>()
            },
            None = new ComponentType[]{
                ComponentType.ReadOnly<LeaveBarricadeAOEComponent>()
            }
        };
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }

    protected override void OnUpdate()
    {
        EntityQuery swirlQuery = GetEntityQuery(swirlQueryDec); // query the entities

        //NativeArray<float3> markerPos = new NativeArray<float3>(1, Allocator.TempJob);
        SwirlBehaviourJob swirlBehaviourJob = new SwirlBehaviourJob
        {
            entityType = GetEntityTypeHandle(),
            translationType = GetComponentTypeHandle<Translation>(true),
            reynoldsMovementValuesType = GetComponentTypeHandle<ReynoldsMovementValues>(),
            destinationType = GetComponentTypeHandle<FinalDestinationComponent>(true),
            swirlType = GetComponentTypeHandle<BarricadeSwirlingMovementComponent>(true),
            commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            //posMarkerAngle = markerPos,
            curTime = Time.ElapsedTime,
        };
        JobHandle jobHandle = swirlBehaviourJob.Schedule(swirlQuery, this.Dependency);
        jobHandle.Complete();

        /*Entities
            .ForEach
            ((ref Translation trans, in PosMarkerComponent posMarker) =>
            {
                
                trans.Value = markerPos[0];
            })
            .WithoutBurst()
            .Schedule();*/
        //this.Dependency = jobHandle;
    }
}