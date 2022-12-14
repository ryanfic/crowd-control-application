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
    private static readonly float anglePercent = 0.75f;
    private EntityQueryDesc swirlQueryDec;

    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    //[BurstCompile]
    private struct SwirlBehaviourJob : IJobChunk
    {
        [ReadOnly] public ComponentTypeHandle<Translation> translationType;
        public ComponentTypeHandle<ReynoldsMovementValues> reynoldsMovementValuesType;
        [ReadOnly] public ComponentTypeHandle<HasReynoldsSeekTargetPos> seekType;
        [ReadOnly] public ComponentTypeHandle<BarricadeSwirlingMovementComponent> swirlType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Translation> transArray = chunk.GetNativeArray(translationType);
            NativeArray<ReynoldsMovementValues> movementArray = chunk.GetNativeArray(reynoldsMovementValuesType);
            NativeArray<HasReynoldsSeekTargetPos> seekTargetArray = chunk.GetNativeArray(seekType);
            NativeArray<BarricadeSwirlingMovementComponent> swirlDataArray = chunk.GetNativeArray(swirlType);

            for (int i = 0; i < chunk.Count; i++)
            {
                Translation trans = transArray[i];
                ReynoldsMovementValues movement = movementArray[i];
                HasReynoldsSeekTargetPos targetPos = seekTargetArray[i];
                BarricadeSwirlingMovementComponent swirlData = swirlDataArray[i];


                float3 finalGoalVec = targetPos.targetPos - swirlData.aoeCenter; // Calculate Vector from AOE center to real goal
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


                // We want angle < 180 if the agent is on the left (+z direction)
                if(trans.Value.z > 0)
                {
                    //float dif = math.PI - crowdAngle;
                    //crowdAngle = -math.PI - dif;
                    crowdAngle = -2 * math.PI + crowdAngle;
                }

                float betweenAngle = (goalAngle + crowdAngle) * anglePercent;
                float absHalfAngle = (absGoalAngle + absCrowdAngle) / 2;

                //Debug.Log("Halfway Angle: " + betweenAngle);
                //Debug.Log("Absolute halfway angle: " + (absHalfAngle));

                quaternion swirlAngle = quaternion.RotateY(-betweenAngle);
                float3 swirlGoalPos = new float3(1, 0, 0);

                //Debug.Log("Goal Before Rotation: " + swirlGoalPos);

                swirlGoalPos = math.mul(swirlAngle, swirlGoalPos);

                //Debug.Log("Goal After Rotation: " + swirlGoalPos);


                swirlGoalPos = swirlGoalPos * swirlData.halfRadius;

                swirlGoalPos.y = 0;

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
                ComponentType.ReadOnly<HasReynoldsSeekTargetPos>(),
                ComponentType.ReadOnly<BarricadeSwirlingMovementComponent>()
            }
        };
        base.OnCreate();
    }

    protected override void OnUpdate()
    {
        EntityQuery swirlQuery = GetEntityQuery(swirlQueryDec); // query the entities

        SwirlBehaviourJob swirlBehaviourJob = new SwirlBehaviourJob
        {
            translationType = GetComponentTypeHandle<Translation>(true),
            reynoldsMovementValuesType = GetComponentTypeHandle<ReynoldsMovementValues>(),
            seekType = GetComponentTypeHandle<HasReynoldsSeekTargetPos>(true),
            swirlType = GetComponentTypeHandle<BarricadeSwirlingMovementComponent>(true),
        };
        JobHandle jobHandle = swirlBehaviourJob.Schedule(swirlQuery, this.Dependency);
        this.Dependency = jobHandle;
    }
}