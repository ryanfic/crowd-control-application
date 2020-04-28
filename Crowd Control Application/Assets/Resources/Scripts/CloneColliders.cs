using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

public class CloneColliders : MonoBehaviour
{
    public float timeToWait = 5f;


    // Start is called before the first frame update
    void Start()
    {   
        StartCoroutine(CopyAfterSeconds());
    }

    IEnumerator CopyAfterSeconds(){
        yield return new WaitForSeconds(timeToWait); // Wait after a certain amount of time
        Debug.Log("Finished Waiting");

        //Copy all of the game objects into entities

        UnityEngine.MeshCollider[] meshes = GetComponentsInChildren<UnityEngine.MeshCollider>(this.transform.gameObject);
        foreach(UnityEngine.MeshCollider mesh in meshes){
            CopyToEntity(mesh.gameObject);
        }
    }

    private Entity CopyToEntity(GameObject obj){
        EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;

        Mesh m = obj.GetComponent<UnityEngine.MeshCollider>().sharedMesh;
        float3[] vertices = new float3[m.vertices.Length]; // a list of vertices
        for(int i = 0; i < m.vertices.Length; i++){
            vertices[i] = m.vertices[i]; // get the vertices
        }

        NativeArray<float3> nativeVertices = new NativeArray<float3>(vertices,Allocator.Temp);
        NativeArray<int> nativeIndices = new NativeArray<int>(m.triangles,Allocator.Temp);
        float3 pos = obj.transform.position;
        Quaternion rot = obj.transform.rotation;

        var mCol = Unity.Physics.MeshCollider.Create(nativeVertices,nativeIndices); // create a mesh collider from the vertices and indices
        
        Entity en = em.CreateEntity(typeof(Translation),typeof(Rotation),typeof(PhysicsCollider)); // Create the entity

        // Add the data to the entity
        em.SetComponentData<PhysicsCollider>(en,new PhysicsCollider{
            Value = mCol
        });
        em.SetComponentData<Translation>(en, new Translation{
            Value = pos
        });
        em.SetComponentData<Rotation>(en, new Rotation{
            Value = rot
        });

        //Remove the arrays to not waste memory
        nativeIndices.Dispose();
        nativeVertices.Dispose();

        //Set the name of the entity
        string name = obj.name;
        em.SetName(en,name);
        return en;
    }

    
}
