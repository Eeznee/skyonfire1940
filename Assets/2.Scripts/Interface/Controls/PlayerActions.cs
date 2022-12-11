using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActions : MonoBehaviour
{
    public static PlayerActions instance;
    public Actions actions;
    public MenuActions menuActions;

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
        pilot.LandingGear.performed += ctx => Action("LandingGear");
        pilot.BombBay.performed += ctx => Action("BombBay"); ;
        pilot.Airbrakes.performed += ctx => Action("AirBrakes");
        pilot.Canopy.performed += ctx => Action("Canopy");
        pilot.EngineToggle.performed += ctx => Action("Engines");
        pilot.Bomb.performed += ctx => Action("Bomb");
        pilot.Rocket.performed += ctx => Action("Rocket");
        pilot.Throttle.performed += ctx => SetThrottle(ctx.ReadValue<float>());

        bombardier.BombBay.performed += ctx => Action("BombBay");
        bombardier.Bomb.performed += ctx => Action("Bomb");
        bombardier.Mode.performed += ctx => Action("BombsightMode");
        bombardier.Interval.performed += ctx => Action("BombsightInterval");
        bombardier.Quantity.performed += ctx => Action("BombsightQuantity");

        seat.Reload.performed += ctx => Action("Reload");
        seat.BailOut.performed += ctx => Action("StartBailout");
        seat.BailOut.canceled += ctx => Action("CancelBailout");

        general.SwitchSeat.performed += ctx => PlayerManager.SetPlayer(PlayerManager.player.crew, (PlayerManager.player.crew.currentSeat + 1) % PlayerManager.player.crew.seats.Length, false); ;
        general.SwitchPilot.performed += _ => PlayerManager.SetPlayer(Player().crew[0], true);
        general.SwitchGunner1.performed += _ => PlayerManager.SetPlayer(Player().crew[1], true);
        general.SwitchGunner2.performed += _ => PlayerManager.SetPlayer(Player().crew[2], true);
        general.SwitchGunner3.performed += _ => PlayerManager.SetPlayer(Player().crew[3], true);
        general.SwitchGunner4.performed += _ => PlayerManager.SetPlayer(Player().crew[4], true);
        general.BombardierView.performed += _ => PlayerManager.SetPlayer(Player(), PlayerManager.player.aircraft.bombardierPath, true);
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

        PlayerManager.OnPlayerChangeEvent += UpdateActions;
        TimeManager.OnPauseEvent += UpdateActions;
    }
    private void Escape()
    {
        bool pause = GameManager.gameUI != GameUI.PauseMenu;
        TimeManager.SetPause(pause, pause ? GameUI.PauseMenu : GameUI.Game);
        if (!pause) PlayerCamera.instance.SetView(PlayerCamera.viewMode == 1 ? 1 : 0);
    }
    private void PhotoMode()
    {
        TimeManager.SetPause(true, GameUI.PhotoMode);
        PlayerCamera.instance.SetView(2);
    }
    private void UpdateActions()
    {
        actions.General.Enable();
        bool paused = TimeManager.paused;
        SeatInterface si = GameManager.seatInterface;
        actions.Seat.Disable();
        actions.Gunner.Disable();
        actions.Pilot.Disable();
        actions.Bombardier.Disable();
        if (!paused && GameManager.gameUI == GameUI.Game) actions.Seat.Enable();// else actions.Seat.Disable();
        if (!paused && si == SeatInterface.Pilot) actions.Pilot.Enable();// else actions.Pilot.Disable();
        if (!paused && si == SeatInterface.Gunner) actions.Gunner.Enable();// else actions.Gunner.Disable();
        if (!paused && si == SeatInterface.Bombardier) actions.Bombardier.Enable();// else actions.Bombardier.Disable();
    }
    private void Action(string action)
    {
        if (PlayerManager.player.aircraft == null || PlayerManager.player.crew.ripped || PlayerManager.player.crew.body.Gloc()) return;
        switch (action)
        {
            case "SwitchSeat":
                CrewMember crew = PlayerManager.player.crew;
                PlayerManager.SetPlayer(crew, (crew.currentSeat + 1) % crew.seats.Length, false);
                break;
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
