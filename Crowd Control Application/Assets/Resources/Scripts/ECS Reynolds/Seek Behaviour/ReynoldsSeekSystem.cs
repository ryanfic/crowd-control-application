using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;

public class ReynoldsSeekSystem : JobComponentSystem
{
    [BurstCompile]
    private struct SeekBehaviourJob : IJobForEachWithEntity<Translation,ReynoldsSeekMovement,HasReynoldsSeekTargetPos>{
        public void Execute(Entity entity, int index, ref Translation trans, ref ReynoldsSeekMovement seekMovement, [ReadOnly] ref HasReynoldsSeekTargetPos targetPos){
            //float maxVectorLength = 5f;
            float3 move = targetPos.targetPos - trans.Value;
            /*if(math.distance(targetPos.targetPos, trans.Value) > maxVectorLength){
                move = math.normalize(move) * maxVectorLength; 
            }*/
            seekMovement.movement = move;
        }
    }
     protected override JobHandle OnUpdate(JobHandle inputDeps){
          SeekBehaviourJob seekBehaviourJob = new SeekBehaviourJob{};
          JobHandle jobHandle = seekBehaviourJob.Schedule(this, inputDeps);
          return jobHandle;
     }
}
