using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SuperchargingMechanism
{
    FixedSupercharger,
    VariableSupercharger,
    Turbocharger
}

[System.Serializable]
public struct SuperchargerPowerSettings
{
    public float power;
    public float criticalAltitude;

    const float hpToWatts = 745.7f;

    public float InvertCritPressure => 1f / Aerodynamics.GetPressure(criticalAltitude);
    public float PowerWatts => power * hpToWatts;

    public static bool operator ==(SuperchargerPowerSettings p1, SuperchargerPowerSettings p2)
    {
        return p1.power == p2.power && p1.criticalAltitude == p2.criticalAltitude;
    }
    public static bool operator !=(SuperchargerPowerSettings p1, SuperchargerPowerSettings p2)
    {
        return !(p1 == p2);
    }
    public override bool Equals(object obj)
    {
        if (obj is SuperchargerPowerSettings)
        {
            return this == (SuperchargerPowerSettings)obj;
        }
        return false;
    }
    public override int GetHashCode()
    {
        return (power, criticalAltitude).GetHashCode();
    }

}

public enum EngineRunMode
{
    Continuous,
    Boost,
    TakeOffBoost
}

[CreateAssetMenu(fileName = "New Piston Engine", menuName = "SOF/Aircraft Modules/Piston Engine")]
public partial class PistonEnginePreset : EnginePreset
{
    [Tooltip("Most engines use a fixed supercharger gear. Variable superchargers are only used on specific german engines")]
    [SerializeField] private SuperchargingMechanism superChargingType = SuperchargingMechanism.FixedSupercharger;
    [Tooltip("Most engines have one or two supercharger gears, make sure you are using values for either max continuous or 30 min combat limit. \n " +
        "If the altitude is not indicated, you can use the altitude at which the aircraft reaches its top speed, as they are generally the same\n" +
        "Watch out for units !! metric HorsePower (or PS) & imperial HorsePower are different. Power values are in imperial HP.")]
    [SerializeField] private SuperchargerPowerSettings[] powerSettings;
    [SerializeField] private Pressure continuousMP = new Pressure(Pressure.PressureUnit.bar);
    [Tooltip("Adjust this value so that power at sea level matches IRL data. The value is usually between 2 and 3, the more modern the engine is, the lesser the value usually.")]
    [SerializeField] private float throttlingLossPerKm = 2.5f;
    [Tooltip("Adjust this value so that boosted power matches IRL data.")]
    [SerializeField] private float rpmEfficiencyDelta = 0.2f;
    [Tooltip("Some engines (like db 601 A) increase continuous RPM when the supercharger can't provide maximum Manifold Pressure. It is the case when boosted and continuous power merge together in a graph. If this does not happen for this engine, set this value to zero.")]
    [SerializeField] private float adjustedRpmAltitudeDetla = 0f;
    public SuperchargingMechanism SuperChargingType => superChargingType;
    public float ContinuousMP => continuousMP.PressurePa;
    public float ThrottlingLossTurboIncluded => superChargingType == SuperchargingMechanism.Turbocharger ? 0f : throttlingLossPerKm * 0.00001f;
    public float RpmEfficiencyDelta => rpmEfficiencyDelta;
    public float AdjustedRpmAltitudeDetla => HasCombatBoost ? adjustedRpmAltitudeDetla : 0f;
    public bool VariableGearAvailable => SuperChargingType == SuperchargingMechanism.VariableSupercharger && powerSettings.Length == 2;
    public SuperchargerPowerSettings PowerSetting(int id)
    {
        id = Mathf.Clamp(id, 0, powerSettings.Length - 1);
        return powerSettings[id];
    }

    public bool HasBoost => combatBoost || takeOffBoost;

    [SerializeField] private bool combatBoost = true;
    [SerializeField] private Pressure combatBoostMP = new Pressure(Pressure.PressureUnit.bar);
    [SerializeField] private float combatBoostRpm = 3200f;
    [SerializeField] private float combatBoostMaxTime = 300f;
    public bool HasCombatBoost => combatBoost;
    public float CombatBoostMP => combatBoostMP.PressurePa;
    public float CombatBoostRadPerSec => combatBoostRpm * rpmToRadPerSec;
    public float CombatBoostMaxTime => combatBoostMaxTime;


    [SerializeField] private bool takeOffBoost = false;
    [SerializeField] private Pressure takeOffBoostMP = new Pressure(Pressure.PressureUnit.bar);
    [SerializeField] private float takeOffBoostRpm = 3200f;
    public bool HasTakeOffBoost => takeOffBoost;
    public float TakeOffBoostMP => takeOffBoostMP.PressurePa;
    public float TakeOffBoostRadPerSec => takeOffBoostRpm * rpmToRadPerSec;
    public float TakeOffBoostMaxTime => 60f;


    [SerializeField] private float propellerReductionGear = 0.5f;
    [Tooltip("Fuel consumption is not always easy to find when looking at engine documentation. \n" +
        "Make sure to watch out for unit, metric horse power (or PS) must be converted to imperial Hp.\n" +
        " If you can't find any, you can extrapolate it from the fuel capacity and flight time of a given aircraft")]
    [SerializeField] private float fuelConsumption = 0.3f;
    public float PropellerReductionGear => propellerReductionGear;

    public int LastPowerSetting => powerSettings.Length - 1;


    public float ManifoldPressureLimit(EngineRunMode runMode)
    {
        if (runMode == EngineRunMode.Boost && HasCombatBoost) return CombatBoostMP;
        if (runMode == EngineRunMode.TakeOffBoost && HasTakeOffBoost) return TakeOffBoostMP;
        return ContinuousMP;
    }
    public float ManifoldPressure(int settingsId, EngineRunMode runMode, float radPerSec, float altitude)
    {
        float pressureLimit = ManifoldPressureLimit(runMode);
        float ambientPressure = Aerodynamics.GetPressure(altitude);
        float compressingRatio = CompressingRatio(settingsId, radPerSec);

        if (VariableGearAvailable)
        {
            float minCompressingRatio = CompressingRatio(0, radPerSec);
            float maxCompressingRatio = CompressingRatio(1, radPerSec);

            compressingRatio = pressureLimit / Aerodynamics.GetPressure(altitude);
            compressingRatio = Mathf.Clamp(compressingRatio, minCompressingRatio, maxCompressingRatio);
        }

        float manifoldPressure = ambientPressure * compressingRatio;
        return Mathf.Min(manifoldPressure, pressureLimit);
    }
    public string ManifoldPressureString(int settingsId, EngineRunMode runMode, float radPerSec, float altitude)
    {
        float pressure = continuousMP.ConvertPAUsingUnit(ManifoldPressure(settingsId, runMode, radPerSec, altitude));
        return continuousMP.CompleteValueWithUnit(pressure);
    }
    public float CompressingRatio(int settingId, float radPerSec)
    {
        SuperchargerPowerSettings setting = powerSettings[settingId];

        float baseCompressingRatio = ContinuousMP / Aerodynamics.GetPressure(setting.criticalAltitude);
        return Mathf.Max(1f, baseCompressingRatio * radPerSec / NominalRadPerSec);
    }
    public float OptimumAltitude(int settingId, EngineRunMode engineRunMode)
    {
        SuperchargerPowerSettings setting = powerSettings[settingId];

        float compressingRatio = CompressingRatio(settingId, TargetRadPerSec(1f, engineRunMode, settingId, setting.criticalAltitude));
        return OptimumAltitude(compressingRatio, ManifoldPressureLimit(engineRunMode));
    }
    public float OptimumAltitude(float compressingRatio, float manifoldPressureLimit)
    {
        float ambiantPressureAtOptimumAltitude = manifoldPressureLimit / compressingRatio;
        return Aerodynamics.GetAltitude(ambiantPressureAtOptimumAltitude);
    }

    public float TargetRadPerSec(float throttle, EngineRunMode runMode, int settingId, float altitude)
    {
        if (runMode == EngineRunMode.Boost && HasCombatBoost) return CombatBoostRadPerSec;
        if (runMode == EngineRunMode.TakeOffBoost && HasTakeOffBoost) return TakeOffBoostRadPerSec;

        SuperchargerPowerSettings setting = powerSettings[settingId];

        float fullThrottleRadPerSec = NominalRadPerSec;

        bool highAltitudeRpmIncrease = AdjustedRpmAltitudeDetla > 0f;
        highAltitudeRpmIncrease &= altitude > setting.criticalAltitude;
        highAltitudeRpmIncrease &= setting == powerSettings[powerSettings.Length - 1] || VariableGearAvailable;

        if (highAltitudeRpmIncrease)
        {
            float startAltitude = powerSettings[^1].criticalAltitude;
            float finalAltitude = powerSettings[^1].criticalAltitude + AdjustedRpmAltitudeDetla;

            float altitudeT = Mathf.InverseLerp(startAltitude, finalAltitude, altitude);
            fullThrottleRadPerSec = Mathf.Lerp(NominalRadPerSec, CombatBoostRadPerSec, altitudeT);
        }

        return Mathf.Lerp(IdleRadPerSec, fullThrottleRadPerSec, throttle);
    }
    public float Power(int settingId, EngineRunMode engineRunMode, float altitude, float radPerSec)
    {
        if (powerSettings.Length == 0) return 0f;

        SuperchargerPowerSettings setting = powerSettings[settingId];
        float pressureLimit = ManifoldPressureLimit(engineRunMode);
        float basePower;
        float compressingRatio;

        if (VariableGearAvailable)
        {
            float minCompressingRatio = CompressingRatio(0, radPerSec);
            float maxCompressingRatio = CompressingRatio(1, radPerSec);

            compressingRatio = pressureLimit / Aerodynamics.GetPressure(altitude);
            compressingRatio = Mathf.Clamp(compressingRatio, minCompressingRatio, maxCompressingRatio);

            float lerp = Mathf.InverseLerp(minCompressingRatio, maxCompressingRatio, compressingRatio);

            basePower = Mathf.Lerp(powerSettings[0].PowerWatts, powerSettings[1].PowerWatts, Mathv.SmoothStop(lerp, 2));
        }
        else
        {
            basePower = setting.PowerWatts;
            compressingRatio = CompressingRatio(settingId, radPerSec);
        }

        return Power(basePower, compressingRatio, pressureLimit, altitude, radPerSec);
    }
    public float Power(float basePower, float compressingRatio, float pressureLimit, float altitude, float radPerSec)
    {
        float ambientPressure = Aerodynamics.GetPressure(altitude);
        float currentManifoldPressure = ambientPressure * compressingRatio;
        float rpmEfficiency = Efficiency(radPerSec);

        float throttlingMultiplier = 1f;

        if (currentManifoldPressure > pressureLimit)
        {
            float ambientPressureAtCritAltitude = pressureLimit / compressingRatio;
            float critAltitude = Aerodynamics.GetAltitude(ambientPressureAtCritAltitude);
            throttlingMultiplier = 1f - (critAltitude - altitude) * ThrottlingLossTurboIncluded;

            currentManifoldPressure = pressureLimit;
        }

        return basePower * throttlingMultiplier * rpmEfficiency * (currentManifoldPressure / ContinuousMP);
    }
    public float BestPower(float altitude, EngineRunMode engineRunMode)
    {
        if (VariableGearAvailable || powerSettings.Length == 1)
        {
            float radPerSec = TargetRadPerSec(1f, engineRunMode, 0, altitude);
            return Power(0, engineRunMode, altitude, radPerSec);
        }

        int setting = BestSuperchargerSetting(1f, altitude, engineRunMode);
        float targetRadPerSec = TargetRadPerSec(1f, engineRunMode, setting, altitude);
        return Power(setting, engineRunMode, altitude, targetRadPerSec);
    }
    public float HighestContinuousPower => powerSettings[0].power * hpToWatts;

    public int BestSuperchargerSetting(float throttle, float altitude, EngineRunMode engineRunMode)
    {
        if (powerSettings.Length == 1) return 0;
        if (VariableGearAvailable) return 0;

        int bestPowerAtGear = 0;
        float bestPower = 0f;

        for (int i = 0; i < powerSettings.Length; i++)
        {
            float radPerSec = TargetRadPerSec(throttle, engineRunMode, i, altitude);
            float power = Power(i, engineRunMode, altitude, radPerSec);

            if (power > bestPower)
            {
                bestPower = power;
                bestPowerAtGear = i;
            }
        }
        return bestPowerAtGear;
    }

    //RPM efficiency
    public float Efficiency(float radPerSec)
    {
        float mostEfficientRadPerSec = NominalRadPerSec * (1f + RpmEfficiencyDelta);

        float nominal = EfficiencyFormula(NominalRadPerSec / mostEfficientRadPerSec);
        float current = EfficiencyFormula(radPerSec / mostEfficientRadPerSec);

        return current / nominal;
    }
    private float EfficiencyFormula(float x)
    {
        return -x * x * x + x * x + x;
    }


    //Fuel Consumption
    const float cruiseFuelMultiplier = 0.8f;
    const float boostedFuelMultiplier = 1.2f;
    const float cruisingThrottle = 0.7f;

    public override float FuelConsumption(float throttle)
    {
        if (throttle > 1f) return boostedFuelMultiplier * fuelConsumption * wattsToHp;

        float cruisingCoeff = Mathf.InverseLerp(throttle > cruisingThrottle ? 1f : 0f, cruisingThrottle, throttle);
        float multiplier = Mathf.Lerp(1f, cruiseFuelMultiplier, cruisingCoeff);

        return multiplier * fuelConsumption * wattsToHp;
    }

    public bool BoostIsEffective(EngineRunMode runMode, float altitude)
    {
        if (runMode == EngineRunMode.Continuous) return false;
        bool available = runMode == EngineRunMode.TakeOffBoost ? HasTakeOffBoost : HasCombatBoost;
        if (!available) return false;

        float boostedRadPerSec = runMode == EngineRunMode.TakeOffBoost ? TakeOffBoostRadPerSec : CombatBoostRadPerSec;
        if (NominalRadPerSec < boostedRadPerSec) return true;
        return altitude < powerSettings[LastPowerSetting].criticalAltitude;
    }
}
