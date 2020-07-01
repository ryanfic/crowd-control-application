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
        int i = 0;
        UnityEngine.MeshCollider[] meshes = GetComponentsInChildren<UnityEngine.MeshCollider>(this.transform.gameObject);
        foreach(UnityEngine.MeshCollider mesh in meshes){
            Debug.Log("Converting " + i);
            CopyToEntity(mesh.gameObject);
            i++;
        }
    }

    private Entity CopyToEntity(GameObject obj){
        EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;

        Mesh m = obj.GetComponent<UnityEngine.MeshCollider>().sharedMesh;
        float3[] vertices = new float3[m.vertices.Length]; // a list of vertices
        for(int i = 0; i < m.vertices.Length; i++){
            vertices[i] = m.vertices[i]; // get the vertices
        }

        // Convert an array of ints to an array of int3 (has 3 ints)
        int3[] indices = new int3[m.triangles.Length/3]; //set up the array of int3
        for(int i = 0; i < m.triangles.Length/3; i++){
            int x = 0;
            int y = 0;
            int z = 0;
            for(int j = 0; j < 3; j++){ //assign the 3 values of the triangle
                int toAdd = m.triangles[3*i+j]; // get the next index to add
                switch (j) 
                {                               //add the index based on the position
                    case 0:
                        x = toAdd;
                        break;
                    case 1:
                        y = toAdd;
                        break;
                    case 2:
                        z = toAdd;
                        break;
                }
            }
            int3 newInt3 = new int3(x,y,z); // create a new int3 based on the values obtained
            indices[i] = newInt3;// add the int3 to the int3 array
        }

        NativeArray<float3> nativeVertices = new NativeArray<float3>(vertices,Allocator.Temp);
        NativeArray<int3> nativeIndices = new NativeArray<int3>(indices,Allocator.Temp);
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
