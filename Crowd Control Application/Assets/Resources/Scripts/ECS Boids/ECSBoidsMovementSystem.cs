using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class ECSBoidsMovementSystem : ComponentSystem
{
    [SerializeField] float movementSpeed = 5f;
    //[SerializeField] float tolerance = 0.2f;
    protected override void OnUpdate(){
        Entities.ForEach((Entity crowd, ref BoidsMovement boidsMovement, ref Translation transl)=>{
            //if(World.Active.EntityManager.Exists(hasTar.targetEntity)){ //if the target still exists
                //Translation targetTranslation = World.Active.EntityManager.GetComponentData<Translation>(hasTar.targetEntity);
        
                //float3 targetDir = math.normalize(targetTranslation.Value - transl.Value); //the direction for movement
                //if(math.distancesq(boidsMovement.movement, float3.zero) < tolerance * tolerance)
                    Debug.Log(boidsMovement.movement);
                    transl.Value += boidsMovement.movement * movementSpeed * Time.deltaTime; //add movement to the translation
            
                /*if(math.distance(transl.Value, targetTranslation.Value) < 0.2f){// check distance to target
                    //Close to target, destroy it
                    PostUpdateCommands.DestroyEntity(hasTar.targetEntity); //destroy the target
                    PostUpdateCommands.RemoveComponent(seeker, typeof(HasTarget)); //remove the hasTarget component of the Seeker
                } */
            //}
            /*else{ //if the target no longer exists
                PostUpdateCommands.RemoveComponent(seeker, typeof(HasTarget)); //remove the hasTarget component of the Seeker
            }*/
            
        });
    }
}
