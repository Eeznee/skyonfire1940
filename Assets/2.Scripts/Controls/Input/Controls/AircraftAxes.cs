using System;
using UnityEngine;

[Serializable]
public struct AircraftAxes
{
    public float pitch;
    public float roll;
    public float yaw;

    public static AircraftAxes zero { get { return new AircraftAxes(0f, 0f, 0f); } }

    public AircraftAxes(float _pitch, float _roll, float _yaw)
    {
        pitch = _pitch;
        roll = _roll;
        yaw = _yaw;
    }
    public AircraftAxes(Vector3 pitchYawRoll)
    {
        pitch = pitchYawRoll.x;
        yaw = pitchYawRoll.y;
        roll = pitchYawRoll.z;
    }
    public void CorrectPitch(SofAircraft aircraft, float target, float t)
    {
        ObjectData data = aircraft.data;

        float spd = Mathf.Max(data.tas.Get, 30f);
        AirfoilSim airfoil = aircraft.stats.airfoil.airfoilSim;
        float maxCl = Mathf.Lerp(-airfoil.minCl, airfoil.maxCl, target * 0.5f + 0.5f);
        float maxLift = maxCl * data.density.Get * Mathv.SmoothStart(spd, 2) * aircraft.stats.wingsArea * 0.45f;
        float gravityEffect = Physics.gravity.y * aircraft.tr.up.y * target;

        float maxTurnRate = (maxLift / aircraft.rb.mass + gravityEffect) / spd;

        float turnRateState = data.turnRate.Get / maxTurnRate;

        float inAirError = target - turnRateState;
        pitch = Mathf.Clamp(aircraft.pidElevator.UpdateAndDebugUnclamped(inAirError, t), -1f, 1f);
    }

    public void MoveTowards(AircraftAxes target, AircraftAxes speed, float dt)
    {
        pitch = Mathf.MoveTowards(pitch, target.pitch, speed.pitch * dt);
        roll = Mathf.MoveTowards(roll, target.roll, speed.roll * dt);
        yaw = Mathf.MoveTowards(yaw, target.yaw, speed.yaw * dt);
    }
}
