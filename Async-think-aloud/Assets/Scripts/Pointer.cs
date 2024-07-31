using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class Pointer : MonoBehaviour
{
    public enum WhichHand { Left = 0, Right = 1};
    public WhichHand whichHand;
    public GameObject syncAvatar;
    Transform indexTip;
    AvatarController avatarController;

    void Start()
    {
        avatarController = GameObject.Find("ScirptsController").GetComponent<AvatarController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (indexTip == null)
        {
            string indexTipName = (whichHand == WhichHand.Left) ? "FullBody_LeftHandIndexTip" : "FullBody_RightHandIndexTip";
            indexTip = syncAvatar.transform.FindChildRecursive(indexTipName).transform;
        }
        else
        {
            transform.position = indexTip.position;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "AvatarControlUI")
        {
            if (other.name == "Sync")
            {
                avatarController.avatarMovements = AvatarController.AvatarMovements.Sync;
            }
            else if (other.name == "Delayed")
            {
                avatarController.avatarMovements = AvatarController.AvatarMovements.Delayed;
            }
            else if (other.name == "BrownianMotion")
            {
                avatarController.avatarMovements = AvatarController.AvatarMovements.BrownianMotion;
            }
            else if (other.name == "Prerecorded")
            {
                avatarController.avatarMovements = AvatarController.AvatarMovements.Prerecorded;
            }
            else if (other.name == "Noise")
            {
                avatarController.avatarMovements = AvatarController.AvatarMovements.Noise;
            }
            AvatarController.switchAvatarMovement = true;
        }
        else if (other.tag == "DelayUI")
        {
            avatarController.delayedTime = float.Parse(other.name);
            avatarController.changeDelayTime = true;
        }
        
    }
}
