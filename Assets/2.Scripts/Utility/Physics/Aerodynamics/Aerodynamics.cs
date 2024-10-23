using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Aerodynamics
{
    // Air parameters -----------------------------------------------------------------------------------------------------------------------------------------------------------------
    const float atmosphericLayerHeight = 12000f;
    const float temperatureLapseRate = 0.0068f;
    const float airConstant = 287f;
    const float kelvin = 273.15f;
    public const float SeaLvlPressure = 101325f;
    const float seaLvlTemp = 20f;
    public const float seaLvlDensity = SeaLvlPressure / ((20f + kelvin) * airConstant);
    public const float invertSeaLvlDensity = 1f / seaLvlDensity;

    public static float GetTemperature(float alt) { return seaLvlTemp - alt * temperatureLapseRate; }
    public static float GetPressure(float alt) { return SeaLvlPressure * Mathv.SmoothStart(1f - temperatureLapseRate/(seaLvlTemp+kelvin) * alt,5); }
    public static float GetAirDensity(float temp, float press) { return press / ((temp + kelvin) * airConstant); }
    public static float GetAirDensity(float alt) { return GetAirDensity(GetTemperature(alt),GetPressure(alt)); }



    //Aerodynamic forces calculations ------------------------------------------------------------------------------------------------------------------------------------------------
    const float maxDrag = 3f;

    public static float GetGroundEffect(float relativeAltitude, float wingSpan)
    {
        float ratio = relativeAltitude / wingSpan;
        float groundEffect = ratio * Mathf.Sqrt(ratio) * 33f;
        return 1f / groundEffect + 1f;
    }

    public static Vector3 Lift(Vector3 velocity, float speed,Vector3 aeroDir, float dens, float surface , float cl, float dmg)
    {
        Vector3 liftDir = Vector3.Cross(velocity, aeroDir);
        float dmgLiftCoeff = dmg*dmg*dmg;
        return liftDir * speed * 0.5f * surface * dens * cl * dmgLiftCoeff;
    }
    public static Vector3 Drag(Vector3 velocity, float speed, float dens, float surface, float cd, float dmg)
    {
        float dmgDragCoeff = maxDrag - dmg * (maxDrag - 1f);
        return -velocity * speed * 0.5f * surface * dens * cd * dmgDragCoeff;
    }
    public static float MaxDeflection(float speed, float constant)
    {
        return Mathv.QuickASin(constant / (speed * speed)) * Mathf.Rad2Deg;
    }
}

