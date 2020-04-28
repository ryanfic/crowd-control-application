using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PoliceSpawner : MonoBehaviour
{
    private float xScale;
    private Quaternion rotation;
    public GameObject prefab;

    public string fileName = "policeLocations";
    public int numberToSpawn = 10;
    public float delay = 0f;

    // Start is called before the first frame update
    void Start()
    {   
        string file = fileName + ".txt";
        if(File.Exists(file)){ // if there is a 
            Invoke("DataSpawnPolice",delay);
        }
        else{
            Invoke("SpawnPolice",delay);
        }
    }

    private void SpawnPolice(){
        rotation = this.transform.rotation;
        xScale = this.transform.localScale.x;


        IList<Vector3> positions = new List<Vector3>();

        Vector3 position;

        for(int i = 0; i < numberToSpawn; i++){ // i is in the x direction
            position = new Vector3( // create a vector based on which partition the agent is in
                (-xScale/2)+((i+0.5f)*xScale/numberToSpawn),
                0,
                0
            );
            position = rotation * position; // rotate the vector
            position += this.transform.position; // move the position to where the game object is
            Object.Instantiate(prefab, position,rotation); // create the crowd agent

            positions.Add(position); // add the position to the list 
        }
        WritePoliceData(positions);
        RemoveGameObject();
    }

    //write the locations of all the police to a file
    private void WritePoliceData(IList<Vector3> positions)
    {
        string file = fileName + ".txt";
        StreamWriter sw = new StreamWriter(file);
        string towrite;
        for(int i = 0; i < positions.Count; i++){
            //clear towrite
            towrite = "";
            //add location
            //x y z separation delimited by ,
            towrite += positions[i].x +","+positions[i].y +","+positions[i].z;

            sw.WriteLine(towrite);
        }
        sw.Close();
    }

    private void DataSpawnPolice()
    {
        rotation = this.transform.rotation; // get the rotation of the parent object
        string file = fileName + ".txt";
        StreamReader sr = new StreamReader(file);
        string line = "";
        while ((line = sr.ReadLine()) != null)
        {
            //parse the file for data
            //the field is location
            //get xyz
            string[] posString = line.Split(',');
            float[] xyz = new float[3];
            for(int i=0;i<posString.Length;i++)
            {
                xyz[i] = float.Parse(posString[i]);
            }
            Vector3 position = new Vector3(xyz[0],xyz[1],xyz[2]);

            UnityEngine.Object.Instantiate(prefab, position,rotation); // create the police agent
        }
        RemoveGameObject();
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
