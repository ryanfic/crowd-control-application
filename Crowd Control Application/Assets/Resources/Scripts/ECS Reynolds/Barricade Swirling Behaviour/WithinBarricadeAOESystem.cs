using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;



//[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
//[UpdateAfter(typeof(ExportPhysicsWorld))]
[UpdateBefore(typeof(EndFramePhysicsSystem))]
public class EnterBarricadeAOESystem : JobComponentSystem//SystemBase
{
    private BuildPhysicsWorld m_BuildPhysicsWorldSystem;
    private StepPhysicsWorld m_StepPhysicsWorldSystem;

    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else
    protected override void OnCreate()
    {
        base.OnCreate();

        m_BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
        m_StepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }
    [Unity.Burst.BurstCompile]
    struct TriggerWarningJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentDataFromEntity<Crowd> crowdGroup;
        [ReadOnly] public ComponentDataFromEntity<BarricadeAOEComponent> aoeGroup;
        public ComponentDataFromEntity<LeaveBarricadeAOEComponent> leavingGroup;

        public EntityCommandBuffer commandBuffer;
        public double curTime;

        public void Execute(TriggerEvent evt)
        {
            Entity entityA = evt.EntityA;
            Entity entityB = evt.EntityB;

            bool isBodyACrowd = crowdGroup.HasComponent(entityA);
            bool isBodyBCrowd = crowdGroup.HasComponent(entityB);

            bool isBodyAAOE = aoeGroup.HasComponent(entityA);
            bool isBodyBAOE = aoeGroup.HasComponent(entityB);
            

            if (isBodyACrowd && isBodyBAOE)
            {
                bool isLeaving = leavingGroup.HasComponent(entityA);
                if (isLeaving)
                {
                    LeaveBarricadeAOEComponent component = new LeaveBarricadeAOEComponent
                    {
                        lastTriggerCollision = curTime
                    };
                    leavingGroup[entityA] = component;

                    //commandBuffer.AddComponent<LeaveBarricadeAOEComponent>(entityA, component);
                }
                else
                {
                    BarricadeAOEComponent aoeComponent = aoeGroup[entityB];
                    BarricadeSwirlingMovementComponent component = new BarricadeSwirlingMovementComponent
                    {
                        totalRadius = aoeComponent.totalRadius,
                        halfRadius = aoeComponent.halfRadius,
                        aoeCenter = aoeComponent.aoeCenter,
                        lastTriggerCollision = curTime
                    };
                    commandBuffer.AddComponent<BarricadeSwirlingMovementComponent>(entityA, component);
                }
            }
            if (isBodyBCrowd && isBodyAAOE)
            {
                bool isLeaving = leavingGroup.HasComponent(entityB);
                if (isLeaving)
                {
                    LeaveBarricadeAOEComponent component = new LeaveBarricadeAOEComponent
                    {
                        lastTriggerCollision = curTime
                    };
                    leavingGroup[entityB] = component;
                    //commandBuffer.AddComponent<LeaveBarricadeAOEComponent>(entityA, component);
                }
                else
                {
                    BarricadeAOEComponent aoeComponent = aoeGroup[entityA];
                    BarricadeSwirlingMovementComponent component = new BarricadeSwirlingMovementComponent
                    {
                        totalRadius = aoeComponent.totalRadius,
                        halfRadius = aoeComponent.halfRadius,
                        aoeCenter = aoeComponent.aoeCenter,
                        lastTriggerCollision = curTime
                    };
                    commandBuffer.AddComponent<BarricadeSwirlingMovementComponent>(entityB, component);
                }
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        JobHandle jobHandle = new TriggerWarningJob
        {
            crowdGroup = GetComponentDataFromEntity<Crowd>(true),
            aoeGroup = GetComponentDataFromEntity<BarricadeAOEComponent>(true),
            leavingGroup = GetComponentDataFromEntity<LeaveBarricadeAOEComponent>(),
            commandBuffer = commandBufferSystem.CreateCommandBuffer(),
            curTime = Time.ElapsedTime
        }.Schedule(m_StepPhysicsWorldSystem.Simulation, ref m_BuildPhysicsWorldSystem.PhysicsWorld, inputDeps);
        commandBufferSystem.AddJobHandleForProducer(jobHandle); // tell the system to execute the command buffer after the job has been completed
        return jobHandle;
    }
}
