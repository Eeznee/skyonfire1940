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
    public Transform inlet;
    public float thrust;
    const float torqueCoeff = 0.2f;
    const float minTorque = 60f;
    const float friction = 10f;


    public override float MinimumRps => Preset.idleRPS * 0.8f;
    public override float ConsumptionRate => Preset.ConsumptionRate(Throttle ,thrust);


    public ForceAtPoint SimulatePointForce(FlightConditions flightConditions)
    {
        if (igniting) return new ForceAtPoint(Vector3.zero, flightConditions.position);

        Vector3 direction = flightConditions.TransformWorldDir(tr.forward);
        Vector3 point = flightConditions.TransformWorldPos(tr.position);

        return new ForceAtPoint(thrust * direction, point);
    }

    protected override void UpdatePowerAndRPS(float dt)
    {
        float targetRps = Mathf.Lerp(Preset.idleRPS, Preset.fullRps, Throttle);
        float torque = workingAndRunning ? Mathf.Max(Mathf.Abs(targetRps - radiansPerSeconds) * torqueCoeff, minTorque) : friction;
        radiansPerSeconds = Mathf.MoveTowards(radiansPerSeconds, workingAndRunning ? targetRps : 0f, torque * dt);

        thrust = data.relativeDensity.Get * Throttle.TrueThrottle * Preset.maxThrust;
    }
    private void Update()
    {
        inlet.Rotate(Vector3.forward * (radiansPerSeconds * Mathf.Rad2Deg * Time.deltaTime));
    }
}
