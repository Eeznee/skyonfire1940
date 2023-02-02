using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActions : MonoBehaviour
{
    public static PlayerActions instance;
    public Actions actions;
    public MenuActions menuActions;

    public static Actions.GeneralActions General()
    {
        return instance.actions.General;
    }
    public static Actions.PilotActions Pilot()
    {
        return instance.actions.Pilot;
    }
    public static Actions.GunnerActions Gunner()
    {
        return instance.actions.Gunner;
    }
    public static Actions.BombardierActions Bombardier()
    {
        return instance.actions.Bombardier;
    }

    private void Awake()
    {
        instance = this;
        actions = new Actions();
        actions.Enable();
        menuActions = new MenuActions();
        menuActions.Enable();

    }
    private void OnDisable()
    {
        actions.Disable();
        menuActions.Disable();
    }
    private SofObject Player()
    {
        return PlayerManager.player.sofObj;
    }
    private void Start()
    {
        Actions.PilotActions pilot = actions.Pilot;
        Actions.GunnerActions gunner = actions.Gunner;
        Actions.BombardierActions bombardier = actions.Bombardier;
        Actions.SeatActions seat = actions.Seat;
        Actions.GeneralActions general = actions.General;
        pilot.LandingGear.performed += _ => Action("LandingGear");
        pilot.BombBay.performed += _ => Action("BombBay"); ;
        pilot.Airbrakes.performed += _ => Action("AirBrakes");
        pilot.Canopy.performed += _ => Action("Canopy");
        pilot.EngineToggle.performed += _ => Action("Engines");
        pilot.Bomb.performed += _ => Action("Bomb");
        pilot.Rocket.performed += _ => Action("Rocket");
        pilot.Throttle.performed += val => SetThrottle(val.ReadValue<float>());
        pilot.Dynamic.performed += _ => PlayerCamera.ToggleDynamic();

        bombardier.BombBay.performed += _ => Action("BombBay");
        bombardier.Bomb.performed += _ => Action("Bomb");
        bombardier.Mode.performed += _ => Action("BombsightMode");
        bombardier.Interval.performed += _ => Action("BombsightInterval");
        bombardier.Quantity.performed += _ => Action("BombsightQuantity");

        seat.Reload.performed += _ => Action("Reload");
        seat.BailOut.performed += _ => Action("StartBailout");
        seat.BailOut.canceled += _ => Action("CancelBailout");

        general.SwitchSeat.performed += _ => PlayerManager.SetSeat((PlayerManager.player.crew.currentSeat + 1) % PlayerManager.player.crew.seats.Length);
        general.SwitchPilot.performed += _ => PlayerManager.SetPlayer(Player().crew[0]);
        general.SwitchGunner1.performed += _ => PlayerManager.SetPlayer(Player().crew[1]);
        general.SwitchGunner2.performed += _ => PlayerManager.SetPlayer(Player().crew[2]);
        general.SwitchGunner3.performed += _ => PlayerManager.SetPlayer(Player().crew[3]);
        general.SwitchGunner4.performed += _ => PlayerManager.SetPlayer(Player().crew[4]);
        general.BombardierView.performed += _ => PlayerManager.SetSeat(PlayerManager.player.aircraft.bombardierPath);
        general.NextSquadron.performed += _ => PlayerManager.NextSquadron(1);
        general.PreviousSquadron.performed += _ => PlayerManager.NextSquadron(-1);
        general.NextWing.performed += _ => PlayerManager.NextWing(1);
        general.PreviousWing.performed += _ => PlayerManager.NextWing(-1);
        general.Screenshot.performed += _ => GameManager.ScreenShot();
        general.Pause.performed += _ => TimeManager.SetPause(!TimeManager.paused, GameManager.gameUI);
        general.CamerasEditor.performed += _ => TimeManager.SetPause(TimeManager.paused, GameUI.CamerasEditor);
        general.FreeView.performed += _ => PhotoMode();
        general.TimeScale.performed += t => TimeManager.SetSlowMo(Mathf.InverseLerp(1f, -1f, t.ReadValue<float>()));
        menuActions.General.Cancel.performed += _ => Escape();

        general.CustomCam1.performed += _ => PlayerCamera.SetView(-1);
        general.CustomCam2.performed += _ => PlayerCamera.SetView(-2);
        general.CustomCam3.performed += _ => PlayerCamera.SetView(-3);
        general.CustomCam4.performed += _ => PlayerCamera.SetView(-4);
        general.CustomCam5.performed += _ => PlayerCamera.SetView(-5);
        general.CustomCam6.performed += _ => PlayerCamera.SetView(-6);
        general.LookAround.performed += val => PlayerCamera.ToggleLookAround(true);
        general.LookAround.canceled += val => PlayerCamera.ToggleLookAround(false);
        general.ResetCamera.performed += _ => PlayerCamera.ResetView(false);
        general.ToggleViewMode.performed += _ => PlayerCamera.SetView(PlayerCamera.viewMode == 0 ? 1 : 0);

        PlayerManager.OnSeatChangeEvent += UpdateActions;
        TimeManager.OnPauseEvent += UpdateActions;
    }
    private void Escape()
    {
        bool pause = GameManager.gameUI != GameUI.PauseMenu;
        TimeManager.SetPause(pause, pause ? GameUI.PauseMenu : GameUI.Game);
        if (!pause) PlayerCamera.SetView(PlayerCamera.viewMode == 1 ? 1 : 0);
    }
    private void PhotoMode()
    {
        TimeManager.SetPause(true, GameUI.PhotoMode);
        PlayerCamera.SetView(2);
    }
    private void UpdateActions()
    {
        actions.General.Enable();
        bool paused = TimeManager.paused;
        SeatInterface si = PlayerManager.seatInterface;
        if (!paused && GameManager.gameUI == GameUI.Game) actions.Seat.Enable();else actions.Seat.Disable();
        if (!paused && si == SeatInterface.Pilot) actions.Pilot.Enable(); else actions.Pilot.Disable();
        if (!paused && si == SeatInterface.Gunner) actions.Gunner.Enable(); else actions.Gunner.Disable();
        if (!paused && si == SeatInterface.Bombardier) actions.Bombardier.Enable(); else actions.Bombardier.Disable();
    }
    public static void Action(string action)
    {
        if (PlayerManager.player.aircraft == null || PlayerManager.player.crew.ripped || PlayerManager.player.crew.body.Gloc()) return;
        switch (action)
        {
            case "Reload":
                PlayerManager.player.crew.Seat().TryReload();
                break;
            case "Bomb":
                PlayerManager.player.aircraft.DropBomb();
                break;
            case "Rocket":
                PlayerManager.player.aircraft.FireRocket();
                break;
            case "LandingGear":
                PlayerManager.player.aircraft.SetGear();
                break;
            case "BombBay":
                PlayerManager.player.aircraft.SetBombBay();
                break;
            case "AirBrakes":
                PlayerManager.player.aircraft.SetAirBrakes();
                break;
            case "Canopy":
                PlayerManager.player.aircraft.SetCannopy();
                break;
            case "Engines":
                PlayerManager.player.aircraft.SetEngines();
                break;
            case "BombsightMode":
                PlayerManager.player.aircraft.bombSight.ToggleMode();
                break;
            case "BombsightInterval":
                PlayerManager.player.aircraft.bombSight.ToggleInterval();
                break;
            case "BombsightQuantity":
                PlayerManager.player.aircraft.bombSight.ToggleAmount();
                break;
            case "StartBailout":
                for (int i = 0; i < PlayerManager.player.aircraft.crew.Length; i++)
                    PlayerManager.player.aircraft.crew[i].StartBailout(Random.Range(0.1f, 1f));
                break;
            case "CancelBailout":
                for (int i = 0; i < PlayerManager.player.aircraft.crew.Length; i++)
                    PlayerManager.player.aircraft.crew[i].CancelBailout();
                break;
        }
    }
    private void SetThrottle(float thr)
    {
        if (PlayerManager.player.aircraft == null || PlayerManager.player.crew.body.Gloc()) return;
        PlayerManager.player.aircraft.SetThrottle(thr);
    }
}
