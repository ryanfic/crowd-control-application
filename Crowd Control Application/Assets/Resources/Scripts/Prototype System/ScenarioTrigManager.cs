using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System;
using TMPro;
using RTS_Cam;
using UnityEngine.SceneManagement;



public class ScenarioTrigManager : MonoBehaviour
{

    public event EventHandler HaltPoliceEvent;

    public Transform policeUnit;
    public Transform intersectionTransform;
    public GameObject intersectionGO;
    public Transform crowdSpawnTransform;
    public GameObject crowdSpawnerGO;
    public Transform beforeExitLoc;
    public Transform afterExitLoc;
    public Transform exitLoc;
    public GameObject exitGO;
    public RTS_Camera cam;

    public TMP_Text displayText;

    public float timer = 0f;

    private bool shouldUpdateTimer = true;
    private bool processingDelay = false;

    private int blockNo = 0;


    #region ApproachIntersectionPhaseData

    private bool onApproachPhase = true;

    private string[] approachTextBlocks = {"You can see buildings to either side of the police unit", 
                                            "Here, you can see an intersection",
                                            "Please move into the intersection"};

    private float[] approachTextBlockTimes = {5, 12, float.PositiveInfinity};

    private bool approachedInt = false;

    //private float approachDelay = 3f;

    #endregion

    #region BoxPhaseData

    private bool onBoxPhase = true;

    private string[] boxTextBlocks = {"Oh no, a riot appears to be forming near the intersection", 
                                        "Quickly, have the police unit change to the three sided box formation"};

    private float[] boxTextBlockTimes = {12, float.PositiveInfinity};

    private bool changedToBox = false;

    private float boxDelay = 3f;

    #endregion

    #region RepelRiotersPhaseData

    private bool onRepelPhase = true;

    private string[] repelTextBlocks = {"Have the police unit enter the intersection to repel the rioters"};

    private float[] repelTextBlockTimes = {float.PositiveInfinity};

    private bool repelledRioters = false;

    #endregion

    #region CordonPhaseData

    private bool onCordonPhase = true;

    private string[] cordonTextBlocks = {"Good job repelling the rioters", 
                                        "Before exiting the intersection, have the police unit change to the parallel tight cordon formation"};

    private float[] cordonTextBlockTimes = {3, float.PositiveInfinity};

    private bool changedToCordon = false;

    private float cordonDelay = 3f;

    #endregion

    #region GoToExitPhaseData

    private bool onGoToExitPhase = true;

    private string[] goToExitTextBlocks = { "",
                                            "Here is the exit",
                                            "Here is the exit",
                                            "Please move the police unit to the exit",
                                            "Please move the police unit to the exit"};

    private float[] goToExitTextBlockTimes = {0, 8, 12, 8, float.PositiveInfinity};

    private bool gotToExit = false;

    #endregion

    #region EndPhaseData

    private bool onEndPhase = true;

    private string[] endPhaseTextBlocks = {"Thank you for trying the Crowd Control Software",
                                        "If you could provide some feedback on the software, that would be much appreciated",
                                        ""};

    private float[] endPhaseTextBlockTimes = {5, 10, float.PositiveInfinity};

    //private bool changedToWedge = false;

    //private float wedgeDelay = 3f;

    #endregion

    // Approach, box, repel, go to exit, end


    // Start is called before the first frame update
    private void Start()
    {
        cam.SetTarget(policeUnit);
        //World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<CrowdCountingSystem>().NoCrowdLeftEvent += RepelledRiotersResponse;

        PoliceUnitVoiceController[] voiceControllers = UnityEngine.Object.FindObjectsOfType<PoliceUnitVoiceController>();
        if(voiceControllers.Length > 0){
            PoliceUnitVoiceController voiceController = voiceControllers[0]; // grab the voice controller if there is one
            voiceController.OnTo3SidedBoxVoiceCommand += VoiceToBoxResponse;
            voiceController.OnToParallelTightCordonVoiceCommand += VoiceToCordonResponse;
        }

        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PoliceUnitRemoveMovementSystem>().ConnectToPrototypeManager();

        UpdateDisplayText(approachTextBlocks);
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateTimer(Time.deltaTime);
        //Debug.Log("Timer: " + timer);
        if(onApproachPhase){
            if(ShouldMoveToNextBlock(approachTextBlockTimes)){
                MoveToNextBlock();
                ResetTimer();
                UpdateDisplayText(approachTextBlocks);
                if(blockNo == 1){
                    cam.SetTarget(intersectionTransform);//set camera to follow intersection trigger
                }
                else if(blockNo == (approachTextBlocks.Length - 1)){ // if moved into the move camera phase
                    shouldUpdateTimer = false;
                    cam.SetTarget(policeUnit);//set camera to follow police
                    intersectionGO.SetActive(true);
                }
            }
            else if(blockNo == (approachTextBlocks.Length - 1)){ // if at move camera phase
                if(approachedInt){ 
                    onApproachPhase = false;
                    ResetTimer();
                    blockNo = 0;
                    UpdateDisplayText(boxTextBlocks);
                    shouldUpdateTimer = true;
                    HaltPoliceEvent?.Invoke(this, EventArgs.Empty);//halt the police movement
                    cam.SetTarget(crowdSpawnTransform);//change camera to view crowd agents
                    crowdSpawnerGO.SetActive(true);//spawn rioters
                    //World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<CrowdCountingSystem>().setOnCrowdRepelStep();//Tell system to count rioters
                }
            }
        }
        else if(onBoxPhase){
            if(ShouldMoveToNextBlock(boxTextBlockTimes)){
                MoveToNextBlock();
                ResetTimer();
                UpdateDisplayText(boxTextBlocks);
                if(blockNo == 1){
                    cam.SetTarget(policeUnit);//set camera to follow the police unit again
                    intersectionGO.SetActive(false);
                    //World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<CrowdCountingSystem>().setOnCrowdRepelStep();//Tell system to count rioters
                }
                else if(blockNo == (boxTextBlocks.Length - 1)){ // if moved into the move camera phase
                    shouldUpdateTimer = false;
                }
            }
            else if(blockNo == (boxTextBlocks.Length - 1)){ // if at move camera phase
                if(changedToBox && !processingDelay){ // if changed formation but haven't started the delay
                    processingDelay = true;
                    shouldUpdateTimer = true;             
                }
                else if(changedToBox && processingDelay){ 
                    if(FinishedDelay(boxDelay)){
                        onBoxPhase = false;
                        ResetTimer();
                        blockNo = 0;
                        UpdateDisplayText(repelTextBlocks);
                        processingDelay = false;
                    }
                }
            }
        }
        else if(onRepelPhase){
            //World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<CrowdCountingSystem>().setOnCrowdRepelStep();//Tell system to count rioters
            Debug.Log("Count " + World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<CrowdCountingSystem>().checkCrowdNumber());
            if(World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<CrowdCountingSystem>().checkCrowdNumber() == 0){
                repelledRioters = true;
            }
            if(repelledRioters){
                onRepelPhase = false;
                ResetTimer();
                blockNo = 0;
                UpdateDisplayText(cordonTextBlocks);
                shouldUpdateTimer = true;
                //cam.SetTarget(intersectionTransform);//change camera to view crowd agents
            }
        }
        else if(onCordonPhase){
            if(ShouldMoveToNextBlock(cordonTextBlockTimes)){
                MoveToNextBlock();
                ResetTimer();
                UpdateDisplayText(cordonTextBlocks);
                if(blockNo == (cordonTextBlocks.Length - 1)){ // if moved into the move camera phase
                    shouldUpdateTimer = false;
                }
            }
            else if(blockNo == (cordonTextBlocks.Length - 1)){ // if at move camera phase
                if(changedToCordon && !processingDelay){ // if changed formation but haven't started the delay
                    processingDelay = true;
                    shouldUpdateTimer = true;             
                }
                else if(changedToCordon && processingDelay){ 
                    if(FinishedDelay(boxDelay)){
                        onCordonPhase = false;
                        ResetTimer();
                        blockNo = 0;
                        UpdateDisplayText(goToExitTextBlocks);
                        processingDelay = false;
                    }
                }
            }
        }
        else if(onGoToExitPhase){
            if(ShouldMoveToNextBlock(goToExitTextBlockTimes)){
                MoveToNextBlock();
                ResetTimer();
                UpdateDisplayText(goToExitTextBlocks);
                if(blockNo == 1){
                    HaltPoliceEvent?.Invoke(this, EventArgs.Empty);//halt the police movement
                    cam.SetTarget(beforeExitLoc);//set camera to follow the exit trigger
                    exitGO.SetActive(true);
                }
                else if(blockNo == 2){
                    HaltPoliceEvent?.Invoke(this, EventArgs.Empty);//halt the police movement
                    cam.SetTarget(exitLoc);//set camera to follow the exit trigger
                }
                else if(blockNo == 3){
                    HaltPoliceEvent?.Invoke(this, EventArgs.Empty);//halt the police movement
                    cam.SetTarget(afterExitLoc);//set camera to follow the exit trigger
                }
                else if(blockNo == (goToExitTextBlocks.Length - 1)){ // if moved into the move camera phase
                    shouldUpdateTimer = false;
                    cam.SetTarget(policeUnit);//set camera to follow police
                }
            }
            else if(blockNo == (goToExitTextBlocks.Length - 1)){ // if at move camera phase
                if(gotToExit){ 
                    onGoToExitPhase = false;
                    ResetTimer();
                    blockNo = 0;
                    UpdateDisplayText(endPhaseTextBlocks);
                    shouldUpdateTimer = true;
                }
            }
        }
        else if(onEndPhase){
            if(ShouldMoveToNextBlock(endPhaseTextBlockTimes)){
                MoveToNextBlock();
                ResetTimer();
                UpdateDisplayText(endPhaseTextBlocks);
                if(blockNo == (endPhaseTextBlocks.Length - 1)){ // if moved into command phase
                    var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                    entityManager.DestroyEntity(entityManager.UniversalQuery);
                    SceneManager.LoadScene(0);
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
    
    public void ApproachIntersectionTriggered(){
        //OnApproachIntersectionTriggered?.Invoke(this, EventArgs.Empty);
        approachedInt = true;
    }

    public void ExitTriggered(){
        //OnApproachIntersectionTriggered?.Invoke(this, EventArgs.Empty);
        gotToExit = true;
    }

    private void VoiceToBoxResponse(object sender, OnTo3SidedBoxEventArgs eventArgs){
        if(onBoxPhase && !onApproachPhase){
            changedToBox = true;
        }
    }   
    private void VoiceToCordonResponse(object sender, System.EventArgs eventArgs){
        if(onCordonPhase && !onRepelPhase){
            changedToCordon = true;
        }
    }    
    private void RepelledRiotersResponse(object sender, System.EventArgs eventArgs){
            repelledRioters = true;
    } 
}
