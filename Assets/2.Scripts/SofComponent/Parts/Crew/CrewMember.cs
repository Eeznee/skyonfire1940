using System.Collections.Generic;
using UnityEngine;

public class CrewMember : SofModule
{
    public List<CrewSeat> seats;
    public Parachute parachute;
    public Parachute specialPlayerParachute;
    public int seatIdTest = 0;

    public CrewAnimator crewAnimator { get; private set; }
    public CrewBailing bailOut { get; private set; }
    public HumanBody humanBody { get; private set; }
    public CrewSeat seat { get; private set; }

    public bool IsPlayer => Player.crew == this;
    public bool IsVrPlayer => Player.crew == this && GameManager.gm.vr && Application.isPlaying;
    public int SeatId => seats.IndexOf(seat);

    public override bool NoCustomMass => true;
    public override float AdditionalMass => HumanBody.Weight();
    public override float EmptyMass => 0f;

    public const float eyeShift = 0.05f;
    public Vector3 EyesPosition() { return transform.position + transform.parent.up * eyeShift; }
    public string Action() { return seat.Action; }

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);

        bailOut = new CrewBailing(this);
        humanBody = new HumanBody(this);
        crewAnimator = GetComponent<CrewAnimator>();

        UpdateSeatsList();
        SwitchSeat(0);
    }
    private bool SkipUpdate { get { return ripped || Time.timeScale == 0f || !sofObject; } }
    private bool SkipActions { get { return IsVrPlayer || humanBody.Gloc(); } }
    private void Update()
    {
        if (SkipUpdate) return;

        humanBody.ApplyForces(data.gForce, Time.deltaTime);

        if (SkipActions) return;

        bailOut.Update();

        if (Player.crew == this) seat.PlayerUpdate(this);
        else
        {
            SwitchToPrioritySeat();
            seat.AiUpdate(this);
        }
    }
    private void FixedUpdate()
    {
        if (SkipActions || SkipUpdate) return;

        if (Player.crew == this) seat.PlayerFixed(this);
        else seat.AiFixed(this);
    }
    private void SwitchToPrioritySeat()
    {
        int newSeatId = 0;
        for (int i = 1; i < seats.Count; i++) if (seats[i].Priority > seats[newSeatId].Priority) newSeatId = i;
        SwitchSeat(newSeatId);
    }
    public void AttachPlayer()
    {
        if (crewAnimator) crewAnimator.ToggleFirstPersonModel();
    }
    public void DetachPlayer()
    {
        if (crewAnimator) crewAnimator.ToggleFirstPersonModel();
    }
    private void UpdateSeatsList()
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
        if(seatError) UpdateSeatsList();
        if (seatError || newSeat == seat) return;

        CrewSeat oldSeat = seat;
        if(oldSeat) oldSeat.OnCrewLeaves();

        seat = newSeat;
        seat.OnCrewSeats(this);
    }
    public void SwitchSeat(int newSeatId)
    {
        if (newSeatId < 0 || newSeatId >= seats.Count) return;
        ChangeSeat(seats[newSeatId]);
    }
    public override void Rip()
    {
        if (aircraft && this == aircraft.crew[0]) { aircraft.hasPilot = false; aircraft.destroyed = true; }
        base.Rip();
    }
}
