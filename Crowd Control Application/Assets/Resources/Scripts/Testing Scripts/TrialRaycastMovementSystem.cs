using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Physics;
using Unity.Physics.Systems;

public class TrialRaycastMovementSystem : SystemBase
{
    private EntityQuery query; // store entity query
    //private EntityQueryDesc queryDesc;
    private BuildPhysicsWorld m_BuildPhysicsWorldSystem;
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else
    private int m_raysPerAgent = 10;
    private int m_visionAngle = 180;
    private float m_movePerRay = 0.5f;

    //[BurstCompile]
    /*private struct RaycastingJob : IJobChunk {
        public EntityCommandBuffer.ParallelWriter commandBuffer;

        [ReadOnly] public EntityTypeHandle entityType;
        [ReadOnly] public ComponentTypeHandle<Translation> translationType;
        [ReadOnly] public ComponentTypeHandle<Rotation> rotationType;

        public BuildPhysicsWorld physicsWorldSystem;

        public int raysPerAgent;
        public int visionAngle;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            NativeArray<Translation> transArray = chunk.GetNativeArray(translationType);
            NativeArray<Rotation> rotArray = chunk.GetNativeArray(rotationType);

            

            //BuildPhysicsWorld physicsWorldSystem = World.GetExistingSystem<BuildPhysicsWorld>();

            for(int i = 0; i < chunk.Count; i++){
                Entity entity = entityArray[i];
                Translation trans = transArray[i];
                Rotation rot = rotArray[i];


                float3 origin = trans.Value;
                float3 direction = new float3(1, 0, 0); // this value would change depending on what direction is 'forward' for the agent
                //Entity hitEntity = Raycast(origin, (origin+direction*100));

                //var physicsWorldSystem = World.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();
                var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;
                uint collideBitmask = 1 << 1;

                //
                quaternion leftmostRotation = quaternion.RotateY(math.radians(-visionAngle / 2));
                quaternion angleBetweenRays;// = quaternion.RotateY(math.radians(visionAngle / raysPerAgent));

                float3 leftmostRay = math.mul(leftmostRotation, direction);

                for (int j = 0; j < raysPerAgent; j++)
                {
                    int n = j + 1; // store j + 1 because we reference that a lot
                    float angle = j * math.radians(visionAngle / (raysPerAgent - 1));

                    Debug.Log(string.Format("Angle for ray {0}: {1}",
                       n, angle));
                    angleBetweenRays = quaternion.RotateY(angle);
                    //Debug.Log(string.Format("Ray {0}'s angle = {1}",
                    //    j, angleBetweenRays));
                    direction = math.mul(angleBetweenRays, leftmostRay);
                    Debug.Log(string.Format("Ray {0}'s direction = {1}",
                        n, direction));
                    RaycastInput input = new RaycastInput()
                    {
                        Start = origin,
                        End = (origin + direction * 100),
                        Filter = new CollisionFilter()
                        {
                            BelongsTo = ~0u,
                            CollidesWith = collideBitmask,//~0u, // all 1s, so all layers, collide with everything
                            GroupIndex = 0
                        }
                    };

                    Unity.Physics.RaycastHit hit = new Unity.Physics.RaycastHit();
                    Debug.Log(string.Format("Firing ray number {0}",
                        n));
                    bool haveHit = collisionWorld.CastRay(input, out hit);
                    if (haveHit)
                    {
                        Debug.Log(string.Format("Ray number {0} hit something!",
                        n));
                    }
                    else
                    {
                        Debug.Log(string.Format("Ray number {0} did not hit anything...",
                        n));
                    }
                }


                // Fire Some Rays
                // For loop for each laser
                // Calculation of the angle to fire
                //FIRE
                // If there was a hit, do the calculation for the movement
                //hitEntity
            }
        }*/
        /*public Entity Raycast(float3 RayFrom, float3 RayTo, BuildPhysicsWorld physicsWorldSystem)
        {
            //var physicsWorldSystem = World.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();
            var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;
            RaycastInput input = new RaycastInput()
            {
                Start = RayFrom,
                End = RayTo,
                Filter = new CollisionFilter()
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1 << 1, // want to collide with only buildings,buildings are on layer 1
                                           // So do 1 << 1 bitshift to say 'only hit buildings'
                    GroupIndex = 0
                }
            };

            Unity.Physics.RaycastHit hit = new Unity.Physics.RaycastHit();
            bool haveHit = collisionWorld.CastRay(input, out hit);
            if (haveHit)
            {
                // see hit.Position
                // see hit.SurfaceNormal
                Entity e = physicsWorldSystem.PhysicsWorld.Bodies[hit.RigidBodyIndex].Entity;
                return e;
            }
            return Entity.Null;
        }*/
    //}
    protected override void OnCreate()
    {
        /*queryDesc = new EntityQueryDesc
        {
            All = new ComponentType[]{
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<Rotation>(),
                ComponentType.ReadOnly<RaycasterComponent>()
                //Should Add in crowd agent too
            }
        };*/
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        m_BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
        base.OnCreate();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.ParallelWriter commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(); // create a command buffer

        /*EntityQuery rcQuery = GetEntityQuery(queryDesc); // query the entities

        RaycastingJob rcJob = new RaycastingJob
        {
            commandBuffer = commandBuffer,
            entityType = GetEntityTypeHandle(),
            translationType = GetComponentTypeHandle<Translation>(true),
            rotationType = GetComponentTypeHandle<Rotation>(true),

            physicsWorldSystem = m_BuildPhysicsWorldSystem.,

            raysPerAgent = raysPerAgent,
            visionAngle = visionAngle
    
        };
        JobHandle jobHandle = rcJob.Schedule(rcQuery, this.Dependency);

        this.Dependency = jobHandle;*/



        //Stuff from first tutorial
        /*int dataCount = query.CalculateEntityCount();
        
        //Storage of data between jobs
        NativeArray<UnityEngine.RaycastHit> results = new NativeArray<UnityEngine.RaycastHit>(dataCount , Allocator.TempJob);
        NativeArray<RaycastCommand> raycastCommand = new NativeArray<RaycastCommand>(dataCount, Allocator.TempJob);
        // Would need to add an accessor 

        //build raycast commands based on selected entities
        JobHandle jobHandle = Entities.WithStoreEntityQueryInField(ref query)
            .ForEach((Entity entity, int entityInQueryIndex, in Translation translation, in Rotation rotation, in RaycasterComponent raycasterComp)=>{
                Vector3 origin = (Vector3)translation.Value;
                Vector3 direction = new Vector3(1,0,0);
                raycastCommand[entityInQueryIndex] = new RaycastCommand(origin, direction);
            }).ScheduleParallel(Dependency);
        
        JobHandle handle = RaycastCommand.ScheduleBatch(raycastCommand, results, results.Length, jobHandle);

        //Do the raycasts since we need that info to do the rest
        handle.Complete();

        for(int i = 0; i < dataCount; i++){
            if(results[i].collider != null){
                Debug.Log("Hit something!!");
            }
        }*/

        int visionAngle = m_visionAngle;
        int raysPerAgent = m_raysPerAgent;
        var physicsWorldSystem = World.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();
        var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;
        //JobHandle jobHandle =
        Entities.WithStoreEntityQueryInField(ref query).WithoutBurst() // TODO: REMOVE .WithoutBurst() when we are done debugging
            .ForEach((Entity entity, int entityInQueryIndex, in Translation translation, in Rotation rotation, in RaycasterComponent raycasterComp) =>
            {
                float3 origin = translation.Value;
                float3 direction = new float3(1, 0, 0); // this value would change depending on what direction is 'forward' for the agent
                //Entity hitEntity = Raycast(origin, (origin+direction*100));

                //var physicsWorldSystem = World.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();
                //var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;
                uint collideBitmask = 1 << 1;

                //
                quaternion leftmostRotation = quaternion.RotateY(math.radians(-visionAngle / 2));
                quaternion angleBetweenRays;// = quaternion.RotateY(math.radians(visionAngle / raysPerAgent));

                float3 leftmostRay = math.mul(leftmostRotation, direction);

                float3 resultingMovement = float3.zero;

                for (int i = 0; i < raysPerAgent; i++)
                {
                    float angle = i * math.radians(visionAngle / (raysPerAgent - 1));

                    /*Debug.Log(string.Format("Angle for ray {0}: {1}",
                       i + 1, angle));*/
                    angleBetweenRays = quaternion.RotateY(angle);
                    //Debug.Log(string.Format("Ray {0}'s angle = {1}",
                    //    i, angleBetweenRays));
                    direction = math.mul(angleBetweenRays, leftmostRay);
                    /*Debug.Log(string.Format("Ray {0}'s direction = {1}",
                        i + 1, direction));*/
                    RaycastInput input = new RaycastInput()
                    {
                        Start = origin,
                        End = (origin + direction * 3),
                        Filter = new CollisionFilter()
                        {
                            BelongsTo = ~0u,
                            CollidesWith = collideBitmask,//~0u, // all 1s, so all layers, collide with everything
                            GroupIndex = 0
                        }
                    };

                    Unity.Physics.RaycastHit hit = new Unity.Physics.RaycastHit();
                    /*Debug.Log(string.Format("Firing ray number {0}",
                        i + 1));*/
                    bool haveHit = collisionWorld.CastRay(input, out hit);
                    if (haveHit)
                    {
                        
                        Debug.Log(string.Format("Ray number {0} hit something!",
                        i + 1));
                        Debug.Log("hit something!");
                        Debug.Log("How far along: " + hit.Fraction);
                    }
                    else
                    {
                        /*Debug.Log(string.Format("Ray number {0} did not hit anything...",
                        i + 1));*/
                        Debug.Log("Didn't hit anything...");
                    }
                }
            }).Schedule(Dependency).Complete();//.ScheduleParallel(Dependency).Complete();
    }
    
}
