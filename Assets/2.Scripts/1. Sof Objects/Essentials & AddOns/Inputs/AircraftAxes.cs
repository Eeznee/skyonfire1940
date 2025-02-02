using System;
using UnityEngine;

[Serializable]
public struct AircraftAxes
{
    public override string ToString()
    {
        return "pitch : " + pitch.ToString("0.0") + " ; roll : " + roll.ToString("0.0") + " ; yaw : " + yaw.ToString("0.0");
    }

    public float pitch;
    public float roll;
    public float yaw;

    public static AircraftAxes operator *(AircraftAxes axes, float factor)
    {
        AircraftAxes final = axes;
        final.pitch *= factor;
        final.roll *= factor;
        final.yaw *= factor;
        return final;
    }
    public static AircraftAxes operator *(AircraftAxes a, AircraftAxes b)
    {
        AircraftAxes final = a;
        final.pitch *= b.pitch;
        final.roll *= b.roll;
        final.yaw *= b.yaw;
        return final;
    }
    public static AircraftAxes operator +(AircraftAxes a, AircraftAxes b)
    {
        AircraftAxes final = a;
        final.pitch += b.pitch;
        final.roll += b.roll;
        final.yaw += b.yaw;
        return final;
    }
    public static bool operator ==(AircraftAxes a, AircraftAxes b)
    {
        return a.pitch == b.pitch && a.roll == b.roll && a.yaw == b.yaw;
    }
    public static bool operator !=(AircraftAxes a, AircraftAxes b)
    {
        return !(a == b);
    }
    public override bool Equals(object obj)
    {
        if (obj is AircraftAxes)
        {
            return this == (AircraftAxes)obj;
        }
        return false;
    }
    public override int GetHashCode()
    {
        int hash = 3; // A non-zero constant prime number

        // Use prime numbers to combine hash codes of individual components
        hash = hash * 17 + pitch.GetHashCode(); // 23 is another prime number
        hash = hash * 17 + roll.GetHashCode();
        hash = hash * 17 + yaw.GetHashCode();

        return hash;
    }
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

    public static AircraftAxes MoveTowards(AircraftAxes start, AircraftAxes target, AircraftAxes speed, float deltaTime)
    {
        AircraftAxes final = new AircraftAxes();

        final.pitch = Mathf.MoveTowards(start.pitch, target.pitch, speed.pitch * deltaTime);
        final.roll = Mathf.MoveTowards(start.roll, target.roll, speed.roll * deltaTime);
        final.yaw = Mathf.MoveTowards(start.yaw, target.yaw, speed.yaw * deltaTime);

        final.Clamp();

        return final;
    }

    public void Set(AircraftAxis axis, float value)
    {
        switch (axis)
        {
            case AircraftAxis.Pitch: pitch = value; break;
            case AircraftAxis.Roll: roll = value; break;
            case AircraftAxis.Yaw: yaw = value; break;
        }
    }

    public float Get(AircraftAxis axis)
    {
        switch (axis)
        {
            case AircraftAxis.Pitch: return pitch;
            case AircraftAxis.Roll: return roll;
            case AircraftAxis.Yaw: return yaw;

            default: return 0f;
        }
    }
    public float Magnitude => Mathf.Sqrt(pitch * pitch + roll * roll + yaw * yaw);
    public float Sum => pitch + roll + yaw;
    public void Clamp()
    {
        pitch = Mathf.Clamp(pitch, -1f, 1f);
        roll = Mathf.Clamp(roll, -1f, 1f);
        yaw = Mathf.Clamp(yaw, -1f, 1f);
    }
    public void Clamp(float pitchClamp)
    {
        pitch = Mathf.Clamp(pitch, -pitchClamp, pitchClamp);
        roll = Mathf.Clamp(roll, -1f, 1f);
        yaw = Mathf.Clamp(yaw, -1f, 1f);
    }
    public void Clamp(AircraftAxes limits)
    {
        limits.pitch = Mathf.Abs(limits.pitch);
        limits.roll = Mathf.Abs(limits.roll);
        limits.yaw = Mathf.Abs(limits.yaw);

        pitch = Mathf.Clamp(pitch, -limits.pitch, limits.pitch);
        roll = Mathf.Clamp(roll, -limits.roll, limits.roll);
        yaw = Mathf.Clamp(yaw, -limits.yaw, limits.yaw);
    }
}

public enum AircraftAxis
{
    Pitch,
    Roll,
    Yaw
}