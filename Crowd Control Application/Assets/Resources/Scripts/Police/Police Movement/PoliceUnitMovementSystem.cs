using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;

// Moves all selected police units to the given location
public class PoliceUnitMovementSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else
    private static float moveTolerance = 0.1f;
    private static float rotTolerance = 10f;

    protected override void OnCreate(){
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps){
        float deltaTime = Time.DeltaTime;
        EntityCommandBuffer.Concurrent commandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent(); // create a command buffer

        JobHandle jobHandle = Entities
            .WithAll<PoliceUnitComponent>()
            .ForEach((Entity policeUnit, int entityInQueryIndex, ref Translation transl, ref Rotation rot, in PoliceUnitMaxSpeed speed, in PoliceUnitRotationSpeed rotSpeed, in PoliceUnitMovementDestination destination)=>{
                float3 direction = math.normalize((destination.Value - transl.Value));
                quaternion turn = quaternion.LookRotationSafe(direction, new float3(0f,1f,0f));
                float angle = math.degrees(AngleBetweenQuaternions(rot.Value,turn));
                Debug.Log("Angle: " + angle);
                if(math.distance(transl.Value,destination.Value) > moveTolerance  || angle > rotTolerance){
                    float3 result = (destination.Value - transl.Value) * deltaTime; // the direction of movement
                    if(math.distance(result,float3.zero) > speed.Value){// if the movement is faster than the max speed, cull the movement to match the max speed
                        result = math.normalize(result) * speed.Value;
                    }
                    transl.Value += result; //add movement to the translation    
                    rot.Value = RotateTowards(rot.Value, transl.Value, destination.Value, rotSpeed.Value * deltaTime);
                }
                else{
                    commandBuffer.RemoveComponent<PoliceUnitMovementDestination>(entityInQueryIndex, policeUnit); // remove the destination
                }          
            }).Schedule(inputDeps);

        commandBufferSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }

    // don't use a speed greater than 1
    // does not rotate upwards
    private static quaternion RotateTowards(quaternion initRotation, float3 fromPos, float3 toPos, float speed = 1){
        float3 start = new float3(fromPos.x,0,fromPos.z);
        float3 end = new float3(toPos.x,0,toPos.z);
        float3 direction = math.normalize((end - start));
        quaternion turn = quaternion.LookRotationSafe(direction, new float3(0f,1f,0f));
        return math.slerp(initRotation, turn, speed);
    }


    // angle from q1 to q2
    // angle is always positive
    // angle is in radians (use math.degrees() to get degrees)
    private static float AngleBetweenQuaternions(quaternion q1, quaternion q2){
        quaternion difference = math.mul(q1,math.inverse(q2)); // the angle between q1 and q2
        float angle = 2 * math.acos(difference.value[3]); // angle in rads
        return angle;

    }
}

