using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingEvents : MonoBehaviour {

    public event EventHandler<OnSpacePressedEventArgs> OnSpacePressed;
    public class OnSpacePressedEventArgs : EventArgs{
        public int spaceCount;
    }

    private int spaceCount;
    
    // Start is called before the first frame update
    void Start() {
        //OnSpacePressed += Testing_OnSpacePressed;
    }

    private void Testing_OnSpacePressed(object sender, EventArgs e){
        Debug.Log("Space pressed!");
    }

    // Update is called once per frame
    void Update() {
        if(Input.GetKeyDown(KeyCode.Space)){
            //Space Pressed!
            spaceCount++;
            //if(OnSpacePressed != null) OnSpacePressed(this, EventArgs.Empty);//
            OnSpacePressed?.Invoke(this, new OnSpacePressedEventArgs {
                spaceCount = spaceCount
            });
        }
    }
}
