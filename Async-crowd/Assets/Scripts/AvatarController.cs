using Klak.Motion;
using Oculus.Platform.Models;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;

public class AvatarController : MonoBehaviour
{
    public enum AvatarMovements { Sync = 0, Delayed = 1, BrownianMotion = 2, Prerecorded = 3, Noise = 4};
    public AvatarMovements avatarMovements;
    public static bool switchAvatarMovement;
    public Material selectedBlue;
    public Material unselectedWhite;
    public GameObject syncBtn;
    public GameObject delayedBtn;
    public GameObject bmBtn;
    public GameObject prerecordedBtn;
    public GameObject noiseBtn;

    public GameObject syncAvatar;
    public GameObject asyncAvatar;

    [Header("Delayed")]
    // some variables are reused in the other implementations.
    public GameObject trackingAvatar;
    public GameObject delayedAvatar;
    Transform[] allChildrenTrackedAvatar;
    Transform[] allChildrenAsyncAvatar;

    SkinnedMeshRenderer[] syncAvatarSMRs;
    SkinnedMeshRenderer[] asyncAvatarSMRs;
    [Range(0.0f, 3f)]
    public float delayedTime = 0.5f;
    public GameObject delayedBtns;
    public bool changeDelayTime;

    [Header("Brownian Motion")]
    public GameObject bmEffect;

    [Header("Noise")]
    public float lamda = 0.9f; // parameter lamda determines the weight of the present location in favor of the noisy virtual location.
    public float sigma = 0.015f;
    //List<Vector3> lastFrameTrackedPos = new List<Vector3>();
    //List<Vector3> lastFrameAsyncPos = new List<Vector3>();
    Vector3 lastFrameRealHand;
    Vector3 lastFrameAsyncHand;
    public float speed = 0.01f;


    void Start()
    {
        bmEffect.GetComponent<BrownianMotion>().enabled = false;
        allChildrenTrackedAvatar = trackingAvatar.GetComponentsInChildren<Transform>();
        allChildrenAsyncAvatar = delayedAvatar.GetComponentsInChildren<Transform>();
        syncAvatarSMRs = syncAvatar.GetComponentsInChildren<SkinnedMeshRenderer>();
        asyncAvatarSMRs = asyncAvatar.GetComponentsInChildren<SkinnedMeshRenderer>();
        asyncAvatar.SetActive(false);
        delayedBtns.SetActive(false);
    }

    void Update()
    {
        if (OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.RTouch))
        {
            OVRPlugin.ResetBodyTrackingCalibration();
        }





        if (switchAvatarMovement)
        {
            switchAvatarMovement = false;
            // get BM back to normal

            bmEffect.transform.localPosition = Vector3.zero;
            bmEffect.GetComponent<BrownianMotion>().enabled = false;

            if (avatarMovements == AvatarMovements.Sync)
            {
                syncBtn.GetComponent<Renderer>().material = selectedBlue;
                delayedBtn.GetComponent<Renderer>().material = unselectedWhite;
                bmBtn.GetComponent<Renderer>().material = unselectedWhite;
                prerecordedBtn.GetComponent<Renderer>().material = unselectedWhite;
                noiseBtn.GetComponent<Renderer>().material = unselectedWhite;

                asyncAvatar.SetActive(false);
                delayedBtns.SetActive(false);
                foreach (var t in syncAvatarSMRs) t.enabled = true;
            }
            else if (avatarMovements == AvatarMovements.Delayed)
            {
                syncBtn.GetComponent<Renderer>().material = unselectedWhite;
                delayedBtn.GetComponent<Renderer>().material = selectedBlue;
                bmBtn.GetComponent<Renderer>().material = unselectedWhite;
                prerecordedBtn.GetComponent<Renderer>().material = unselectedWhite;
                noiseBtn.GetComponent<Renderer>().material = unselectedWhite;

                asyncAvatar.SetActive(true);
                delayedBtns.SetActive(true);
                foreach (var t in syncAvatarSMRs) t.enabled = false;

            }
            else if (avatarMovements == AvatarMovements.BrownianMotion)
            {
                syncBtn.GetComponent<Renderer>().material = unselectedWhite;
                delayedBtn.GetComponent<Renderer>().material = unselectedWhite;
                bmBtn.GetComponent<Renderer>().material = selectedBlue;
                prerecordedBtn.GetComponent<Renderer>().material = unselectedWhite;
                noiseBtn.GetComponent<Renderer>().material = unselectedWhite;

                asyncAvatar.SetActive(false);
                delayedBtns.SetActive(false);
                foreach (var t in syncAvatarSMRs) t.enabled = true;

                if (bmEffect.transform.parent == null)
                {
                    AssignNoiseParent();
                }

            }
            else if (avatarMovements == AvatarMovements.Prerecorded)
            {

                syncBtn.GetComponent<Renderer>().material = unselectedWhite;
                delayedBtn.GetComponent<Renderer>().material = unselectedWhite;
                bmBtn.GetComponent<Renderer>().material = unselectedWhite;
                prerecordedBtn.GetComponent<Renderer>().material = selectedBlue;
                noiseBtn.GetComponent<Renderer>().material = unselectedWhite;

                asyncAvatar.SetActive(false);
                delayedBtns.SetActive(false);
                foreach (var t in syncAvatarSMRs) t.enabled = true;
            }
            else if (avatarMovements == AvatarMovements.Noise)
            {
                syncBtn.GetComponent<Renderer>().material = unselectedWhite;
                delayedBtn.GetComponent<Renderer>().material = unselectedWhite;
                bmBtn.GetComponent<Renderer>().material = unselectedWhite;
                prerecordedBtn.GetComponent<Renderer>().material = unselectedWhite;
                noiseBtn.GetComponent<Renderer>().material = selectedBlue;

                asyncAvatar.SetActive(false);
                delayedBtns.SetActive(false);
                foreach (var t in syncAvatarSMRs) t.enabled = true;

                if (bmEffect.transform.parent == null)
                {
                    AssignNoiseParent();
                }

                //lastFrameTrackedPos = new List<Vector3>();
                //lastFrameAsyncPos = new List<Vector3>();
                lastFrameRealHand  = Vector3.zero;
                lastFrameAsyncHand = Vector3.zero;

            }
        }

        if (changeDelayTime)
        {
            changeDelayTime = false;
            MeshRenderer[] btns = delayedBtns.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < 5; i++) btns[i].material = unselectedWhite;
            delayedBtns.transform.Find(delayedTime.ToString()).GetComponent<MeshRenderer>().material = selectedBlue;
            StartCoroutine(HideAsyncAvatar(4f));
        }

        if (avatarMovements == AvatarMovements.Noise) StartCoroutine(NoiseMovement(sigma, lamda));

        



    }




    private void LateUpdate()
    {
        if (avatarMovements == AvatarMovements.Delayed) StartCoroutine(DelayedAvatar(delayedTime));
        if (avatarMovements == AvatarMovements.Noise)
        {
            //foreach (var t in allChildrenTrackedAvatar)
            //{
            //    lastFrameTrackedPos.Add(t.position);
            //}

            //foreach (var t in allChildrenAsyncAvatar)
            //{
            //    lastFrameAsyncPos.Add(t.position);
            //}
            lastFrameRealHand = bmEffect.transform.Find("FullBody_RightHandWrist").transform.localPosition;
            lastFrameAsyncHand = bmEffect.transform.Find("FullBody_RightHandWrist").transform.position; // include the offset of bmEffect
            //Debug.Log(1);
        }
    }


    private void AssignNoiseParent()
    {
        bmEffect.transform.SetParent(syncAvatar.transform.Find("Bones").transform);

        GameObject.Find("FullBody_RightShoulder").transform.SetParent(bmEffect.transform);
        GameObject.Find("FullBody_RightScapula").transform.SetParent(bmEffect.transform);
        GameObject.Find("FullBody_RightArmUpper").transform.SetParent(bmEffect.transform);
        GameObject.Find("FullBody_RightArmLower").transform.SetParent(bmEffect.transform);
        GameObject.Find("FullBody_RightHandWristTwist").transform.SetParent(bmEffect.transform);


        GameObject.Find("FullBody_RightHandPalm").transform.SetParent(bmEffect.transform);
        GameObject.Find("FullBody_RightHandWrist").transform.SetParent(bmEffect.transform);
        GameObject.Find("FullBody_RightHandThumbMetacarpal").transform.SetParent(bmEffect.transform);
        GameObject.Find("FullBody_RightHandThumbProximal").transform.SetParent(bmEffect.transform);
        GameObject.Find("FullBody_RightHandThumbDistal").transform.SetParent(bmEffect.transform);
        GameObject.Find("FullBody_RightHandThumbTip").transform.SetParent(bmEffect.transform);
        GameObject.Find("FullBody_RightHandIndexMetacarpal").transform.SetParent(bmEffect.transform);
        GameObject.Find("FullBody_RightHandIndexProximal").transform.SetParent(bmEffect.transform);
        GameObject.Find("FullBody_RightHandIndexIntermediate").transform.SetParent(bmEffect.transform);
        GameObject.Find("FullBody_RightHandIndexDistal").transform.SetParent(bmEffect.transform);
        GameObject.Find("FullBody_RightHandIndexTip").transform.SetParent(bmEffect.transform);
        GameObject.Find("FullBody_RightHandMiddleMetacarpal").transform.SetParent(bmEffect.transform);
        GameObject.Find("FullBody_RightHandMiddleProximal").transform.SetParent(bmEffect.transform);
        GameObject.Find("FullBody_RightHandMiddleIntermediate").transform.SetParent(bmEffect.transform);
        GameObject.Find("FullBody_RightHandMiddleDistal").transform.SetParent(bmEffect.transform);
        GameObject.Find("FullBody_RightHandMiddleTip").transform.SetParent(bmEffect.transform);
        GameObject.Find("FullBody_RightHandRingMetacarpal").transform.SetParent(bmEffect.transform);
        GameObject.Find("FullBody_RightHandRingProximal").transform.SetParent(bmEffect.transform);
        GameObject.Find("FullBody_RightHandRingIntermediate").transform.SetParent(bmEffect.transform);
        GameObject.Find("FullBody_RightHandRingDistal").transform.SetParent(bmEffect.transform);
        GameObject.Find("FullBody_RightHandRingTip").transform.SetParent(bmEffect.transform);
        GameObject.Find("FullBody_RightHandLittleMetacarpal").transform.SetParent(bmEffect.transform);
        GameObject.Find("FullBody_RightHandLittleProximal").transform.SetParent(bmEffect.transform);
        GameObject.Find("FullBody_RightHandLittleIntermediate").transform.SetParent(bmEffect.transform);
        GameObject.Find("FullBody_RightHandLittleDistal").transform.SetParent(bmEffect.transform);
        GameObject.Find("FullBody_RightHandLittleTip").transform.SetParent(bmEffect.transform);


        bmEffect.GetComponent<BrownianMotion>().enabled = true;
    }
    private IEnumerator HideAsyncAvatar(float _duration)
    {
        foreach (var t in asyncAvatarSMRs) t.enabled = false;


        yield return new WaitForSeconds(_duration);
        foreach (var t in asyncAvatarSMRs) t.enabled = true;

        yield break;
    }

    private IEnumerator DelayedAvatar(float _duration=0f)
    {

        List<Vector3> thisFramePos = new List<Vector3>();
        List<Quaternion> thisFrameRot = new List<Quaternion>();
        foreach (var t in allChildrenTrackedAvatar)
        {
            thisFramePos.Add(t.position);
            thisFrameRot.Add(t.rotation);
        }

        yield return new WaitForSeconds(_duration);

        for (int i = 0; i < allChildrenTrackedAvatar.Length; i++)
        {
            allChildrenAsyncAvatar[i].position = thisFramePos[i];
            allChildrenAsyncAvatar[i].rotation = thisFrameRot[i];
            //allChildrenDelayedAvatar[i].localScale = allChildrenTrackedAvatar[i].localScale;
        }

        yield break;
    }


    private IEnumerator NoiseMovement(float _sigma = 0.015f, float _lamda=0.9f)
    {
        /// <summary>
        /// The incongurence movement implementation in Brugada-Ramentol et al., Consciousness and Cognition '19
        /// </summary>>
        float noise = Helpers.RandomGaussian(_sigma * -3, _sigma * 3);
        Vector3 r = new Vector3(noise, noise, noise); // Gaussian random noise ~ N(0, sigma)
        //for (int i = 0; i < allChildrenTrackedAvatar.Length; i++)
        //{
            //allChildrenAsyncAvatar[i].position = _lamda * allChildrenTrackedAvatar[i].position +
            //    (1 - _lamda) * (lastFrameAsyncPos[i] + (allChildrenTrackedAvatar[i].position - lastFrameTrackedPos[i]) + r);
        //}
        Vector3 newPos = _lamda * bmEffect.transform.Find("FullBody_RightHandWrist").transform.localPosition +
                (1 - _lamda) * (lastFrameAsyncHand + (bmEffect.transform.Find("FullBody_RightHandWrist").transform.localPosition - lastFrameRealHand) + r);

        Vector3 destination = newPos - bmEffect.transform.Find("FullBody_RightHandWrist").transform.localPosition;
        float totalMovementTime = 0.5f; //the amount of time you want the movement to take
        float currentMovementTime = 0f;//The amount of time that has passed
        // assign offset back to bmEffect
        while (Vector3.Distance(bmEffect.transform.localPosition, destination) > 0)
        {
            currentMovementTime += Time.deltaTime;
            bmEffect.transform.localPosition = Vector3.Lerp(bmEffect.transform.localPosition, destination, currentMovementTime / totalMovementTime);
            yield return null;
        }
        yield break;

    }

}
