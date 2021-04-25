using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApproachIntersectionTrigger : MonoBehaviour
{
    public ScenarioTrigManager scenarioManager;
    void OnTriggerEnter(Collider other){
        scenarioManager.ApproachIntersectionTriggered();
        gameObject.SetActive(false);
    }
}
