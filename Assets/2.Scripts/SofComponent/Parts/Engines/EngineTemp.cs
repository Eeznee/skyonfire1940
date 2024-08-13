using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineTemp
{
    private Engine engine;
    private EnginePreset preset;
    public float maximumTemperature { get; private set; }
    public float temperature { get; private set; }
    public float waterTemperature { get; private set; }
    public float oilTemperature { get; private set; }

    const float maxThrottleNoCoolant = 0.5f;
    const float minDpsOverHeat = 1f / 100f;
    const float dpsGrowthPerDegree = 1f / 600f;
    const float fullFrictionDps = 1f / 40f;

    public EngineTemp(Engine attachedEngine)
    {
        engine = attachedEngine;
        preset = engine.preset;

        //Max temperature is equilibrum temperature at 3/4 boost
        float t = (preset.boostValue * 3f + 1f) / 4f;
        maximumTemperature = Mathf.LerpUnclamped(preset.tempIdle, preset.tempFull, t); 
    }

    private float MaxSafeThrottle()
    {
        float maxThrottle = Mathf.Lerp(maxThrottleNoCoolant, 1f, engine.oilCircuit.mainTank.FillRatio);
        if (preset.WaterCooled()) maxThrottle *= Mathf.Lerp(maxThrottleNoCoolant, 1f, engine.waterCircuit.mainTank.FillRatio);
        return maxThrottle;
    }

    public float EquilibrumTemp(float throttle)
    {
        float t = Mathf.Clamp01(throttle) / MaxSafeThrottle();
        return Mathf.LerpUnclamped(preset.tempIdle, preset.tempFull, t);
    }

    public void SetTemperature(float value)
    {
        temperature = value;

        if (preset.WaterCooled()) waterTemperature = Thermodynamics.CoolantTemperature(preset.waterTemperature, engine.water.FillRatio, preset.tempFull, temperature);
        oilTemperature = Thermodynamics.CoolantTemperature(preset.oilTemperature, engine.oil.FillRatio, preset.tempFull, temperature);
    }

    public void Update(float dt)
    {
        float targetTemp = engine.Working() ? EquilibrumTemp(engine.throttleInput) : engine.data.temperature.Get;
        SetTemperature(Thermodynamics.LerpTemperature(temperature, targetTemp,dt));

        if (temperature > maximumTemperature && engine.Working())
        {
            float damagePerSecond = minDpsOverHeat + dpsGrowthPerDegree * (temperature - maximumTemperature);
            engine.DirectStructuralDamage(damagePerSecond * dt / engine.MaxHp);
        }

        if (temperature > maximumTemperature * 1.12f) engine.Rip();


        float frictionFactor = 1f - engine.structureDamage * engine.oil.FillRatio;
        if (frictionFactor > 0.3f)
            engine.DirectStructuralDamage(frictionFactor * engine.trueThrottle * fullFrictionDps * dt / engine.MaxHp);
    }
}
