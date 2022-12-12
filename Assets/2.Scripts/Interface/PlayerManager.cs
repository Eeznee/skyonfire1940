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
    public static GunnerSeat playerGunner;
    private static int squad = 0;
    private static int wing = 0;
    public delegate void PlayerChange();
    public static PlayerChange OnPlayerChangeEvent;

    private void Awake()
    {
        OnPlayerChangeEvent = null;
    }
    private void Update()
    {
        if (!GameManager.gm.playableScene) return;

        if (player.crew == null)
        {
            SetPlayer(GameManager.squadrons[PlayerPrefs.GetInt("PlayerSquadron", 0)][0], true);
        }
    }
    public static void NextSquadron(int offset)
    {
        SetPlayer(OffsetSquadron(offset, squad), true);
        if (PlayerCamera.viewMode < 0) PlayerCamera.instance.SetView(0);
    }
    public static void NextWing(int offset)
    {
        SetPlayer(OffsetWing(offset, wing, squad), true);
        if (PlayerCamera.viewMode < 0) PlayerCamera.instance.SetView(0);
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
    public static void PlayerNull()
    {
        //player.Set(null);
        //PlayerCamera.instance.ResetView(true);
    }
    public static void SetPlayer(SofObject obj, bool original){ SetPlayer(obj.crew[0],original); }
    public static void SetPlayer(CrewMember tCrew, bool original){ SetPlayer(tCrew, 0,original); }
    public static void SetPlayer(SofObject obj, SeatPath path, bool original){ SetPlayer(path.Crew(obj), path.seat, original); }
    public static void SetPlayer(CrewMember tCrew, int seat,bool original)
    {
        if (tCrew == null) return;

        CrewMember oldPlayer = player.crew;
        player.Set(tCrew);
        if (player.aircraft)
        {
            squad = player.aircraft.squadronId;
            wing = player.aircraft.placeInSquad;
        }
        playerGunner = tCrew.seats[0].GetComponent<GunnerSeat>();
        if (tCrew.aircraft) tCrew.aircraft.PointGuns();
        tCrew.SwitchSeat(seat);
        GameManager.seatInterface = player.crew == null ? SeatInterface.Empty : player.crew.Interface();
        if (GameManager.gm.vr)
        {
            SofVrRig.instance.ResetView();
            if (!oldPlayer || oldPlayer.sofObject != player.sofObj)
            {
                if (oldPlayer) SofVrRig.DisableVR(oldPlayer.sofObject);
                SofVrRig.EnableVR(player.sofObj);
            }
        }
        else
            PlayerCamera.instance.ResetView(true);

        if (OnPlayerChangeEvent != null) OnPlayerChangeEvent();
    }
}
