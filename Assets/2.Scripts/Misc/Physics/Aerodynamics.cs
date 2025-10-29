using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Aerodynamics
{
    //Atmospheric calculations -----------------------------------------------------------------------------------------------------------------------------------------------------------------
    const float temperatureLapseRate = 0.0068f;
    const float airConstant = 287f;
    const float kelvin = 273.15f;

    public const float seaLvlTemp = 20f;
    public const float SeaLvlPressure = 101325f;
    public const float seaLvlDensity = SeaLvlPressure / ((seaLvlTemp + kelvin) * airConstant);
    public const float invertSeaLvlDensity = 1f / seaLvlDensity;


    public static float GetTemperature(float alt) { return seaLvlTemp - alt * temperatureLapseRate; }
    public static float GetPressure(float alt) { return SeaLvlPressure * M.Pow(1f - alt * temperatureLapseRate / (seaLvlTemp + kelvin), 5); }
    public static float GetAirDensity(float temp, float press) { return press / ((temp + kelvin) * airConstant); }
    public static float GetAirDensity(float alt) { return GetAirDensity(GetTemperature(alt), GetPressure(alt)); }

    public static float GetAltitude(float pressure)
    {
        return (1f - Mathf.Pow(pressure / SeaLvlPressure, 0.2f)) * (seaLvlTemp + kelvin) / temperatureLapseRate;
    }

    //Airfoils & Aerodynaimc forces calculations ------------------------------------------------------------------------------------------------------------------------------------------------

    public const float liftLine = 0.75f;
    const float maxDragCoeffDamaged = 3f;

    public static float GetDragGroundEffect(float relativeAltitude, float wingSpan)
    {
        if (relativeAltitude > 50f) return 1f;
        float ratio = relativeAltitude / wingSpan;
        float groundEffect = ratio * Mathf.Sqrt(ratio) * 33f;
        return 1f / groundEffect + 1f;
    }
    const float peakGroundEffect = 1.25f;
    public static float GetLiftGroundEffect(float relativeAltitude, float wingSpan)
    {
        if (relativeAltitude > wingSpan) return 1f;

        float ratio = relativeAltitude / wingSpan;
        return Mathf.Lerp(peakGroundEffect,1f, ratio);
    }

    public static Vector3 Lift(Vector3 velocity, Vector3 aeroDir, float dens, float surface, float cl, float dmg)
    {
        return Lift(velocity, velocity.magnitude, aeroDir, dens, surface, cl, dmg);
    }
    public static Vector3 Drag(Vector3 velocity, float dens, float surface, float cd, float dmg)
    {
        return Drag(velocity, velocity.magnitude, dens, surface, cd, dmg);
    }

    public static Vector3 Lift(Vector3 velocity, float velocityMagnitude, Vector3 aeroDir, float dens, float surface, float cl, float dmg)
    {
        Vector3 liftDir = Vector3.Cross(velocity, aeroDir);
        float dmgLiftCoeff = dmg * dmg * dmg;
        return 0.5f * cl * dens * dmgLiftCoeff * surface * velocityMagnitude * liftDir;
    }
    public static Vector3 Drag(Vector3 velocity, float velocityMagnitude, float dens, float surface, float cd, float dmg)
    {
        float dmgDragCoeff = maxDragCoeffDamaged - dmg * (maxDragCoeffDamaged - 1f);
        return 0.5f * cd * dens * dmgDragCoeff * surface * velocityMagnitude * -velocity;
    }

    public static Vector3 LiftAndDrag(Vector3 velocity, float velocityMagnitude, Vector3 aeroDir, float dens, float surface, Vector2 coeffs, float dmg)
    {
        Vector3 liftDir = Vector3.Cross(velocity, aeroDir);

        float commonFactor = 0.5f * dens * surface * velocityMagnitude;
        float lift = dmg * dmg * dmg * coeffs.y * commonFactor;
        float drag = (maxDragCoeffDamaged - dmg * (maxDragCoeffDamaged - 1f)) * coeffs.x * commonFactor;
        

        return lift * liftDir + drag *  -velocity;
    }
}