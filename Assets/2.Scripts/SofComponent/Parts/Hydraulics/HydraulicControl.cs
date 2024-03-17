using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HydraulicControl
{
    public enum Type
    {
        Custom,
        LandingGear,
        Flaps,
        AirBrakes,
        Canopy,
        SecondCanopy,
        BombBay
    }
    public static bool IsAlwaysBinary(Type control) { return control == Type.BombBay || control == Type.LandingGear || control == Type.AirBrakes; }
    public static string GetParameter(Type control)
    {
        switch (control)
        {
            case Type.Flaps: return "FlapsState";
            case Type.LandingGear: return "GearState";
            case Type.AirBrakes: return "AirbrakeState";
            case Type.Canopy: return "CanopyState";
            case Type.SecondCanopy: return "SecondCanopyState";
            case Type.BombBay: return "BombBayState";
        }
        Debug.LogError("Custom Hydraulic cannot use this function");
        return "";
    }

    public static void AssignHydraulics(SofAircraft aircraft)
    {
        HydraulicSystem[] systems = aircraft.GetComponentsInChildren<HydraulicSystem>();

        foreach(HydraulicSystem hs in systems)
        {
            switch (hs.control)
            {
                case Type.Flaps: aircraft.flaps = hs; break;
                case Type.LandingGear: aircraft.gear = hs; break;
                case Type.AirBrakes: aircraft.airBrakes = hs; break;
                case Type.BombBay: aircraft.bombBay = hs; break;
                case Type.Canopy: aircraft.canopy = hs; break;
            }
        }
    }
}
