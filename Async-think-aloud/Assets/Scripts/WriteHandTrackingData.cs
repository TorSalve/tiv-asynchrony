using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            //"Hand_Thumb1", "Hand_Thumb2", "Hand_Thumb3",
            "Hand_Index1", "Hand_Index2", "Hand_Index3",
            "Hand_Middle1", "Hand_Middle2", "Hand_Middle3",
            "Hand_Ring1", "Hand_Ring2", "Hand_Ring3",
            //"Hand_Pinky0", "Hand_Pinky1", "Hand_Pinky2",
            "Hand_Pinky1", "Hand_Pinky2", "Hand_Pinky3"
        };
    }

    void Update()
    {
        for (int i = 0; i < fingerBonesID.Count; i++)
        {
            //lefthand[i] = leftHandTracking.transform.FindChildRecursive(fingerBonesID[i]);
            //righthand[i] = rightHandTracking.transform.FindChildRecursive(fingerBonesID[i]);

            //lefthand[i].localPosition = leftHandTracking.transform.FindChildRecursive(fingerBonesID[i]).localPosition;
            lefthand[i].rotation = leftHandTracking.transform.FindChildRecursive(fingerBonesID[i]).rotation;

            //righthand[i].localPosition = rightHandTracking.transform.FindChildRecursive(fingerBonesID[i]).localPosition;
            righthand[i].rotation = rightHandTracking.transform.FindChildRecursive(fingerBonesID[i]).rotation * Quaternion.Euler(rotationOffset);

            // legs


        }

    }
}