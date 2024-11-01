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
    public void CorrectPitch(SofAircraft aircraft, float target, float dt)
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

        pitch = Mathf.Clamp(aircraft.pidElevator.UpdateUnclamped(inAirError, dt), -1f, 1f);
    }


    public void CorrectPitcher(SofAircraft aircraft, float targetAoA)
    {
        Wing[] wings = aircraft.GetComponentsInChildren<Wing>();
        float cl = wings[0].Coefficients(targetAoA).y;
        float wingsAreaCl = cl * aircraft.stats.wingsArea;

        float d = aircraft.rb.centerOfMass.z - aircraft.tr.InverseTransformPoint(wings[0].quad.CenterAero(true)).z;
        float torque = wingsAreaCl * d;

        Debug.Log(targetAoA);
    }

    public void MoveTowards(AircraftAxes target, AircraftAxes speed, float dt)
    {

        TonedMovedForward(ref pitch, target.pitch, speed.pitch, dt);
        roll = Mathf.MoveTowards(roll, target.roll, speed.roll * dt);
        TonedMovedForward(ref yaw, target.yaw, speed.yaw, dt);
    }
    private void TonedMovedForward(ref float axis,float target, float speed, float dt)
    {
        float tonedSpeed = Mathf.Lerp(speed * 0.1f, speed , Mathf.Abs(axis - target));

        axis = Mathf.MoveTowards(axis, target, tonedSpeed * dt);
    }
}
