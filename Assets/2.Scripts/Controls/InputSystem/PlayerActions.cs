using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActions : MonoBehaviour
{
    public static Actions actions;

    public static Actions.GeneralActions general;
    public static Actions.PilotActions pilot;
    public static Actions.GunnerActions gunner;
    public static Actions.BombardierActions bomber;

    public static Actions.MenuActions menu;
    public static Actions.CameraActions cam;
    public static Actions.SwitcherActions switcher;

    public const float throttleBoostThrehold = 0.9f;

    private static bool PlayerAvailable()
    {
        return Player.aircraft && !Player.crew.ActionsUnavailable;
    }
    private void OnEnable()
    {
        actions = new Actions();
        actions.Enable();
        general = actions.General;
        pilot = actions.Pilot;
        gunner = actions.Gunner;
        bomber = actions.Bombardier;
        menu = actions.Menu;
        cam = actions.Camera;
        switcher = actions.Switcher;


    }
    private void OnDisable()
    {
        actions.Disable();
    }
    public static bool dynamic = true;
    private void Start()
    {
        Actions.PilotActions pilot = actions.Pilot;
        Actions.GunnerActions gunner = actions.Gunner;
        Actions.BombardierActions bombardier = actions.Bombardier;
        Actions.GeneralActions general = actions.General;
        Actions.MenuActions menu = actions.Menu;
        Actions.CameraActions camera = actions.Camera;
        Actions.SwitcherActions switcher = actions.Switcher;


        pilot.LandingGear.performed += _ => Action("LandingGear");
        pilot.BombBay.performed += _ => Action("BombBay"); ;
        pilot.Airbrakes.performed += _ => Action("AirBrakes");
        pilot.Canopy.performed += _ => Action("Canopy");
        pilot.EngineToggle.performed += _ => Action("Engines");
        pilot.Bomb.performed += _ => Action("Bomb");
        pilot.Rocket.performed += _ => Action("Rocket");
        pilot.Throttle.performed += val => SetThrottle(val.ReadValue<float>());

        bombardier.BombBay.performed += _ => Action("BombBay");
        bombardier.Bomb.performed += _ => Action("Bomb");
        bombardier.Mode.performed += _ => Action("BombsightMode");
        bombardier.Interval.performed += _ => Action("BombsightInterval");
        bombardier.Quantity.performed += _ => Action("BombsightQuantity");

        general.Reload.performed += _ => Action("Reload");
        general.BailOut.performed += _ => Action("StartBailout");
        general.BailOut.canceled += _ => Action("CancelBailout");
        general.SwitchSeat.performed += _ => Player.CycleSeats();

        menu.Cancel.performed += _ => Escape();
        menu.Screenshot.performed += _ => GameManager.gm.ScreenShot();
        menu.Pause.performed += _ => TimeManager.SetPause(!TimeManager.paused);
        menu.CamEditor.performed += _ => UIManager.SwitchGameUI(UIManager.gameUI == GameUI.CamEditor ? GameUI.Game : GameUI.CamEditor);
        menu.Photo.performed += _ => PhotoMode();

        camera.Custom1.performed += _ => SofCamera.SwitchViewMode(-1);
        camera.Custom2.performed += _ => SofCamera.SwitchViewMode(-2);
        camera.Custom3.performed += _ => SofCamera.SwitchViewMode(-3);
        camera.Custom4.performed += _ => SofCamera.SwitchViewMode(-4);
        camera.Custom5.performed += _ => SofCamera.SwitchViewMode(-5);
        camera.Custom6.performed += _ => SofCamera.SwitchViewMode(-6);
        camera.FreeView.performed += val => SofCamera.StartLookAround();
        camera.FreeView.canceled += val => SofCamera.StopLookAround();
        camera.Reset.performed += _ => SofCamera.ResetRotation();
        camera.Dynamic.performed += _ => dynamic = !dynamic;
        camera.ToggleViewMode.performed += _ => SofCamera.SwitchViewMode(SofCamera.viewMode == 0 ? 1 : 0);

        switcher.Pilot.performed += _ => Player.SetCrew(0);
        switcher.Crew1.performed += _ => Player.SetCrew(1);
        switcher.Crew2.performed += _ => Player.SetCrew(2);
        switcher.Crew3.performed += _ => Player.SetCrew(3);
        switcher.Crew4.performed += _ => Player.SetCrew(4);
        switcher.Crew5.performed += _ => Player.SetCrew(5);
        switcher.Crew6.performed += _ => Player.SetCrew(6);
        switcher.NextCrew.performed += _ => Player.SetCrew(Player.crewId + 1);
        switcher.PreviousCrew.performed += _ => Player.SetCrew(Player.crewId - 1);
        switcher.ToBombardier.performed += _ => Player.SetSeat(Player.aircraft.bombardierSeat);
        switcher.NextSquadron.performed += _ => Player.NextSquadron(1);
        switcher.PreviousSquadron.performed += _ => Player.NextSquadron(-1);
        switcher.NextWing.performed += _ => Player.NextWing(1);
        switcher.PreviousWing.performed += _ => Player.NextWing(-1);

        invertPitch = PlayerPrefs.GetInt("InvertPitch", 0) == 1;
    }
    private void Escape()
    {
        bool toPauseUI = UIManager.gameUI != GameUI.Pause && !(UIManager.gameUI == GameUI.Game && TimeManager.paused);
        UIManager.SwitchGameUI(toPauseUI ? GameUI.Pause : GameUI.Game);
        TimeManager.SetPause(toPauseUI);
        if (!toPauseUI && SofCamera.viewMode != 1 && SofCamera.viewMode != 0) SofCamera.SwitchViewMode(0);
    }
    private void PhotoMode()
    {
        TimeManager.SetPause(true);
        UIManager.SwitchGameUI(GameUI.PhotoMode);
        SofCamera.SwitchViewMode(2);
    }
    private void Update()
    {
        UpdateActions();
    }
    //Does not work with events (when 2 events are called at once it creates an error for some reason). Forced to use update
    private void UpdateActions()
    {
        bool gameActions = !TimeManager.paused && UIManager.gameUI == GameUI.Game && Player.controllingPlayer;
        SeatRole si = Player.role;

        if (gameActions != actions.General.enabled)
            if (gameActions) actions.General.Enable(); else actions.General.Disable();

        bool pilotActions = gameActions && si == SeatRole.Pilot;
        if (pilotActions != actions.Pilot.enabled)
            if (pilotActions) actions.Pilot.Enable(); else actions.Pilot.Disable();

        bool gunnerActions = gameActions && si == SeatRole.Gunner;
        if (gunnerActions != actions.Gunner.enabled)
            if (gunnerActions) actions.Gunner.Enable(); else actions.Gunner.Disable();

        bool bomberActions = gameActions && si == SeatRole.Bombardier;
        if (bomberActions != actions.Bombardier.enabled)
            if (bomberActions) actions.Bombardier.Enable(); else actions.Bombardier.Disable();

        bool inTheGame = GameManager.gm.playableScene;

        if(inTheGame != actions.Switcher.enabled)
            if (pilotActions) actions.Switcher.Enable(); else actions.Switcher.Disable();

        if (inTheGame != actions.Camera.enabled)
            if (pilotActions) actions.Camera.Enable(); else actions.Camera.Disable();

        if (inTheGame != actions.Menu.enabled)
            if (pilotActions) actions.Menu.Enable(); else actions.Menu.Disable();
    }
    public static void Action(string action)
    {
        if (!PlayerAvailable()) return;
        switch (action)
        {
            case "Reload":
                Player.crew.Seat.TryReload(false);
                break;
            case "Bomb":
                if (Player.role == SeatRole.Bombardier) Player.aircraft.bombSight.StartReleaseSequence();
                else Player.aircraft.armament.DropBomb();
                break;
            case "Rocket":
                Player.aircraft.armament.FireRocket();
                break;
            case "LandingGear":
                Player.aircraft.hydraulics.SetGear();
                break;
            case "BombBay":
                Player.aircraft.hydraulics.SetBombBay();
                break;
            case "AirBrakes":
                Player.aircraft.hydraulics.SetAirBrakes();
                break;
            case "Canopy":
                Player.aircraft.hydraulics.SetCanopy();
                break;
            case "Engines":
                Player.aircraft.engines.ToggleSetEngines();
                break;
            case "BombsightMode":
                Player.aircraft.bombSight.ToggleMode();
                break;
            case "BombsightInterval":
                Player.aircraft.bombSight.ToggleInterval();
                break;
            case "BombsightQuantity":
                Player.aircraft.bombSight.ToggleAmount();
                break;
            case "StartBailout":
                for (int i = 0; i < Player.aircraft.crew.Length; i++)
                    Player.aircraft.crew[i].bailOut?.StartBailing(Random.Range(0.1f, 1f));
                break;
            case "CancelBailout":
                for (int i = 0; i < Player.aircraft.crew.Length; i++)
                    Player.aircraft.crew[i].bailOut?.CancelBailing();
                break;
        }
    }
    private void SetThrottle(float throttleInput)
    {
        if (!PlayerAvailable()) return;
        float throttle = Mathf.Clamp01(throttleInput / throttleBoostThrehold);
        if (throttleInput > 0.5f * (throttleBoostThrehold + 1f))
        {
            throttle = 2f;
        }
        Player.aircraft.engines.SetThrottleAllEngines(throttle, true);
    }

    public static bool forceCameraPointDirection = false;

    static float maintainBank = 0f;
    static bool invertPitch;
    static float forceCameraPointDirectionTimer = 0f;

    const float throttleIncrementFactor = 0.0002f;

    public static void PilotUpdateAxes(CrewMember crew)
    {
        SofAircraft aircraft = crew.aircraft;

        Actions.PilotActions pilot = PlayerActions.pilot;
        if (pilot.FirePrimaries.ReadValue<float>() > 0.1f) aircraft.armament.FirePrimaries();
        if (pilot.FireSecondaries.ReadValue<float>() > 0.7f) aircraft.armament.FireSecondaries();
        aircraft.hydraulics.SetFlaps(Mathf.RoundToInt(pilot.Flaps.ReadValue<float>()));
        aircraft.controls.brake = pilot.Brake.ReadValue<float>();

        float scrollValue = PlayerActions.general.Scroll.ReadValue<float>();
        if (scrollValue != 0f)
        {
            float currentThrottle = aircraft.engines.Throttle;
            float throttleIncrement = scrollValue * throttleIncrementFactor;

            bool maxedThrottleAndPositiveIncrement = currentThrottle >= 1f && throttleIncrement > 0f;
            bool boostedAndNegativeIncrement = aircraft.engines.Throttle.Boost && throttleIncrement < 0f;

            if (maxedThrottleAndPositiveIncrement)
                aircraft.engines.SetThrottleAllEngines(1.1f, true);

            else if (boostedAndNegativeIncrement)

                aircraft.engines.SetThrottleAllEngines(1f, false);

            else
                aircraft.engines.SetThrottleAllEngines(currentThrottle + throttleIncrement, false);
        }
    }
    public static void PilotFixedUpdateAxes(CrewMember crew)
    {
        SofAircraft aircraft = crew.aircraft;
        Actions.PilotActions actions = PlayerActions.pilot;
        AircraftAxes axes = AircraftAxes.zero;

        float pitch = -actions.Pitch.ReadValue<float>();
        if (invertPitch) pitch = -pitch;
        float roll = actions.Roll.ReadValue<float>();
        float yaw = -actions.Rudder.ReadValue<float>();

        if (ControlsManager.CurrentMode() == ControlsMode.Tracking) //Tracking input, mouse
        {
            bool pitching = actions.Pitch.phase == InputActionPhase.Started;
            bool rolling = actions.Roll.phase == InputActionPhase.Started;
            bool yawing = actions.Rudder.phase == InputActionPhase.Performed;

            AircraftAxes forcedAxes = new(float.NaN, float.NaN, float.NaN);

            if (pitching)
            {
                forceCameraPointDirection = true;
                forceCameraPointDirectionTimer = 1f;
                forcedAxes.pitch = pitch;
            }
            else
            {
                forceCameraPointDirectionTimer -= Time.deltaTime;
                if (forceCameraPointDirectionTimer < 0f) forceCameraPointDirection = false;
            }
            if (rolling || pitching)
            {
                forcedAxes.roll = Mathf.MoveTowards(aircraft.controls.current.roll, roll, Time.fixedDeltaTime * 3f);
                maintainBank = 1f;
            }
            else
            {
                maintainBank = Mathf.MoveTowards(maintainBank, 0f, 0.5f * Time.fixedDeltaTime);
            }
            if (yawing || pitching) forcedAxes.yaw = yaw;


            //PointTracking.Tracking(aircraft.tr.position + SofCamera.directionInput * 500f, aircraft, 0f, 0f, true);
            axes = NewPointTracking.FindOptimalControls(SofCamera.directionInput, aircraft, forcedAxes, SofCamera.lookAround ? 1f : maintainBank);
            aircraft.controls.SetTargetInput(axes, PitchCorrectionMode.Raw);
        }
        else //Direct input, joystick, phone
        {
            axes = new AircraftAxes(pitch, roll, yaw);
            aircraft.controls.SetTargetInput(axes, ControlsManager.pitchCorrectionMode);
        }
    }
}
