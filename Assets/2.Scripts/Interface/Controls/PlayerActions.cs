using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActions : MonoBehaviour
{
    private void Start()
    {
        Actions.PilotActions pilot = GameManager.gm.actions.Pilot;
        Actions.GunnerActions gunner = GameManager.gm.actions.Gunner;
        Actions.BombardierActions bombardier = GameManager.gm.actions.Bombardier;
        Actions.SeatActions seat = GameManager.gm.actions.Seat;
        pilot.LandingGear.performed += ctx => Action("LandingGear");
        pilot.BombBay.performed += ctx => Action("BombBay"); ;
        pilot.Airbrakes.performed += ctx => Action("AirBrakes");
        pilot.Canopy.performed += ctx => Action("Canopy");
        pilot.EngineToggle.performed += ctx => Action("Engines");
        pilot.Bomb.performed += ctx => Action("Bomb");
        pilot.Throttle.performed += ctx => SetThrottle(ctx.ReadValue<float>());

        bombardier.BombBay.performed += ctx => Action("BombBay");
        bombardier.Bomb.performed += ctx => Action("Bomb");
        bombardier.Mode.performed += ctx => Action("BombsightMode");
        bombardier.Interval.performed += ctx => Action("BombsightInterval");
        bombardier.Quantity.performed += ctx => Action("BombsightQuantity");

        seat.Reload.performed += ctx => Action("Reload");
        seat.SwitchSeat.performed += ctx => Action("SwitchSeat");
        seat.BailOut.performed += ctx => Action("StartBailout");
        seat.BailOut.canceled += ctx => Action("CancelBailout");
    }

    private void Action(string action)
    {
        if (GameManager.player.aircraft == null || GameManager.player.crew.ripped || GameManager.player.crew.body.Gloc()) return;
        switch (action)
        {
            case "SwitchSeat":
                GameManager.player.crew.SwitchSeat();
                break;
            case "Reload":
                GameManager.player.crew.Seat().TryReload();
                break;
            case "Bomb":
                GameManager.player.aircraft.DropBomb();
                break;
            case "LandingGear":
                GameManager.player.aircraft.SetGear();
                break;
            case "BombBay":
                GameManager.player.aircraft.SetBombBay();
                break;
            case "AirBrakes":
                GameManager.player.aircraft.SetAirBrakes();
                break;
            case "Canopy":
                GameManager.player.aircraft.SetCannopy();
                break;
            case "Engines":
                GameManager.player.aircraft.SetEngines();
                break;
            case "BombsightMode":
                GameManager.player.aircraft.bombSight.ToggleMode();
                break;
            case "BombsightInterval":
                GameManager.player.aircraft.bombSight.ToggleInterval();
                break;
            case "BombsightQuantity":
                GameManager.player.aircraft.bombSight.ToggleAmount();
                break;
            case "StartBailout":
                for (int i = 0; i < GameManager.player.aircraft.crew.Length; i++)
                    GameManager.player.aircraft.crew[i].StartBailout(Random.Range(0.1f, 1f));
                break;
            case "CancelBailout":
                for (int i = 0; i < GameManager.player.aircraft.crew.Length; i++)
                    GameManager.player.aircraft.crew[i].CancelBailout();
                break;
        }
    }
    private void SetThrottle(float thr)
    {
        if (GameManager.player.aircraft == null ||GameManager.player.crew.body.Gloc()) return;
        GameManager.player.aircraft.SetThrottle(thr);
    }
}
