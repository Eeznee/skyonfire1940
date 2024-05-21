using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
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
    private Vector3 headPosition;

    private CrewSeat Seat => Application.isPlaying ? crew.seat : crew.seats[crew.seatIdTest];
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
    }
    private void OnValidate() { GetReferences(); }
    private void GetReferences()
    {
        tr = transform;
        crew = GetComponent<CrewMember>();
        animator = GetComponent<Animator>();
        meshRend = GetComponentInChildren<SkinnedMeshRenderer>();
        defaultModel = meshRend.sharedMesh;
    }
    public void ToggleFirstPersonModel()
    {
        bool firstPerson = crew.IsVrPlayer || (crew.IsPlayer && SofCamera.viewMode == 1);

        meshRend.enabled = !firstPerson || firstPersonBody;
        meshRend.sharedMesh = firstPerson ? headLessModel : defaultModel;
    }
    private void Update()
    {
        if (!Application.isPlaying && crew.complex)
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
            headPosition = HeadPosition();
        }

        SetBodyPosition(headPosition);
        SetBodyRotation(headPosition);

        AnimateLeaning();
        animator.SetLookAtWeight(1);
        animator.SetLookAtPosition(headLookAtPosition);

        leftHand.SetHandPose(animator, CurrentLeftGrip);
        rightHand.SetHandPose(animator, CurrentRightGrip);

        CurrentRightFoot.SetFootPose(animator, AvatarIKGoal.RightFoot);
        CurrentLeftFoot.SetFootPose(animator, AvatarIKGoal.LeftFoot);
    }
    private Vector3 HeadPosition()
    {
        ObjectData data = crew.data;

        if (crew.IsVrPlayer) return SofCamera.tr.position;

        Vector3 headPosition = CameraInputs.zoomed ? Seat.ZoomedHeadPosition : Seat.DefaultHeadPosition;

        if (crew.IsPlayer)
        {
            Vector3 gOffsetLocal = data.acceleration / Physics.gravity.y;
            gOffsetLocal.y *= 0.5f;
            Vector3 accelerationOffset = data.tr.TransformDirection(gOffsetLocal);
            accelerationOffset += (data.tr.up - Vector3.up) * 4f;
            accelerationCompensation = Vector3.MoveTowards(accelerationCompensation, accelerationOffset, Time.deltaTime * 2f);
            accelerationCompensation = Vector3.ClampMagnitude(accelerationCompensation, accelerationOffset.magnitude);
            headPosition += (accelerationOffset - accelerationCompensation) / 50f;
        }

        return headPosition;
    }

    const float bodyMoveSpeed = 1f;
    const float smoothTime = 0.2f;
    private Vector3 accelerationCompensation = Vector3.zero;
    private Vector3 vel = Vector3.zero;
    private void SetBodyPosition(Vector3 headPosition)
    {
        bool instant = !Application.isPlaying;

        Vector3 localHeadPosition = tr.parent.InverseTransformPoint(headPosition);
        localHeadPosition += CrewMember.eyeShift * Vector3.down;
        if (instant) tr.localPosition = localHeadPosition;
        else tr.localPosition = Vector3.SmoothDamp(tr.localPosition, localHeadPosition, ref vel, smoothTime, bodyMoveSpeed, Time.deltaTime);
    }
    private void SetBodyRotation(Vector3 headPosition)
    {
        Vector3 bodyForward = Seat.transform.position - headPosition;
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
