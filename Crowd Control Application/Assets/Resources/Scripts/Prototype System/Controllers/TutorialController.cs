using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RTS_Cam;
using UnityEngine.SceneManagement;
using Unity.Entities;

public class TutorialController : MonoBehaviour
{
    public Transform policeUnit;
    public RTS_Camera cam;

    public Transform cameraTransform;

    public TMP_Text displayText;

    public float timer = 0f;

    private bool shouldUpdateTimer = true;
    private bool processingDelay = false;

    private int blockNo = 0;

    #region RotateCameraPhaseData

    private bool onRotateCameraPhase = true;

    private string[] rotateCameraTextBlocks = {"Welcome to the Crowd Control Software",
                                            "You can see the police unit in the center of the screen in blue",
                                            "You can rotate the camera by pressing and holding the right mouse button down and moving the mouse",
                                            "Please try moving the camera by pressing and holding the right mouse button down and moving the mouse"};

    private float[] rotateCameraBlockTimes = {5,5,5, float.PositiveInfinity};

    private Quaternion originalCameraRotation;

    private float rotationMinAngle = 30f;
    private bool rotatedCamera = false;

    //private float wedgeDelay = 3f;

    #endregion

    #region WedgeFormationPhaseData

    private bool onWedgeFormationPhase = true;

    private string[] wedgeTextBlocks = {"Good job rotating the camera", "You can also change the formation of the police unit by speaking commands into your microphone", "Try using the 'To Wedge Form' Command"};

    private float[] wedgeTextBlockTimes = {3, 5, float.PositiveInfinity};

    private bool changedToWedge = false;

    private float wedgeDelay = 3f;

    #endregion

    #region ParallelLooseCordonFormPhaseData

    private bool onParaLooseCordonPhase = true;

    private string[] paraLooseCordonTextBlocks = {"You can see how the police unit gets into the wedge formation", "There are other formations as well", "Try saying the 'To Parallel Loose Cordon Form' Command"};

    private float[] paraLooseCordonTextBlockTimes = {5,5, float.PositiveInfinity};

    private bool changedToParaLooseCordon = false;

    private float paraLooseCordonDelay = 5f;

    #endregion

    #region BoxFormPhaseData

    private bool onBoxPhase = true;

    private string[] boxTextBlocks = {"You can see how the police unit gets into the parallel loose cordon formation",
                                    "Another formation is the 'three sided box'",
                                    "Try saying the 'To Three Sided Box Form' Command"};

    private float[] boxTextBlockTimes = {5,5, float.PositiveInfinity};

    private bool changedToBox = false;

    private float boxDelay = 5f;

    #endregion

    #region ParallelTightCordonFormPhaseData

    private bool onParaTightCordonPhase = true;

    private string[] paraTightCordonTextBlocks = {"You can see how the police unit gets into the three sided box formation",
                                                "Another formation is the parallel tight cordon form",
                                                "Try saying the 'To Parallel Tight Cordon Form' Command"};

    private float[] paraTightCordonTextBlockTimes = {5,5, float.PositiveInfinity};

    private bool changedToParaTightCordon = false;

    private float paraTightCordonDelay = 5f;

    #endregion

    #region RotateLeftPhaseData

    private bool onRotateLeftPhase = true;

    private string[] rotateLeftTextBlocks = {"You can see how the police unit gets into the parallel tight cordon formation",
                                            "The police unit can change the direction it is facing as well",
                                            "Try telling the police unit to 'Rotate Left'"};

    private float[] rotateLeftTextBlockTimes = {5,5, float.PositiveInfinity};

    private bool rotatedLeft = false;

    private float rotateLeftDelay = 3f;

    #endregion

    #region RotateRightPhaseData

    private bool onRotateRightPhase = true;

    private string[] rotateRightTextBlocks = {"The police unit will continue to change the direction it faces until you tell it to stop",
                                            "You can also have the police unit change to the other direction as well",
                                            "Try telling the police unit to 'Rotate Right'"};

    private float[] rotateRightTextBlockTimes = {5,5, float.PositiveInfinity};

    private bool rotatedRight = false;

    private float rotateRightDelay = 3f;

    #endregion

    #region FirstHaltPhaseData

    private bool onFirstHaltPhase = true;

    private string[] firstHaltTextBlocks = {"The police unit will continue to change the direction it faces until you tell it to stop",
                                            "Let's try having the police unit stop changing its direction",
                                            "Try telling the police unit to 'Halt'"};

    private float[] firstHaltTextBlockTimes = {5,5, float.PositiveInfinity};

    private bool haltedFirstTime = false;

    private float haltedFirstTimeDelay = 0f;

    #endregion

    #region MoveForwardPhaseData

    private bool onMoveForwardPhase = true;

    private string[] moveForwardTextBlocks = {"The police unit stops changing its direction when you tell it to halt",
                                            "The police unit can also move forward",
                                            "Try telling the police unit to 'Advance'"};

    private float[] moveForwardTextBlockTimes = {5,5, float.PositiveInfinity};

    private bool hasMovedForward = false;

    private float moveForwardDelay = 3f;

    #endregion

    #region SecondHaltPhaseData

    private bool onSecondHaltPhase = true;

    private string[] secondHaltTextBlocks = {"The police unit will continue to move in the direction it faces until you tell it to stop",
                                            "Let's try having the police unit stop moving again",
                                            "Try telling the police unit to 'Halt'"};

    private float[] secondHaltTextBlockTimes = {5,5, float.PositiveInfinity};

    private bool haltedSecondTime = false;

    private float haltedSecondTimeDelay = 0f;

    #endregion

    #region EndTutorialPhaseData

    private bool onEndTutorialPhase = true;

    private string[] endTutorialTextBlocks = {"Good job getting through the tutorial",
                                            "The commands you have learned will be applied in the next part of the simulation",
                                            "Let's move to the next part of the simulation",
                                            ""};

    private float[] endTutorialTextBlockTimes = {3,5,5, float.PositiveInfinity};

    #endregion

    //Wedge, parallel loose cordon, box, tight cordon, rotate left, rotate right, first halt, move forward, second halt, end tutorial


    // Start is called before the first frame update
    private void Start()
    {
        cam.SetTarget(policeUnit);

        PoliceUnitVoiceController[] voiceControllers = Object.FindObjectsOfType<PoliceUnitVoiceController>();
        if(voiceControllers.Length > 0){
            PoliceUnitVoiceController voiceController = voiceControllers[0]; // grab the voice controller if there is one
            voiceController.OnToWedgeVoiceCommand += VoiceToWedgeResponse;
            voiceController.OnToParallelLooseCordonVoiceCommand += VoiceToParaLooseCordonResponse;
            voiceController.OnTo3SidedBoxVoiceCommand += VoiceToBoxResponse;
            voiceController.OnToParallelTightCordonVoiceCommand += VoiceToParaTightCordonResponse;
            voiceController.OnRotateCommand += VoiceRotateResponse;
            voiceController.OnHaltCommand += VoiceHaltResponse;
            voiceController.OnMoveForwardCommand += VoiceMoveForwardResponse;
        }
        UpdateDisplayText(rotateCameraTextBlocks);
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateTimer(Time.deltaTime);
        //Debug.Log("Timer: " + timer);

        if(onRotateCameraPhase){
            if(ShouldMoveToNextBlock(rotateCameraBlockTimes)){
                MoveToNextBlock();
                ResetTimer();
                UpdateDisplayText(rotateCameraTextBlocks);
                if(blockNo == (rotateCameraTextBlocks.Length - 1)){ // if moved into the move camera phase
                    shouldUpdateTimer = false;
                    originalCameraRotation = cameraTransform.rotation;
                }
            }
            else if(blockNo == (rotateCameraTextBlocks.Length - 1)){ // if at move camera phase
                if(rotatedCamera){ 
                    onRotateCameraPhase = false;
                    ResetTimer();
                    blockNo = 0;
                    UpdateDisplayText(wedgeTextBlocks);
                    shouldUpdateTimer = true;
                }
                else{
                    //Debug.Log("Angle: " + Quaternion.Angle(cameraTransform.rotation,originalCameraRotation));
                    if(Quaternion.Angle(cameraTransform.rotation,originalCameraRotation) > rotationMinAngle){
                        rotatedCamera = true;
                    }
                }
            }
        }
        else if(onWedgeFormationPhase){
            processPhase(wedgeTextBlocks,wedgeTextBlockTimes,changedToWedge,wedgeDelay,paraLooseCordonTextBlocks);
        }
        else if(onParaLooseCordonPhase){
            processPhase(paraLooseCordonTextBlocks, paraLooseCordonTextBlockTimes, changedToParaLooseCordon,paraLooseCordonDelay,boxTextBlocks);
        }
        else if(onBoxPhase){
            processPhase(boxTextBlocks, boxTextBlockTimes, changedToBox, boxDelay, paraTightCordonTextBlocks);
        }
        else if(onParaTightCordonPhase){
            processPhase(paraTightCordonTextBlocks, paraTightCordonTextBlockTimes, changedToParaTightCordon, paraTightCordonDelay, rotateLeftTextBlocks);
        }
        else if(onRotateLeftPhase){
            processPhase(rotateLeftTextBlocks, rotateLeftTextBlockTimes, rotatedLeft, rotateLeftDelay, rotateRightTextBlocks);
        }
        else if(onRotateRightPhase){
            processPhase(rotateRightTextBlocks, rotateRightTextBlockTimes, rotatedRight, rotateRightDelay, firstHaltTextBlocks);
        }
        else if(onFirstHaltPhase){
            processPhase(firstHaltTextBlocks, firstHaltTextBlockTimes, haltedFirstTime, haltedFirstTimeDelay, moveForwardTextBlocks);
        }
        else if(onMoveForwardPhase){
            processPhase(moveForwardTextBlocks, moveForwardTextBlockTimes, hasMovedForward, moveForwardDelay, secondHaltTextBlocks);
        }
        else if(onSecondHaltPhase){
            processPhase(secondHaltTextBlocks, secondHaltTextBlockTimes, haltedSecondTime, haltedSecondTimeDelay, endTutorialTextBlocks);
        }
        else if(onEndTutorialPhase){
            if(ShouldMoveToNextBlock(endTutorialTextBlockTimes)){
                MoveToNextBlock();
                ResetTimer();
                UpdateDisplayText(endTutorialTextBlocks);
                if(blockNo == (endTutorialTextBlocks.Length - 1)){ // if moved into command phase
                    var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                    entityManager.DestroyEntity(entityManager.UniversalQuery);
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                }
            }
        }
        

         // rotate left, rotate right, first halt, move forward, second halt, end tutorial
    }

    private void UpdateTimer(float timeToAdd){
        if(shouldUpdateTimer){
            timer += timeToAdd;
        }   
    }

    private void UpdateDisplayText(string[] textBlocks){
        displayText.text = textBlocks[blockNo];
    }

    private bool ShouldMoveToNextBlock(float[] times){
        return timer >= times[blockNo];
    }

    private bool FinishedDelay(float delay){
        return timer >= delay;
    }

    private void MoveToNextBlock(){
        blockNo += 1;
    }

    private void ResetTimer(){
        timer = 0f;
    }
    
    private void processPhase(string[] phaseTextBlocks, float[] phaseTextBlockTimes, bool commandDone, float delay, string[] nextPhaseTextBlocks){
        if(ShouldMoveToNextBlock(phaseTextBlockTimes)){
            MoveToNextBlock();
            ResetTimer();
            UpdateDisplayText(phaseTextBlocks);
            if(blockNo == (phaseTextBlocks.Length - 1)){ // if moved into command phase
                shouldUpdateTimer = false;
            }
        }
        else if(blockNo == (phaseTextBlocks.Length - 1)){ // if at the command phase
            if(commandDone && !processingDelay){ // if changed formation but haven't started the delay
                processingDelay = true;
                shouldUpdateTimer = true;             
            }
            else if(commandDone && processingDelay){ 
                if(FinishedDelay(delay)){
                    finishCurrentPhase();
                    ResetTimer();
                    blockNo = 0;
                    UpdateDisplayText(nextPhaseTextBlocks);
                    processingDelay = false;
                }
            }
        }
    }

    private void finishCurrentPhase(){
        if(onWedgeFormationPhase){
            onWedgeFormationPhase = false;
        }
        else if(onParaLooseCordonPhase){
            onParaLooseCordonPhase = false;
        }
        else if(onBoxPhase){
            onBoxPhase = false;
        }
        else if(onParaTightCordonPhase){
            onParaTightCordonPhase = false;
        }
        else if(onRotateLeftPhase){
            onRotateLeftPhase = false;
        }
        else if(onRotateRightPhase){
            onRotateRightPhase = false;
        }
        else if(onFirstHaltPhase){
            onFirstHaltPhase = false;
        }
        else if(onMoveForwardPhase){
            onMoveForwardPhase = false;
        }
        else if(onSecondHaltPhase){
            onSecondHaltPhase = false;
        }

    }

    private void VoiceToWedgeResponse(object sender, System.EventArgs eventArgs){
        if(onWedgeFormationPhase && !onRotateCameraPhase){
            changedToWedge = true;
        }
    }
    //Wedge, parallel loose cordon, box, tight cordon, rotate left, rotate right, first halt, move forward, second halt, end tutorial

    private void VoiceToParaLooseCordonResponse(object sender, System.EventArgs eventArgs){
        if(onParaLooseCordonPhase && !onWedgeFormationPhase){
            changedToParaLooseCordon = true;
        }
    }

    private void VoiceToBoxResponse(object sender, OnTo3SidedBoxEventArgs eventArgs){
        if(onBoxPhase && !onParaLooseCordonPhase){
            changedToBox = true;
        }
    }

    private void VoiceToParaTightCordonResponse(object sender, System.EventArgs eventArgs){
        if(onParaTightCordonPhase && !onBoxPhase){
            changedToParaTightCordon = true;
        }
    }

    private void VoiceRotateResponse(object sender, OnRotateEventArgs eventArgs){
        if(onRotateLeftPhase && eventArgs.RotateLeft && !onParaTightCordonPhase){
            rotatedLeft = true;
        }
        else if(onRotateRightPhase && ! eventArgs.RotateLeft && !onRotateLeftPhase){
            rotatedRight = true;
        }
    }

    private void VoiceHaltResponse(object sender, System.EventArgs eventArgs){
        if(onFirstHaltPhase && !onRotateRightPhase){
            haltedFirstTime = true;
        }
        else if(onSecondHaltPhase && !onMoveForwardPhase){
            haltedSecondTime = true;
        }
    }

    private void VoiceMoveForwardResponse(object sender, System.EventArgs eventArgs){
        if(onMoveForwardPhase && !onFirstHaltPhase){
            hasMovedForward = true;
        }
    }
}
