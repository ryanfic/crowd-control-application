using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using System.IO;
using UnityEngine;

public class CrowdSwirlCounter : MonoBehaviour
{
    public int CrowdToSpawn = 500;
    private int crowdDeleted = 0;
    private float timer = 0f;
    private float timeIntoSimulation = 0f;
    public float outputFrequency = 1f; // time after which the crowd count is output
    public int numSpawners = 1;

    // Start is called before the first frame update
    void Start()
    {
        CrowdToDeleteSystem crowdDeleteSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<CrowdToDeleteSystem>();
        crowdDeleteSystem.OnCrowdAgentDeleted += CrowdAgentDeletedResponse;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        timeIntoSimulation += Time.deltaTime;
        if(timer > outputFrequency){
            timer = 0f;
            StreamWriter sw = new StreamWriter("crowdflowdata.txt",true);
            string toadd = timeIntoSimulation + "," + (CrowdToSpawn - crowdDeleted);
            sw.WriteLine(toadd);
            sw.Close();
        }
    }
    private void CrowdAgentDeletedResponse(object sender, System.EventArgs eventArgs){
        crowdDeleted++;
        //Debug.Log("Captured deletion event");
    }
}
