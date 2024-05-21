using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class HydraulicControl
{
    public enum Type
    {
        Custom,
        LandingGear,
        Flaps,
        AirBrakes,
        Canopy,
        SecondCanopy,
        BombBay,
    }
    public static bool IsAlwaysBinary(this Type control) { return control == Type.BombBay || control == Type.LandingGear ||  control == Type.AirBrakes; }
    public static bool HasCustomDefaultState(this Type control) { return control == Type.Custom || control == Type.Canopy || control == Type.SecondCanopy; }

    public static float DefaultState(this Type control)
    {
        switch (control)
        {
            case Type.Flaps: return 0f;
            case Type.LandingGear: return 0f;
            case Type.AirBrakes: return 0f;
            case Type.BombBay: return 0f;
        }
        Debug.LogError("This control type doesnt have a default state");
        return 0f;
    }

    public static string StringParameter(this Type control)
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
}
