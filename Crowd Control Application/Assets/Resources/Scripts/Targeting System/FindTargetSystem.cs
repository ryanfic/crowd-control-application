using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class FindTargetSystem : ComponentSystem
{
    protected override void OnUpdate(){
        Entities.WithNone<HasTarget>().WithAll<Seeker>().ForEach((Entity seeker, ref Translation seekerTranslation) => {
            //Code running on all Seekers
            //Debug.Log(seeker);
            
            float3 seekerPosition = seekerTranslation.Value;

            Entity closestTargetEntity = Entity.Null; //Since entity is a struct, it cannot simply be null, it must be Entity.Null
            float3 closestTargetPosition = float3.zero;


            Entities.WithAll<Target>().ForEach((Entity targetEntity, ref Translation targetTranslation) => {
                //Cycling through all entities with Target tag
                //Debug.Log(targetEntity);

                if(closestTargetEntity == Entity.Null){ //if there was no closest target entity
                    //No target
                    closestTargetEntity = targetEntity;
                    closestTargetPosition = targetTranslation.Value;
                }
                else{
                    if(math.distance(seekerPosition, targetTranslation.Value) < math.distance(seekerPosition, closestTargetPosition))
                    {
                        //this target is closer
                        closestTargetEntity = targetEntity;
                        closestTargetPosition = targetTranslation.Value;
                    }
                }
            });

            //Closest target
            if(closestTargetEntity != Entity.Null){ //if there is a closest entity
                PostUpdateCommands.AddComponent(seeker, new HasTarget{targetEntity = closestTargetEntity});
            }

        });
    }
}
