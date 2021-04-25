using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;

// Moves all selected police units to the given location
public class PoliceUnitMovementSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else
    private static readonly float moveTolerance = 0.1f;
    private static readonly float rotTolerance = 10f;

    protected override void OnCreate(){
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }
    protected override void OnUpdate(){
        float deltaTime = Time.DeltaTime;
        EntityCommandBuffer.ParallelWriter commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(); // create a command buffer

        //Movement of Police Towards a particular destination via Police Unit Movement Destination
        JobHandle destinationJobHandle = Entities
            .WithAll<PoliceUnitComponent>()
            .WithNone<PoliceUnitGettingIntoFormation>()
            .ForEach((Entity policeUnit, int entityInQueryIndex, ref Translation transl, ref Rotation rot, in PoliceUnitMaxSpeed speed, in PoliceUnitRotationSpeed rotSpeed, in PoliceUnitMovementDestination destination)=>{
                float3 direction = math.normalize((destination.Value - transl.Value));
                quaternion turn = quaternion.LookRotationSafe(direction, new float3(0f,1f,0f));
                float angle = math.degrees(AngleBetweenQuaternions(rot.Value,turn));
                //Debug.Log("Angle: " + angle);
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
            })
            .ScheduleParallel(this.Dependency);

        commandBufferSystem.AddJobHandleForProducer(destinationJobHandle);

        //Forward movement of the police unit when the unit has the Police Unit Move Forward Component
        //Z is the forward direction
        JobHandle forwardJobHandle = Entities
            .WithAll<PoliceUnitComponent,PoliceUnitMoveForward>()
            .WithNone<PoliceUnitGettingIntoFormation>()
            .ForEach((Entity policeUnit, int entityInQueryIndex, ref Translation transl, in Rotation rot, in PoliceUnitMaxSpeed speed)=>{ // Update the position of all police units with the move forward tag
                float3 movement = math.forward(rot.Value)*deltaTime; // math.forward gets a unit vector pointing in the direction of a quaternion (the correct direction)
                transl.Value += movement;
            })
            .ScheduleParallel(destinationJobHandle);

        //Rotate a police unit that has the Police Unit Continuous Rotation component
        JobHandle rotateJobHandle = Entities
            .WithAll<PoliceUnitComponent>()
            .WithNone<PoliceUnitGettingIntoFormation>()
            .ForEach((Entity policeUnit, int entityInQueryIndex, ref Rotation rot, ref PoliceUnitContinuousRotation continuousRot, in PoliceUnitRotationSpeed rotSpeed)=>{
                //Check if we are waiting at an angle
                if(continuousRot.WaitingAtAngle){
                    //If so, add time
                    continuousRot.WaitTime += deltaTime;
                    if(continuousRot.WaitTime > 3){ // if finished waiting
                        continuousRot.WaitingAtAngle = false; // say that done finished waiting
                        continuousRot.WaitTime = 0; // reset timer
                    }
                }
                else{//if we are not waiting 
                    float3 destination = new float3(-1,0,0);
                    if(!continuousRot.RotateLeft){ // if we aren't rotating to the left, rotate in the opposite direction (to the right)
                        destination.x = -destination.x;
                    }
                    destination = math.mul(rot.Value,destination); // make the location relative to the current rotation by multiplying the destination by the current rotation
                    rot.Value = RotateTowards(rot.Value, float3.zero, destination, (rotSpeed.Value * deltaTime)/3); //rotate

                    //check if we are near north,south,east or west
                    quaternion north = quaternion.RotateY(0);
                    quaternion east = quaternion.RotateY(math.PI/2);
                    quaternion south = quaternion.RotateY(math.PI);
                    quaternion west = quaternion.RotateY(3*math.PI/2);

                    //Debug.Log("Angle Diff: " + math.degrees());
                    if(continuousRot.LastWaitAngle != 1 // did not wait north last time
                        && (math.degrees(AngleBetweenQuaternions(rot.Value,north)) < 2 
                        || math.degrees(AngleBetweenQuaternions(rot.Value,north)) > 358)){
                            //wait & update last wait direction
                            continuousRot.WaitingAtAngle = true;
                            continuousRot.LastWaitAngle = 1;
                    }
                    else if(continuousRot.LastWaitAngle != 2 // did not wait east last time
                        && (math.degrees(AngleBetweenQuaternions(rot.Value,east)) < 2 
                        || math.degrees(AngleBetweenQuaternions(rot.Value,east)) > 358)){
                            //wait & update last wait direction
                            continuousRot.WaitingAtAngle = true;
                            continuousRot.LastWaitAngle = 2;
                    }
                    else if(continuousRot.LastWaitAngle != 3 // did not wait south last time
                        && (math.degrees(AngleBetweenQuaternions(rot.Value,south)) < 2 
                        || math.degrees(AngleBetweenQuaternions(rot.Value,south)) > 358)){
                            //wait & update last wait direction
                            continuousRot.WaitingAtAngle = true;
                            continuousRot.LastWaitAngle = 3;
                    }
                    else if(continuousRot.LastWaitAngle != 4 // did not wait west last time
                        && (math.degrees(AngleBetweenQuaternions(rot.Value,west)) < 2 
                        || math.degrees(AngleBetweenQuaternions(rot.Value,west)) > 358)){
                            //wait & update last wait direction
                            continuousRot.WaitingAtAngle = true;
                            continuousRot.LastWaitAngle = 4;
                    }
                }
            })
            .ScheduleParallel(forwardJobHandle);

        this.Dependency = rotateJobHandle;
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

