using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickPointer : MonoBehaviour
{
    public GameObject rightHandTracking;
    private PickAvatar pickAvatar;

    private Transform rightThumbTip;
    private Transform rightIndexTip;
    private Transform rightMiddleTip;
    private Transform rightRingTip;
    private Transform rightPinkyTip;

    private void Start()
    {
        pickAvatar = GameObject.Find("ScriptsHandler").GetComponent<PickAvatar>();
        if (pickAvatar == null)
        {
            Debug.LogError("PickAvatar component not found on ScriptsHandler");
        }

        if (rightHandTracking == null)
        {
            Debug.LogError("RightHandTracking GameObject is not assigned");
        }
    }

    private void Update()
    {
        if (rightThumbTip == null || rightIndexTip == null || rightMiddleTip == null || rightRingTip == null || rightPinkyTip == null)
        {
            rightThumbTip = rightHandTracking.transform.FindChildRecursiveCustom("Hand_ThumbTip");
            rightIndexTip = rightHandTracking.transform.FindChildRecursiveCustom("Hand_IndexTip");
            rightMiddleTip = rightHandTracking.transform.FindChildRecursiveCustom("Hand_MiddleTip");
            rightRingTip = rightHandTracking.transform.FindChildRecursiveCustom("Hand_RingTip");
            rightPinkyTip = rightHandTracking.transform.FindChildRecursiveCustom("Hand_PinkyTip");
        }
        else
        {
            // Example: Moving the pointer to the average position of all finger tips
            Vector3 averagePosition = (rightThumbTip.position + rightIndexTip.position + rightMiddleTip.position + rightRingTip.position + rightPinkyTip.position) / 5;
            this.transform.position = averagePosition;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WomanBox"))
        {
            pickAvatar.isStartFlagOn = true;
            pickAvatar.sceneToLoad = "woman"; // Set scene name for the woman box
        }
        else if (other.CompareTag("ManBox"))
        {
            pickAvatar.isStartFlagOn = true;
            pickAvatar.sceneToLoad = "man"; // Set scene name for the man box
        }
    }
}

public static class TransformExtensions
{
    public static Transform FindChildRecursiveCustom(this Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform found = child.FindChildRecursiveCustom(childName);
            if (found != null)
                return found;
        }
        return null;
    }
}
