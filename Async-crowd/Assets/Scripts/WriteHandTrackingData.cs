using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AsyncCrowd
{
    public class WriteHandTrackingData : MonoBehaviour
    {
        public List<Transform> lefthand;
        public List<Transform> righthand;
        public GameObject leftHandTracking;
        public GameObject rightHandTracking;
        public List<string> fingerBonesID;
        public Vector3 rotationOffset;

        void Start()
        {
            fingerBonesID = new List<string> {
            "Hand_Thumb0", "Hand_Thumb2", "Hand_Thumb3",
            "Hand_Index1", "Hand_Index2", "Hand_Index3",
            "Hand_Middle1", "Hand_Middle2", "Hand_Middle3",
            "Hand_Ring1", "Hand_Ring2", "Hand_Ring3",
            "Hand_Pinky1", "Hand_Pinky2", "Hand_Pinky3"
        };
        }

        // added try-catch block to stop the exceptions before the experiment started and the avatar tracking is enabled 
        void Update()
        {
            for (int i = 0; i < fingerBonesID.Count; i++)
            {
                try
                {
                    if (lefthand[i] == null)
                    {
                        // Debug.Log("i, " + i);
                        continue;
                    }

                    if (leftHandTracking == null || leftHandTracking.transform == null)
                    {
                        // Debug.LogWarning("leftHandTracking or its transform is null");
                        continue;
                    }

                    Transform foundLeftFingerObject = Utils.RecursiveFindChild(leftHandTracking.transform, fingerBonesID[i]);
                    if (foundLeftFingerObject == null)
                    {
                        // Debug.Log("Finger object not found for: " + fingerBonesID[i]);
                        continue;
                    }

                    lefthand[i].rotation = foundLeftFingerObject.rotation;

                    if (rightHandTracking == null || rightHandTracking.transform == null)
                    {
                        // Debug.LogWarning("rightHandTracking or its transform is null");
                        continue;
                    }

                    Transform foundRightFingerObject = Utils.RecursiveFindChild(rightHandTracking.transform, fingerBonesID[i]);
                    if (foundRightFingerObject == null)
                    {
                        // Debug.Log("Finger object not found for: " + fingerBonesID[i]);
                        continue;
                    }

                    righthand[i].rotation = foundRightFingerObject.rotation * Quaternion.Euler(rotationOffset);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Exception caught in Update loop: " + ex.Message);
                }
            }
        }
    }
}