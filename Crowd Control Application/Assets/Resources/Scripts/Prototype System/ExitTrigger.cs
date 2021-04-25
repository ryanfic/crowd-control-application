using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    public ScenarioTrigManager scenarioManager;
    void OnTriggerEnter(Collider other){
        scenarioManager.ExitTriggered();
        gameObject.SetActive(false);
    }
}
