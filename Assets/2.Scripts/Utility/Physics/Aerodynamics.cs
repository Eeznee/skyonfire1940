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

    public static Vector2 SimpleCoefficients(float alpha,float maxCl,float minCd,float maxCd)
    {
        float rad = alpha * Mathf.Deg2Rad;
        float cd = (1f - Mathv.QuickLoopingCos(rad)) * (maxCd - minCd) * 0.5f + minCd;
        float cl = Mathv.QuickLoopingSin(rad) * maxCl;

        return new Vector2(cd, cl);
    }


    //Aerodynamic forces calculations ------------------------------------------------------------------------------------------------------------------------------------------------
    const float maxDrag = 3f;

    public static float GetGroundEffect(float relativeAltitude, float wingSpan)
    {
        float ratio = relativeAltitude / wingSpan;
        float groundEffect = ratio * Mathf.Sqrt(ratio) * 33f;
        return 1f / groundEffect + 1f;
    }

    public static Vector3 ComputeLift(Vector3 velocity, float vel,Vector3 rootTipDir, float dens, float surface , float cl, float dmg)
    {
        Vector3 liftDir = Vector3.Cross(velocity, rootTipDir);
        float dmgLiftCoeff = dmg*dmg*dmg;
        return liftDir * vel * 0.5f * surface * dens * cl * dmgLiftCoeff;
    }
    public static Vector3 ComputeDrag(Vector3 velocity, float vel, float dens, float surface, float cd, float dmg)
    {
        float dmgDragCoeff = maxDrag - dmg * (maxDrag - 1f);
        return -velocity * vel * 0.5f * surface * dens * cd * dmgDragCoeff;
    }
    public static float MaxDeflection(float speed, float constant)
    {
        return Mathv.QuickASin(constant / (speed * speed)) * Mathf.Rad2Deg;
    }
}

