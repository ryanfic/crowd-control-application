using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Entities;

public class SceneSkipper : MonoBehaviour
{
    void Update()
    {
        /*if (Input.GetKeyDown("space"))
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            entityManager.DestroyEntity(entityManager.UniversalQuery);
            switch(SceneManager.GetActiveScene().buildIndex){
                case 0:
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                    break;
                case 1:
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);            
                    break;
                case 2:
                    
                    SceneManager.LoadScene(0);
                    break;
                default:
                    Debug.Log("Somehow defaulted?");
                    break;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            entityManager.DestroyEntity(entityManager.UniversalQuery);
            SceneManager.LoadScene(0); 
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            entityManager.DestroyEntity(entityManager.UniversalQuery);
            SceneManager.LoadScene(1); 
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            entityManager.DestroyEntity(entityManager.UniversalQuery);
            SceneManager.LoadScene(2); 
        }*/
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            entityManager.DestroyEntity(entityManager.UniversalQuery);
            SceneManager.LoadScene(0); 
        }
    }
}
