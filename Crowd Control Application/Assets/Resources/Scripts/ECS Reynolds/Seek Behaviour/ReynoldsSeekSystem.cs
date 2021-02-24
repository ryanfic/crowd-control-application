using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;

public class ReynoldsSeekSystem : SystemBase
{
    private EntityQueryDesc seekQueryDec;

    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    [BurstCompile]
    private struct SeekBehaviourJob : IJobChunk {      
        [ReadOnly] public ComponentTypeHandle<Translation> translationType;
        public ComponentTypeHandle<ReynoldsMovementValues> reynoldsMovementValuesType;
        [ReadOnly] public ComponentTypeHandle<HasReynoldsSeekTargetPos> seekType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Translation> transArray = chunk.GetNativeArray(translationType);
            NativeArray<ReynoldsMovementValues> movementArray = chunk.GetNativeArray(reynoldsMovementValuesType);
            NativeArray<HasReynoldsSeekTargetPos> seekTargetArray = chunk.GetNativeArray(seekType);

            for(int i = 0; i < chunk.Count; i++){   
                Translation trans = transArray[i];
                ReynoldsMovementValues movement = movementArray[i];
                HasReynoldsSeekTargetPos targetPos = seekTargetArray[i];
                
                float3 move = targetPos.targetPos - trans.Value;
                
                movementArray[i] = new ReynoldsMovementValues{
                    flockMovement = movement.flockMovement,
                    seekMovement = move,
                    fleeMovement = movement.fleeMovement
                };
            }
        }
    }

    protected override void OnCreate() {
        seekQueryDec = new EntityQueryDesc{
            All = new ComponentType[]{
                ComponentType.ReadOnly<Translation>(),
                typeof(ReynoldsMovementValues),
                ComponentType.ReadOnly<HasReynoldsSeekTargetPos>()
            }
        };
        base.OnCreate();
    }

    protected override void OnUpdate(){
        EntityQuery seekQuery = GetEntityQuery(seekQueryDec); // query the entities

        SeekBehaviourJob seekBehaviourJob = new SeekBehaviourJob{
            translationType = GetComponentTypeHandle<Translation>(true),
            reynoldsMovementValuesType = GetComponentTypeHandle<ReynoldsMovementValues>(),
            seekType = GetComponentTypeHandle<HasReynoldsSeekTargetPos>(true)
        };
        JobHandle jobHandle = seekBehaviourJob.Schedule(seekQuery, this.Dependency);
        this.Dependency = jobHandle;
    }
}
