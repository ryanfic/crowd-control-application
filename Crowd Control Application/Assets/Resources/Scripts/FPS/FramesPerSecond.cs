using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class FramesPerSecond : MonoBehaviour
{
    private Rect fpsRect;
    private GUIStyle style;

    public float frequency = 1; // how frequently to update the FPS
    private float fps;

    private float avgFPS = 0; // the average FPS
    private float numFPSAdded = 0; // A count of how many times FPS has been added to the average

    public string file = "FPSdata.txt"; // Where to save the FPS data

    // Start is called before the first frame update
    void Start()
    {
        fpsRect = new Rect(25,25,400,100);
        style = new GUIStyle();
        style.fontSize = 20;

        StartCoroutine(RecalculateFPS()); // Start up the FPS counter
                                            // Needs to happen only once
    }

    /*
        Use Coroutine to calculate (and output) FPS and AVG FPS
    */
    private IEnumerator RecalculateFPS(){
        while(true){//Every second (always)
            yield return new WaitForSeconds(frequency); // Do nothing for a second (or how long frequency is set to)
            fps = 1 / Time.smoothDeltaTime; //recalculate FPS
            avgFPS = ((avgFPS * numFPSAdded) + fps)/(++numFPSAdded);
            //WriteFPSToFile();
        }
    }

    /*
        Display The FPS on the screen
    */
    void OnGUI(){
        GUI.Label(fpsRect,"FPS: " + string.Format("{0:0.0}",fps) 
            + "\nAverage FPS: " + string.Format("{0:0.0}",avgFPS)
            + "\nTime: " + string.Format("{0:0.0}",Time.time) ,style); // string.Format makes the FPS have only 1 decimal place
    }

    /*
        Output the Avg FPS to the specified file
    */
    void WriteFPSToFile()
    {
        //append writes the data
        StreamWriter sw = new StreamWriter(file,true);
        string toadd = Time.time + "," + avgFPS;
        sw.WriteLine(toadd);
        sw.Close();
    }
}
