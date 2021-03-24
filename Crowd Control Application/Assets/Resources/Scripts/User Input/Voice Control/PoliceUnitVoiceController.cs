using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Linq;
using System;
using Unity.Entities;

public class OnToFilterCordonEventArgs : EventArgs{
    public bool StepLeft;
}
public class OnTo3SidedBoxEventArgs : EventArgs{
    public int TopLineNum;
    public int LeftLineNum;
    public int RightLineNum;
}
public class OnRotateEventArgs : EventArgs{
    public bool RotateLeft;
}
public class OnPoliceUnitSelectionEventArgs : EventArgs{
    public string UnitName;
}

public class PoliceUnitVoiceController : MonoBehaviour
{
    public static string AllUnitSelectName = "*AllUnits";

    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, Action> actions = new Dictionary<string, Action>();
    private Dictionary<string, Action<string>> policeUnitNameActions = new Dictionary<string, Action<string>>();

    //Cordon Command Events
    public event EventHandler OnToParallelLooseCordonVoiceCommand;
    public event EventHandler OnToSingleLooseCordonVoiceCommand;
    public event EventHandler<OnToFilterCordonEventArgs> OnToFilterCordonVoiceCommand; // 2nd Serial can step to left or right
    public event EventHandler OnToParallelTightCordonVoiceCommand;
    public event EventHandler OnToSingleTightCordonVoiceCommand;
    public event EventHandler OnToSingleBeltCordonVoiceCommand;
    public event EventHandler OnToDoubleBeltCordonVoiceCommand;
    public event EventHandler OnUnlinkCordonVoiceCommand;


    //Wedge Command Events
    public event EventHandler OnToWedgeVoiceCommand;
    public event EventHandler OnToSingleBeltWedgeVoiceCommand;
    public event EventHandler OnToDoubleBeltWedgeVoiceCommand;
    public event EventHandler OnWedgeAdvanceVoiceCommand;
    public event EventHandler OnWedgeHaltVoiceCommand;

    //3 Sided Box Command Events
    public event EventHandler<OnTo3SidedBoxEventArgs> OnTo3SidedBoxVoiceCommand; // Needs args to say what line goes where

    //Movement Command Events
    public event EventHandler OnMoveToIntersectionCommand;
    public event EventHandler OnMoveForwardCommand;
    public event EventHandler OnHaltCommand;
    public event EventHandler<OnRotateEventArgs> OnRotateCommand;

    //Unit Selection Command Events
    public event EventHandler<OnPoliceUnitSelectionEventArgs> OnPoliceUnitSelectionCommand;
    public event EventHandler OnDeselectPoliceUnitsCommand;


    void Start()
    {
        //Add actions for changing into formations
        AddFormationCommands();

        //Add actions for movement
        AddMovementCommands();

        //Add actions for police unit selection
        AddSelectionCommands();

        //Set up the keyword recognizer
        SetUpKeywordRecognizer();

        //subscribe to events from jobs
        //subscribe to police units added events
        PoliceUnitJustCreatedSystem policeUnitCreatedSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PoliceUnitJustCreatedSystem>();
        policeUnitCreatedSystem.OnPoliceUnitCreatedWithName += PoliceUnitCreatedResponse;

        //subscribe to police units deleted events
        PoliceUnitToDeleteSystem policeUnitDeleteSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PoliceUnitToDeleteSystem>();
        policeUnitDeleteSystem.OnPoliceUnitDeletedWithName += PoliceUnitDeletedResponse;
    }

    private void SetUpKeywordRecognizer(){
        string[] commandKeys = actions.Keys.ToArray().Concat(policeUnitNameActions.Keys.ToArray()).ToArray();
        keywordRecognizer?.Dispose();
        keywordRecognizer = new KeywordRecognizer(commandKeys,ConfidenceLevel.Low);
        keywordRecognizer.OnPhraseRecognized += RecognizedSpeech;
        keywordRecognizer.Start();
    }

    private void AddFormationCommands(){
        //Add Cordon Commands
        AddCordonCommands();
        AddWedgeCommands();//Add Wedge Commands
        Add3SidedBoxCommands();//Add 3 Sided box commands
        //to running line
        //to ...handling all three lines separately
    }

    private void AddCordonCommands(){
        actions.Add("Parallel Loose Cordon Form", ToParallelLooseCordon);//to parallel Loose Cordon
        actions.Add("Single Loose Cordon Form", ToSingleLooseCordon);//to single Loose Cordon
        actions.Add("Second Serial One Step To The Left Form", ToFilterCordonLeft);//to Filter Cordon Stepping left
        actions.Add("Second Serial One Step To The Right Form", ToFilterCordonRight);//to Filter Cordon Stepping right
        actions.Add("Parallel Tight Cordon Form", ToParallelTightCordon);//to parallel Tight Cordon
        actions.Add("Single Tight Cordon Form", ToSingleTightCordon);//to single Tight Cordon
        actions.Add("Single Belt Cordon Form", ToSingleBeltCordon);//to Single Belt Cordon
        actions.Add("Double Belt Cordon Form", ToDoubleBeltCordon);//to Double Belt Cordon
        actions.Add("Disengage", UnlinkCordon);//Unlink linked cordon
    }

    

    private void AddWedgeCommands(){
        actions.Add("Wedge Form", ToWedge); //to  Wedge
        //actions.Add("Single Belt Wedge Form", ToSingleBeltWedge); //to Single Belt Wedge
        //actions.Add("Double Belt Wedge Form", ToDoubleBeltWedge); //to Double Belt Wedge
        actions.Add("Prepare to Advance Advance", WedgeAdvance);//To move forward
        //actions.Add("Halt", WedgeHalt);//Halt forward movement
        //Create a corridor -> "Front Officer...Disengage"
    }

    private void Add3SidedBoxCommands(){
        actions.Add("Rear Rank Stack Right Center Rank Stack Left Three Sided Box Form", To3SidedBoxRRCL);//to 3 Sided box -> rear right center left
        actions.Add("Rear Rank Stack Left Center Rank Stack Right Three Sided Box Form", To3SidedBoxRLCR); // to 3 Sided Box -> Rear left center right
        actions.Add("Center Rank Stack Left Rear Rank Stack Right Three Sided Box Form", To3SidedBoxRRCL);//to 3 Sided box -> rear right center left
        actions.Add("Center Rank Stack Right Rear Rank Stack Left Three Sided Box Form", To3SidedBoxRLCR); // to 3 Sided Box -> Rear left center right
        actions.Add("Three Sided Box Form", To3SidedBoxRLCR); // to 3 Sided Box -> Rear left center right
        //Front line standing fast at T junctionn when entering from bottom of T
        //Standing fast at T Junction When moving across top of T
    }

    private void AddMovementCommands(){
        //Move to...
        //Intersection
        actions.Add(/*"The objective is to take the intersection. Do you understand? Advance"*/"The Objective Is To Take The Intersection Advance", MoveToIntersection); // move to the intersection
        //Stop sign

        //Wheeling

        //Move forward
        actions.Add("Section Advance", MoveForward);
        //Halt
        actions.Add("Halt", Halt);
        //Rotate
        actions.Add("Rotate Left", RotateLeft);
        actions.Add("Rotate Right", RotateRight);
    }

    private void AddSelectionCommands(){
        //Select all police units
        actions.Add("All Units", SelectAllPoliceUnits);

        //Deselect all police units
        actions.Add("Deselect Units", DeselectAllPoliceUnits);
    }

    private void RecognizedSpeech(PhraseRecognizedEventArgs speech){
        Debug.Log(speech.text);
        InvokeAction(speech.text);
    }

    private void InvokeAction(string commandKey){
        if(actions.ContainsKey(commandKey)){
            actions[commandKey].Invoke();
        }
        else { // if the command was not in the other command dictionary, must be in the police unit name dictionary
            policeUnitNameActions[commandKey](commandKey);
        }   
    }


    /*
        Event Triggering Functions Called When Voice Command Observed
        Cordon Commands
    */
    private void ToParallelLooseCordon(){
        OnToParallelLooseCordonVoiceCommand?.Invoke(this, EventArgs.Empty);
    }
    private void ToSingleLooseCordon(){
        OnToSingleLooseCordonVoiceCommand?.Invoke(this, EventArgs.Empty);
    }

    private void ToFilterCordonLeft(){
        OnToFilterCordonVoiceCommand?.Invoke(this, new OnToFilterCordonEventArgs{
            StepLeft = true
        }); 
    }

    private void ToFilterCordonRight(){
        OnToFilterCordonVoiceCommand?.Invoke(this, new OnToFilterCordonEventArgs{
            StepLeft = false
        });
    }

    private void ToParallelTightCordon(){
        OnToParallelTightCordonVoiceCommand?.Invoke(this, EventArgs.Empty);
    }
    private void ToSingleTightCordon(){
        OnToSingleTightCordonVoiceCommand?.Invoke(this, EventArgs.Empty);
    }

    private void ToSingleBeltCordon(){
        OnToSingleBeltCordonVoiceCommand?.Invoke(this, EventArgs.Empty);
    }

    private void ToDoubleBeltCordon(){
        OnToDoubleBeltCordonVoiceCommand?.Invoke(this, EventArgs.Empty);
    }

    private void UnlinkCordon(){
        OnUnlinkCordonVoiceCommand?.Invoke(this, EventArgs.Empty);
    }

    /*
        Event Triggering Functions Called When Voice Command Observed
        Wedge Commands
    */
    private void ToWedge(){
        OnToWedgeVoiceCommand?.Invoke(this, EventArgs.Empty);
    }
    private void ToSingleBeltWedge(){
        OnToSingleBeltWedgeVoiceCommand?.Invoke(this, EventArgs.Empty);
    }

    private void ToDoubleBeltWedge(){
        OnToDoubleBeltWedgeVoiceCommand?.Invoke(this, EventArgs.Empty);
    }

    private void WedgeAdvance(){
        OnWedgeAdvanceVoiceCommand?.Invoke(this, EventArgs.Empty);
    }

    private void WedgeHalt(){
        OnWedgeHaltVoiceCommand?.Invoke(this, EventArgs.Empty);
    }

    /*
        Event Triggering Functions Called When Voice Command Observed
        Three Sided Box Commands
    */
    //Rear line to left and Center line to right
    private void To3SidedBoxRLCR(){
        OnTo3SidedBoxVoiceCommand?.Invoke(this, new OnTo3SidedBoxEventArgs{
            TopLineNum = 1,
            LeftLineNum = 3,
            RightLineNum = 2
        });
    }
    //Rear line to right and Center line to left
    private void To3SidedBoxRRCL(){
        OnTo3SidedBoxVoiceCommand?.Invoke(this, new OnTo3SidedBoxEventArgs{
            TopLineNum = 1,
            LeftLineNum = 2,
            RightLineNum = 3
        });
    }

    /*
        Movement Events Called When Voice Command Observed
    */
    private void MoveToIntersection(){
        OnMoveToIntersectionCommand?.Invoke(this, EventArgs.Empty);
    }
    private void MoveForward(){
        OnMoveForwardCommand?.Invoke(this, EventArgs.Empty);
    }
    private void Halt(){
        OnHaltCommand?.Invoke(this, EventArgs.Empty);
    }
    private void RotateLeft(){
        OnRotateCommand?.Invoke(this, new OnRotateEventArgs{
            RotateLeft = true
        });
    }
    private void RotateRight(){
        OnRotateCommand?.Invoke(this, new OnRotateEventArgs{
            RotateLeft = false
        });
    }

    /*
        Event Triggering Functions called when a Police Unit's Name is called
    */
    private void SelectAllPoliceUnits(){
        OnPoliceUnitSelectionCommand?.Invoke(this, new OnPoliceUnitSelectionEventArgs{
            UnitName = AllUnitSelectName
        });
    }

    private void DeselectAllPoliceUnits(){
        OnDeselectPoliceUnitsCommand?.Invoke(this, EventArgs.Empty);
    }

    private void SelectPoliceUnit(string unitName){
        OnPoliceUnitSelectionCommand?.Invoke(this, new OnPoliceUnitSelectionEventArgs{
            UnitName = unitName
        });
    }

    /*
        Methods responding to police creation / deletion events
    */
    private void PoliceUnitCreatedResponse(object sender, OnPoliceUnitCreatedWithNameArgs eventArgs){
        AddPoliceUnitNameCommand(eventArgs.PoliceUnitName);

    }
    //Add a police unit name to the list of commands
    private void AddPoliceUnitNameCommand(string unitName){
        //check if the name is a command already
        if(!policeUnitNameActions.ContainsKey(unitName)){
            //if not, add the police unit name to the list of commands
            policeUnitNameActions.Add(unitName,SelectPoliceUnit);
            SetUpKeywordRecognizer();
            Debug.Log(unitName + " was added to the list of police unit name commands");
        }
        else{
            Debug.Log(unitName + " is already a command, so it was not added to the commands.");
        }
    }

    //Responds to the event where police units are deleted
    private void PoliceUnitDeletedResponse(object sender, OnPoliceUnitDeletedWithNameArgs eventArgs){
        RemovePoliceUnitNameCommand(eventArgs.PoliceUnitName);

    }
    //Remove a police unit name from the list of commands
    private void RemovePoliceUnitNameCommand(String unitName){
        //check if the name is a command
        if(policeUnitNameActions.ContainsKey(unitName)){
            //if the name is a command, remove the police unit name from the list of commands
            policeUnitNameActions.Remove(unitName);
            SetUpKeywordRecognizer();
            Debug.Log(unitName + " was removed from the list of police unit name commands");
        }
        else{
            Debug.Log(unitName + " is not already a command, so it was not removed from the commands.");
        }
    }

    
}
