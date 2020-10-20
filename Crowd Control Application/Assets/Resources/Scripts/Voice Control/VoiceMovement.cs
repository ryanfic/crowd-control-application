using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Linq;
using System;

public class VoiceMovement : MonoBehaviour
{
    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, Action> actions = new Dictionary<string, Action>();

    void Start(){
        actions.Add("Forward", Forward);
        actions.Add("Up", Up);
        actions.Add("Down", Down);
        actions.Add("Back", Back);

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
}
