using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;

public class ReynoldsMovementSystem : JobComponentSystem
{
    //[SerializeField] float movementSpeed = 5f;
    //[SerializeField] float tolerance = 0.2f;

    protected override JobHandle OnUpdate(JobHandle inputDeps){
        float deltaTime = Time.DeltaTime;
        //float moveSpeed = movementSpeed;
        // float3 resultingMovement


        /*JobHandle jobHandle = Entities.ForEach((Entity crowd, ref Translation transl, in ReynoldsFlockMovement flockMovement)=>{
            transl.Value += flockMovement.movement * moveSpeed * deltaTime; //add movement to the translation            
        }).Schedule(inputDeps);*/

        /*JobHandle jobHandle = Entities.ForEach((Entity crowd, ref Translation transl, in ReynoldsSeekMovement seekMovement)=>{
            transl.Value += seekMovement.movement * moveSpeed * deltaTime; //add movement to the translation            
        }).Schedule(inputDeps);*/

        /*JobHandle jobHandle = Entities.ForEach((Entity crowd, ref Translation transl, in ReynoldsFleeMovement fleeMovement)=>{
            transl.Value += fleeMovement.movement * moveSpeed * deltaTime; //add movement to the translation            
        }).Schedule(inputDeps);*/

        /*JobHandle jobHandle1 = Entities.ForEach((Entity crowd, ref Translation transl, in ReynoldsFlockMovement flockMovement, in ReynoldsSeekMovement seekMovement, in ReynoldsFleeMovement fleeMovement, in ReynoldsBehaviourData behaviour)=>{
            float3 result = float3.zero;//= fleeMovement.movement * behaviour.fleeWeight + seekMovement.movement * behaviour.seekWeight + flockMovement.movement * behaviour.flockWeight;
            if(math.distance(fleeMovement.movement,float3.zero) > behaviour.fleeWeight){ //if the flee movement is larger than its weight (its cap)
                result += math.normalize(fleeMovement.movement) * behaviour.fleeWeight; // scale the movement to the weight, then add it to the overall movement
            }
            else{
                result += fleeMovement.movement;
            }
            if(math.distance(seekMovement.movement,float3.zero) > behaviour.seekWeight){ //if the seek movement is larger than its weight (its cap)
                result += math.normalize(seekMovement.movement) * behaviour.seekWeight; // scale the movement to the weight, then add it to the overall movement
            }
            else{
                result += seekMovement.movement;
            }
            if(math.distance(flockMovement.movement,float3.zero) > behaviour.flockWeight){ //if the flock movement is larger than its weight (its cap)
                result += math.normalize(flockMovement.movement) * behaviour.flockWeight; // scale the movement to the weight, then add it to the overall movement
            }
            else{
                result += flockMovement.movement;
            }
            if(math.distance(result,float3.zero) > behaviour.maxVelocity){ // if the overall movement is longer than the maxVelocity
                result = math.normalize(result) * behaviour.maxVelocity; // scale the overall movement to the maxSpeed (keeping the direction)
            }
            transl.Value += result * deltaTime; //add movement to the translation            
        }).Schedule(inputDeps);*/

        /*jobHandle1 jobHandle2 = Entities.ForEach((Entity crowd, ref Translation transl, in ReynoldsWallAvoidanceMovement wallAvoidMovement, in ReynoldsObstacleAvoidanceMovement obsAvoidMovement)=>{

        }).Schedule(jobHandle1);*/

        JobHandle jobHandle = Entities.ForEach((Entity crowd, ref Translation transl, in ReynoldsMovementValues movement, /*in ReynoldsSeekMovement seekMovement, in ReynoldsFleeMovement fleeMovement,*/ in ReynoldsBehaviourWeights behaviour)=>{
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
            if(math.distance(movement.flockMovement,float3.zero) > behaviour.flockWeight){ //if the flock movement is larger than its weight (its cap)
                result += math.normalize(movement.flockMovement) * behaviour.flockWeight; // scale the movement to the weight, then add it to the overall movement
            }
            else{
                result += movement.flockMovement;
            }
            if(math.distance(result,float3.zero) > behaviour.maxVelocity){ // if the overall movement is longer than the maxVelocity
                result = math.normalize(result) * behaviour.maxVelocity; // scale the overall movement to the maxSpeed (keeping the direction)
            }
            transl.Value += result * deltaTime; //add movement to the translation            
        }).Schedule(inputDeps);

        //return jobHandle2;
        return jobHandle;
    }
}
