using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Klak.Motion;
using RootMotion.Demos;
using RootMotion.FinalIK;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace AsyncCrowd
{
    public class SingleAvatar : MonoBehaviour
    {
        public enum VisuomotorType { Sync = 1, Async = 2, Delay = 3, Prerec = 4 };

        [Header("Conditions")]
        private VisuomotorType vmType; // Changed to private
        public VisuomotorType VMType
        {
            get { return vmType; }
            private set { vmType = value; }
        }
        private VisuomotorType selectedVmType;
        public float currentTime;
        public bool isQuestionnaireDone;

        private bool isAvatarRunning;
        private bool isQuestionnaireRunning;
        private bool hasExperimentEnded = false; // Added missing variable
        public float exposureDuration = 180f;

        [Header("Audio")]
        public AudioSource audioSource;
        public AudioClip startingAudioClip; // Audio to play at the start
        public AudioClip calibrationAudioClip;
        public AudioClip ongoingAudioClip; // Audio to play when start button is pressed
        public AudioClip questionnaireAudioClip; // Audio to play when questionnaire starts
        public AudioClip endAudioClip;

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
        private QuestionnaireController questionnaireController;

        public VRIKCalibrationBasic ikCalibration;

        private Queue<MovementData> movementQueue = new Queue<MovementData>();
        public float delayTime = 1.5f;

        private readonly Vector3 leftHandRotationOffset = new Vector3(180, 0, 180);
        private readonly Vector3 rightHandRotationOffset = new Vector3(180, 0, 0);
        private readonly Vector3 rightHandFingerRotationOffset = new Vector3(0, 0, 180);

        // private OVRSkeletonRenderer leftHandSkeletonRenderer;
        // private OVRMesh leftHandMesh;
        // private OVRMeshRenderer leftHandMeshRenderer;
        // private SkinnedMeshRenderer leftHandSkinnedMeshRenderer;

        // private OVRSkeletonRenderer rightHandSkeletonRenderer;
        // private OVRMesh rightHandMesh;
        // private OVRMeshRenderer rightHandMeshRenderer;
        // private SkinnedMeshRenderer rightHandSkinnedMeshRenderer;

        private Animator leftHandAnimator;
        private Animator rightHandAnimator;

        private XRInputModalityManager inputModalityManager;

        private void Awake()
        {
            inputModalityManager = FindFirstObjectByType<XRInputModalityManager>();
        }

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

            // Randomly select a VisuomotorType
            VMType = (VisuomotorType)Random.Range(1, 5); // Random.Range is exclusive of the upper bound, so use 5

            syncAvatar = (VMType == VisuomotorType.Prerec) ? prerecordedAvatar : avatar;

            activeLeftHandFingers = leftHandFingers;
            activeRightHandFingers = rightHandFingers;

            ikCalibration = syncAvatar.GetComponent<VRIKCalibrationBasic>();
            DisableAllAvatarsExcept(syncAvatar);

            rocketboxSMR = syncAvatar.GetComponentInChildren<SkinnedMeshRenderer>();

            leftArmBM.transform.localPosition = Vector3.zero;
            leftArmBM.GetComponent<BrownianMotion>().enabled = false;
            rightArmBM.transform.localPosition = Vector3.zero;
            rightArmBM.GetComponent<BrownianMotion>().enabled = false;

            rocketboxSMR.enabled = false;

            mainInstructions.text = mainInstructions.text + "\n\n" +
                "";

            questionnaireController = this.GetComponent<QuestionnaireController>();

            if (VMType == VisuomotorType.Delay)
            {
                InvokeRepeating("UpdateBuffer", 0, Time.fixedDeltaTime);
            }

            // leftHandSkeletonRenderer = leftHandTracking.GetComponentInChildren<OVRSkeletonRenderer>();
            // leftHandMesh = leftHandTracking.GetComponentInChildren<OVRMesh>();
            // leftHandMeshRenderer = leftHandTracking.GetComponentInChildren<OVRMeshRenderer>();
            // leftHandSkinnedMeshRenderer = leftHandTracking.GetComponentInChildren<SkinnedMeshRenderer>();

            // rightHandSkeletonRenderer = rightHandTracking.GetComponentInChildren<OVRSkeletonRenderer>();
            // rightHandMesh = rightHandTracking.GetComponentInChildren<OVRMesh>();
            // rightHandMeshRenderer = rightHandTracking.GetComponentInChildren<OVRMeshRenderer>();
            // rightHandSkinnedMeshRenderer = rightHandTracking.GetComponentInChildren<SkinnedMeshRenderer>();

            leftHandAnimator = leftHandTracking.GetComponentInChildren<Animator>();
            rightHandAnimator = rightHandTracking.GetComponentInChildren<Animator>();

            // Play the starting audio
            if (startingAudioClip != null)
            {
                audioSource.clip = startingAudioClip;
                audioSource.Play();
            }
        }

        void Update()
        {
            if (isStartFlagOn)
            {
                isStartFlagOn = false;
                startBox.SetActive(false);
                pointer.GetComponent<Renderer>().enabled = false;

                StartCoroutine(StartCondition());
            }
            else
            {
                if (!hasExperimentEnded && Utils.IsHandTrackingActive() && !isCountDown && !isAvatarRunning && !isQuestionnaireRunning)
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
                    if (VMType == VisuomotorType.Sync)
                    {
                        // Sync condition, do nothing special
                    }
                    else if (VMType == VisuomotorType.Async)
                    {
                        EnableBrownianMotion();
                    }
                    else if (VMType == VisuomotorType.Delay)
                    {
                        ApplyDelayedMovements();
                    }
                    else if (VMType == VisuomotorType.Prerec)
                    {
                        // Prerecorded condition, avatars remain static
                    }
                }
                else
                {
                    DisableBrownianMotion();
                    rocketboxSMR.enabled = false;

                    isAvatarRunning = false;
                    isQuestionnaireRunning = true;

                    if (questionnaireAudioClip != null)
                    {
                        audioSource.clip = questionnaireAudioClip;
                        audioSource.Play();
                    }

                    questionnaireController.questionnaireCanvas.SetActive(true);
                    questionnaireController.InitializeQuestionnaire();
                    mainInstructionsCanvas.SetActive(false);
                }
            }

            if (isQuestionnaireRunning)
            {
                if (isQuestionnaireDone)
                {
                    isQuestionnaireDone = false;
                    questionnaireController.questionnaireCanvas.SetActive(false);
                    isQuestionnaireRunning = false;
                    currentTime = 0f;
                    DisplayEndMessage();
                }
            }
        }

        IEnumerator StartCondition()
        {
            isCountDown = true;
            ikCalibration.calibrateAvatar = true;

            if (isCountDown)
            {
                DisableHandTrackingComponents();
                audioSource.clip = calibrationAudioClip;
                audioSource.Play();
            }

            yield return new WaitForSeconds(10f);
            ikCalibration.calibrateAvatar = true;

            isCountDown = false;
            currentTime = 0f;
            isAvatarRunning = true;
            mainInstructions.text = "Please tilt your head downwards as if looking down at your body.";
            rocketboxSMR.enabled = true;

            // Play the ongoing audio when calibration starts
            if (ongoingAudioClip != null)
            {
                audioSource.clip = ongoingAudioClip;
                audioSource.Play();
            }

            if (VMType == VisuomotorType.Prerec)
            {
                DisableAvatarTracking();
                Vector3 calibratedPosition = ikCalibration.transform.position;
                prerecordedAvatar.transform.position = new Vector3(calibratedPosition.x, calibratedPosition.y, calibratedPosition.z - 0.035f);

            }

            yield return 0;
        }

        private void DisplayEndMessage()
        {
            mainInstructionsCanvas.SetActive(true);
            mainInstructions.text = "Thank you for your participation. This is the end of the experiment. Your reference code is LOCH NESS 44.";

            startBox.SetActive(false);
            hasExperimentEnded = true;

            // Play the end message audio if it's assigned
            if (endAudioClip != null)
            {
                audioSource.clip = endAudioClip;
                audioSource.Play();
            }
        }


        // Other methods remain unchanged...


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
                try
                {
                    Transform leftFinger = Utils.RecursiveFindChild(leftHandTracking.transform, fingerBonesID[i]);
                    if (leftFinger != null)
                    {
                        currentMovement.leftHandFingerRotations[i] = leftFinger.rotation;
                    }

                    Transform rightFinger = Utils.RecursiveFindChild(rightHandTracking.transform, fingerBonesID[i]);
                    if (rightFinger != null)
                    {
                        currentMovement.rightHandFingerRotations[i] = rightFinger.rotation;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Exception caught in UpdateBuffer loop: " + ex.Message);
                }
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
                    try
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
                    catch (System.Exception ex)
                    {
                        Debug.LogError("Exception caught in ApplyDelayedMovements loop: " + ex.Message);
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

        private void DisableHandTrackingComponents()
        {
            inputModalityManager.leftHand.SetActive(false);
            inputModalityManager.rightHand.SetActive(false);

            // Disable all relevant components
            // if (leftHandSkeletonRenderer != null) leftHandSkeletonRenderer.enabled = false;
            // if (leftHandMesh != null) leftHandMesh.enabled = false;
            // if (leftHandMeshRenderer != null) leftHandMeshRenderer.enabled = false;
            // if (leftHandSkinnedMeshRenderer != null) leftHandSkinnedMeshRenderer.enabled = false;
            // if (leftHandAnimator != null) leftHandAnimator.enabled = false;

            // if (rightHandSkeletonRenderer != null) rightHandSkeletonRenderer.enabled = false;
            // if (rightHandMesh != null) rightHandMesh.enabled = false;
            // if (rightHandMeshRenderer != null) rightHandMeshRenderer.enabled = false;
            // if (rightHandSkinnedMeshRenderer != null) rightHandSkinnedMeshRenderer.enabled = false;
            // if (rightHandAnimator != null) rightHandAnimator.enabled = false;

            // Optionally, disable the entire tracking GameObject to ensure no lingering effects
            // if (leftHandTracking != null) leftHandTracking.SetActive(false);
            // if (rightHandTracking != null) rightHandTracking.SetActive(false);
        }
    }
}