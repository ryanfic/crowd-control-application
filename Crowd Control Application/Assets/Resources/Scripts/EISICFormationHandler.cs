using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class EISICFormationHandler : MonoBehaviour
{
    public bool Is3SidedBox = false;
    public bool IsWedge = false;
    public bool invoked = false;

    public event System.EventHandler OnToParallelTightCordonCommand;
    public event System.EventHandler OnToWedgeCommand;
    public event System.EventHandler<OnTo3SidedBoxEventArgs> OnTo3SidedBoxCommand; // Needs args to say what line goes where
    void Start(){
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PoliceFormationChangeSystem>().ConnectToFormationHandler();
    }

    void Update(){
        if(!invoked){
            if(Is3SidedBox){
                OnTo3SidedBoxCommand?.Invoke(this, new OnTo3SidedBoxEventArgs{
                TopLineNum = 1,
                LeftLineNum = 3,
                RightLineNum = 2
            });
            }
            else if(IsWedge){
                OnToWedgeCommand?.Invoke(this, System.EventArgs.Empty);
            }
            else{
                OnToParallelTightCordonCommand?.Invoke(this, System.EventArgs.Empty);
            }
            invoked = true;
        }
    }

}
