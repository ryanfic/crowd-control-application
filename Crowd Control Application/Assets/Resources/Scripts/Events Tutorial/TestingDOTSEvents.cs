using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class TestingDOTSEvents : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PipeMoveSystem>().OnPipePassed += TestingDOTSEvents_OnPipePassed;
    }

    private void TestingDOTSEvents_OnPipePassed(object sender, System.EventArgs e){
        Debug.Log("Pipe Event!");
    }
}
