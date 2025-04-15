using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineTemperature
{
    private Engine engine;
    private EnginePreset preset;
    private PistonEngine pistonEngine;
    private JetEngine jetEngine;

    public float CoolingCoefficient { get; private set; }
    public float Temperature { get; private set; }
    public float WaterToEngineTempRatio { get; private set; }
    public float CylinderFinsToEngineTempRatio { get; private set; }
    public float OilToEngineTempRatio { get; private set; }

    private bool jet;


    public const float fullEngineTemp = 350f;
    public const float damageEngineTemp = 370f;
    public const float destructionEngineTemp = 430f;
    public const float finsMaxSafeTemp = 200f;
    public const float waterTempFull  = 110f;
    public const float oilTempFull = 90f;

    public float CoolingDeviceTemp => preset.LiquidCooled ? WaterTemperature : RadialCylinderFinsTemp;
    public float CoolingDevicePeakTemp => preset.LiquidCooled ? waterTempFull : finsMaxSafeTemp;

    public float WaterTemperature
    {
        get
        {
            if (!preset.LiquidCooled) return 0f;
            float ratioAdjustedForDamage = Mathf.Lerp(1f, WaterToEngineTempRatio, engine.WaterTank.FillRatio);
            return Mathf.Lerp(engine.data.temperature.Get, Temperature, ratioAdjustedForDamage);
        }
    }
    public float OilTemperature
    {
        get
        {
            float ratioAdjustedForDamage = Mathf.Lerp(1f, OilToEngineTempRatio, engine.OilTank.FillRatio);
            return Mathf.Lerp(engine.data.temperature.Get, Temperature, ratioAdjustedForDamage);
        }
    }
    public float RadialCylinderFinsTemp => Mathf.Lerp(engine.data.temperature.Get, Temperature, CylinderFinsToEngineTempRatio);

    public float CoolingCoefficientDamageIncluded
    {
        get
        {
            float damageModifier = engine.OilTank.FillRatio * 0.5f + 0.5f;
            if (preset.LiquidCooled)
            {
                damageModifier *= engine.WaterTank.FillRatio * 0.85f + 0.15f;
            }

            return CoolingCoefficient * damageModifier;
        }

    }

    public void SetTemperature(float temperature)
    {
        Temperature = temperature;
    }

    public EngineTemperature(Engine attachedEngine)
    {
        engine = attachedEngine;
        preset = engine.Preset;

        jet = engine.Class == EngineClass.JetEngine;
        if (jet) jetEngine = engine as JetEngine;
        else pistonEngine = engine as PistonEngine;

        Temperature = engine.data.temperature.Get;

        ComputeCoolingFactor();
    }

    const float steelHeatCapacity = 500f;
    const float engineThermalEnergyRatio = 2f;
    private void ComputeCoolingFactor()
    {
        float heatingSpeed;
        float peakTempAltitude;

        if (jet)
        {
            JetEnginePreset jetPreset = jetEngine.JetPreset;

            peakTempAltitude = 0f;
            heatingSpeed = HeatingSpeed(jetPreset.MaxThrust);
        }
        else
        {
            PistonEnginePreset pistonPreset = pistonEngine.PistonPreset;

            SuperchargerPowerSettings highestSetting = pistonPreset.PowerSetting(pistonPreset.LastPowerSetting);
            peakTempAltitude = pistonPreset.OptimumAltitude(pistonPreset.LastPowerSetting, EngineRunMode.Boost);

            float powerForPeakTemp = pistonPreset.Power(pistonPreset.LastPowerSetting, EngineRunMode.Boost, peakTempAltitude, pistonPreset.CombatBoostRadPerSec);
            heatingSpeed = HeatingSpeed(powerForPeakTemp);
        }

        float airTemperature = Aerodynamics.GetTemperature(peakTempAltitude);
        float temperatureDelta = fullEngineTemp - airTemperature;
        float peakTempDensity = Aerodynamics.GetAirDensity(peakTempAltitude);

        CoolingCoefficient = heatingSpeed / (temperatureDelta * peakTempDensity);

        WaterToEngineTempRatio = Mathf.InverseLerp(airTemperature, fullEngineTemp, waterTempFull);
        OilToEngineTempRatio = Mathf.InverseLerp(airTemperature, fullEngineTemp, oilTempFull);
        CylinderFinsToEngineTempRatio = Mathf.InverseLerp(airTemperature, fullEngineTemp, finsMaxSafeTemp);
    }

    public float HeatingSpeed(float powerOrThrust)
    {
        float heatingPower = powerOrThrust * engineThermalEnergyRatio;
        return heatingPower / (preset.Weight * steelHeatCapacity);
    }

    public float CoolingSpeed(float coolingDeviceTemp, float altitude)
    {
        float airDensity = Aerodynamics.GetAirDensity(altitude);
        float airTemperature = Aerodynamics.GetTemperature(altitude);

        if (jet)
            return (airTemperature - coolingDeviceTemp) * CoolingCoefficientDamageIncluded;
        else
            return (airTemperature - coolingDeviceTemp) * CoolingCoefficientDamageIncluded * airDensity;
    }

    public float EquilibrumTemp(float powerOrThrust, float altitude)
    {
        float heatingSpeed = HeatingSpeed(powerOrThrust);
        float ambientTemp = Aerodynamics.GetTemperature(altitude);
        float ambientDensity = Aerodynamics.GetAirDensity(altitude);


        if (jet) return heatingSpeed / CoolingCoefficientDamageIncluded + ambientTemp;
        else return heatingSpeed / (CoolingCoefficientDamageIncluded * ambientDensity) + ambientTemp;
    }
    public float EquilibrumTempMaxContinuous()
    {
        float altitude = engine.data.altitude.Get;
        if (jet) return EquilibrumTemp(jetEngine.JetPreset.MaxThrust, altitude);
        else return EquilibrumTemp(pistonEngine.PistonPreset.BestPower(altitude, EngineRunMode.Continuous), altitude);
    }
    public void Update(float dt)
    {
        Temperature += HeatingSpeed(jet ? jetEngine.Thrust : pistonEngine.BrakePower) * dt;
        Temperature += CoolingSpeed(Temperature, engine.data.altitude.Get) * dt;

        if (Temperature > damageEngineTemp && engine.Working)
        {
            float tempDelta = Temperature - damageEngineTemp;
            float damagePerSecond = minDpsOverHeat + dpsGrowthPerDegree * tempDelta * engine.TrueThrottle;
            engine.DirectStructuralDamage(damagePerSecond * dt);

            if (Temperature > destructionEngineTemp) engine.Rip();
        }
    }

    const float minDpsOverHeat = 5f * 0.001f;
    const float dpsGrowthPerDegree = 0.25f * 0.001f;
}