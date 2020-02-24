using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

/*
    A file that is attached to game objects to add the Reynolds Behaviours (implemented in DOTS) to the Entity.
*/
public class ReynoldsBehaviourAuthoring : MonoBehaviour, IConvertGameObjectToEntity 
{
    [Range(0,100)]
    public float maxVelocity = 0f;

    /*
        If behaviours are exhibited in the agent
    */
    public bool flocking = false; // if the agent has the flocking behaviour
    public bool fleeing = false; // if the agent has the fleeing behaviour
    public bool seeking = false; // if the agent has the seeking behaviour

    /*
        Behaviour Weights
    */

    [Range(0,100)]
    public float flockingWeight = 0f; // how much the flocking behaviour affects movement
    [Range(0,100)]
    public float fleeingWeight = 0f; // how much the fleeing behaviour affects movement
    [Range(0,100)]
    public float seekingWeight = 0f; // how much the seeking behaviour affects movement
    
    /*
        Data used for Flocking
    */
    public float avoidanceRadius = 0f; // for flocking
    [Range(0,100)]
    public float avoidanceWeight = 0f; // for flocking
    public float cohesionRadius = 0f; // for flocking
    [Range(0,100)]
    public float cohesionWeight = 0f; // for flocking
    
    /*
        Data used for Fleeing
    */

    public float3 fleeTargetPos = float3.zero; // where the agent flees from
    public float fleeSafeDistance = 0f; // how far away the agent accepts as being safe from their flee target

    /*
        Data used for Seeking
    */

    public float3 seekTargetPos = float3.zero; // where the agent move towards


 
    public void Convert(Entity entity, EntityManager eManager, GameObjectConversionSystem conversionSystem){
        if(flocking){ // if the agent has the flocking behaviour
            DynamicBuffer<ReynoldsNearbyFlockPos> dynamicBuffer = eManager.AddBuffer<ReynoldsNearbyFlockPos>(entity); // add the nearby flock positioin buffer to the entity
            eManager.AddComponent<ReynoldsFlockBehaviour>(entity); // Add a Reynolds Flock Behaviour to the entity
            eManager.SetComponentData(entity,new ReynoldsFlockBehaviour{  // Set the Reynolds Flock Behaviour Data
                AvoidanceRadius = this.avoidanceRadius,
                AvoidanceWeight = this.avoidanceWeight,
                CohesionRadius = this.cohesionRadius,
                CohesionWeight = this.cohesionWeight});
            eManager.AddComponent<QuadrantEntity>(entity);// add the quadrant entity component, used to sort the agent for finding nearby agents
            eManager.SetComponentData(entity, new QuadrantEntity{ typeEnum = QuadrantEntity.TypeEnum.Crowd}); //set the type of entity
        }

        if(fleeing){ // if the agent has the fleeing behaviour
            eManager.AddComponent<HasReynoldsFleeTargetPos>(entity);// add the Reynolds Flee Behaviour component
            eManager.SetComponentData(entity, new HasReynoldsFleeTargetPos{
                targetPos = fleeTargetPos
            });
            eManager.AddComponent<ReynoldsFleeSafeDistance>(entity);// add the Reynolds Flee Behaviour component
            eManager.SetComponentData(entity, new ReynoldsFleeSafeDistance{
                safeDistance = fleeSafeDistance
            });
        }

        if(seeking){ // if the agent has the seeking behaviour
            eManager.AddComponent<HasReynoldsSeekTargetPos>(entity);// add the Reynolds Seek Behaviour component
            eManager.SetComponentData(entity, new HasReynoldsSeekTargetPos{
                targetPos = seekTargetPos
            });
        }

        if(flocking || fleeing || seeking){ //if there is a behaviour exhibited
            eManager.AddComponent<ReynoldsMovementValues>(entity);// add the values component
            eManager.SetComponentData(entity,new ReynoldsMovementValues{  // Set the Reynolds Behaviour Value Data
                flockMovement = 0f,
                fleeMovement = 0f,
                seekMovement = 0f});
            eManager.AddComponent<ReynoldsBehaviourWeights>(entity);// add the weights component
            eManager.SetComponentData(entity,new ReynoldsBehaviourWeights{  // Set the Reynolds Behaviour Weight Data
                maxVelocity = this.maxVelocity,
                flockWeight = flockingWeight,
                fleeWeight = fleeingWeight,
                seekWeight = seekingWeight});
            
        }
        
    }
}
