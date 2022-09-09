using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Audio;
using System.Collections.Generic;

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

public class GameManager : MonoBehaviour
{
    public bool playableScene = true;
    public bool vr = false;
    public UIStyle style;
    public GameObject playerCamera;
    public SofAudioListener listener;
    public GameObject vrRig;
    public AircraftsList list;
    public Transform mapTr;
    public MapData mapData;
    public Airfield[] airfields;
    public Rigidbody mapRb;
    public AudioMixer mixer;
    public Actions actions;

    public static float timeScale = 1f;
    public static bool paused;
    public static GameUI gameUI = GameUI.Game;

    public static bool trackingControl;
    public static bool fullElevator;

    public static bool war;
    public static GameManager gm;
    public static UIManager ui;
    public static Weather weather;
    public static MapTool map;
    public static List<SofAircraft[]> squadrons = new List<SofAircraft[]>(0);
    public static List<SofObject> sofObjects = new List<SofObject>(0);
    public static List<SofAircraft> axisAircrafts = new List<SofAircraft>(0);
    public static List<SofAircraft> allyAircrafts = new List<SofAircraft>(0);

    //Player References
    public static SeatInterface seatInterface = SeatInterface.Empty;
    public static AircraftReferences ogPlayer = new AircraftReferences();
    public static AircraftReferences player = new AircraftReferences();
    public static GunnerSeat playerGunner;
    private static int ogSquad = 0;
    private static int ogWing = 0;

    public static AircraftReferences AvailablePlayer()
    {
        return player.sofObj ? player : ogPlayer;
    }

    public void Awake()
    {
        Application.targetFrameRate = PlayerPrefs.GetInt("TargetFrameRate", Application.targetFrameRate);
        QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel()));
        fullElevator = PlayerPrefs.GetInt("FullElevatorControl", 0) == 1;
        trackingControl = true;
#if MOBILE_INPUT
        trackingControl = false;
#endif
        timeScale = 1f;
        UnitsConverter.Initialize();

        if (!playableScene)
        {
            PlayerPrefs.SetFloat("CockpitVolume", -80f);
            PlayerPrefs.SetFloat("ExternalVolume", -80f);
        }

        gm = GetComponent<GameManager>();
        weather = GetComponent<Weather>();
        map = GetComponent<MapTool>();
        list.UpdateCards();

        if (actions == null)
        {
            actions = new Actions();
        }
        squadrons = new List<SofAircraft[]>(0);
        sofObjects = new List<SofObject>(0);
        axisAircrafts = new List<SofAircraft>(0);
        allyAircrafts = new List<SofAircraft>(0);

        actions.Enable();
        UpdateInterface(SeatInterface.Empty);

        if (playableScene)
        {
            if (vr) Instantiate(vrRig);
            else
            {
                Instantiate(playerCamera);
                ui = style.SpawnUI().GetComponent<UIManager>();
            }
            CreateMission();
        }
        SetPause(false, false);
    }
    private void OnDisable()
    {
        actions.Disable();
    }
    private void Start()
    {
        if (playableScene)
        {
            listener = Instantiate(listener);

            actions.General.SwitchPilot.performed += _ => SetPlayer(ogPlayer.sofObj.crew[0], true);
            actions.General.SwitchGunner1.performed += _ => SetPlayer(ogPlayer.sofObj.crew[1], true);
            actions.General.SwitchGunner2.performed += _ => SetPlayer(ogPlayer.sofObj.crew[2], true);
            actions.General.SwitchGunner3.performed += _ => SetPlayer(ogPlayer.sofObj.crew[3], true);
            actions.General.SwitchGunner4.performed += _ => SetPlayer(ogPlayer.sofObj.crew[4],true);
            actions.General.BombardierView.performed +=_ => SetPlayer(ogPlayer.sofObj, ogPlayer.aircraft.bombardierPath, true);
            actions.General.NextSquadron.performed += _ => NextSquadron(1);
            actions.General.PreviousSquadron.performed += _ => NextSquadron(-1);
            actions.General.NextWing.performed += _ => NextWing(1);
            actions.General.PreviousWing.performed += _ => NextWing(-1);
            actions.General.Screenshot.performed += _ => ScreenShot();
            actions.General.Pause.performed += _ => SetPause(!paused, gameUI);
            actions.General.Cancel.performed += _ => SetPause(gameUI != GameUI.PauseMenu, gameUI != GameUI.PauseMenu);
            actions.General.CamerasEditor.performed += _ => SetPause(paused, GameUI.CamerasEditor);
            actions.General.FreeView.performed += _ => SetPause(true, GameUI.PhotoMode);
            actions.General.TimeScale.performed += t => SetSlowMo(Mathf.InverseLerp(1f,-1f,t.ReadValue<float>()));
        }
    }
    private void NextSquadron(int offset)
    {
        SetPlayer(OffsetSquadron(offset, ogSquad), true);
        if (PlayerCamera.viewMode < 0) PlayerCamera.instance.SetView(0);
    }
    private void NextWing(int offset)
    {
        SetPlayer(OffsetWing(offset, ogWing, ogSquad), true);
        if (PlayerCamera.viewMode < 0) PlayerCamera.instance.SetView(0);
    }

    private float TimeScaleFactor()
    {
        return Mathf.Log(timeScale, 0.5f) / 5f;
    }
    private void SetSlowMo(float factor)
    {
        float oldTimeScale = timeScale;
        factor = Mathf.Max(0f, factor);
        timeScale = Mathf.Pow(0.5f, factor * 5f);
        if (oldTimeScale == timeScale) return;

        string txt = timeScale < 0.1f ? "1/" + (1f / timeScale).ToString("0") : timeScale.ToString("0.00");
        Log.Print("Slow Mo : " + txt, "TimeScale");
    }
    private void UpdateInterface(SeatInterface si)
    {
        seatInterface = si;
        gm.actions.General.Enable();
        gm.actions.Seat.Enable();
        gm.actions.Pilot.Disable();
        gm.actions.Gunner.Disable();
        gm.actions.Bombardier.Disable();

        switch (seatInterface)
        {
            case SeatInterface.Pilot: gm.actions.Pilot.Enable(); break;
            case SeatInterface.Gunner: gm.actions.Gunner.Enable(); break;
            case SeatInterface.Bombardier: gm.actions.Bombardier.Enable(); break;
        }
    }
    private void Update()
    {
        if (!playableScene) return;

        if (player.crew) player.Set(player.crew);
        ogPlayer.Set(ogPlayer.crew);

        float input = -actions.General.TimeScaleRelative.ReadValue<float>();
        input *= Time.unscaledDeltaTime * 0.25f;
        SetSlowMo(TimeScaleFactor() + input);

        SeatInterface currentInterface = player.crew == null ? SeatInterface.Empty : player.crew.Interface();
        if (currentInterface != seatInterface) UpdateInterface(currentInterface);

        Time.timeScale = paused ? 0f : timeScale;
        Time.fixedDeltaTime = 1f / 60f * Time.timeScale;
    }
    private void FixedUpdate()
    {
        //Physics.SyncTransforms();
    }
    private void CreateMission()
    {
        war = PlayerPrefs.GetInt("War", 0) == 1;

        int squads = PlayerPrefs.GetInt("SquadronsAmount");
        for (int i = 0; i < squads; i++)
        {
            //Get squadron data
            Game.Team team = (Game.Team)PlayerPrefs.GetInt("Squadron" + i + "Team");
            int aircraftId = PlayerPrefs.GetInt("Squadron" + i + "Aircraft");
            int amount = PlayerPrefs.GetInt("Squadron" + i + "Amount");
            bool playerSquadron = PlayerPrefs.GetInt("Squadron" + i + "Player") == 1;
            int airfield = PlayerPrefs.GetInt("Squadron" + i + "Airfield");
            Vector3 position;
            position.x = PlayerPrefs.GetFloat("Squadron" + i + "PosX");
            position.y = PlayerPrefs.GetFloat("Squadron" + i + "PosY");
            position.z = PlayerPrefs.GetFloat("Squadron" + i + "PosZ");
            float heading = PlayerPrefs.GetFloat("Squadron" + i + "Heading");
            float difficulty = PlayerPrefs.GetFloat("Squadron" + i + "Difficulty");

            //Feed
            Game.Squadron squad = new Game.Squadron(list.list[aircraftId], team, amount,difficulty, playerSquadron);
            if (airfield == -1)
            {
                squad.startPosition = position;
                squad.startHeading = heading;
                squadrons.Add(SpawnAir(squad));
            }
            else squadrons.Add(airfields[airfield].Spawn(squad));

            if (playerSquadron) SetPlayer(squadrons[i][0],true);
        }
        PlayerPrefs.SetInt("FirstGame", 1);
    }
    public SofAircraft[] SpawnAir(Game.Squadron squad)
    {
        SofAircraft[] squadronAircrafts = new SofAircraft[squad.amount];
        for (int i = 0; i < squad.amount; i++)
        {
            GameObject plane = Instantiate(squad.aircraftCard.aircraft, squad.startPosition, Quaternion.Euler(0f,-squad.startHeading,0f));
            plane.transform.Translate(squad.aircraftCard.formation.aircraftPositions[i], Space.Self);

            SofAircraft aircraft = squadronAircrafts[i] = plane.GetComponent<SofAircraft>();
            aircraft.data.rb.velocity = plane.transform.forward * squad.aircraftCard.startingSpeed / 3.6f;
            aircraft.SpawnInitialization(Spawner.Type.InAir, squad.team, squadrons.Count, i, squad.difficulty);

            if (squad.includePlayer && i == 0)
                SetPlayer(aircraft,true);
        }
        return squadronAircrafts;
    }

    public static SofAircraft OffsetSquadron(int offset, int squad)
    {
        SofAircraft[] squadron = squadrons[(int)Mathf.Repeat(squad + offset, squadrons.Count)];
        foreach(SofAircraft a in squadron)
        {
            if (a.crew[0] && a.crew[0].sofObject == a) return a;
        }
        if (Mathf.Abs(offset) < squadrons.Count)
            return OffsetSquadron(offset + (int)Mathf.Sign(offset), squad);
        return null;
    }
    public static SofAircraft OffsetWing(int offset, int wing, int squad)
    {
        SofAircraft[] squadron = squadrons[squad];
        SofAircraft aircraft = squadron[(int)Mathf.Repeat(wing + offset, squadron.Length)];
        if (aircraft.crew[0] && aircraft.crew[0].sofObject == aircraft)
            return aircraft;
        if (Mathf.Abs(offset) < squadron.Length)
            return OffsetWing(offset + (int)Mathf.Sign(offset),wing,squad);
        return null;
    }

    public static void PlayerNull()
    {
        player.Set(null);
        PlayerCamera.instance.ResetView(true);
    }
    public static void SetPlayer(SofObject obj, bool original){ SetPlayer(obj.crew[0],original); }
    public static void SetPlayer(CrewMember tCrew, bool original){ SetPlayer(tCrew, 0,original); }
    public static void SetPlayer(SofObject obj, SeatPath path, bool original){ SetPlayer(path.Crew(obj), path.seat, original); }
    public static void SetPlayer(CrewMember tCrew, int seat,bool original)
    {
        if (tCrew == player.crew || tCrew == null) return;

        player.Set(tCrew);
        if (original)
        {
            ogPlayer.Set(tCrew);
            ogSquad = ogPlayer.aircraft.squadronId;
            ogWing = ogPlayer.aircraft.placeInSquad;
            playerGunner = tCrew.seats[0].GetComponent<GunnerSeat>();
        }
        if (tCrew.aircraft) tCrew.aircraft.PointGuns();
        tCrew.SwitchSeat(seat);

        if (gm.vr)
            SofVrRig.instance.ResetView();
        else
            PlayerCamera.instance.ResetView(true);

    }
    public static void SetPause(bool _paused, GameUI _ui)
    {
        if (!gm.playableScene) return;
        paused = _paused;
        gameUI = _ui;
        AudioListener.volume = paused ? 0.2f : 1f;

        if (ui) ui.ResetInterface();
    }
    public static void SetPause(bool _paused, bool _pauseMenu)
    {
        SetPause(_paused, _pauseMenu ? GameUI.PauseMenu : GameUI.Game);
    }
    void ScreenShot()
    {
        int screenshots = PlayerPrefs.GetInt("Screenshots", 0);
        ScreenCapture.CaptureScreenshot("Capture" + screenshots + ".png", 2);
        screenshots++;
        PlayerPrefs.SetInt("Screenshots", screenshots);
    }
}
