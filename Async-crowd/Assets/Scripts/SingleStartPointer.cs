using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SingleStartPointer : MonoBehaviour
{
    public GameObject rightHandTracking;
    private SingleAvatar singleAvatar;
    private Transform rightIndexTip;

    private void Start()
    {
        singleAvatar = GameObject.Find("ScriptsHandler").GetComponent<SingleAvatar>();
    }

    private void Update()
    {
        if (rightIndexTip == null)
        {
            //if (singleAvatar.syncAvatar != null && singleAvatar.syncAvatar.transform.FindChildRecursive("FullBody_RightHandIndexTip") != null)
            //    rightIndexTip = singleAvatar.syncAvatar.transform.FindChildRecursive("FullBody_RightHandIndexTip").transform;
            if (singleAvatar.syncAvatar != null &&
                rightHandTracking.transform.FindChildRecursive("Hand_IndexTip") != null)
            {
                rightIndexTip = rightHandTracking.transform.FindChildRecursive("Hand_IndexTip").transform;
            }
        }
        else
        {
            this.transform.position = rightIndexTip.position;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "StartBox")
        {
            singleAvatar.isStartFlagOn = true;
        }
    }
}
