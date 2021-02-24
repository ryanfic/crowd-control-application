using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;

public class ReynoldsFleeSystem : SystemBase
{
    private EntityQueryDesc fleeQueryDec;

    [BurstCompile]
    private struct FleeBehaviourJob : IJobChunk {      
        [ReadOnly] public ComponentTypeHandle<Translation> translationType;
        public ComponentTypeHandle<ReynoldsMovementValues> reynoldsMovementValuesType;
        [ReadOnly] public ComponentTypeHandle<HasReynoldsFleeTargetPos> fleeType;
        [ReadOnly] public ComponentTypeHandle<ReynoldsFleeSafeDistance> safeDistType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Translation> transArray = chunk.GetNativeArray(translationType);
            NativeArray<ReynoldsMovementValues> movementArray = chunk.GetNativeArray(reynoldsMovementValuesType);
            NativeArray<HasReynoldsFleeTargetPos> fleeTargetArray = chunk.GetNativeArray(fleeType);
            NativeArray<ReynoldsFleeSafeDistance> safeDistArray = chunk.GetNativeArray(safeDistType);

            for(int i = 0; i < chunk.Count; i++){   
                Translation trans = transArray[i];
                ReynoldsMovementValues movement = movementArray[i];
                HasReynoldsFleeTargetPos targetPos = fleeTargetArray[i];
                ReynoldsFleeSafeDistance safeDist = safeDistArray[i];

                float3 move = trans.Value - targetPos.targetPos; // from the target to the agent
                if(math.distance(targetPos.targetPos, trans.Value) < safeDist.safeDistance){
                    //get a vector from target through the agent to the safe distance (a point in the safe distance sphere in the same direction as the direction from target to agent)
                    move = (math.normalize(move) * safeDist.safeDistance)
                            - trans.Value; // then get the vector from the agent to the point on the safe distance sphere
                            // this makes the flee movement greater the closer the agent is to the flee target
                    movementArray[i] = new ReynoldsMovementValues{
                        flockMovement = movement.flockMovement,
                        seekMovement = movement.seekMovement,
                        fleeMovement = move
                    };
                }
                else{
                    movementArray[i] = new ReynoldsMovementValues{
                        flockMovement = movement.flockMovement,
                        seekMovement = movement.seekMovement,
                        fleeMovement = float3.zero
                    };
                }
            }
        }
    }

    protected override void OnCreate() {
        fleeQueryDec = new EntityQueryDesc{
            All = new ComponentType[]{
                ComponentType.ReadOnly<Translation>(),
                typeof(ReynoldsMovementValues),
                ComponentType.ReadOnly<HasReynoldsFleeTargetPos>(),
                ComponentType.ReadOnly<ReynoldsFleeSafeDistance>(),
            }
        };
        base.OnCreate();
    }

    protected override void OnUpdate(){
        EntityQuery fleeQuery = GetEntityQuery(fleeQueryDec); // query the entities

        FleeBehaviourJob fleeBehaviourJob = new FleeBehaviourJob{
            translationType = GetComponentTypeHandle<Translation>(true),
            reynoldsMovementValuesType = GetComponentTypeHandle<ReynoldsMovementValues>(),
            fleeType = GetComponentTypeHandle<HasReynoldsFleeTargetPos>(true),
            safeDistType = GetComponentTypeHandle<ReynoldsFleeSafeDistance>(true)
        };
        JobHandle jobHandle = fleeBehaviourJob.Schedule(fleeQuery, this.Dependency);

        this.Dependency = jobHandle;
    }
}
