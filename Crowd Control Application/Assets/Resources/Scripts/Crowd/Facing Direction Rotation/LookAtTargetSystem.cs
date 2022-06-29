using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;

public class LookAtTargetSystem : SystemBase
{
    private EntityQueryDesc lookQueryDec;

    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    [BurstCompile]
    private struct LookAtTargetJob : IJobChunk
    {
        [ReadOnly] public ComponentTypeHandle<Translation> translationType;
        public ComponentTypeHandle<Rotation> rotationType;
        [ReadOnly] public ComponentTypeHandle<LookAtTarget> lookType;
        [ReadOnly] public ComponentTypeHandle<HasReynoldsSeekTargetPos> seekType;

        public float deltaTime;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Translation> transArray = chunk.GetNativeArray(translationType);
            NativeArray<Rotation> rotArray = chunk.GetNativeArray(rotationType);
            NativeArray<LookAtTarget> lookArray = chunk.GetNativeArray(lookType);
            NativeArray<HasReynoldsSeekTargetPos> seekTargetArray = chunk.GetNativeArray(seekType);

            

            for (int i = 0; i < chunk.Count; i++)
            {
                Translation trans = transArray[i];
                Rotation rot = rotArray[i];
                LookAtTarget look = lookArray[i];
                HasReynoldsSeekTargetPos targetPos = seekTargetArray[i];

                float3 targetDirection = targetPos.targetPos - trans.Value;
                targetDirection.y = 0f;

                float rotationSpeed = deltaTime * look.rotationSpeed;
                float3 up = new float3(0f, 1f, 0f);
                quaternion calcRot = math.slerp(rot.Value, quaternion.LookRotationSafe(targetDirection, up), rotationSpeed);


                rotArray[i] = new Rotation
                {
                    Value = calcRot
                };

                
            }
        }
    }

    protected override void OnCreate()
    {
        lookQueryDec = new EntityQueryDesc
        {
            All = new ComponentType[]{
                ComponentType.ReadOnly<Translation>(),
                typeof(Rotation),
                ComponentType.ReadOnly<LookAtTarget>(),
                ComponentType.ReadOnly<HasReynoldsSeekTargetPos>()
            }
        };
        base.OnCreate();
    }

    protected override void OnUpdate()
    {
        EntityQuery lookQuery = GetEntityQuery(lookQueryDec); // query the entities

        LookAtTargetJob lookAtTargetJob = new LookAtTargetJob
        {
            translationType = GetComponentTypeHandle<Translation>(true),
            rotationType = GetComponentTypeHandle<Rotation>(),
            lookType = GetComponentTypeHandle<LookAtTarget>(true),
            seekType = GetComponentTypeHandle<HasReynoldsSeekTargetPos>(true),
            deltaTime = Time.DeltaTime
        };
        JobHandle jobHandle = lookAtTargetJob.Schedule(lookQuery, this.Dependency);
        this.Dependency = jobHandle;
    }
}