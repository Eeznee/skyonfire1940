using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HydraulicsManager
{
    private SofAircraft aircraft;

    public HydraulicSystem gear;
    public HydraulicSystem canopy;
    public HydraulicSystem flaps;
    public HydraulicSystem airBrakes;
    public HydraulicSystem bombBay;

    public HydraulicsManager(SofAircraft _aircraft)
    {
        aircraft = _aircraft;

        HydraulicSystem[] systems = aircraft.GetComponentsInChildren<HydraulicSystem>();

        foreach (HydraulicSystem hs in systems)
        {
            switch (hs.control)
            {
                case HydraulicControl.Type.Flaps: flaps = hs; break;
                case HydraulicControl.Type.LandingGear: gear = hs; break;
                case HydraulicControl.Type.AirBrakes: airBrakes = hs; break;
                case HydraulicControl.Type.BombBay: bombBay = hs; break;
                case HydraulicControl.Type.Canopy: canopy = hs; break;
            }
        }
    }

    public void SetFlaps(int input)
    {
        if (!flaps) return;
        flaps.SetDirection(input);
        if (Player.aircraft == aircraft && input != 0)
            Log.Print(flaps.GetLog("Flaps", "Deploying", "Retracting", "Unoperational"), "flaps");
    }
    public void SetGear()
    {
        if (!gear || aircraft.data.relativeAltitude.Get < 4f) return;
        gear.Set();
        if (Player.aircraft == aircraft)
            Log.Print(gear.GetLog("Landing Gear", "Deploying", "Retracting", "Damaged"), "gear");
    }
    public void SetAirBrakes()
    {
        if (!airBrakes) return;

        airBrakes.Set();
        if (Player.aircraft == aircraft)
            Log.Print(airBrakes.GetLog("Airbrakes", "Deploying", "Retracting", "Unavailable"), "airbrakes");
    }
    public void SetBombBay()
    {
        if (!bombBay) return;
        bombBay.Set();
        if (Player.aircraft == aircraft)
            Log.Print(bombBay.GetLog("Bomb Bay", "Opening", "Closing", "Not Working"), "bombbay");
    }
    public void SetCanopy()
    {
        if (!canopy) return;
        canopy.Set();
        if (Player.aircraft == aircraft)
            Log.Print(canopy.GetLog("Canopy", "Opening", "Closing", " Is Missing"), "canopy");
    }

}
