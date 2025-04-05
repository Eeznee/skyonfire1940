using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[AddComponentMenu("Sof Components/Power Group/Jet Engine")]
public class JetEngine : Engine, IAircraftForce
{
    [SerializeField] protected JetEnginePreset jetPreset;
    public Transform inlet;

    public float Thrust { get; private set; }

    const float torqueCoeff = 0.2f;
    const float minTorque = 60f;
    const float friction = 10f;



    public JetEnginePreset JetPreset => jetPreset;
    public override EngineClass Class => EngineClass.JetEngine;
    public override EnginePreset Preset => jetPreset;
    public override float MinimumRps => Preset.IdleRadPerSec * 0.8f;
    public override float ConsumptionRate => Thrust * JetPreset.FuelConsumption(Throttle);
    public override float MaxHp => ModulesHPData.engineJet;
    public override float MinTrueThrottle => 0.05f;


    public ForceAtPoint SimulatePointForce(FlightConditions flightConditions)
    {
        if (Igniting) return new ForceAtPoint(Vector3.zero, flightConditions.position);

        Vector3 direction = flightConditions.TransformWorldDir(tr.forward);
        Vector3 point = flightConditions.TransformWorldPos(tr.position);

        return new ForceAtPoint(Thrust * direction, point);
    }

    protected override void UpdatePowerAndRPS(float dt)
    {
        float targetRps = Mathf.Lerp(Preset.IdleRadPerSec, Preset.NominalRadPerSec, Throttle);
        float torque = Working ? Mathf.Max(Mathf.Abs(targetRps - RadPerSec) * torqueCoeff, minTorque) : friction;
        RadPerSec = Mathf.MoveTowards(RadPerSec, Working ? targetRps : 0f, torque * dt);

        Thrust = data.relativeDensity.Get * TrueThrottle * jetPreset.MaxThrust * structureDamage;
    }
    private void Update()
    {
        inlet.Rotate(Vector3.forward * (RadPerSec * Mathf.Rad2Deg * Time.deltaTime));
    }
    public override IEnumerator Ignition()
    {
        Igniting = true;

        OnIgnition?.Invoke(this);
        float timeCount = 0f;
        float startRps = RadPerSec;

        while (timeCount < Preset.IgnitionTime)
        {
            float previousRadPerSec = RadPerSec;

            RadPerSec = Mathf.Lerp(startRps, Preset.IdleRadPerSec, timeCount / Preset.IgnitionTime);
            timeCount += Time.deltaTime;

            float angularAcceleration = (RadPerSec - previousRadPerSec) / Time.deltaTime;


            yield return null;
        }

        Igniting = false;
    }
}
