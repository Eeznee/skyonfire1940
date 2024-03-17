using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SofAircraftActionsExtensions
{
    public static void SetEngines(this SofAircraft aircraft, bool on, bool instant)
    {
        foreach (Engine pe in aircraft.engines)
        {
            pe.throttleInput = aircraft.throttle;
            pe.Set(on, instant);
        }
        if (Player.aircraft == aircraft) Log.Print((aircraft.engines.Length == 1 ? "Engine " : "Engines ") + (on ? "On" : "Off"), "engines");
    }
    public static void SetEngines(this SofAircraft aircraft)
    {
        aircraft.SetEngines(aircraft.enginesState == Engine.EnginesState.Off, false);
    }
    public static void SetThrottle(this SofAircraft aircraft, float thr)
    {
        aircraft.throttle = Mathf.Clamp01(thr);
        foreach (Engine engine in aircraft.engines) engine.throttleInput = aircraft.throttle;
    }
    public static void SetFlaps(this SofAircraft aircraft, int input)
    {
        if (!aircraft.flaps) return;
        aircraft.flaps.SetDirection(input);
        if (Player.aircraft == aircraft && input != 0)
            Log.Print(aircraft.flaps.GetLog("Flaps","Deploying","Retracting","Unoperational"), "flaps");
    }
    public static void SetGear(this SofAircraft aircraft)
    {
        if (!aircraft.gear || aircraft.data.relativeAltitude.Get < 4f) return;
        aircraft.gear.Set();
        if (Player.aircraft == aircraft)
            Log.Print(aircraft.gear.GetLog("Landing Gear", "Deploying", "Retracting", "Damaged"), "gear");
    }
    public static void SetAirBrakes(this SofAircraft aircraft)
    {
        if (!aircraft.airBrakes) return;

        aircraft.airBrakes.Set();
        if (Player.aircraft == aircraft)
            Log.Print(aircraft.airBrakes.GetLog("Airbrakes","Deploying","Retracting","Unavailable"), "airbrakes");
    }
    public static void SetBombBay(this SofAircraft aircraft)
    {
        if (!aircraft.bombBay) return;
        aircraft.bombBay.Set();
        if (Player.aircraft == aircraft)
            Log.Print(aircraft.bombBay.GetLog("Bomb Bay","Opening","Closing","Not Working"), "bombbay");
    }
    public static void SetCanopy(this SofAircraft aircraft)
    {
        if (!aircraft.canopy) return;
        aircraft.canopy.Set();
        if (Player.aircraft == aircraft)
            Log.Print(aircraft.canopy.GetLog("Canopy","Opening","Closing"," Is Missing"), "canopy");
    }

    public static void FirePrimaries(this SofAircraft aircraft) { foreach (Gun g in aircraft.primaries) if (g.data == aircraft.data && (g.gunPreset.name != "MP40" || aircraft.bombBay.state > 0.8f)) g.Trigger(); }
    public static void FireSecondaries(this SofAircraft aircraft) { foreach (Gun g in aircraft.secondaries) if (g.data == aircraft.data) g.Trigger(); }
    public static void DropBomb(this SofAircraft aircraft)
    {
        OrdnanceLoad.LaunchOptimal(aircraft.bombs, 5f);
    }
    public static void FireRocket(this SofAircraft aircraft)
    {
        OrdnanceLoad.LaunchOptimal(aircraft.rockets, 0f);
    }
    public static void DropTorpedo(this SofAircraft aircraft)
    {
        OrdnanceLoad.LaunchOptimal(aircraft.torpedoes, 0f);
    }
}
