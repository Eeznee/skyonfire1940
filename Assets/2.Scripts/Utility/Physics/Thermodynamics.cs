using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Thermodynamics
{
    const float proportionalMin = 5f;
    const float proportionalMax = 30f;
    const float conductivity = 0.07f;

    public static float CoolantTemperature(float fullTemp,float fill,float fullEngineTemp, float engineTemp)
    {
        float ratio = Mathf.InverseLerp(20f, fullEngineTemp, fullTemp);
        ratio = Mathf.Lerp(1f, ratio, Mathf.Sqrt(fill));
        return Mathf.LerpUnclamped(20f, engineTemp, ratio);
    }
    public static float LerpTemperature(float from,float to)
    {
        float temperatureDiff = Mathf.Clamp(Mathf.Abs(to - from), proportionalMin, proportionalMax);
        return Mathf.MoveTowards(from, to, temperatureDiff * conductivity * Time.deltaTime);
    }
}
