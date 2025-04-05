using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Animator))]
[AddComponentMenu("Sof Components/Crew Seats/Crew Animator")]
public class CrewAnimator : SofComponent
{
    public Mesh headLessModel;
    private Mesh defaultModel;

    public CrewHand rightHand;
    public CrewHand leftHand;
    public FootRest defaultRightFoot;
    public FootRest defaultLeftFoot;
    public HandGrip restingGrip;

    private SkinnedMeshRenderer meshRend;
    private CrewMember crew;
    private Animator crewAnimator;

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
    public override void SetReferences(SofComplex _complex)
    {
        base.SetReferences(_complex);

        crew = GetComponent<CrewMember>();
        if(!crew) crew = transform.parent.GetComponent<CrewMember>();
        if (!crew) Debug.LogError("This script must have a CrewMember script parent", this);
        crewAnimator = GetComponent<Animator>();
        meshRend = GetComponentInChildren<SkinnedMeshRenderer>();
        defaultModel = meshRend?.sharedMesh;

#if UNITY_EDITOR
        gameObject.hideFlags = HideFlags.HideAndDontSave;
#endif
    }
    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);


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
    public void ToggleFirstPersonModel()
    {
        bool firstPerson = crew.IsVrPlayer || (crew.IsPlayer && SofCamera.viewMode == 1);

        if (!meshRend) return;
        meshRend.enabled = !firstPerson || firstPersonBody;
        meshRend.sharedMesh = firstPerson ? headLessModel : defaultModel;
    }
#if UNITY_EDITOR
    private void Update()
    {
        if (!Application.isPlaying && crew && crew.complex && crewAnimator != null)
        {
            crewAnimator.Update(0f);
            crewAnimator.Update(0f);
            crew.MoveToSeatInstant();
        }
    }
#endif
    private void OnAnimatorIK()
    {
        if (Seat == null) return;
        if (Time.deltaTime > 0f || !Application.isPlaying)
        {
            headLookAtPosition = HeadLookAtPosition();
        }
        SetBodyRotation();

        AnimateLeaning();
        crewAnimator.SetLookAtWeight(1);
        crewAnimator.SetLookAtPosition(headLookAtPosition);

        if (leftHand) leftHand.SetHandPose(crewAnimator, CurrentLeftGrip);
        if (rightHand) rightHand.SetHandPose(crewAnimator, CurrentRightGrip);

        CurrentRightFoot.SetFootPose(crewAnimator, AvatarIKGoal.RightFoot);
        CurrentLeftFoot.SetFootPose(crewAnimator, AvatarIKGoal.LeftFoot);
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
        crewAnimator.SetFloat("Leaning", leaning);
    }
    private Vector3 HeadLookAtPosition()
    {
        if (crew.ripped)
            return tr.position + tr.forward - tr.up * 2f;

        Vector3 headDirection = crew.IsPlayer ? SofCamera.CurrentDirection : Seat.LookingDirection;
        headDirection += Seat.transform.forward * 0.05f;

        Vector3 localHeadDirection = tr.InverseTransformDirection(headDirection);
        localHeadDirection.z = Mathf.Abs(localHeadDirection.z);

        return tr.position + tr.TransformDirection(localHeadDirection);
    }
}
