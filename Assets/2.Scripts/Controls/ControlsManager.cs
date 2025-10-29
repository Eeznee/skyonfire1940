using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
public partial class ControlsManager : MonoBehaviour
{
    public static Actions actions;
    public static UIActions uiActions;

    public static Actions.AnySeatActions anySeat;
    public static Actions.PilotActions pilot;
    public static Actions.GunnerActions gunner;
    public static Actions.BomberActions bomber;

    public static Actions.ActionWheelActions actionWheel;

    public static Actions.MenuActions menu;
    public static Actions.CameraActions cam;

    public const float throttleBoostThrehold = 0.9f;

    private static bool PlayerAvailable()
    {
        return Player.aircraft && !Player.crew.ActionsUnavailable;
    }
    private void OnEnable()
    {
        actions = new Actions();
        UpdateBindingsFromJson();
        actions.Enable();

        uiActions = new UIActions();
        uiActions.Enable();

        anySeat = actions.AnySeat;
        pilot = actions.Pilot;
        gunner = actions.Gunner;
        bomber = actions.Bomber;
        actionWheel = actions.ActionWheel;
        menu = actions.Menu;
        cam = actions.Camera;

        SofSettingsSO.OnUpdateSettings += ResetActionsToDisableWithWheel;
        SofSettingsSO.OnUpdateSettings += UpdateBindingsFromJson;
        ResetActionsToDisableWithWheel();
    }
    private void OnDisable()
    {
        SofSettingsSO.OnUpdateSettings -= ResetActionsToDisableWithWheel;
        SofSettingsSO.OnUpdateSettings -= UpdateBindingsFromJson;

        pilot.Disable();
        gunner.Disable();
        bomber.Disable();
        anySeat.Disable();
        actionWheel.Disable();
        menu.Disable();
        cam.Disable();

        actions.Disable();
        actions.Dispose();
        actions = null;
        uiActions.Disable();
    }
    private void UpdateBindingsFromJson()
    {
        string saveFilePath = Path.Combine(Application.persistentDataPath, "sofbindings.json");
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            actions.LoadBindingOverridesFromJson(json);
        }
    }



    public static bool dynamic = true;
    private void Start()
    {
        pilot.LandingGear.performed += _ => Action("LandingGear");
        pilot.BombBay.performed += _ => Action("BombBay"); ;
        pilot.Airbrakes.performed += _ => Action("AirBrakes");
        pilot.Canopy.performed += _ => Action("Canopy");
        pilot.EngineToggle.performed += _ => Action("Engines");
        pilot.Bomb.performed += _ => Action("Bomb");
        pilot.Rocket.performed += _ => Action("Rocket");
        pilot.ThrottleRaw.performed += val => SetThrottle(val.ReadValue<float>());
        pilot.FlapsRaw.performed += val => SetFlaps(val.ReadValue<float>());

        bomber.BombBay.performed += _ => Action("BombBay");
        bomber.Bomb.performed += _ => Action("Bomb");
        bomber.BombSightMode.performed += _ => Action("BombsightMode");
        bomber.Interval.performed += _ => Action("BombsightInterval");
        bomber.Quantity.performed += _ => Action("BombsightQuantity");

        anySeat.Reload.performed += _ => Action("Reload");
        anySeat.BailOut.performed += _ => Action("StartBailout");
        anySeat.BailOut.canceled += _ => Action("CancelBailout");
        anySeat.SwitchSeat.performed += _ => Player.CycleSeats();

        anySeat.Pilot.performed += _ => Player.SetCrew(0);
        anySeat.Crew2.performed += _ => Player.SetCrew(1);
        anySeat.Crew3.performed += _ => Player.SetCrew(2);
        anySeat.Crew4.performed += _ => Player.SetCrew(3);
        anySeat.Crew5.performed += _ => Player.SetCrew(4);
        anySeat.Crew6.performed += _ => Player.SetCrew(5);
        anySeat.Crew7.performed += _ => Player.SetCrew(6);
        anySeat.Crew8.performed += _ => Player.SetCrew(7);
        anySeat.Crew9.performed += _ => Player.SetCrew(8);
        anySeat.Crew10.performed += _ => Player.SetCrew(9);
        anySeat.Crew11.performed += _ => Player.SetCrew(10);
        anySeat.Crew12.performed += _ => Player.SetCrew(11);
        anySeat.CycleCrew.performed += _ => Player.SetCrew((Player.crewId + 1) % Player.aircraft.crew.Length);
        anySeat.NextCrew.performed += _ => Player.SetCrew(Player.crewId + 1);
        anySeat.PreviousCrew.performed += _ => Player.SetCrew(Player.crewId - 1);
        anySeat.ToBombardier.performed += _ => Player.SetSeat(Player.aircraft.bombardierSeat);

        menu.OpenPauseScreen.performed += _ => Escape();
        menu.Screenshot.performed += _ => Screenshot.ScreenShot();
        menu.CamEditor.performed += _ => UIManager.SwitchGameUI(UIManager.gameUI == GameUI.CamEditor ? GameUI.Game : GameUI.CamEditor);
        menu.PhotoMode.performed += _ => PhotoMode();
        menu.NextSquadron.performed += _ => Player.NextSquadron(1);
        menu.PreviousSquadron.performed += _ => Player.NextSquadron(-1);
        menu.NextWing.performed += _ => Player.NextWing(1);
        menu.PreviousWing.performed += _ => Player.NextWing(-1);

        cam.FreeLook.performed += val => SofCamera.StartLookAround();
        cam.FreeLook.canceled += val => SofCamera.StopLookAround();
        cam.ResetRotation.performed += _ => SofCamera.ResetRotation();
        cam.ToggleDynamic.performed += _ => dynamic = !dynamic;
        cam.ToggleViewMode.performed += _ => SofCamera.ToggleViewMode();
        cam.LookBehind.performed += _ => SofCamera.LookBehind();
        cam.LookBehind.canceled += _ => SofCamera.ResetRotation();
        cam.LookBehind.canceled += _ => SofCamera.StopLookAround();
        cam.PauseTime.performed += _ => TimeManager.SetPause(!TimeManager.paused);

        cam.CustomCamera1.performed += _ => SofCamera.SwitchViewMode(-1);
        cam.CustomCamera2.performed += _ => SofCamera.SwitchViewMode(-2);
        cam.CustomCamera3.performed += _ => SofCamera.SwitchViewMode(-3);
        cam.CustomCamera4.performed += _ => SofCamera.SwitchViewMode(-4);
        cam.CustomCamera5.performed += _ => SofCamera.SwitchViewMode(-5);
        cam.CustomCamera6.performed += _ => SofCamera.SwitchViewMode(-6);
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
    //Does not work with events (when 2 events are called at once it creates an error for some reason). Forced to use update
    private void Update()
    {
        bool gameActions = !TimeManager.paused && UIManager.gameUI == GameUI.Game && Player.controllingPlayer;
        SeatRole si = Player.role;

        if (gameActions != actions.AnySeat.enabled)
            if (gameActions) actions.AnySeat.Enable(); else actions.AnySeat.Disable();

        if (gameActions != actions.ActionWheel.enabled)
            if (gameActions) actions.ActionWheel.Enable(); else actions.ActionWheel.Disable();

        bool pilotActions = gameActions && si == SeatRole.Pilot;
        if (pilotActions != actions.Pilot.enabled)
            if (pilotActions) actions.Pilot.Enable(); else actions.Pilot.Disable();

        bool gunnerActions = gameActions && si == SeatRole.Gunner;
        if (gunnerActions != actions.Gunner.enabled)
            if (gunnerActions) actions.Gunner.Enable(); else actions.Gunner.Disable();

        bool bomberActions = gameActions && si == SeatRole.Bombardier;
        if (bomberActions != actions.Bomber.enabled)
            if (bomberActions) actions.Bomber.Enable(); else actions.Bomber.Disable();

        bool menuActionsAvailable = GameManager.gm.playableScene && !PauseMenu.settingsActive;

        if (menuActionsAvailable != actions.Camera.enabled)
            if (menuActionsAvailable) actions.Camera.Enable(); else actions.Camera.Disable();

        if (menuActionsAvailable != actions.Menu.enabled)
            if (menuActionsAvailable) actions.Menu.Enable(); else actions.Menu.Disable();
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
            case "TurnOnEngines":
                Player.aircraft.engines.TurnOnOneEngine();
                break;
            case "TurnOffEngines":
                Player.aircraft.engines.SetAllEngines(false, false);
                break;
            case "BombsightMode":
                Player.aircraft.bombSight?.ToggleMode();
                break;
            case "BombsightInterval":
                Player.aircraft.bombSight?.ToggleInterval();
                break;
            case "BombsightQuantity":
                Player.aircraft.bombSight?.ToggleAmount();
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
    private void SetFlaps(float flapsInput)
    {
        if (!PlayerAvailable()) return;

        Player.aircraft.hydraulics.SetFlapsRaw(flapsInput);
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

    private static List<InputAction> actionsToDisableWithWheels;

    public void ResetActionsToDisableWithWheel()
    {
        if (actions == null) return;
        InputActionAsset asset = actions.asset;

        InputAction[]  actionBindingsToMatch = new InputAction[] {actionWheel.ToggleWheel, actionWheel.Navigate, actionWheel.Back, uiActions.Main.Submit,
            actionWheel.Option1, actionWheel.Option2, actionWheel.Option3, actionWheel.Option4, actionWheel.Option5, actionWheel.Option6 };

        actionsToDisableWithWheels = new List<InputAction>();

        foreach (InputActionMap map in asset.actionMaps)
        {
            if (map.name == "ActionWheel") continue;

            foreach (InputAction action in map.actions)
                foreach (InputAction actionBindingToMatch in actionBindingsToMatch)
                    if (map != actionBindingToMatch.actionMap && HasBindingsInCommon(action, actionBindingToMatch))
                        actionsToDisableWithWheels.Add(action);
        }
    }
    public static void SetActiveAllActionsContainingBinding(bool enable)
    {
        foreach(InputAction action in actionsToDisableWithWheels)
        {
            if (enable) action.Enable();
            else action.Disable();
        }
    }
    public bool HasBindingsInCommon(InputAction action1, InputAction action2)
    {
        foreach (InputBinding binding1 in action1.bindings)
        {
            if (string.IsNullOrEmpty(binding1.effectivePath)) continue;

            string path1 = ShortenPath(binding1.path);
            foreach (InputBinding binding2 in action2.bindings)
            {
                if (string.IsNullOrEmpty(binding2.effectivePath)) continue;

                string path2 = ShortenPath(binding2.path);
                if (path1.Equals(path2)) return true;
            }
        }

        return false;
    }
    private static string ShortenPath(string path)
    {
        string[] splitPath = path.Split('/');
        if (splitPath.Length > 2) path = splitPath[0] + "/" + splitPath[1];
        return path;
    }
}
