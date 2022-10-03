using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CrewAnimator : ObjectElement
{
    public Transform head;

    private CrewMember crew;
    private CrewHand rightHand;
    private CrewHand leftHand;
    private FootRest standingRightFoot;
    private FootRest standingLeftFoot;
    private Animator animator;

    private Vector3 headLookAt;
    private Vector3 accelerationOffset = Vector3.zero;
    private Vector3 accelerationCompensation = Vector3.zero;

    const float standingButtHead = 0.88f;
    const float leaningButtHead = 0.68f;

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        if (firstTime) GetReferences();
    }
    private void GetReferences()
    {
        crew = GetComponent<CrewMember>();

        CrewHand[] hands = GetComponentsInChildren<CrewHand>();
        rightHand = hands[0].ikGoal == AvatarIKGoal.RightHand ? hands[0] : hands[1];
        leftHand = hands[0].ikGoal == AvatarIKGoal.RightHand ? hands[1] : hands[0];

        FootRest[] feet = GetComponentsInChildren<FootRest>();
        standingRightFoot = feet[0].transform.localPosition.x > 0f ? feet[0] : feet[1];
        standingLeftFoot = feet[0].transform.localPosition.x > 0f ? feet[1] : feet[0];

        animator = GetComponent<Animator>();
    }
    private void Update()
    {
        if (!Application.isPlaying)
        {
            GetReferences();
            animator.Update(0f);
        }
        //References
        CrewSeat seat = crew.seats[Mathf.Clamp(crew.currentSeat, 0, crew.seats.Length - 1)];
        bool player = GameManager.player.crew == crew;
        bool vrPlayer = Application.isPlaying && GameManager.gm.vr && player;
        Vector3 pos = Vector3.zero;
        Quaternion rot = Quaternion.identity;
        if (seat)
        {
            pos = vrPlayer ? Camera.main.transform.position : seat.HeadPosition(player);
            if (data)
            {
                Vector3 gOffsetLocal = data.acceleration / Physics.gravity.y;
                gOffsetLocal.y *= 0.5f;
                accelerationOffset = transform.root.TransformDirection(gOffsetLocal);
                accelerationOffset += (transform.root.up - Vector3.up) * 4f;
                accelerationCompensation = Vector3.MoveTowards(accelerationCompensation, accelerationOffset, Time.deltaTime * 2f);
                pos += (accelerationOffset - accelerationCompensation) / 50f;
            }
            rot = Quaternion.LookRotation(seat.transform.position - pos, seat.transform.forward * 2f + (vrPlayer ? Camera.main.transform.forward : Vector3.zero));
            rot *= Quaternion.Euler(-90f, 0f, 0f);
        }
        transform.SetPositionAndRotation(pos, rot);
    }
    private void OnAnimatorIK()
    {
        if (!head || crew.seats.Length == 0 || crew.seats[0] == null) return;

        //References
        Transform tr = crew.transform;
        CrewSeat seat = crew.seats[Mathf.Clamp(crew.currentSeat, 0,crew.seats.Length - 1)];
        bool vrPlayer = Application.isPlaying && GameManager.gm.vr && GameManager.player.crew == crew;

        //Body
        float distance = (tr.position - seat.transform.position).magnitude;
        float leaning = Mathf.InverseLerp(standingButtHead, leaningButtHead, distance);
        animator.SetFloat("Leaning", leaning);

        //Arms
        HandGrip right = vrPlayer ? SofVrRig.instance.rightHandGrip : seat.rightHandGrip;
        HandGrip left = vrPlayer ? SofVrRig.instance.leftHandGrip : seat.leftHandGrip;
        if (right) rightHand.SetHandPose(animator, right);
        if (left) leftHand.SetHandPose(animator, left);

        //Legs
        FootRest rightFoot = seat.rightFootRest ? seat.rightFootRest : standingRightFoot;
        FootRest leftFoot = seat.leftFootRest ? seat.leftFootRest : standingLeftFoot;
        if (rightFoot) rightFoot.SetFootPose(animator, AvatarIKGoal.RightFoot);
        if (leftFoot) leftFoot.SetFootPose(animator, AvatarIKGoal.LeftFoot);

        //Head
        head.forward = seat.headLookDirection + seat.transform.forward * 0.05f;
        head.localPosition = Vector3.zero;
        Vector3 localDir = tr.InverseTransformDirection(head.forward);
        localDir.y *= 0.35f;
        localDir.z = Mathf.Abs(localDir.z);
        Vector3 dir = tr.TransformDirection(localDir) * 100f;
        if (crew.ripped) dir = tr.forward - tr.up * 2f;
        headLookAt = tr.position + dir;

        animator.SetLookAtWeight(1);
        animator.SetLookAtPosition(headLookAt);
    }
}
