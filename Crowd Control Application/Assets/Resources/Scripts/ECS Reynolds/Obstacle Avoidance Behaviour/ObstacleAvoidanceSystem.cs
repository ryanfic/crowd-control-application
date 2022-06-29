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


public class ObstacleAvoidanceSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var physicsWorldSystem = World.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();
        var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;
        Entities.WithBurst()/*.WithoutBurst()*/ // TODO: REMOVE .WithoutBurst() when we are done debugging
            .ForEach((Entity entity, int entityInQueryIndex, ref ReynoldsMovementValues movementValues, in Translation translation, in Rotation rotation, in ObstacleAvoidanceMovementComponent obstacleAvoidanceComponent) =>
            {
                float3 origin = translation.Value;
                float3 direction = new float3(0, 0, 1); // this value would change depending on what direction is 'forward' for the agent
                direction = math.mul(rotation.Value, direction);

                uint collideBitmask = 1 << 1; // To collide with Buildings (layer 1, so bitshift once)

                quaternion leftmostRotation = quaternion.RotateY(math.radians(-obstacleAvoidanceComponent.visionAngle / 2));
                quaternion angleBetweenRays;// = quaternion.RotateY(math.radians(visionAngle / raysPerAgent));

                float3 leftmostRay = math.mul(leftmostRotation, direction);

                float3 resultingMovement = float3.zero;
                bool hitOccurred = false;
                

                float3 firstNoHitVector = float3.zero; // Should make this backwards
                bool foundFirstNoHitVector = false;

                int maxNoHitRayNum = -1;
                int minNoHitRayNum = -1;

                int multiplier = -1;
                //if have left tendency, multiply count by -1
                int rayNumber = (obstacleAvoidanceComponent.numberOfRays - 1) / 2;
                int midRayNumber = rayNumber;
                //string resultString = "";

                for (int i = 0; i < obstacleAvoidanceComponent.numberOfRays; i++, multiplier *= -1)
                {
                    float angle = rayNumber * math.radians(obstacleAvoidanceComponent.visionAngle / (obstacleAvoidanceComponent.numberOfRays - 1));

                    //Debug.Log(string.Format("Angle for ray {0}: {1}",
                    //   i + 1, angle));
                    angleBetweenRays = quaternion.RotateY(angle);
                    //Debug.Log(string.Format("Ray {0}'s angle = {1}",
                    //    i, angleBetweenRays));
                    direction = math.mul(angleBetweenRays, leftmostRay);
                    //Debug.Log(string.Format("Ray {0}'s direction = {1}",
                    //    i + 1, direction));
                    RaycastInput input = new RaycastInput()
                    {
                        Start = origin,
                        End = (origin + direction * obstacleAvoidanceComponent.visionLength),
                        Filter = new CollisionFilter()
                        {
                            BelongsTo = ~0u,
                            CollidesWith = collideBitmask,//~0u, // all 1s, so all layers, collide with everything
                            GroupIndex = 0
                        }
                    };

                    Unity.Physics.RaycastHit hit = new Unity.Physics.RaycastHit();
                    //Debug.Log(string.Format("Firing ray number {0}",
                    //    i + 1));
                    bool haveHit = collisionWorld.CastRay(input, out hit);
                    if (haveHit)
                    {
                        //Debug.Log("Ray number " + (rayNumber + 1) + " hit something");
                        //Debug.Log("Ray number " + (i+1) + " hit something. How far along: " + hit.Fraction);
                            
                        hitOccurred = true;
                        
                    }
                    else // If a hit has occurred
                    {
                        if (!foundFirstNoHitVector)
                        {
                            firstNoHitVector = input.End-input.Start;
                            foundFirstNoHitVector = true;
                            resultingMovement = obstacleAvoidanceComponent.movementPerRay * math.normalize(input.End - input.Start);
                            minNoHitRayNum = rayNumber;
                            maxNoHitRayNum = rayNumber;
                        }
                        else
                        {
                            if(rayNumber > midRayNumber) // the ray is to the right
                            {
                                if(rayNumber - maxNoHitRayNum == 1) // if this ray is next to the max no hit ray
                                {
                                    maxNoHitRayNum = rayNumber;// this ray is the new max no hit ray
                                    resultingMovement += obstacleAvoidanceComponent.movementPerRay * math.normalize(input.End - input.Start);
                                }
                                
                            }
                            else // the ray is to the left
                            {
                                if (minNoHitRayNum - rayNumber == 1) // if this ray is next to the min no hit ray
                                {
                                    minNoHitRayNum = rayNumber;// this ray is the new max no hit ray
                                    resultingMovement += obstacleAvoidanceComponent.movementPerRay * math.normalize(input.End - input.Start);
                                }
                            }
                        }
                    }

                    //Debug.Log("Max ray:" + maxNoHitRayNum + ", Min ray: " + minNoHitRayNum);
                    // Calculate the next ray number
                    rayNumber += multiplier * i;
                    //resultString += rayNumber + " ";
                }
                //Debug.Log(resultString);


                if (hitOccurred) // if there was at least one hit
                {
                    //Debug.Log("Some hit occurred!");
                    //if (math.distance(resultingMovement, float3.zero) > 0.1f)// if the resulting movement is over some threshold
                    //{
                    //    Debug.Log("AND WE CHANGED THE OBSTACLE AVOIDANCE");
                    //    movementValues.obstacleAvoidanceMovement = resultingMovement;
                    //}

                    movementValues.obstacleAvoidanceMovement = new float3(resultingMovement.x, 0, resultingMovement.z);//firstNoHitVector;
                }
                else
                {
                    movementValues.obstacleAvoidanceMovement = float3.zero;
                }

            }).Schedule(Dependency).Complete();//.ScheduleParallel(Dependency).Complete();
    }
}
