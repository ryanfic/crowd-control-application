using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;

public class RemoveAOEComponentSystem : SystemBase
{
    private static readonly float timeToRemove = 1f;
    private static readonly float anglePercent = 0.75f;
    private static readonly float exitAngle = 10;
    private EntityQueryDesc removeSwirlQueryDesc;
    private EntityQueryDesc removeLeavingQueryDesc;

    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    //[BurstCompile]
    private struct RemoveSwirlJob : IJobChunk
    {
        [ReadOnly] public EntityTypeHandle entityType;
        [ReadOnly] public ComponentTypeHandle<BarricadeSwirlingMovementComponent> swirlType;
        public EntityCommandBuffer.ParallelWriter commandBuffer;

        public double curTime;


        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            NativeArray<BarricadeSwirlingMovementComponent> swirlDataArray = chunk.GetNativeArray(swirlType);

            for (int i = 0; i < chunk.Count; i++)
            {
                Entity entity = entityArray[i];
                BarricadeSwirlingMovementComponent swirlData = swirlDataArray[i];

                if(timeToRemove < curTime - swirlData.lastTriggerCollision)
                {
                    commandBuffer.RemoveComponent<BarricadeSwirlingMovementComponent>(chunkIndex, entity);
                }

            }
        }
    }

    //[BurstCompile]
    private struct RemoveLeavingJob : IJobChunk
    {
        [ReadOnly] public EntityTypeHandle entityType;
        [ReadOnly] public ComponentTypeHandle<LeaveBarricadeAOEComponent> leaveType;
        public EntityCommandBuffer.ParallelWriter commandBuffer;

        public double curTime;


        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            NativeArray<LeaveBarricadeAOEComponent> leaveDataArray = chunk.GetNativeArray(leaveType);

            for (int i = 0; i < chunk.Count; i++)
            {
                Entity entity = entityArray[i];
                LeaveBarricadeAOEComponent leaveData = leaveDataArray[i];

                if (timeToRemove < curTime - leaveData.lastTriggerCollision)
                {
                    commandBuffer.RemoveComponent<BarricadeSwirlingMovementComponent>(chunkIndex, entity);
                }

            }
        }
    }
    protected override void OnCreate()
    {
        removeSwirlQueryDesc = new EntityQueryDesc
        {
            All = new ComponentType[]{
                ComponentType.ReadOnly<BarricadeSwirlingMovementComponent>()
            }
        };
        removeLeavingQueryDesc = new EntityQueryDesc
        {
            All = new ComponentType[]{
                ComponentType.ReadOnly<LeaveBarricadeAOEComponent>()
            }
        };
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }

    protected override void OnUpdate()
    {
        EntityQuery swirlQuery = GetEntityQuery(removeSwirlQueryDesc); // query the entities

        RemoveSwirlJob removeSwirlJob = new RemoveSwirlJob
        {
            entityType = GetEntityTypeHandle(),
            swirlType = GetComponentTypeHandle<BarricadeSwirlingMovementComponent>(true),
            commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            curTime = Time.ElapsedTime,
        };
        JobHandle jobHandle1 = removeSwirlJob.Schedule(swirlQuery, this.Dependency);

        RemoveLeavingJob removeLeavingJob = new RemoveLeavingJob
        {
            entityType = GetEntityTypeHandle(),
            leaveType = GetComponentTypeHandle<LeaveBarricadeAOEComponent>(true),
            commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            curTime = Time.ElapsedTime,
        };
        JobHandle jobHandle2 = removeSwirlJob.Schedule(swirlQuery, jobHandle1);
        jobHandle2.Complete();
        //this.Dependency = jobHandle2;
    }
}

