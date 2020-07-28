/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Entities;
using Unity.Collections;

public class TestingSystem : ComponentSystem {

    protected override void OnUpdate() {
        if (Input.GetKeyDown(KeyCode.R)) {
            // Clean up all Entities
            EntityManager.DestroyEntity(EntityManager.UniversalQuery);

            // Load Scene
            SceneManager.LoadScene("GameScene_Events");
            //SceneManager.LoadScene("MainMenu");
        }
    }

}