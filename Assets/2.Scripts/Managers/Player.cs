using UnityEngine;
using System;

public class Player : MonoBehaviour
{
    public static bool controllingPlayer;
    public static Transform tr;
    public static SofObject sofObj;
    public static SofModular modular;
    public static SofAircraft aircraft;
    public static Game.Squadron Squadron => aircraft ? aircraft.squadron : null;

    private static int squad = 0;
    private static int wing = 0;

    public static CrewMember crew;
    public static int crewId
    {
        get
        {
            if (modular == null || crew == null) return -1; 
            for (int i = 0; i < modular.crew.Length; i++)
                if (modular.crew[i] == crew) return i;
            return -1;
        }
    }
    public static string Tag => sofObj ? sofObj.tag : "Ally";

    public static CrewSeat seat;
    public static PilotSeat pilotSeat;
    public static GunnerSeat gunnerSeat;
    public static BombardierSeat bombardierSeat;
    public static SeatRole role = SeatRole.Simple;

    public static event Action OnSeatChange;
    public static event Action OnCrewChange;
    public static event Action OnComplexChange;

    private void Awake()
    {
        controllingPlayer = true;
        Set(GameManager.squadrons[PlayerPrefs.GetInt("PlayerSquadron", 0)][0]);
    }
    public static void NextSquadron(int offset)
    {
        SofModular newPlayer = OffsetSquadron(offset, squad);
        if (newPlayer == null) return;
        Set(newPlayer);
        if (SofCamera.viewMode < 0) SofCamera.SwitchViewMode(0);
    }
    public static void NextWing(int offset)
    {
        SofModular newPlayer = OffsetWing(offset, wing, squad);
        if (newPlayer == null) return;
        Set(newPlayer);
        if (SofCamera.viewMode < 0) SofCamera.SwitchViewMode(0);
    }
    public static SofModular OffsetSquadron(int offset, int squad)
    {
        int squadronId = (int)Mathf.Repeat(squad + offset, GameManager.squadrons.Count + 1);
        SofModular[] squadron = squadronId == GameManager.squadrons.Count ? GameManager.crewedModulars.ToArray() : GameManager.squadrons[squadronId];

        foreach (SofModular a in squadron)
            if (a.crew[0] && a.crew[0].sofObject == a)
                return a;


        //if squadron is empty, switch to the next one
        if (Mathf.Abs(offset) < GameManager.squadrons.Count)
            return OffsetSquadron(offset + (int)Mathf.Sign(offset), squad);


        return null;
    }
    public static SofModular OffsetWing(int offset, int wing, int squadronId)
    {
        SofModular[] squadron = squadronId == GameManager.squadrons.Count ? GameManager.crewedModulars.ToArray() : GameManager.squadrons[squadronId];


        SofModular modular = squadron[(int)Mathf.Repeat(wing + offset, squadron.Length)];
        if (modular.crew[0].sofModular == modular)
            return modular;

        //if squadron is empty, switch to the next one
        if (Mathf.Abs(offset) < squadron.Length)
            return OffsetWing(offset + (int)Mathf.Sign(offset), wing, squad);


        return null;
    }
    public static void PlayerNull()
    {
        sofObj = modular = aircraft = null;
        tr = null;
        crew = null;
        seat = null;
        gunnerSeat = null;
        pilotSeat = null;
        bombardierSeat = null;
        role = SeatRole.Simple;

        OnComplexChange?.Invoke();
        OnCrewChange?.Invoke();
        OnSeatChange?.Invoke();
        modular.OnDetachPlayer?.Invoke();
        crew.OnDetachPlayer?.Invoke();
    }
    public static void SetPlayer(CrewMember newCrew, int newSeatId)
    {
        bool nullId = newSeatId != Mathf.Clamp(newSeatId, 0, newCrew.seats.Count - 1);
        if (nullId) return;

        CrewMember previousCrew = crew;
        SofModular previousModular = modular;
        SofModular newComplex = newCrew.GetComponentInParent<SofModular>(true);

        bool differentComplex = newComplex != modular;
        bool differentCrew = crew != newCrew;
        bool differentSeat = differentComplex || differentCrew || crew.SeatId != newSeatId;

        if (!differentSeat) return;

        if (differentComplex)
        {
            sofObj = modular = newComplex;
            aircraft = modular.GetComponent<SofAircraft>();
            tr = modular.transform;

            if (aircraft)
            {
                squad = aircraft.SquadronId;
                wing = aircraft.placeInSquad;

                Log.Print("Controlling " + aircraft.card.completeName, "Controlling Object");
                string logTxt = "Squadron " + (aircraft.SquadronId + 1);
                logTxt += "; Aircraft n°" + (aircraft.placeInSquad + 1);
                Log.Print(logTxt, "Squadron");
            }
            else
            {
                squad = GameManager.squadrons.Count;
                wing = GameManager.crewedModulars.IndexOf(modular);
                Log.Print("Controlling " + modular.name, "Controlling Object");
            }
        }

        crew = newCrew;

        string txt = "Switched to crew " + (crewId + 1);
        if (aircraft)
        {
            if (crewId == 0) txt = "Switched to pilot";
            else txt = "Switched to crewman n°" + (crewId + 1);
        }
        Log.Print(txt, "Switch Crew");

        crew.SwitchSeat(newSeatId);

        seat = crew.Seat;
        role = crew.Seat.role;
        pilotSeat = role == SeatRole.Pilot ? (PilotSeat)seat : null;
        gunnerSeat = role == SeatRole.Gunner ? (GunnerSeat)seat : null;
        bombardierSeat = role == SeatRole.Bombardier ? (BombardierSeat)seat : null;

        if (differentComplex)
        {
            OnComplexChange?.Invoke();
            modular.OnAttachPlayer?.Invoke();
            if (previousModular != null) previousModular.OnDetachPlayer?.Invoke();
        }
        if (differentCrew)
        {
            OnCrewChange?.Invoke();
            newCrew.OnAttachPlayer?.Invoke();
            if (previousCrew) previousCrew.OnAttachPlayer?.Invoke();
        }
        if (differentSeat) OnSeatChange?.Invoke();
    }

    public static void Set(SofModular newComplex)
    {
        if (newComplex == modular) return;
        if (newComplex == null) return;
        SetPlayer(newComplex.crew[0], 0);
    }
    public static void SetCrew(CrewMember newCrew)
    {
        if (newCrew == crew) return;
        SetPlayer(newCrew, 0);
    }
    public static void SetCrew(int newCrewId)
    {
        bool nullId = newCrewId != Mathf.Clamp(newCrewId, 0, modular.crew.Length - 1);
        if (nullId) return;

        SetCrew(modular.crew[newCrewId]);
    }
    public static void SetSeat(CrewSeat seat)
    {
        SetSeat(seat.id);
    }
    public static void SetSeat(int newSeatId)
    {
        if (newSeatId == crew.SeatId) return;

        bool nullId = newSeatId != Mathf.Clamp(newSeatId, 0, crew.seats.Count - 1);
        if (nullId || !crew) return;

        SetPlayer(crew, newSeatId);
    }
    public static void CycleSeats()
    {
        SetSeat((crew.SeatId + 1) % crew.seats.Count);
    }
    public static void SetSeat(SeatId seatId) { SetPlayer(modular.crew[seatId.crewId], seatId.seatId); }


    /*
   public static void Set(SofComplex target)
   {
       if (target == null || target == complex) return;

       sofObj = complex = target;
       aircraft = sofObj.GetComponent<SofAircraft>();
       tr = sofObj.transform;

       if (aircraft)
       {
           squad = aircraft.SquadronId;
           wing = aircraft.placeInSquad;

           Log.Print("Controlling " + aircraft.card.completeName, "Controlling Object");
           string txt = "Squadron " + (aircraft.SquadronId + 1);
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
   */
}
