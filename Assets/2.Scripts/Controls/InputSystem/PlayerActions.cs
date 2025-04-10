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
        pilot.Boost.performed += val => SetThrottle(1.1f);
        pilot.Boost.canceled += val => SetThrottle(pilot.Throttle.ReadValue<float>());
        pilot.Dynamic.performed += _ => dynamic = !dynamic;

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
        menu.Screenshot.performed += _ => GameManager.ScreenShot();
        menu.Pause.performed += _ => TimeManager.SetPause(!TimeManager.paused);
        menu.CamEditor.performed += _ => UIManager.SwitchGameUI(UIManager.gameUI == GameUI.CamEditor ? GameUI.Game : GameUI.CamEditor);
        menu.Photo.performed += _ => PhotoMode();

        menu.TimeScale.performed += t => TimeManager.SetSlowMo(Mathf.InverseLerp(1f, -1f, t.ReadValue<float>()));
        camera.Custom1.performed += _ => SofCamera.SwitchViewMode(-1);
        camera.Custom2.performed += _ => SofCamera.SwitchViewMode(-2);
        camera.Custom3.performed += _ => SofCamera.SwitchViewMode(-3);
        camera.Custom4.performed += _ => SofCamera.SwitchViewMode(-4);
        camera.Custom5.performed += _ => SofCamera.SwitchViewMode(-5);
        camera.Custom6.performed += _ => SofCamera.SwitchViewMode(-6);
        camera.FreeView.performed += val => SofCamera.StartLookAround();
        camera.FreeView.canceled += val => SofCamera.StopLookAround();
        camera.Reset.performed += _ => SofCamera.ResetRotation();
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
        bool gameActions = !TimeManager.paused && UIManager.gameUI == GameUI.Game;
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
    private void SetThrottle(float thr)
    {
        if (!PlayerAvailable()) return;
        Player.aircraft.engines.SetThrottleAllEngines(thr, true);
    }
}
