using UnityEngine;
using System;

public class Player : MonoBehaviour
{
    public static Transform tr;
    public static SofObject sofObj;
    public static SofComplex complex;
    public static SofAircraft aircraft;

    private static int squad = 0;
    private static int wing = 0;

    public static CrewMember crew;
    public static int crewId
    {
        get
        {
            for (int i = 0; i < complex.crew.Length; i++)
                if (complex.crew[i] == crew) return i;
            return -1;
        }
    }

    public static CrewSeat seat;
    public static PilotSeat pilotSeat;
    public static GunnerSeat gunnerSeat;
    public static BombardierSeat bombardierSeat;
    public static SeatRole role = SeatRole.Simple;

    public static event Action OnSeatChange;
    public static event Action OnCrewChange;

    private void Awake()
    {
        Set(GameManager.squadrons[PlayerPrefs.GetInt("PlayerSquadron", 0)][0]);
    }
    public static void NextSquadron(int offset)
    {
        Set(OffsetSquadron(offset, squad));
        if (SofCamera.viewMode < 0) SofCamera.SwitchViewMode(0);
    }
    public static void NextWing(int offset)
    {
        Set(OffsetWing(offset, wing, squad));
        if (SofCamera.viewMode < 0) SofCamera.SwitchViewMode(0);
    }
    public static SofAircraft OffsetSquadron(int offset, int squad)
    {
        int squadronId = (int)Mathf.Repeat(squad + offset, GameManager.squadrons.Count);
        SofAircraft[] squadron = GameManager.squadrons[squadronId];

        foreach (SofAircraft a in squadron)
            if (a.crew[0] && a.crew[0].sofObject == a)
                return a;

        if (Mathf.Abs(offset) < GameManager.squadrons.Count)
            return OffsetSquadron(offset + (int)Mathf.Sign(offset), squad);
        return null;
    }
    public static SofAircraft OffsetWing(int offset, int wing, int squad)
    {
        SofAircraft[] squadron = GameManager.squadrons[squad];
        return squadron[(int)Mathf.Repeat(wing + offset, squadron.Length)];
    }
    public static void PlayerNull()
    {
        sofObj = complex = aircraft = null;
        tr = null;
        crew = null;
        seat = null;
        gunnerSeat = null;
        pilotSeat = null;
        bombardierSeat = null;
        role = SeatRole.Simple;

        OnCrewChange?.Invoke();
        OnSeatChange?.Invoke();
    }
    public static void Set(SofComplex target)
    {
        if (target == null || target == complex) return;

        sofObj = complex = target;
        aircraft = sofObj.GetComponent<SofAircraft>();
        tr = sofObj.transform;

        if (aircraft)
        {
            squad = aircraft.squadronId;
            wing = aircraft.placeInSquad;

            Log.Print("Controlling " + aircraft.card.completeName, "Controlling Object");
            string txt = "Squadron " + (aircraft.squadronId + 1);
            txt += "; Aircraft n°" + (aircraft.placeInSquad + 1);
            Log.Print(txt, "Squadron");
        }
        else Log.Print("Controlling " + complex.name, "Controlling Object");

        SetCrew(0);
    }
    public static void SetCrew(CrewMember newCrew)
    {
        if (newCrew == crew || !complex) return;

        if (newCrew.complex != complex)
        {
            Set(newCrew.complex);
        }

        CrewMember previousCrew = crew;

        crew = newCrew;
        newCrew.AttachPlayer();
        if (previousCrew) previousCrew.DetachPlayer();

        string txt = "Switched to crew " + (crewId + 1);
        if (aircraft)
        {
            if (crewId == 0) txt = "Switched to pilot";
            else txt = "Switched to crewman n°" + (crewId + 1);
        }
        Log.Print(txt, "Switch Crew");

        SetSeat(0);

        OnCrewChange?.Invoke();
    }
    public static void SetCrew(int newCrewId)
    {
        bool nullId = newCrewId != Mathf.Clamp(newCrewId, 0, complex.crew.Length - 1);
        if (nullId) return;

        SetCrew(complex.crew[newCrewId]);
    }
    public static void SetSeat(CrewSeat seat)
    {
        SetSeat(seat.id);
    }
    public static void SetSeat(int seatId)
    {
        bool nullId = seatId != Mathf.Clamp(seatId, 0, crew.seats.Count - 1);
        if (nullId || !crew) return;

        crew.SwitchSeat(seatId);

        seat = crew.Seat;
        role = crew.Seat.role;
        pilotSeat = role == SeatRole.Pilot ? (PilotSeat)seat : null;
        gunnerSeat = role == SeatRole.Gunner ? (GunnerSeat)seat : null;
        bombardierSeat = role == SeatRole.Bombardier ? (BombardierSeat)seat : null;

        OnSeatChange?.Invoke();
    }
    public static void CycleSeats()
    {
        SetSeat((crew.SeatId + 1) % crew.seats.Count);
    }
    public static void SetSeat(SeatId seatId) { SetCrew(seatId.crewId); SetSeat(seatId.seatId); }
}
