using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;

public class ReynoldsFleeSystem : JobComponentSystem
{
    [BurstCompile]
    private struct FleeBehaviourJob : IJobForEachWithEntity<Translation,ReynoldsFleeMovement,HasReynoldsFleeTargetPos,ReynoldsFleeSafeDistance>{
        public void Execute(Entity entity, int index, ref Translation trans, ref ReynoldsFleeMovement fleeMovement, [ReadOnly] ref HasReynoldsFleeTargetPos targetPos, [ReadOnly] ref ReynoldsFleeSafeDistance safeDist){
            float3 move = trans.Value - targetPos.targetPos; // from the target to the agent
            if(math.distance(targetPos.targetPos, trans.Value) < safeDist.safeDistance){
                //get a vector from target through the agent to the safe distance (a point in the safe distance sphere in the same direction as the direction from target to agent)
                move = (math.normalize(move) * safeDist.safeDistance)
                        - trans.Value; // then get the vector from the agent to the point on the safe distance sphere
                        // this makes the flee movement greater the closer the agent is to the flee target
                fleeMovement.movement = move;
            }
            else{
                fleeMovement.movement = float3.zero;
            }
            
        }
    }
     protected override JobHandle OnUpdate(JobHandle inputDeps){
          FleeBehaviourJob fleeBehaviourJob = new FleeBehaviourJob{};
          JobHandle jobHandle = fleeBehaviourJob.Schedule(this, inputDeps);
          return jobHandle;
     }
}
