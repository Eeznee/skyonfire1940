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
    public static int crewId;

    public static CrewSeat seat;
    public static SeatInterface seatInterface = SeatInterface.Empty;

    public static event Action OnSeatChange;

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
        seatInterface = SeatInterface.Empty;

        OnSeatChange?.Invoke();
    }
    public static void Set(SofComplex target)
    {
        if (target == null || target == complex) return;

        sofObj = complex = target;
        aircraft = sofObj.GetComponent<SofAircraft>();
        tr = sofObj.tr;

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

        crew = null;
        SetCrew(0);
    }
    public static void SetCrew(int newCrewId)
    {
        CrewMember[] crewList = crew ? crew.crewGroup : complex.crew;

        bool nullId = newCrewId != Mathf.Clamp(newCrewId, 0, crewList.Length - 1);

        if (nullId || crewList[newCrewId] == crew || !complex) return;

        crewId = newCrewId;
        crew = crewList[crewId];

        string txt = "Switched to crew " + (crewId + 1);
        if (aircraft)
        {
            if (crewId == 0) txt = "Switched to pilot";
            else txt = "Switched to crewman n°" + (crewId + 1);
        }
        Log.Print(txt, "Switch Crew");

        SetSeat(0);
    }
    public static void SetSeat(int seatId)
    {
        bool nullId = seatId != Mathf.Clamp(seatId, 0, crew.seats.Length - 1);
        if (nullId || !crew) return;

        crew.SwitchSeat(seatId);

        seat = crew.Seat;
        seatInterface = crew.Seat.SeatUI();

        OnSeatChange?.Invoke();
    }
    public static void SetSeat(int crewId, int seatId) { SetCrew(crewId); SetSeat(seatId); }
}
