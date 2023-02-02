using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CrewAnimator : MonoBehaviour
{
    public Mesh headLessModel;
    private Mesh defaultModel;
    private bool noHead = false;

    private bool disabled = false;

    [HideInInspector] public SkinnedMeshRenderer meshRend;
    private CrewMember crew;
    private CrewHand rightHand;
    private CrewHand leftHand;
    private FootRest standingRightFoot;
    private FootRest standingLeftFoot;
    public HandGrip restingGrip;
    private Animator animator;

    const float standingButtHead = 0.88f;
    const float leaningButtHead = 0.68f;
    const float headMoveSpeed = 1f;
    const float smoothTime = 0.2f;

    private bool firstPersonModel;

    private void Awake()
    {
        GetReferences();
        firstPersonModel = PlayerPrefs.GetInt("FirstPersonModel", 1) == 1;
    }
    private void GetReferences()
    {
        crew = GetComponent<CrewMember>();
        animator = GetComponent<Animator>();
        meshRend = GetComponentInChildren<SkinnedMeshRenderer>();
        defaultModel = meshRend.sharedMesh;

        CrewHand[] hands = GetComponentsInChildren<CrewHand>();
        rightHand = hands[0].ikGoal == AvatarIKGoal.RightHand ? hands[0] : hands[1];
        leftHand = hands[0].ikGoal == AvatarIKGoal.RightHand ? hands[1] : hands[0];
        
        FootRest[] feet = GetComponentsInChildren<FootRest>();
        standingRightFoot = feet[0].transform.localPosition.x > 0f ? feet[0] : feet[1];
        standingLeftFoot = feet[0].transform.localPosition.x > 0f ? feet[1] : feet[0];
    }

    private Vector3 accelerationCompensation = Vector3.zero;
    private Vector3 vel = Vector3.zero;
    private void Update()
    {
        if (!Application.isPlaying)
        {
            GetReferences();
            animator.Update(0f);
        }
        bool firstPerson = PlayerManager.player.crew == crew && (GameManager.gm.vr || PlayerCamera.subCam.pos == CamPosition.FirstPerson);
        bool newDisabled = (crew.aircraft && crew.aircraft.lod.LOD() > 1) || (firstPerson && !firstPersonModel);
        if (newDisabled != disabled)
        {
            disabled = newDisabled;
            meshRend.enabled = !disabled;
        }
        if (disabled && !firstPerson) return;
        if (firstPerson != noHead)
        {
            meshRend.sharedMesh = firstPerson ? headLessModel : defaultModel;
            noHead = firstPerson;
        }
        if (Time.timeScale == 0f) return;

        ObjectData data = crew.data;
        Transform tr = transform;

        //References
        CrewSeat seat = crew.seats[Mathf.Clamp(crew.currentSeat, 0, crew.seats.Length - 1)];
        bool player = PlayerManager.player.crew == crew;
        bool vrPlayer = Application.isPlaying && GameManager.gm.vr && player;
        Vector3 pos = Vector3.zero;
        Quaternion rot = Quaternion.identity;
        if (seat)
        {
            pos = vrPlayer ? PlayerCamera.camPos : seat.HeadPosition(player);
            if (data && PlayerManager.player.sofObj == crew.sofObject && !vrPlayer)
            {
                Vector3 gOffsetLocal = data.acceleration / Physics.gravity.y;
                gOffsetLocal.y *= 0.5f;
                Vector3 accelerationOffset = data.tr.TransformDirection(gOffsetLocal);
                accelerationOffset += (data.tr.up - Vector3.up) * 4f;
                accelerationCompensation = Vector3.MoveTowards(accelerationCompensation, accelerationOffset, Time.deltaTime * 2f);
                accelerationCompensation = Vector3.ClampMagnitude(accelerationCompensation, accelerationOffset.magnitude);
                pos += (accelerationOffset - accelerationCompensation) / 50f;
            }
            if (!Application.isPlaying) seat.tr = seat.transform;
            rot = Quaternion.LookRotation(seat.tr.position - pos, seat.tr.forward * 2f + (vrPlayer ? PlayerCamera.camTr.forward : Vector3.zero));
            rot *= Quaternion.Euler(-90f, 0f, 0f);

            pos = tr.parent.InverseTransformPoint(pos);
            pos += CrewMember.eyeShift * Vector3.down;
        }
        tr.localPosition = Vector3.SmoothDamp(tr.localPosition, pos, ref vel, smoothTime, headMoveSpeed, Time.deltaTime) ;
        tr.rotation = rot;
    }
    private void OnAnimatorIK()
    {
        if (Application.isPlaying)
            if (crew.seats.Length == 0 || crew.seats[0] == null || !crew.complex || crew.complex.lod.LOD() > 1) return;

        //References
        Transform tr = crew.transform;
        CrewSeat seat = crew.seats[Mathf.Clamp(crew.currentSeat, 0, crew.seats.Length - 1)];

        AnimateBody(tr, seat);
        AnimateArms(tr, seat);
        AnimateLegs(seat);
        AnimateHead(tr, seat);
    }
    private void AnimateHead(Transform tr, CrewSeat seat)
    {
        Vector3 localDir = tr.InverseTransformDirection(seat.headLookDirection + seat.transform.forward * 0.05f);
        localDir.y *= 0.35f;
        localDir.z = Mathf.Abs(localDir.z);
        Vector3 dir = tr.TransformDirection(localDir) * 100f;
        if (crew.ripped) dir = tr.forward - tr.up * 2f;
        crew.headLookAt = tr.position + dir;

        animator.SetLookAtWeight(1);
        animator.SetLookAtPosition(crew.headLookAt);
    }
    private void AnimateBody(Transform tr, CrewSeat seat)
    {
        float distance = (tr.position - seat.transform.position).magnitude;
        float leaning = Mathf.InverseLerp(standingButtHead, leaningButtHead, distance);
        animator.SetFloat("Leaning", leaning);
    }
    private void AnimateArms(Transform tr, CrewSeat seat)
    {
        bool vrPlayer = Application.isPlaying && GameManager.gm.vr && PlayerManager.player.crew == crew;
        HandGrip right = vrPlayer ? SofVrRig.instance.rightHandGrip : seat.rightHandGrip;
        if (!right || right.transform.root != tr.root) right = restingGrip;
        HandGrip left = vrPlayer ? SofVrRig.instance.leftHandGrip : seat.leftHandGrip;
        if (!left || left.transform.root != tr.root) left = restingGrip;
        if (right) rightHand.SetHandPose(animator, right);
        if (left) leftHand.SetHandPose(animator, left);
    }
    private void AnimateLegs(CrewSeat seat)
    {
        FootRest rightFoot = seat.rightFootRest ? seat.rightFootRest : standingRightFoot;
        FootRest leftFoot = seat.leftFootRest ? seat.leftFootRest : standingLeftFoot;
        if (rightFoot) rightFoot.SetFootPose(animator, AvatarIKGoal.RightFoot);
        if (leftFoot) leftFoot.SetFootPose(animator, AvatarIKGoal.LeftFoot);
    }
}
