using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Linq;
using System;

/*
    A test class for voice controls (NOT FOR ACTUAL USE IN THE CROWD CONTROL SYSTEM)
*/
public class VoiceMovement : MonoBehaviour
{
    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, Action> actions = new Dictionary<string, Action>();

    private string[] targets =          {"street",  "intersection", "stop sign"};
    private Action[] targetActions =    {Street,    Intersection,   StopSign};

    void Start(){
        actions.Add("Forward", Forward);
        actions.Add("Up", Up);
        actions.Add("Down", Down);
        actions.Add("Back", Back);
        actions.Add("This is a sentence", Sentence);
        actions.Add("Let's", Sentence);
        for(int i = 0; i < targets.Length; i++){
            actions.Add("Move to the " + targets[i], targetActions[i]);
        }


        keywordRecognizer = new KeywordRecognizer(actions.Keys.ToArray(),ConfidenceLevel.Low);
        keywordRecognizer.OnPhraseRecognized += RecognizedSpeech;
        keywordRecognizer.Start();
    }

    private void RecognizedSpeech(PhraseRecognizedEventArgs speech){
        Debug.Log(speech.text);
        actions[speech.text].Invoke();
    }

    private void Forward(){
        transform.Translate(1,0,0);
    }
    private void Back(){
        transform.Translate(-1,0,0);
    }
    private void Up(){
        transform.Translate(0,1,0);
    }
    private void Down(){
        transform.Translate(0,-1,0);
    }
    private void Sentence(){
        Debug.Log("You said a sentence!");
    }
    private static void Street(){
        Debug.Log("Moving to the Street!");
    }
    private static void Intersection(){
        Debug.Log("Moving to the intersection!");
    }
    private static void StopSign(){
        Debug.Log("Moving to the StopSign");
    }
}
