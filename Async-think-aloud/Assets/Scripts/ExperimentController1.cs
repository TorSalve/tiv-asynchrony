/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Klak.Motion;
using UnityEngine.UIElements;
using RootMotion.Demos;

public class ExperimentController1 : MonoBehaviour
{
    [Header("Participant Info")]
    public int participantID = 0;
    public enum GenderMatchedAvatar { woman = 0, man = 1 };
    public GenderMatchedAvatar genderMatchedAvatar;

    [Header("Conditions")]
    public int layoutBlockNum = 0;
    /// <summary> a Latin square for 4 conditions
    /// 4 layouts of physical targets
    /// A C B D | P0, P4, P8 
    /// B A D C | P1, P5, P9
    /// C D A B | P2, P6, P10
    /// D B C A | P3, P7, P11
    /// </summary>
    int[,] latinSquare4x4 = new int[4, 4] { {1, 3, 2, 4},
                                            {2, 1, 4, 3},
                                            {3, 4, 1, 2},
                                            {4, 2, 3, 1} };
    public enum VisuomotorType { Sync = 1, Async = 2, Async_begin = 3, Async_end = 4 };
    public VisuomotorType vmType;
    public int currentConditionNum;
    public float currentTime;
    public bool isQuestionnaireDone;

    private bool isAvatarRunning;
    private bool isQuestionnaireRunning;
    public float exposureDuration = 180f;

    [Header("Avatars")]
    public GameObject femaleAvatar;
    public GameObject maleAvatar;
    public GameObject syncAvatar;
    private SkinnedMeshRenderer[] syncAvatarSMRs;
    private SkinnedMeshRenderer rocketboxSMR;
    // for async avatar
    public GameObject leftArmBM;
    public GameObject rightArmBM;

    [Header("Procedure")]
    public GameObject pointer;
    public GameObject startBox;
    public GameObject mainInstructionsCanvas;
    public TMP_Text mainInstructions;
    private QuestionnaireController questionnaireController;
    public bool isStartFlagOn;
    private bool isCountDown;

    public VRIKCalibrationBasic ikCalibration;


    void Start()
    {
        startBox.SetActive(false);
        pointer.GetComponent<Renderer>().enabled = false;

        syncAvatar = (genderMatchedAvatar == GenderMatchedAvatar.woman) ? femaleAvatar : maleAvatar;
        ikCalibration = syncAvatar.GetComponent<VRIKCalibrationBasic>();
        if (genderMatchedAvatar == GenderMatchedAvatar.woman) maleAvatar.SetActive(false);
        else femaleAvatar.SetActive(false);

        // get mesh renderers for transitions between avatar (hand tracking) and controllers
        //syncAvatarSMRs = syncAvatar.GetComponentsInChildren<SkinnedMeshRenderer>();
        rocketboxSMR = syncAvatar.GetComponentInChildren<SkinnedMeshRenderer>();

        leftArmBM.transform.localPosition = Vector3.zero;
        leftArmBM.GetComponent<BrownianMotion>().enabled = false;
        rightArmBM.transform.localPosition = Vector3.zero;
        rightArmBM.GetComponent<BrownianMotion>().enabled = false;

        // enable the gender matched avatar
        questionnaireController = this.GetComponent<QuestionnaireController>();
        PrepareCondition();
        //foreach (var t in syncAvatarSMRs) t.enabled = false;
        rocketboxSMR.enabled = false;

        mainInstructions.text = mainInstructions.text + "\n\n" +
            "Participant ID: " + participantID.ToString() + ", " + genderMatchedAvatar + " avatar";

    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Q))
        {
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
        }

        if (Input.GetKeyDown(KeyCode.S) || isStartFlagOn)
        {
            isStartFlagOn = false;
            startBox.SetActive(false);
            pointer.GetComponent<Renderer>().enabled = false;
            StartCoroutine(StartCondition());
        }
        else
        {
            if (OVRPlugin.GetHandTrackingEnabled() && !isCountDown && !isAvatarRunning && !isQuestionnaireRunning)
            {
                // switch between controller and hand tracking.
                startBox.SetActive(true);
                pointer.GetComponent<Renderer>().enabled = true;
            }
        }

        //Debug.LogWarning(OVRPlugin.GetHandTrackingEnabled() + "hand");
        // holding controllers --> false
        // untracked like occlusion --> false
        // hands in front of the HMD --> true

        if (currentConditionNum < 4)
        {
            if (isCountDown)
            {
                currentTime += Time.deltaTime;
                mainInstructions.text = "Stretch out your arms for calibration. The study will begin in " + (5f - currentTime).ToString("F0") + " seconds.";
            }
            // switch from controllers to hands

            if (isAvatarRunning)
            { 
                currentTime += Time.deltaTime;

                // difference between the four conditions.
                if (currentTime < exposureDuration)
                {                
                    if (vmType == VisuomotorType.Sync)
                    {

                    }
                    else if (vmType == VisuomotorType.Async)
                    {
                        //if (leftArmBM.transform.parent == null) AssignLBMParent();
                        //if (rightArmBM.transform.parent == null) AssignRBMParent();

                        if (!leftArmBM.GetComponent<BrownianMotion>().enabled)
                            leftArmBM.GetComponent<BrownianMotion>().enabled = true;
                        
                        if (!rightArmBM.GetComponent<BrownianMotion>().enabled)
                            rightArmBM.GetComponent<BrownianMotion>().enabled = true;
                    }
                    else if (vmType == VisuomotorType.Async_begin)
                    {
                        if (currentTime < (exposureDuration / 2))
                        {
                            //if (leftArmBM.transform.parent == null) AssignLBMParent();
                            //if (rightArmBM.transform.parent == null) AssignRBMParent();

                            if (!leftArmBM.GetComponent<BrownianMotion>().enabled)
                                leftArmBM.GetComponent<BrownianMotion>().enabled = true;

                            if (!rightArmBM.GetComponent<BrownianMotion>().enabled)
                                rightArmBM.GetComponent<BrownianMotion>().enabled = true;
                        }
                        else
                        {
                            if (leftArmBM.GetComponent<BrownianMotion>().enabled)
                            {
                                leftArmBM.transform.localPosition = Vector3.zero;
                                leftArmBM.GetComponent<BrownianMotion>().enabled = false;
                            }
                            if (rightArmBM.GetComponent<BrownianMotion>().enabled)
                            {
                                rightArmBM.transform.localPosition = Vector3.zero;
                                rightArmBM.GetComponent<BrownianMotion>().enabled = false;
                            }
                        }
                    }
                    else if (vmType == VisuomotorType.Async_end)
                    {
                        if ((exposureDuration / 2) < currentTime)
                        {
                            //if (leftArmBM.transform.parent == null) AssignLBMParent();
                            //if (rightArmBM.transform.parent == null) AssignRBMParent();

                            if (!leftArmBM.GetComponent<BrownianMotion>().enabled)
                                leftArmBM.GetComponent<BrownianMotion>().enabled = true;

                            if (!rightArmBM.GetComponent<BrownianMotion>().enabled)
                                rightArmBM.GetComponent<BrownianMotion>().enabled = true;
                        }
                    }
                }
                else
                {

                    Debug.LogWarning(2);
                    if (leftArmBM.GetComponent<BrownianMotion>().enabled)
                    {
                        leftArmBM.transform.localPosition = Vector3.zero;
                        leftArmBM.GetComponent<BrownianMotion>().enabled = false;
                    }
                    if (rightArmBM.GetComponent<BrownianMotion>().enabled)
                    {
                        rightArmBM.transform.localPosition = Vector3.zero;
                        rightArmBM.GetComponent<BrownianMotion>().enabled = false;
                    }

                    // stop showing avatar, disable avatars
                    //foreach (var t in syncAvatarSMRs) t.enabled = false;
                    rocketboxSMR.enabled = false;

                    // enable questionnaire gameobjects
                    isAvatarRunning = false;
                    isQuestionnaireRunning = true;
                    questionnaireController.questionnaireCanvas.SetActive(true);
                    questionnaireController.InitializeQuestionnaire();
                    mainInstructionsCanvas.SetActive(false);
                    // highlight the position of the right controller, if needed
                }
            }

            if (isQuestionnaireRunning)
            {
                currentTime += Time.deltaTime;

                if (isQuestionnaireDone)
                {
                    // reset variables of the avatar and questionnaire part
                    isQuestionnaireDone = false;
                    Debug.LogWarning("Questionnaire is done");
                    questionnaireController.questionnaireCanvas.SetActive(false);
                    isQuestionnaireRunning = false;
                    currentConditionNum += 1;
                    currentTime = 0f;
                    PrepareCondition();
                    mainInstructionsCanvas.SetActive(true);
                    mainInstructions.text = "Please put down your controllers and then touch 'start' with the pink dot.";
                }
            }
        }
        else
        {
            Debug.LogWarning("The end.");

            mainInstructions.text = "The end!";
        }
    }

    IEnumerator StartCondition()
    {
        //Debug.LogWarning("Start the current condition!");
        isCountDown = true;
        //OVRPlugin.ResetBodyTrackingCalibration();
        ikCalibration.calibrateAvatar = true;

        yield return new WaitForSeconds(5f);
        //OVRPlugin.ResetBodyTrackingCalibration();
        ikCalibration.calibrateAvatar = true;

        isCountDown = false;
        currentTime = 0f;
        isAvatarRunning = true;
        mainInstructions.text = "Please tilt your head downwards as if looking down at your body.";
        //foreach (var t in syncAvatarSMRs) t.enabled = true;
        rocketboxSMR.enabled = true;

        yield return 0;
    }

    private void AssignLBMParent()
    {
        leftArmBM.transform.SetParent(syncAvatar.transform.Find("Bones").transform);

        GameObject.Find("FullBody_LeftShoulder").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftScapula").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftArmUpper").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftArmLower").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftHandWristTwist").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftHandPalm").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftHandWrist").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftHandThumbMetacarpal").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftHandThumbProximal").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftHandThumbDistal").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftHandThumbTip").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftHandIndexMetacarpal").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftHandIndexProximal").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftHandIndexIntermediate").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftHandIndexDistal").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftHandIndexTip").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftHandMiddleMetacarpal").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftHandMiddleProximal").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftHandMiddleIntermediate").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftHandMiddleDistal").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftHandMiddleTip").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftHandRingMetacarpal").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftHandRingProximal").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftHandRingIntermediate").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftHandRingDistal").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftHandRingTip").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftHandLittleMetacarpal").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftHandLittleProximal").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftHandLittleIntermediate").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftHandLittleDistal").transform.SetParent(leftArmBM.transform);
        GameObject.Find("FullBody_LeftHandLittleTip").transform.SetParent(leftArmBM.transform);
    }

    private void AssignRBMParent()
    {
        rightArmBM.transform.SetParent(syncAvatar.transform.Find("Bones").transform);

        GameObject.Find("FullBody_RightShoulder").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightScapula").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightArmUpper").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightArmLower").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightHandWristTwist").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightHandPalm").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightHandWrist").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightHandThumbMetacarpal").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightHandThumbProximal").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightHandThumbDistal").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightHandThumbTip").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightHandIndexMetacarpal").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightHandIndexProximal").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightHandIndexIntermediate").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightHandIndexDistal").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightHandIndexTip").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightHandMiddleMetacarpal").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightHandMiddleProximal").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightHandMiddleIntermediate").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightHandMiddleDistal").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightHandMiddleTip").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightHandRingMetacarpal").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightHandRingProximal").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightHandRingIntermediate").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightHandRingDistal").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightHandRingTip").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightHandLittleMetacarpal").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightHandLittleProximal").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightHandLittleIntermediate").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightHandLittleDistal").transform.SetParent(rightArmBM.transform);
        GameObject.Find("FullBody_RightHandLittleTip").transform.SetParent(rightArmBM.transform);
    }

    private void PrepareCondition()
    {
        // also ajust sync/async avatar options here
        if (latinSquare4x4[participantID % 4, currentConditionNum % 4] == 1)
        {
            vmType = VisuomotorType.Sync;
        }
        else if (latinSquare4x4[participantID % 4, currentConditionNum % 4] == 2)
        {
            vmType = VisuomotorType.Async;
        }
        else if (latinSquare4x4[participantID % 4, currentConditionNum % 4] == 3)
        {
            vmType = VisuomotorType.Async_begin;
        }
        else if (latinSquare4x4[participantID % 4, currentConditionNum % 4] == 4)
        {
            vmType = VisuomotorType.Async_end;
        }
        Debug.LogWarning("The current condition is: " + vmType.ToString());
    }
}*/
