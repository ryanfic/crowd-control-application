using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingEventSubscriber : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TestingEvents testingEvents = GetComponent<TestingEvents>(); // get the reference to the publisher script
        testingEvents.OnSpacePressed += Subscriber_OnSpacePressed; // subscribe
    }

    private void Subscriber_OnSpacePressed(object sender, TestingEvents.OnSpacePressedEventArgs e){
        Debug.Log("Subscriber Noticed Space Pressed!");
        Debug.Log("Space Pressed " + e.spaceCount + " times!");
    }
}
