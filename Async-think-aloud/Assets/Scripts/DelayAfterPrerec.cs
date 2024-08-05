/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Klak.Motion;
using RootMotion.Demos;
using RootMotion.FinalIK;

public class DelayAfterPrerec : MonoBehaviour
{
    [Header("Participant Info")]
    public int participantID = 0;

    public enum VisuomotorType { Sync = 1, Async = 2, Delay = 3, Prerec = 4 };

    [Header("Conditions")]
    public VisuomotorType vmType;
    private VisuomotorType selectedVmType;
    public float currentTime;

    private bool isAvatarRunning;
    private bool hasExperimentEnded = false;
    public float exposureDuration = 180f;
    private int currentConditionIndex = 0;
    private VisuomotorType[] conditions;

    [Header("Avatars")]
    public GameObject avatar;
    public GameObject prerecordedAvatar;
    public GameObject syncAvatar;
    private SkinnedMeshRenderer rocketboxSMR;

    public GameObject leftArmBM;
    public GameObject rightArmBM;

    [Header("Hand Tracking")]
    public GameObject leftHandTracking;
    public GameObject rightHandTracking;

    [Header("Finger Tracking")]
    public List<string> fingerBonesID;
    public List<Transform> leftHandFingers;
    public List<Transform> rightHandFingers;

    private List<Transform> activeLeftHandFingers;
    private List<Transform> activeRightHandFingers;

    [Header("Procedure")]
    public GameObject pointer;
    public GameObject startBox;
    public GameObject mainInstructionsCanvas;
    public TMP_Text mainInstructions;
    public bool isStartFlagOn;
    private bool isCountDown;
    private float countDownTime;

    public VRIKCalibrationBasic ikCalibration;

    private Queue<MovementData> movementQueue = new Queue<MovementData>();
    public float delayTime = 1.5f;

    private readonly Vector3 leftHandRotationOffset = new Vector3(180, 0, 180);
    private readonly Vector3 rightHandRotationOffset = new Vector3(180, 0, 0);

    void Start()
    {
        fingerBonesID = new List<string> {
            "Hand_Thumb0", "Hand_Thumb2", "Hand_Thumb3",
            "Hand_Index1", "Hand_Index2", "Hand_Index3",
            "Hand_Middle1", "Hand_Middle2", "Hand_Middle3",
            "Hand_Ring1", "Hand_Ring2", "Hand_Ring3",
            "Hand_Pinky1", "Hand_Pinky2", "Hand_Pinky3"
        };

        startBox.SetActive(false);
        pointer.GetComponent<Renderer>().enabled = false;

        // Define conditions in a Latin square order
        int[,] latinSquare = {
            { 1, 2, 3, 4 },
            { 2, 3, 4, 1 },
            { 3, 4, 1, 2 },
            { 4, 1, 2, 3 }
        };

        int participantOffset = participantID % 4;
        conditions = new VisuomotorType[4];
        for (int i = 0; i < 4; i++)
        {
            conditions[i] = (VisuomotorType)latinSquare[participantOffset, i];
        }

        PrepareCondition();

        activeLeftHandFingers = leftHandFingers;
        activeRightHandFingers = rightHandFingers;

        ikCalibration = syncAvatar.GetComponent<VRIKCalibrationBasic>();
        DisableAllAvatarsExcept(syncAvatar);

        rocketboxSMR = syncAvatar.GetComponentInChildren<SkinnedMeshRenderer>();

        leftArmBM.transform.localPosition = Vector3.zero;
        leftArmBM.GetComponent<BrownianMotion>().enabled = false;
        rightArmBM.transform.localPosition = Vector3.zero;
        rightArmBM.GetComponent<BrownianMotion>().enabled = false;

        selectedVmType = vmType;
        rocketboxSMR.enabled = false;

        mainInstructions.text = mainInstructions.text + "\n\n" +
            "Participant ID: " + participantID.ToString() + " avatar";

        if (vmType == VisuomotorType.Delay)
        {
            InvokeRepeating("UpdateBuffer", 0, Time.fixedDeltaTime);
        }

        ResetTracking();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S) || isStartFlagOn)
        {
            isStartFlagOn = false;
            startBox.SetActive(false);
            pointer.GetComponent<Renderer>().enabled = false;
            StartCoroutine(StartCondition());
        }
        else
        {
            if (!hasExperimentEnded && OVRPlugin.GetHandTrackingEnabled() && !isCountDown && !isAvatarRunning)
            {
                startBox.SetActive(true);
                pointer.GetComponent<Renderer>().enabled = true;
            }
        }

        if (isCountDown)
        {
            currentTime -= Time.deltaTime;
            int displayTime = Mathf.CeilToInt(currentTime);
            mainInstructions.text = countDownTime > 0 ? "Stretch out your arms for calibration. The study will begin in " + displayTime + " seconds." : "Now you will move to the next condition. The study will start in " + displayTime + " seconds.";
            if (currentTime <= 0)
            {
                isCountDown = false;
                if (countDownTime > 0)
                {
                    StartCalibration();
                }
                else
                {
                    ShowStartButton();
                }
            }
        }

        if (isAvatarRunning)
        {
            currentTime += Time.deltaTime;

            if (currentTime < exposureDuration)
            {
                if (selectedVmType == VisuomotorType.Sync)
                {
                    // Sync condition, do nothing special
                }
                else if (selectedVmType == VisuomotorType.Async)
                {
                    EnableBrownianMotion();
                }
                else if (selectedVmType == VisuomotorType.Delay)
                {
                    ApplyDelayedMovements();
                }
                else if (selectedVmType == VisuomotorType.Prerec)
                {
                    // Prerecorded condition, avatars remain static
                }
            }
            else
            {
                DisableBrownianMotion();
                rocketboxSMR.enabled = false;

                isAvatarRunning = false;
                currentConditionIndex++;
                if (currentConditionIndex < conditions.Length)
                {
                    DisplayNextConditionMessage();
                }
                else
                {
                    DisplayEndMessage();
                }
            }
        }

        if (!isAvatarRunning && currentConditionIndex > 0 && selectedVmType == VisuomotorType.Prerec)
        {
            // Ensure tracking is reset for conditions after Prerec
            RefreshAllTrackingAndComponents();
        }
    }

    IEnumerator StartCondition()
    {
        StartCountdown(10, true);
        yield return new WaitUntil(() => !isCountDown);

        ikCalibration.calibrateAvatar = true;

        currentTime = 0f;
        isAvatarRunning = true;
        mainInstructions.text = "Please tilt your head downwards as if looking down at your body.";
        rocketboxSMR.enabled = true;

        if (selectedVmType == VisuomotorType.Prerec)
        {
            DisableAvatarTracking();
        }
        else
        {
            RefreshAllTrackingAndComponents(); // Ensure tracking is reset at the start of each condition
        }
    }

    private void StartCountdown(float duration, bool calibration)
    {
        currentTime = duration;
        countDownTime = calibration ? 10 : 0;
        isCountDown = true;
    }

    private void StartCalibration()
    {
        ikCalibration.calibrateAvatar = true;
    }

    private void DisplayEndMessage()
    {
        mainInstructionsCanvas.SetActive(true);
        mainInstructions.text = "The end of the experiment. Thank you for your participation.";

        startBox.SetActive(false);
        hasExperimentEnded = true;
    }

    private void DisplayNextConditionMessage()
    {
        mainInstructionsCanvas.SetActive(true);
        StartCountdown(10, false);
    }

    private void ShowStartButton()
    {
        mainInstructions.text = "Press the start button when you are ready.";
        startBox.SetActive(true);
        pointer.GetComponent<Renderer>().enabled = true;
        PrepareCondition();  // Ensure the next condition is prepared when the start button is shown
    }

    private void PrepareCondition()
    {
        if (currentConditionIndex < conditions.Length)
        {
            vmType = conditions[currentConditionIndex];
            selectedVmType = vmType;
            syncAvatar = (vmType == VisuomotorType.Prerec) ? prerecordedAvatar : avatar;
            DisableAllAvatarsExcept(syncAvatar);
            rocketboxSMR = syncAvatar.GetComponentInChildren<SkinnedMeshRenderer>();
            rocketboxSMR.enabled = false;

            if (vmType == VisuomotorType.Delay)
            {
                InvokeRepeating("UpdateBuffer", 0, Time.fixedDeltaTime);
            }
            else
            {
                CancelInvoke("UpdateBuffer");
            }

            if (vmType != VisuomotorType.Prerec)
            {
                EnableAvatarTracking(); // Re-enable tracking if not in Prerec condition
            }
        }
    }

    private void EnableAvatarTracking()
    {
        if (syncAvatar.GetComponent<VRIK>() != null)
        {
            syncAvatar.GetComponent<VRIK>().enabled = true;
        }
        if (leftHandTracking != null)
        {
            leftHandTracking.SetActive(true);
        }
        if (rightHandTracking != null)
        {
            rightHandTracking.SetActive(true);
        }

        // Reassign the fingers after enabling tracking
        AssignFingers();
    }

    private void ResetTracking()
    {
        // Reset hand positions and rotations
        leftHandTracking.transform.localPosition = Vector3.zero;
        leftHandTracking.transform.localRotation = Quaternion.identity;
        rightHandTracking.transform.localPosition = Vector3.zero;
        rightHandTracking.transform.localRotation = Quaternion.identity;

        // Clear any movement data queues if necessary
        movementQueue.Clear();

        // Reassign the fingers after resetting tracking
        AssignFingers();
    }

    private void RefreshAllTrackingAndComponents()
    {
        // Reset tracking
        ResetTracking();

        // Reinitialize and reassign components
        DisableAllAvatarsExcept(syncAvatar);
        EnableAvatarTracking();

        // Reinitialize movement queue
        movementQueue.Clear();
        if (vmType == VisuomotorType.Delay)
        {
            InvokeRepeating("UpdateBuffer", 0, Time.fixedDeltaTime);
        }
        else
        {
            CancelInvoke("UpdateBuffer");
        }

        // Calibrate avatar again
        ikCalibration = syncAvatar.GetComponent<VRIKCalibrationBasic>();
        ikCalibration.calibrateAvatar = true;

        // Ensure finger tracking is re-enabled
        activeLeftHandFingers = leftHandFingers;
        activeRightHandFingers = rightHandFingers;

        // Ensure delay logic is properly reset
        if (selectedVmType == VisuomotorType.Delay)
        {
            movementQueue.Clear();
            CancelInvoke("UpdateBuffer");
            InvokeRepeating("UpdateBuffer", 0, Time.fixedDeltaTime);
        }
    }

    private void AssignFingers()
    {
        leftHandFingers = AssignHandFingers(leftHandTracking);
        rightHandFingers = AssignHandFingers(rightHandTracking);
    }

    private List<Transform> AssignHandFingers(GameObject handTracking)
    {
        List<Transform> handFingers = new List<Transform>();
        foreach (string boneID in fingerBonesID)
        {
            Transform bone = handTracking.transform.FindChildRecursiveAlternative(boneID);
            if (bone != null)
            {
                handFingers.Add(bone);
            }
        }
        return handFingers;
    }

    void UpdateBuffer()
    {
        if (movementQueue.Count > delayTime / Time.fixedDeltaTime)
        {
            movementQueue.Dequeue();
        }

        MovementData currentMovement = new MovementData()
        {
            leftHandPosition = leftHandTracking.transform.position,
            leftHandRotation = Quaternion.Normalize(leftHandTracking.transform.rotation),
            rightHandPosition = rightHandTracking.transform.position,
            rightHandRotation = Quaternion.Normalize(rightHandTracking.transform.rotation),
            leftHandFingerRotations = new Quaternion[fingerBonesID.Count],
            rightHandFingerRotations = new Quaternion[fingerBonesID.Count]
        };

        for (int i = 0; i < fingerBonesID.Count; i++)
        {
            currentMovement.leftHandFingerRotations[i] = leftHandTracking.transform.FindChildRecursiveAlternative(fingerBonesID[i]).rotation;
            currentMovement.rightHandFingerRotations[i] = rightHandTracking.transform.FindChildRecursiveAlternative(fingerBonesID[i]).rotation;
        }

        movementQueue.Enqueue(currentMovement);
    }

    void ApplyDelayedMovements()
    {
        if (movementQueue.Count > 0)
        {
            MovementData delayedMovement = movementQueue.Peek();

            Quaternion correctedLeftHandRotation = delayedMovement.leftHandRotation * Quaternion.Euler(leftHandRotationOffset);
            Quaternion correctedRightHandRotation = delayedMovement.rightHandRotation * Quaternion.Euler(rightHandRotationOffset);

            syncAvatar.GetComponent<VRIK>().solver.leftArm.target.position = delayedMovement.leftHandPosition;
            syncAvatar.GetComponent<VRIK>().solver.leftArm.target.rotation = correctedLeftHandRotation;
            syncAvatar.GetComponent<VRIK>().solver.rightArm.target.position = delayedMovement.rightHandPosition;
            syncAvatar.GetComponent<VRIK>().solver.rightArm.target.rotation = correctedRightHandRotation;

            for (int i = 0; i < fingerBonesID.Count; i++)
            {
                if (activeLeftHandFingers != null && activeLeftHandFingers[i] != null)
                {
                    activeLeftHandFingers[i].rotation = delayedMovement.leftHandFingerRotations[i];
                }

                if (activeRightHandFingers != null && activeRightHandFingers[i] != null)
                {
                    activeRightHandFingers[i].rotation = delayedMovement.rightHandFingerRotations[i];
                }
            }
        }
    }

    private void EnableBrownianMotion()
    {
        if (!leftArmBM.GetComponent<BrownianMotion>().enabled)
            leftArmBM.GetComponent<BrownianMotion>().enabled = true;

        if (!rightArmBM.GetComponent<BrownianMotion>().enabled)
            rightArmBM.GetComponent<BrownianMotion>().enabled = true;
    }

    private void DisableBrownianMotion()
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

    private void DisableAvatarTracking()
    {
        if (syncAvatar.GetComponent<VRIK>() != null)
        {
            syncAvatar.GetComponent<VRIK>().enabled = false;
        }
        if (leftHandTracking != null)
        {
            leftHandTracking.SetActive(false);
        }
        if (rightHandTracking != null)
        {
            rightHandTracking.SetActive(false);
        }
    }

    private void DisableAllAvatarsExcept(GameObject activeAvatar)
    {
        avatar.SetActive(activeAvatar == avatar);
        prerecordedAvatar.SetActive(activeAvatar == prerecordedAvatar);
    }

    private struct MovementData
    {
        public Vector3 leftHandPosition;
        public Quaternion leftHandRotation;
        public Vector3 rightHandPosition;
        public Quaternion rightHandRotation;
        public Quaternion[] leftHandFingerRotations;
        public Quaternion[] rightHandFingerRotations;
    }
}

public static class TransformExtensionsAlternative
{
    public static Transform FindChildRecursiveAlternative(this Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform result = child.FindChildRecursiveAlternative(childName);
            if (result != null)
                return result;
        }
        return null;
    }
}*/
