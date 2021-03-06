﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System.IO;

public class SwirlSpawner : MonoBehaviour
{
    private float xScale, zScale, xToZRatio;
    private Quaternion rotation;
    public GameObject[] prefabs;
    public float[] percentsToSpawn;
    public string fileName = "crowdLocations";
    public int numberToSpawn = 10;
    public float delay = 0f;
    public float frequency = 1f;
    private int[] numAgentsToSpawn;
    // Start is called before the first frame update
    void Start()
    {   
        string file = fileName + ".txt";
        if(File.Exists(file)){ // if there is a file to load from
            InvokeRepeating("DataSpawnCrowd",delay,frequency);
        }
        else{
            Invoke("SpawnCrowd",delay);
            InvokeRepeating("DataSpawnCrowd",delay+frequency,frequency);
        }
    }

    private float3 RotateVector(float3 vector){
        return rotation*vector;
    }

    private void SpawnCrowd(){
        rotation = this.transform.rotation;
        xScale = this.transform.localScale.x;
        zScale = this.transform.localScale.z;
        xToZRatio = xScale/zScale;

        numAgentsToSpawn = new int[percentsToSpawn.Length]; //set up the array of numbers of each agent to spawn
        for(int i = 0; i < numAgentsToSpawn.Length; i++){
            numAgentsToSpawn[i] = (int)Mathf.Floor(percentsToSpawn[i]*numberToSpawn); // set the number of agents to spawn based off of the percents
        }
        int[] numAgentsSpawned = new int[percentsToSpawn.Length]; //set up the array to count number of each agent spawned

        float root = Mathf.Sqrt(numberToSpawn);

        float xPartitions = root*xToZRatio;
        float zPartitions = root/xToZRatio;

        IList<int> availableTypes = new List<int>(); // The indices that can be used to create a crowd agent
        for(int i = 0; i < prefabs.Length; i++){
            availableTypes.Add(i); // set up the indices (1,2,3...n)
        }

        IList<int> types = new List<int>();
        IList<Vector3> positions = new List<Vector3>();

        Vector3 position;
        /*string output ="";
        for(int i = 0; i < availableTypes.Count; i++){
            if(i>0)
                output +=", ";
            output+= availableTypes[i];
        }
        Debug.Log(output);*/
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
                        Debug.Log("Removed " + type);
                        /*output ="";
                        for(int k = 0; k < availableTypes.Count; k++){
                            if(k>0)
                                output +=", ";
                            output+= availableTypes[k];
                        }
                        Debug.Log(output);*/
                    }
                    //Debug.Log("Pos " + (i*10+j) + ": " +position);
                }
            }
        }
        WriteCrowdData(types,positions);
        //RemoveGameObject();
    }

    //write the locations of all the crowd to a file
    private void WriteCrowdData(IList<int> types, IList<Vector3> positions)
    {
        string file = fileName + ".txt";
        StreamWriter sw = new StreamWriter(file);
        string towrite;
        for(int i = 0; i < types.Count; i++){
            //clear towrite
            towrite = "";
            //add type
            //variables delimited by ;
            towrite +=  types[i]+";";
            //add location
            //x y z separation delimited by ,
            towrite += positions[i].x +","+positions[i].y +","+positions[i].z;

            sw.WriteLine(towrite);
        }
        sw.Close();
    }

    private void DataSpawnCrowd()
    {
        rotation = this.transform.rotation; // get the rotation of the parent object
        string file = fileName + ".txt";
        StreamReader sr = new StreamReader(file);
        string line = "";
        while ((line = sr.ReadLine()) != null)
        {
            //parse the file for data
            //split data by ; delimiter
            string[] fields = line.Split(';');
            //first field is crowd type
            int type = int.Parse(fields[0]);
            
            //second field is location
            //get xyz
            string[] posString = fields[1].Split(',');
            float[] xyz = new float[3];
            for(int i=0;i<posString.Length;i++)
            {
                xyz[i] = float.Parse(posString[i]);
            }
            Vector3 position = new Vector3(xyz[0],xyz[1],xyz[2]);

            Object.Instantiate(prefabs[type], position,rotation); // create the crowd agent
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
