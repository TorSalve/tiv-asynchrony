using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AsyncCrowd
{
    public class PickPointer : MonoBehaviour
    {
        public GameObject rightHandTracking;
        private PickAvatar pickAvatar;

        private Transform rightIndexTip;

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
            if (rightIndexTip == null)
            {
                rightIndexTip = rightHandTracking.transform.FindChildRecursiveCustom("Hand_IndexTip");
            }
            else
            {
                // Move the pointer to the position of the index finger tip
                this.transform.position = rightIndexTip.position;
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
}