using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.SceneManagement;
#endif

[AddComponentMenu("Sof Components/Crew Seats/CrewMember")]
public class CrewMember : SofModule, IMassComponent
{
    public override ModuleArmorValues Armor => ModulesHPData.CrewmemberArmor;
    public override float MaxHp => ModulesHPData.crewmember;

    public float EmptyMass => 0f;
    public float LoadedMass => RealMass;
    public float RealMass => CrewForcesEffect.Weight();


    public List<CrewSeat> seats;


    private CrewSeat seat;
    public CrewSeat Seat
    {
        get { return Application.isPlaying ? seat : editorTestSeat; }
        protected set { seat = value; }
    }
    public CrewSeat editorTestSeat
    {
        get
        {
            if (seats == null || seats.Count == 0) return null;
            int id = Mathf.Clamp(seatIdTest, 0, seats.Count - 1);
            return seats[id];
        }
    }
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
    public bool ActionsUnavailable => ripped || IsVrPlayer || (forcesEffect != null && forcesEffect.Gloc());
    public Vector3 HeadPosition => tr.parent.TransformPoint(localHeadPosition);
    public Vector3 CameraPosition => tr.position;


    private Vector3 localHeadPosition;

    public CrewBailing bailOut { get; private set; }
    public CrewForcesEffect forcesEffect { get; protected set; }

    public Action OnAttachPlayer;
    public Action OnDetachPlayer;

    public Vector3 TargetHeadPosition()
    {
        if (!Seat) return transform.position;
        if (IsVrPlayer) return SofCamera.tr.position;

        Vector3 headPosition = CameraInputs.zoomed && IsPlayer ? Seat.ZoomedHeadPosition : Seat.DefaultHeadPosition;

        if (IsPlayer && forcesEffect != null)
            headPosition += forcesEffect.headPositionOffset;

        return headPosition;
    }

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);

        bailOut = GetComponent<CrewBailing>();
        forcesEffect = new CrewForcesEffect(this);

        UpdateSeatsList();
        SwitchSeat(0);

        localHeadPosition = tr.parent.InverseTransformPoint(Seat.DefaultHeadPosition);

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
            visualModel.GetComponent<CrewAnimator>().SetInstanciatedComponent(complex);
        }
    }

    const float headSmoothTime = 0.2f;
    Vector3 velRef = Vector3.zero;
    private void Update()
    {
        if (ActionsUnavailable || SkipUpdate) return;

        if (Player.crew == this) Seat.PlayerUpdate(this);
        else
        {
            SwitchToPrioritySeat();
            Seat.AiUpdate(this);
        }

        MoveToSeatSmooth();
    }
    private void FixedUpdate()
    {
        if(aircraft) forcesEffect.UpdateForces(Time.fixedDeltaTime);

        if (ActionsUnavailable || SkipUpdate) return;
        if (IsPilot) return;

        if (Player.crew == this) Seat.PlayerFixed(this);
        else Seat.AiFixed(this);
    }
    public override void Rip()
    {
        if (aircraft && IsPilot) aircraft.destroyed = true;
        base.Rip();
    }
    protected void SwitchToPrioritySeat()
    {
        int newSeatId = 0;
        for (int i = 1; i < seats.Count; i++) if (seats[i].Priority > seats[newSeatId].Priority) newSeatId = i;

        if (seats[newSeatId] == Seat) return;
        SwitchSeat(newSeatId);
    }
    protected void UpdateSeatsList()
    {
        for (int i = 0; i < seats.Count; i++)
            if (!seats[i] || seats[i].complex != complex)
            {
                seats.RemoveAt(i);
                i--;
            }
    }
    public void ChangeSeat(CrewSeat newSeat)
    {
        bool seatError = newSeat == null || newSeat.complex != complex || newSeat.seatedCrew != null;
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
        Vector3 targetLocalPos = tr.parent.InverseTransformPoint(TargetHeadPosition());
        localHeadPosition = Vector3.SmoothDamp(localHeadPosition, targetLocalPos, ref velRef, headSmoothTime);
        tr.localPosition = localHeadPosition;
    }
    public void MoveToSeatInstant()
    {
        localHeadPosition = tr.parent.InverseTransformPoint(TargetHeadPosition());
        tr.localPosition = localHeadPosition;
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

#if UNITY_EDITOR
    public int seatIdTest = 0;
#endif

#if UNITY_EDITOR
    void OnValidate()
    {

    }
#endif
}
