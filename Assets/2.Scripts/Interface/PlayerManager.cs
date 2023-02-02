using UnityEngine;

public struct AircraftReferences
{
    public SofObject sofObj;
    public SofComplex complex;
    public SofAircraft aircraft;
    public CrewMember crew;
    public Transform tr;
    public void Set(CrewMember _crew)
    {
        crew = _crew;
        if (crew != null)
        {
            sofObj = crew.sofObject;
            complex = crew.complex;
            aircraft = crew.aircraft;
            tr = sofObj.transform;
        }
        else
        {
            sofObj = complex = aircraft = null;
            tr = null;
        }
    }
}
public class PlayerManager : MonoBehaviour
{
    //Player References
    public static SeatInterface seatInterface = SeatInterface.Empty;
    public static AircraftReferences player = new AircraftReferences();
    private static int squad = 0;
    private static int wing = 0;
    public delegate void SeatChange();
    public static SeatChange OnSeatChangeEvent;

    private void Awake()
    {
        OnSeatChangeEvent = null;
        seatInterface = SeatInterface.Empty;
    }
    private void Update()
    {
        if (!GameManager.gm.playableScene) return;

        if (player.crew == null)
        {
            SetPlayer(GameManager.squadrons[PlayerPrefs.GetInt("PlayerSquadron", 0)][0]);
        }
    }
    public static void NextSquadron(int offset)
    {
        SetPlayer(OffsetSquadron(offset, squad));
        if (PlayerCamera.viewMode < 0) PlayerCamera.SetView(0);
    }
    public static void NextWing(int offset)
    {
        SetPlayer(OffsetWing(offset, wing, squad));
        if (PlayerCamera.viewMode < 0) PlayerCamera.SetView(0);
    }
    public static SofAircraft OffsetSquadron(int offset, int squad)
    {
        SofAircraft[] squadron = GameManager.squadrons[(int)Mathf.Repeat(squad + offset, GameManager.squadrons.Count)];
        foreach(SofAircraft a in squadron)
        {
            if (a.crew[0] && a.crew[0].sofObject == a) return a;
        }
        if (Mathf.Abs(offset) < GameManager.squadrons.Count)
            return OffsetSquadron(offset + (int)Mathf.Sign(offset), squad);
        return null;
    }
    public static SofAircraft OffsetWing(int offset, int wing, int squad)
    {
        SofAircraft[] squadron = GameManager.squadrons[squad];
        SofAircraft aircraft = squadron[(int)Mathf.Repeat(wing + offset, squadron.Length)];
        if (aircraft.crew[0] && aircraft.crew[0].sofObject == aircraft)
            return aircraft;
        if (Mathf.Abs(offset) < squadron.Length)
            return OffsetWing(offset + (int)Mathf.Sign(offset),wing,squad);
        return null;
    }
    public static void PlayerNull(){ }
    public static void SetPlayer(SofObject obj){ SetPlayer(obj.crew[0]); }
    public static void SetPlayer(CrewMember tCrew){

        if (tCrew == null) return;
        player.Set(tCrew);
        if (player.aircraft)
        {
            squad = player.aircraft.squadronId;
            wing = player.aircraft.placeInSquad;
            player.aircraft.PointGuns();
        }
        SetSeat(0); }
    public static void SetSeat(SeatPath path){ SetPlayer(path.Crew(player.aircraft));  SetSeat(path.seat); }
    public static void SetSeat(int seat)
    {
        player.crew.SwitchSeat(seat);
        seatInterface = player.crew.Interface();

        if (OnSeatChangeEvent != null) OnSeatChangeEvent();
    }
}
