using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Linq;
using System;

public class OnToFilterCordonEventArgs : EventArgs{
    public bool StepLeft;
}
public class OnTo3SidedBoxEventArgs : EventArgs{
    public int TopLineNum;
    public int LeftLineNum;
    public int RightLineNum;
}

public class PoliceUnitVoiceController : MonoBehaviour
{
    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, Action> actions = new Dictionary<string, Action>();

    //Cordon Command Events
    public event EventHandler OnToLooseCordonVoiceCommand;
    public event EventHandler<OnToFilterCordonEventArgs> OnToFilterCordonVoiceCommand; // 2nd Serial can step to left or right
    public event EventHandler OnToTightCordonVoiceCommand;
    public event EventHandler OnToSingleBeltCordonVoiceCommand;
    public event EventHandler OnToDoubleBeltCordonVoiceCommand;
    public event EventHandler OnUnlinkCordonVoiceCommand;


    //Wedge Command Events
    public event EventHandler OnToSingleBeltWedgeVoiceCommand;
    public event EventHandler OnToDoubleBeltWedgeVoiceCommand;
    public event EventHandler OnWedgeAdvanceVoiceCommand;
    public event EventHandler OnWedgeHaltVoiceCommand;

    //3 Sided Box Command Events
    public event EventHandler<OnTo3SidedBoxEventArgs> OnTo3SidedBoxVoiceCommand; // Needs args to say what line goes where


    void Start()
    {
        //Add actions for changing into formations
        AddFormationCommands();

        //Add actions for movement
        AddMovementCommands();

        keywordRecognizer = new KeywordRecognizer(actions.Keys.ToArray(),ConfidenceLevel.Low);
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
        actions.Add("Loose Cordon Form", ToLooseCordon);//to Loose Cordon
        actions.Add("Second Serial One Step To The Left Form", ToFilterCordonLeft);//to Filter Cordon Stepping left
        actions.Add("Second Serial One Step To The Right Form", ToFilterCordonRight);//to Filter Cordon Stepping right
        actions.Add("Tight Cordon Form", ToTightCordon);//to Tight Cordon
        actions.Add("Single Belt Cordon Form", ToSingleBeltCordon);//to Single Belt Cordon
        actions.Add("Double Belt Cordon Form", ToDoubleBeltCordon);//to Double Belt Cordon
        actions.Add("Disengage", UnlinkCordon);//Unlink linked cordon
    }

    

    private void AddWedgeCommands(){
        actions.Add("Single Belt Wedge Form", ToSingleBeltWedge); //to Single Belt Wedge
        actions.Add("Double Belt Wedge Form", ToDoubleBeltWedge); //to Double Belt Wedge
        actions.Add("Prepare to Advance Advance", WedgeAdvance);//To move forward
        actions.Add("Halt", WedgeHalt);//Halt forward movement
        //Create a corridor -> "Front Officer...Disengage"
    }

    private void Add3SidedBoxCommands(){
        actions.Add("Rear Rank Stack Right Center Rank Stack Left Three Sided Box Form", To3SidedBoxRRCL);//to 3 Sided box -> rear right center left
        actions.Add("Rear Rank Stack Left Center Rank Stack Right Three Sided Box Form", To3SidedBoxRLCR); // to 3 Sided Box -> Rear left center right
        actions.Add("Center Rank Stack Left Rear Rank Stack Right Three Sided Box Form", To3SidedBoxRRCL);//to 3 Sided box -> rear right center left
        actions.Add("Center Rank Stack Right Rear Rank Stack Left Three Sided Box Form", To3SidedBoxRLCR); // to 3 Sided Box -> Rear left center right
        //Front line standing fast at T junctionn when entering from bottom of T
        //Standing fast at T Junction When moving across top of T
    }

    private void AddMovementCommands(){
        //Move to...
        //Intersection
        //Stop sign

        //Wheeling
    }

    private void RecognizedSpeech(PhraseRecognizedEventArgs speech){
        Debug.Log(speech.text);
        actions[speech.text].Invoke();
    }


    /*
        Event Triggering Functions Called When Voice Command Observed
        Cordon Commands
    */
    private void ToLooseCordon(){
        OnToLooseCordonVoiceCommand?.Invoke(this, EventArgs.Empty);
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

    private void ToTightCordon(){
        OnToTightCordonVoiceCommand?.Invoke(this, EventArgs.Empty);
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
}
