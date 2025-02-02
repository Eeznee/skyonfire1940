using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[AddComponentMenu("Sof Components/Power Group/Piston Engine")]
public class PistonEngine : Engine
{
    //References
    public Propeller propeller;

    //Data
    public float BrakePower { get; private set; }
    public float BrakeTorque { get; private set; }
    public float boostTime;


    public override float ConsumptionRate => Preset.ConsumptionRate(Throttle, BrakePower);


    public float ComputeThrottle(float targetRadPerSec)
    {
        if (targetRadPerSec < Preset.fullRps)
        {
            return Mathf.InverseLerp(Preset.idleRPS, preset.fullRps, targetRadPerSec);

        }
        else
        {
            float wepFactor = Mathf.InverseLerp(Preset.fullRps, preset.WEPrps, targetRadPerSec);
            return Mathf.Lerp(1f, preset.WEPValue, wepFactor);
        }
    }

    public float Power(float trueThrottle, float rps)
    {
        float basePower = Preset.gear1.Evaluate(data.altitude.Get) * 745.7f;
        float rpmEfficiency = Preset.Efficiency(rps, trueThrottle);
        return basePower * trueThrottle * rpmEfficiency * Mathv.SmoothStart(structureDamage, 2);
    }
    public float Torque(float power, float rps)
    {
        return (rps <= 0f) ? 0f : power / rps;
    }
    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);

        propeller = GetComponentInChildren<Propeller>();
        boostTime = Preset.boostTime;
    }

    const float engineFriction = 100f;
    const float engineFrictionBroken = 5000f;
    public float Friction(bool on, bool ripped)
    {
        if (on) return 0f;
        if (!on && !ripped) return engineFriction;
        return engineFrictionBroken;
    }
    protected override void UpdatePowerAndRPS(float dt)
    {
        BrakePower = workingAndRunning ? Power(Throttle.TrueThrottle, radiansPerSeconds) : 0f;
        BrakeTorque = workingAndRunning ? Torque(BrakePower, radiansPerSeconds) : 0f;

        float inertia = propeller.reductionGear * propeller.MomentOfInertia;
        float angularAcceleration = (BrakeTorque + propeller.Torque) / inertia;
        float friction = Friction(workingAndRunning, ripped) / inertia;
        radiansPerSeconds = Mathf.MoveTowards(radiansPerSeconds + angularAcceleration * dt, 0f, friction * dt);
    }
}

