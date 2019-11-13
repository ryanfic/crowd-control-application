using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Collections;
using Unity.Jobs;

public class ECSTargetingHandler : MonoBehaviour
{
    [SerializeField] private Material seekerMaterial;
    [SerializeField] private Material targetMaterial;
    [SerializeField] private Mesh cubeMesh;

    private static EntityManager eManager;

    private void Start() {
        eManager = World.Active.EntityManager;

        for(int i = 0; i < 2; i++){
            SpawnSeekerEntity();
        }
        


        for(int i = 0; i < 10; i++){
            SpawnTargetEntity();
        }
    }

    private float spawnTargetTimer = 0f;
    private void Update(){
        spawnTargetTimer -= Time.deltaTime;
        if(spawnTargetTimer < 0){ //if the timer has ended
            spawnTargetTimer = 1f; //reset the timer

            for(int i = 0; i < 5; i++){ //spawn more targets
                SpawnTargetEntity();
            }
        }
    }

    //wrapper function to spawn a seeker
    private void SpawnSeekerEntity(){
        SpawnSeekerEntity(new float3(UnityEngine.Random.Range(-5,5f), UnityEngine.Random.Range(-5,5f), UnityEngine.Random.Range(-5,5f)));
    }
    //spawns a seeker at a given position
    private void SpawnSeekerEntity(float3 position){
        Entity en = eManager.CreateEntity( //create the entity
            typeof(Translation),
            typeof(LocalToWorld),
            typeof(RenderMesh),
            typeof(Scale),
            typeof(Seeker)
        );
        SetEntityComponentData(en, position, cubeMesh, seekerMaterial); //set component data
        eManager.SetComponentData(en, new Scale{ Value = 1.5f}); //Set size of entity
    }

    private void SpawnTargetEntity(){
        Entity en = eManager.CreateEntity( //create the entity
            typeof(Translation),
            typeof(LocalToWorld),
            typeof(RenderMesh),
            typeof(Scale),
            typeof(Target)
        );
        SetEntityComponentData(en, new float3(UnityEngine.Random.Range(-5,5f), UnityEngine.Random.Range(-5,5f), UnityEngine.Random.Range(-5,5f)), cubeMesh, targetMaterial); //set component data
        eManager.SetComponentData(en, new Scale{ Value = 0.5f}); //set size of entity
    }

    //Set up entity components of a given entity
    private void SetEntityComponentData(Entity entity, float3 spawnPosition, Mesh mesh, Material material)
    {
        eManager.SetSharedComponentData<RenderMesh>(entity, //set up the Render mesh of the entity
            new RenderMesh{
                material = material,
                mesh = mesh
            }
        );

        eManager.SetComponentData<Translation>(entity, //set up the position of the entity
            new Translation{
                Value = spawnPosition
            }
        );
    }

}

public struct Seeker : IComponentData{} //label that an entity is a seeker
public struct Target : IComponentData{} //label that an entity is a target

public struct HasTarget : IComponentData{ //Used in Seekers attempt to find targets
    public Entity targetEntity;
}

public class HasTargetDebug : ComponentSystem{
    protected override void OnUpdate(){
        Entities.ForEach((Entity entity, ref Translation translation, ref HasTarget hasTarget)=>{
            if(World.Active.EntityManager.Exists(hasTarget.targetEntity)){ //if the target still exists
                //Cannot just access the position of the targetEntity via HasTarget, need to use the Entity Manager
                Translation targetTranslation = World.Active.EntityManager.GetComponentData<Translation>(hasTarget.targetEntity);
                Debug.DrawLine(translation.Value, targetTranslation.Value);
            }
        });
    }
}