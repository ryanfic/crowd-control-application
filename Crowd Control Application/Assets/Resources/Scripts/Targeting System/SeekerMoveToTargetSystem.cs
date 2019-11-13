using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;

public class SeekerMoveToTargetSystem : ComponentSystem
{
    
    protected override void OnUpdate(){
        Entities.ForEach((Entity seeker, ref HasTarget hasTar, ref Translation transl)=>{
            if(World.Active.EntityManager.Exists(hasTar.targetEntity)){ //if the target still exists
                Translation targetTranslation = World.Active.EntityManager.GetComponentData<Translation>(hasTar.targetEntity);
        
                float3 targetDir = math.normalize(targetTranslation.Value - transl.Value); //the direction for movement
                float moveSpeed = 5f; //movement speed
                transl.Value += targetDir * moveSpeed * Time.deltaTime; //add movement to the translation
            
                if(math.distance(transl.Value, targetTranslation.Value) < 0.2f){// check distance to target
                    //Close to target, destroy it
                    PostUpdateCommands.DestroyEntity(hasTar.targetEntity); //destroy the target
                    PostUpdateCommands.RemoveComponent(seeker, typeof(HasTarget)); //remove the hasTarget component of the Seeker
                } 
            }
            else{ //if the target no longer exists
                PostUpdateCommands.RemoveComponent(seeker, typeof(HasTarget)); //remove the hasTarget component of the Seeker
            }
            
        });
    }
}
