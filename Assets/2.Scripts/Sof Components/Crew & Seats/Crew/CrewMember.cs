using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.SceneManagement;
#endif

[AddComponentMenu("Sof Components/Crew Seats/CrewMember")]
public class CrewMember : SofModule, IMassComponent
{
    public override ModuleArmorValues Armor(Collider collider)
    {
        return ModulesHPData.CrewmemberArmor;
    }
    public override float MaxHp => ModulesHPData.crewmember;

    public float EmptyMass => 0f;
    public float LoadedMass => RealMass;
    public float RealMass => 90f;

    public List<CrewSeat> seats;

    private CrewSeat seat;
    public CrewSeat Seat
    {
#if UNITY_EDITOR
        get { return Application.isPlaying ? seat : editorTestSeat; }
#else
        get { return seat; }
#endif
        protected set { seat = value; }
    }
#if UNITY_EDITOR
    public int seatIdTest = 0;

    public CrewSeat editorTestSeat
    {
        get
        {
            if (seats == null || seats.Count == 0) return null;
            int id = Mathf.Clamp(seatIdTest, 0, seats.Count - 1);
            return seats[id];
        }
    }
#endif
    public bool IsPilot
    {
        get
        {
            foreach (CrewSeat seat in seats)
                if (seat.GetType() == typeof(PilotSeat)) return true;
            return false;
        }
    }
    public bool IsPlayer => Player.crew == this;
    public bool IsVrPlayer => Player.crew == this && GameManager.gm.vr && Application.isPlaying;
    public int SeatId => seats.IndexOf(Seat);
    protected bool SkipUpdate => Time.timeScale == 0f || !sofObject;
    public bool ActionsUnavailable => ripped || IsVrPlayer || (forcesEffect != null && forcesEffect.Gloc) || (stunDuration > 0f && Player.crew != this);

    public float stunDuration;
    public CrewBailing bailOut { get; private set; }
    public CrewForcesEffect forcesEffect { get; protected set; }

    public Action OnAttachPlayer;
    public Action OnDetachPlayer;



    readonly Vector3 eyesOffset = new Vector3(0f, -0.05f, -0.1f);

    public Vector3 TargetHeadWorldPosition
    {
        get
        {
            if (!Seat) return transform.position;
            if (IsVrPlayer) return SofCamera.tr.position;

            Vector3 headPosition = CameraInputs.zoomed && IsPlayer && SofCamera.viewMode == 1 ? Seat.ZoomedHeadPosition : Seat.DefaultHeadPosition;

            if (IsPlayer && forcesEffect != null)
                headPosition += forcesEffect.headPositionOffset;

            if (IsPlayer)
            {
                Vector3 camDir = SofCamera.CurrentDirection;
                camDir = Vector3.ProjectOnPlane(camDir, sofObject.tr.up);

                float dotProduct = Vector3.Dot(camDir, -sofObject.tr.forward);
                if (dotProduct < 0f) dotProduct = 0f;

                float side = Vector3.Dot(sofObject.tr.right, camDir);
                side = Mathf.Sign(side) * Mathf.Min(1f,Mathf.Abs(side) * 8f);
                side *= side > 0f ? seat.lookBehindRightShift : seat.lookBehindLeftShift;

                headPosition += side * dotProduct * sofObject.tr.right;
            }

            return headPosition;
        }
    }
    public Vector3 TargetHeadLocalPosition
    {
        get
        {
            return tr.parent.InverseTransformPoint(TargetHeadWorldPosition + tr.TransformDirection(eyesOffset));
        }
    }
    public Vector3 CameraPosition
    {
        get
        {
            return tr.position + tr.TransformDirection(-eyesOffset);
        }
    }


    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);

        bailOut = GetComponent<CrewBailing>();
        forcesEffect = new CrewForcesEffect(this);

        UpdateSeatsList();
        SwitchSeat(0);

        tr.localPosition = tr.parent.InverseTransformPoint(Seat.DefaultHeadPosition);

        CreateAndSetCapsuleCollider();

        if (aircraft)
        {
            GameObject visualModel = aircraft.card.faction.crewMemberVisualModel;
            bool historicalTeam = aircraft.squadron.team == aircraft.card.faction.defaultTeam;

            if (!historicalTeam)
            {
                visualModel = aircraft.squadron.team == Game.Team.Axis ? StaticReferences.Instance.defaultAxisCrewmember : StaticReferences.Instance.defaultAlliesCrewmember;
            }

            visualModel = Instantiate(visualModel, transform.position, transform.rotation, tr);
            visualModel.GetComponent<CrewAnimator>().SetInstanciatedComponent(sofModular);
        }
    }

    const float headSmoothTime = 0.2f;
    Vector3 velRef = Vector3.zero;
    private void Update()
    {
        if (Player.crew == this) MoveToSeatSmooth();

        if (stunDuration > 0f) stunDuration = Mathf.Min(stunDuration - Time.deltaTime, 15f);
        if (ActionsUnavailable || SkipUpdate) return;

        if (Player.crew == this && Player.controllingPlayer) Seat.PlayerUpdate(this);
        else
        {
            SwitchToPrioritySeat();
            Seat.AiUpdate(this);
        }
    }
    private void FixedUpdate()
    {
        if (aircraft) forcesEffect.UpdateForces(Time.fixedDeltaTime);
    }
    public override void Rip()
    {
        if (aircraft && IsPilot) aircraft.Destroy();
        base.Rip();
    }
    protected void SwitchToPrioritySeat()
    {
        int newSeatId = 0;
        for (int i = 1; i < seats.Count; i++)
        {
            if (seats[i].Priority > seats[newSeatId].Priority) newSeatId = i;
        }
        if (seats[newSeatId] == Seat) return;
        SwitchSeat(newSeatId);
    }
    protected void UpdateSeatsList()
    {
        for (int i = 0; i < seats.Count; i++)
            if (!seats[i] || seats[i].sofModular != sofModular)
            {
                seats.RemoveAt(i);
                i--;
            }
    }
    public void ChangeSeat(CrewSeat newSeat)
    {
        bool seatError = newSeat == null || newSeat.sofModular != sofModular || newSeat.seatedCrew != null;
        if (seatError) UpdateSeatsList();
        if (seatError || newSeat == Seat) return;

        CrewSeat oldSeat = Seat;
        if (oldSeat) oldSeat.OnCrewLeaves();

        Seat = newSeat;
        Seat.OnCrewEnters(this);
    }
    public void SwitchSeat(int newSeatId)
    {
        if (newSeatId < 0 || newSeatId >= seats.Count) return;
        ChangeSeat(seats[newSeatId]);
    }
    public void MoveToSeatSmooth()
    {
        if ((tr.localPosition - TargetHeadLocalPosition).sqrMagnitude < 0.0001f * 0.0001f) return;

        tr.localPosition = Vector3.SmoothDamp(tr.localPosition, TargetHeadLocalPosition, ref velRef, headSmoothTime);
    }
    public void MoveToSeatInstant()
    {
        tr.localPosition = TargetHeadLocalPosition;
    }

    const float capsuleHeight = 0.96f;
    const float capsuleRadius = 0.17f;
    const float capsuleCenter = -0.39f;
    private void CreateAndSetCapsuleCollider()
    {
        CapsuleCollider collider = this.GetCreateComponent<CapsuleCollider>();

        collider.isTrigger = true;
        collider.direction = 1;
        collider.height = capsuleHeight;
        collider.radius = capsuleRadius;
        collider.center = new Vector3(0f, capsuleCenter, 0f);
    }
}
