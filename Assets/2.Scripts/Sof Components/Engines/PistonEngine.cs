using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[AddComponentMenu("Sof Components/Power Group/Piston Engine")]
public class PistonEngine : Engine
{
    [SerializeField] private PistonEnginePreset pistonPreset;
    public Propeller propeller;


    public float BoostTime { get; private set; }
    public int SuperchargerSetting { get; private set; }


    public float BrakePower { get; private set; }
    public float BrakeTorque { get; private set; }

    public override bool BoostIsEffective => Working && pistonPreset.BoostIsEffective(RunMode, data.altitude.Get);

    public PistonEnginePreset PistonPreset => pistonPreset;
    public override EngineClass Class => EngineClass.PistonEngine;
    public override EnginePreset Preset => pistonPreset;
    public override float MaxHp => PistonPreset && PistonPreset.LiquidCooled ? ModulesHPData.engineInLine : ModulesHPData.engineRadial;
    public override float ConsumptionRate => PistonPreset.FuelConsumption(Throttle) * BrakePower;
    public override float MinTrueThrottle => 0.2f;
    public EngineRunMode RunMode
    {
        get
        {
            if (Throttle.Boost)
            {
                bool takeOffBoostAvailable = aircraft && aircraft.TimeSinceLastLanding < pistonPreset.TakeOffBoostMaxTime;
                if (pistonPreset.HasTakeOffBoost && takeOffBoostAvailable) return EngineRunMode.TakeOffBoost;
                if (pistonPreset.HasCombatBoost) return EngineRunMode.Boost;
            }
            return EngineRunMode.Continuous;
        }
    }
    public float Torque(float power, float rps)
    {
        return (rps <= 0f) ? 0f : power / rps;
    }
    public string ManifoldPressureInAppropriateUnit()
    {
        return pistonPreset.ManifoldPressureString(SuperchargerSetting,RunMode, RadPerSec, data.altitude.Get);
    }
    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);

        propeller = GetComponentInChildren<Propeller>();
        BoostTime = pistonPreset.HasCombatBoost ? pistonPreset.CombatBoostMaxTime : pistonPreset.TakeOffBoostMaxTime;
    }

    const float engineFriction = 250f;
    const float engineFrictionBroken = 5000f;
    public float Friction(bool on, bool ripped)
    {
        if (on) return 0f;
        if (!on && !ripped) return engineFriction;
        return engineFrictionBroken;
    }

    const float superchargerSettingUpdate = 2f;
    private float superchargerSettingCounter = 0f;

    protected override void UpdatePowerAndRPS(float dt)
    {
        if (PistonPreset.LastPowerSetting > 0 && Working)
        {
            superchargerSettingCounter += dt;
            if (superchargerSettingCounter > superchargerSettingUpdate)
            {
                superchargerSettingCounter = 0f;
                SuperchargerSetting = PistonPreset.BestSuperchargerSetting(Throttle, data.altitude.Get, RunMode);
            }
        }

        BrakePower = Working ? pistonPreset.Power(SuperchargerSetting, RunMode, data.altitude.Get, RadPerSec) * TrueThrottle * structureDamage : 0f;
        BrakeTorque = Working ? Torque(BrakePower, RadPerSec) : 0f;

        float inertia = propeller.InertiaWithGear;
        float angularAcceleration = (BrakeTorque + propeller.Torque) / inertia;
        float friction = Friction(Working, ripped) / inertia;
        RadPerSec = Mathf.MoveTowards(RadPerSec + angularAcceleration * dt, 0f, friction * dt);

        UpdateBoostTimeAndDamage(dt);
    }

    const float overboostingDamageRate = 0.0005f;
    private void UpdateBoostTimeAndDamage(float dt)
    {
        if (!BoostIsEffective) return;

        BoostTime -= dt;

        if (BoostTime < 0f) DirectStructuralDamage(overboostingDamageRate * dt);
    }

    public override bool SetAutomated(bool on, bool instant)
    {
        bool success = base.SetAutomated(on, instant);
        if (instant && success)
            propeller.EngineSetInstantBladesAngle(rb.linearVelocity.magnitude, RadPerSec);

        return success;
    }

    public const float preIgnitionRadPerSec = 5f;
    public override IEnumerator Ignition()
    {
        Igniting = true;

        OnIgnition?.Invoke(this);
        bool preIgnitionPhase = RadPerSec < preIgnitionRadPerSec;

        if (preIgnitionPhase)
        {
            BrakeTorque = 0f;
            BrakePower = 0f;

            float preIgnitionCount = 0f;
            float startRadPerSec = RadPerSec;
            while (preIgnitionCount < Preset.PreIgnitionTime)
            {
                preIgnitionCount += Time.deltaTime;
                RadPerSec = Mathf.Lerp(startRadPerSec, preIgnitionRadPerSec, preIgnitionCount / Preset.PreIgnitionTime);
                yield return null;
            }
        }


        float timeCount = 0f;
        float startRps = RadPerSec;

        while (timeCount < Preset.IgnitionTime)
        {
            float previousRadPerSec = RadPerSec;

            RadPerSec = Mathf.Lerp(startRps, Preset.IdleRadPerSec, Mathv.SmoothStop(timeCount / Preset.IgnitionTime, 3));
            timeCount += Time.deltaTime;

            float inertia = propeller.InertiaWithGear;
            float angularAcceleration = (RadPerSec - previousRadPerSec) / Time.deltaTime;
            BrakeTorque = angularAcceleration * inertia;
            BrakePower = BrakeTorque * RadPerSec;

            yield return null;
        }

        Igniting = false;
    }
}

