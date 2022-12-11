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

    public static float GetTemperature(float alt, float sealLvlTemp) { return sealLvlTemp - alt * temperatureLapseRate; }
    public static float GetPressure(float alt,float seaLvlTemp) { return SeaLvlPressure * Mathv.SmoothStart(1f - temperatureLapseRate/(seaLvlTemp+kelvin) * alt,5); }
    public static float GetAirDensity(float temp, float press) { return press / ((temp + kelvin) * airConstant); }


    //Aerodynamic forces calculations ------------------------------------------------------------------------------------------------------------------------------------------------
    const float maxDrag = 3f;

    public static Vector3 ComputeLift(Vector3 velocity, float vel,Vector3 wingDir, float dens, float surface , float cl, float dmg)
    {
        Vector3 liftDir = Vector3.Cross(velocity, wingDir);
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

