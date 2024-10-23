using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(CrewMember))]
[AddComponentMenu("Sof Components/Crew Seats/Crew Animator")]
public class CrewAnimator : MonoBehaviour
{
    public Mesh headLessModel;
    private Mesh defaultModel;

    public CrewHand rightHand;
    public CrewHand leftHand;
    public FootRest defaultRightFoot;
    public FootRest defaultLeftFoot;
    public HandGrip restingGrip;

    private Transform tr;
    private Animator animator;
    private SkinnedMeshRenderer meshRend;
    private CrewMember crew;

    private bool firstPersonBody;

    private Vector3 headLookAtPosition;

    private CrewSeat Seat => crew.Seat;
    private FootRest CurrentRightFoot => Seat.rightFootRest ? Seat.rightFootRest : defaultRightFoot;
    private FootRest CurrentLeftFoot => Seat.leftFootRest ? Seat.leftFootRest : defaultLeftFoot;
    private HandGrip CurrentRightGrip
    {
        get
        {
            if (crew.IsVrPlayer)
                return SofVrRig.instance.rightHandGrip;
            if (Seat.rightHandGrip && Seat.rightHandGrip.transform.root == tr.root)
                return Seat.rightHandGrip;
            return restingGrip;
        }
    }
    private HandGrip CurrentLeftGrip
    {
        get
        {
            if (crew.IsVrPlayer)
                return SofVrRig.instance.leftHandGrip;
            if (Seat.leftHandGrip && Seat.leftHandGrip.transform.root == tr.root)
                return Seat.leftHandGrip;
            return restingGrip;
        }
    }
    private void Awake()
    {
        GetReferences();
        firstPersonBody = PlayerPrefs.GetInt("FirstPersonModel", 1) == 1;

        crew.OnAttachPlayer += AttachPlayer;
        crew.OnDetachPlayer += DetachPlayer;
    }

    void AttachPlayer()
    {
        ToggleFirstPersonModel();

        SofCamera.OnSwitchCamEvent += ToggleFirstPersonModel;
    }
    void DetachPlayer()
    {
        ToggleFirstPersonModel();

        SofCamera.OnSwitchCamEvent -= ToggleFirstPersonModel;
    }
    private void OnValidate() { GetReferences(); }
    private void GetReferences()
    {
        tr = transform;
        crew = GetComponent<CrewMember>();
        animator = GetComponent<Animator>();
        meshRend = GetComponentInChildren<SkinnedMeshRenderer>();
        defaultModel = meshRend?.sharedMesh;
    }
    public void ToggleFirstPersonModel()
    {
        bool firstPerson = crew.IsVrPlayer || (crew.IsPlayer && SofCamera.viewMode == 1);

        if (!meshRend) return;
        meshRend.enabled = !firstPerson || firstPersonBody;
        meshRend.sharedMesh = firstPerson ? headLessModel : defaultModel;
    }
    private void Update()
    {
        if (!Application.isPlaying && crew && crew.complex && animator != null)
        {
            animator.Update(0f);
            animator.Update(0f);
        }
    }

    private void OnAnimatorIK()
    {
        if (Seat == null) return;
        if (Time.deltaTime > 0f || !Application.isPlaying)
        {
            headLookAtPosition = HeadLookAtPosition();
        }
        SetBodyRotation();

        AnimateLeaning();
        animator.SetLookAtWeight(1);
        animator.SetLookAtPosition(headLookAtPosition);

        if (leftHand) leftHand.SetHandPose(animator, CurrentLeftGrip);
        if (rightHand) rightHand.SetHandPose(animator, CurrentRightGrip);

        CurrentRightFoot.SetFootPose(animator, AvatarIKGoal.RightFoot);
        CurrentLeftFoot.SetFootPose(animator, AvatarIKGoal.LeftFoot);
    }
    private void SetBodyRotation()
    {
        Vector3 bodyForward = Seat.transform.position - crew.HeadPosition;
        Vector3 bodyUp = Seat.transform.forward;// * 2f + (crew.IsVrPlayer ? SofCamera.tr.forward : Vector3.zero);
        Quaternion bodyRotation = Quaternion.LookRotation(bodyForward, bodyUp);
        bodyRotation *= Quaternion.Euler(-90f, 0f, 0f);

        tr.rotation = bodyRotation;
    }
    const float maxDistanceLean = 0.88f;
    const float minDistanceLean = 0.68f;
    private void AnimateLeaning()
    {
        float distance = (tr.position - Seat.transform.position).magnitude;
        float leaning = Mathf.InverseLerp(maxDistanceLean, minDistanceLean, distance);
        animator.SetFloat("Leaning", leaning);
    }
    private Vector3 HeadLookAtPosition()
    {
        if (crew.ripped)
            return tr.position + tr.forward - tr.up * 2f;

        Vector3 headDirection = crew.IsPlayer ? SofCamera.directionInput : Seat.LookingDirection;
        headDirection += Seat.transform.forward * 0.05f;

        Vector3 localHeadDirection = tr.InverseTransformDirection(headDirection);
        localHeadDirection.z = Mathf.Abs(localHeadDirection.z);

        return tr.position + tr.TransformDirection(localHeadDirection);
    }
}
