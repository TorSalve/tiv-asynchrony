using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace AsyncCrowd
{
    public static class Utils
    {
        public static Transform RecursiveFindChild(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                {
                    return child;
                }
                else
                {
                    Transform found = RecursiveFindChild(child, childName);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }
            return null;
        }

        public static bool IsHandTrackingActive()
        {
            return XRInputModalityManager.currentInputMode.Value == XRInputModalityManager.InputMode.TrackedHand;
        }

        public static XRInputModalityManager.InputMode GetTrackingMode()
        {
            return XRInputModalityManager.currentInputMode.Value;
        }
    }
}