using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

/*public class NeatoScript : ComponentSystem{
    private bool spawned = false;
    protected override void OnUpdate(){
        if(!spawned){
            //PrefabPoliceAgentEntity prefabPoliceAgent = GetSingleton<PrefabPoliceAgentEntity>();
            Entity spawnedEntity = EntityManager.Instantiate(PrefabPoliceAgentEntity.prefabEntity);

            EntityManager.SetComponentData(spawnedEntity,
                new Translation { Value = new float3(0f,0f,0f)}
            );
            spawned = true;
        }
    }
}*/

