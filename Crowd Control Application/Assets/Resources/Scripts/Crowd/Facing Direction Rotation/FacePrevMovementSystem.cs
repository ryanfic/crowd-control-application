using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;

public class FacePrevMovementSystem : SystemBase
{
    private EntityQueryDesc faceQueryDec;

    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    [BurstCompile]
    private struct FacePrevMovementJob : IJobChunk
    {
        [ReadOnly] public ComponentTypeHandle<Translation> translationType;
        public ComponentTypeHandle<Rotation> rotationType;
        [ReadOnly] public ComponentTypeHandle<PreviousMovement> prevMoveType;
        [ReadOnly] public ComponentTypeHandle<FacePrevMovement> lookType;

        public float deltaTime;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Translation> transArray = chunk.GetNativeArray(translationType);
            NativeArray<Rotation> rotArray = chunk.GetNativeArray(rotationType);
            NativeArray<PreviousMovement> prevMoveArray = chunk.GetNativeArray(prevMoveType);
            NativeArray<FacePrevMovement> lookArray = chunk.GetNativeArray(lookType);



            for (int i = 0; i < chunk.Count; i++)
            {
                Translation trans = transArray[i];
                Rotation rot = rotArray[i];
                PreviousMovement prevMovement = prevMoveArray[i];
                FacePrevMovement look = lookArray[i];

                float3 targetDirection = prevMovement.value;
                if(math.length(targetDirection) == 0)
                {
                    targetDirection = new float3(0, 0, 1);
                }
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
        faceQueryDec = new EntityQueryDesc
        {
            All = new ComponentType[]{
                ComponentType.ReadOnly<Translation>(),
                typeof(Rotation),
                ComponentType.ReadOnly<PreviousMovement>(),
                ComponentType.ReadOnly<FacePrevMovement>(),
            }
        };
        base.OnCreate();
    }

    protected override void OnUpdate()
    {
        EntityQuery faceQuery = GetEntityQuery(faceQueryDec); // query the entities

        FacePrevMovementJob facePrevMovementJob = new FacePrevMovementJob
        {
            translationType = GetComponentTypeHandle<Translation>(true),
            rotationType = GetComponentTypeHandle<Rotation>(),
            prevMoveType = GetComponentTypeHandle<PreviousMovement>(true),
            lookType = GetComponentTypeHandle<FacePrevMovement>(true),
            deltaTime = Time.DeltaTime
        };
        JobHandle jobHandle = facePrevMovementJob.Schedule(faceQuery, this.Dependency);
        this.Dependency = jobHandle;
    }
}
