/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Klak.Motion;
using RootMotion.Demos;
using RootMotion.FinalIK;

public class SingleAvatar : MonoBehaviour
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

    [Header("Avatars")]
    public GameObject avatar;
    public GameObject prerecordedAvatar;
    public GameObject syncAvatar; // Changed to public
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

    public VRIKCalibrationBasic ikCalibration;

    private Queue<MovementData> movementQueue = new Queue<MovementData>();
    public float delayTime = 1.5f;

    private readonly Vector3 leftHandRotationOffset = new Vector3(180, 0, 180);
    private readonly Vector3 rightHandRotationOffset = new Vector3(180, 0, 0);
    private readonly Vector3 rightHandFingerRotationOffset = new Vector3(0, 0, 180);

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

        syncAvatar = (vmType == VisuomotorType.Prerec) ? prerecordedAvatar : avatar;

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
            currentTime += Time.deltaTime;
            mainInstructions.text = "Stretch out your arms for calibration. The study will begin in " + (10f - currentTime).ToString("F0") + " seconds.";
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
                DisplayEndMessage();
            }
        }
    }

    IEnumerator StartCondition()
    {
        isCountDown = true;
        ikCalibration.calibrateAvatar = true;

        yield return new WaitForSeconds(10f);
        ikCalibration.calibrateAvatar = true;

        isCountDown = false;
        currentTime = 0f;
        isAvatarRunning = true;
        mainInstructions.text = "Please tilt your head downwards as if looking down at your body.";
        rocketboxSMR.enabled = true;

        if (selectedVmType == VisuomotorType.Prerec)
        {
            DisableAvatarTracking();
        }

        yield return 0;
    }

    private void DisplayEndMessage()
    {
        mainInstructionsCanvas.SetActive(true);
        mainInstructions.text = "Thank you for your participation. This is the end of the experiment.";

        startBox.SetActive(false);
        hasExperimentEnded = true;
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
            currentMovement.leftHandFingerRotations[i] = leftHandTracking.transform.FindChildRecursive(fingerBonesID[i]).rotation;
            currentMovement.rightHandFingerRotations[i] = rightHandTracking.transform.FindChildRecursive(fingerBonesID[i]).rotation;
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
                    activeRightHandFingers[i].rotation = delayedMovement.rightHandFingerRotations[i] * Quaternion.Euler(rightHandFingerRotationOffset);
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
}*/