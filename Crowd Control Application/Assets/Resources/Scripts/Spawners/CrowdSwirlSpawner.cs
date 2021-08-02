using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System.IO;

public class CrowdSwirlSpawner : MonoBehaviour
{
    private float xScale, zScale;
    private Quaternion rotation;
    public GameObject[] prefabs;
    public float[] percentsToSpawn;
    public int numberToSpawn = 10;
    public float delay = 0f;
    public bool repeatSpawn = false;
    public float repeatSpawnDelay = 0f;
    public float repeatSpawnRepeatRate = 3.15f;

    private int[] numAgentsToSpawn;
    // Start is called before the first frame update
    void Start()
    {   
        if(!repeatSpawn){
            Invoke("SpawnCrowd",delay);
        }
        else{
            InvokeRepeating("SpawnCrowd", repeatSpawnDelay, repeatSpawnRepeatRate);
        }
        
    }

    private void SpawnCrowd(){
        rotation = this.transform.rotation;
        xScale = this.transform.localScale.x;
        zScale = this.transform.localScale.z;

        numAgentsToSpawn = new int[percentsToSpawn.Length]; //set up the array of numbers of each agent to spawn
        for(int i = 0; i < numAgentsToSpawn.Length; i++){
            numAgentsToSpawn[i] = (int)Mathf.Floor(percentsToSpawn[i]*numberToSpawn); // set the number of agents to spawn based off of the percents
        }
        int[] numAgentsSpawned = new int[percentsToSpawn.Length]; //set up the array to count number of each agent spawned

        float totArea = xScale * zScale; // area of rect we will spawn agents in
        float spawnArea = totArea / numberToSpawn; // area for each agent to spawn within
        float sideLength = Mathf.Sqrt(spawnArea);
        
        float xPartitions = Mathf.Ceil(xScale/sideLength);
        float zPartitions = Mathf.Ceil(zScale/sideLength);

        //Debug.Log("X Partitions: " + xPartitions);
        //Debug.Log("Z Partitions: " + zPartitions);

        //Debug.Log("Does it spawn enough? " + numberToSpawn + " to spawn, " + xPartitions * zPartitions + " at minimum will spawn.");

        IList<int> availableTypes = new List<int>(); // The indices that can be used to create a crowd agent
        for(int i = 0; i < prefabs.Length; i++){
            availableTypes.Add(i); // set up the indices (1,2,3...n)
        }

        IList<int> types = new List<int>();
        IList<Vector3> positions = new List<Vector3>();

        Vector3 position;

            for(int i = 0; i < xPartitions; i++){ // i is in the x direction
                for(int j = 0; j < zPartitions; j++){ // j is in the z direction
                    if(availableTypes.Count>0){
                        int type = availableTypes[((int)UnityEngine.Random.Range(0,availableTypes.Count))]; // choose a type
                    
                        //Debug.Log("TYPE: " + type);
                        float xOffset = UnityEngine.Random.Range(-xScale/(2*xPartitions),xScale/(2*xPartitions));
                        float zOffset = UnityEngine.Random.Range(-zScale/(2*zPartitions),zScale/(2*zPartitions));
                        position = new Vector3( // create a vector based on which partition the agent is in
                            (xOffset-xScale/2)+((i+0.5f)*xScale/xPartitions),
                            0,
                            (zOffset-zScale/2)+((j+0.5f)*zScale/zPartitions)
                        );
                        position = rotation * position; // rotate the vector
                        position += this.transform.position; // move the position to where the game object is
                        Object.Instantiate(prefabs[type], position,rotation); // create the crowd agent
                        numAgentsSpawned[type]++;

                        types.Add(type); // add the type to the list
                        positions.Add(position); // add the position to the list

                        //Check if we have reached the number of agents to spawn
                        if(numAgentsToSpawn[type] <= numAgentsSpawned[type]){
                            availableTypes.Remove(type);// remove the type from the availableTypes
                            //Debug.Log("Removed " + type);
                        }
                    }
                }
            }

        
        //RemoveGameObject();
    }




    //removes the script
    private void RemoveScript()
    {
        Destroy(this);
    }

    private void RemoveGameObject(){
        Destroy(this.transform.gameObject);
    }

}


