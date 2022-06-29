using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;

public class ReynoldsMovementSystem : SystemBase
{
    protected override void OnUpdate(){
        float deltaTime = Time.DeltaTime;

        JobHandle jobHandle = Entities.ForEach((Entity crowd, ref Translation transl, ref PreviousMovement prevMovement, in ReynoldsMovementValues movement, in ReynoldsBehaviourWeights behaviour)=>{
            float3 result = float3.zero;
            if(math.distance(movement.fleeMovement,float3.zero) > behaviour.fleeWeight){ //if the flee movement is larger than its weight (its cap)
                result += math.normalize(movement.fleeMovement) * behaviour.fleeWeight; // scale the movement to the weight, then add it to the overall movement
            }
            else{
                result += movement.fleeMovement;
            }
            if(math.distance(movement.seekMovement,float3.zero) > behaviour.seekWeight){ //if the seek movement is larger than its weight (its cap)
                result += math.normalize(movement.seekMovement) * behaviour.seekWeight; // scale the movement to the weight, then add it to the overall movement
            }
            else{
                result += movement.seekMovement;
            }
            //Debug.Log("Flock: " + movement.flockMovement);
            if(math.distance(movement.flockMovement,float3.zero) > behaviour.flockWeight){ //if the flock movement is larger than its weight (its cap)
                result += math.normalize(movement.flockMovement) * behaviour.flockWeight; // scale the movement to the weight, then add it to the overall movement
            }
            else{
                result += movement.flockMovement;
            }
            if(math.distance(movement.obstacleAvoidanceMovement, float3.zero) > behaviour.obstacleAvoidanceWeight) //if the obstacle avoidance movement is larger than its weight (its cap)
            {
                result += math.normalize(movement.obstacleAvoidanceMovement) * behaviour.obstacleAvoidanceWeight; // scale the movement to the weight, then add it to the overall movement
            }
            else
            {
                result += movement.obstacleAvoidanceMovement;
            }
            if(math.distance(result,float3.zero) > behaviour.maxVelocity){ // if the overall movement is longer than the maxVelocity
                result = math.normalize(result) * behaviour.maxVelocity; // scale the overall movement to the maxSpeed (keeping the direction)
            }
            result.y = 0f;
            //Debug.Log("Vector: " + result);
            transl.Value += result * deltaTime; //add movement to the translation 
            
               
            prevMovement.value = result * deltaTime;
        }).Schedule(this.Dependency);

        this.Dependency = jobHandle;
    }
}
