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

public class RotationTrialSystem : SystemBase
{
    private EntityQuery query; // store entity query
    private BuildPhysicsWorld m_BuildPhysicsWorldSystem;
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else
    private int raysPerAgent = 10;
    private int visionAngle = 180;
    private float rotationAngle = 2f;
    protected override void OnCreate()
    {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        m_BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
    }

    protected override void OnUpdate()
    {
        float rotAngle = rotationAngle;
        EntityCommandBuffer commandBuffer = commandBufferSystem.CreateCommandBuffer();
        bool aPressed = Input.GetKey(KeyCode.A);
        bool dPressed = Input.GetKey(KeyCode.D);
        bool leftPressed = Input.GetKey(KeyCode.LeftArrow);
        bool rightPressed = Input.GetKey(KeyCode.RightArrow);

        // now it is not - that's part of the system's code
        Entities.ForEach((Entity entity, ref Translation translation, ref Rotation rotation, in RotationTrialComponent rotater) =>
        {
            float3 tmpTranslation = translation.Value;
            quaternion tmpRotation = rotation.Value;
            
            if (aPressed){
                // this always set the same angle
                quaternion leftRotation = quaternion.RotateY(math.radians(-rotAngle)); 
                tmpRotation = math.mul(tmpRotation,leftRotation);
                Debug.Log("a pressed");
            }
            else if(dPressed){
                // this always set the same angle
                quaternion rightRotation = quaternion.RotateY(math.radians(rotAngle)); 
                tmpRotation = math.mul(tmpRotation,rightRotation);
                Debug.Log("d pressed");
            }

            //Debug.Log(string.Format("tmpRotation is {0}",
            //    tmpRotation.ToString()));
            
        

            // ECS need this   
            var newRotation = new Rotation { Value = tmpRotation };
            commandBuffer.SetComponent<Rotation>(entity, newRotation);
            var newTranslation = new Translation { Value = tmpTranslation };
            commandBuffer.SetComponent<Translation>(entity, newTranslation);
        }).Run();
    }
    
}

