using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Propeller : SofModule, IMassComponent, IAircraftForce
{
    public float BladeEfficiency()
    {
        return PowerToThrust(RadPerSec, data.signedTas.Get, BladeAngle) * data.signedTas.Get;
    }
    public float PowerToThrust(float radPerSec, float signedTas, float bladeAngle)
    {
        float bladeAdvanceRatio = AdvanceRatio(bladeAngle);
        Vector2 bladeParams = EfficiencyParameters(bladeAdvanceRatio);

        return PowerToThrust(radPerSec, signedTas, bladeParams);
    }
    public float PowerToThrust(float radPerSec, float signedTas, Vector2 efficiencyParameters)
    {
        if (radPerSec == 0f) return 0f;
        float advanceRatio = AdvanceRatio(radPerSec, signedTas);
        float AR = advanceRatio / functionOverallFactor;

        float value = (efficiencyParameters.y - AR) / (AR * AR + efficiencyParameters.x);
        value = Mathv.CubicRoot(value) / functionOverallFactor;

        //supposed to be multiplied by advanced ratio / tas, but this causes issues when tas = 0
        float powerToThrust = value / (radPerSec * radius);
        return powerToThrust;
    }

    public float TheoricalPowerToThrust(float radPerSec, float signedTas)
    {
        if (radPerSec == 0f) return 0f;

        float advanceRatio = AdvanceRatio(radPerSec, signedTas);
        float bladeAngle = OptimalBladeAngle(advanceRatio);
        switch (pitchControl)
        {
            case PitchControl.Fixed: return PowerToThrust(radPerSec, signedTas, maxPitch);

            case PitchControl.TwoPitch:
                if (bladeAngle < minPitch) return PowerToThrust(radPerSec, signedTas, minPitch);
                if (bladeAngle > maxPitch) return PowerToThrust(radPerSec, signedTas, maxPitch);
                return Mathf.Max(PowerToThrust(radPerSec, signedTas, minPitch), PowerToThrust(radPerSec, signedTas, maxPitch));

            case PitchControl.ConstantSpeed:
                if (bladeAngle < minPitch) return PowerToThrust(radPerSec, signedTas, minPitch);
                if (bladeAngle > maxPitch) return PowerToThrust(radPerSec, signedTas, maxPitch);
                return PowerToThrust(radPerSec, signedTas, bladeAngle);

            default: return PowerToThrust(radPerSec, signedTas, bladeAngle);
        }
    }


    const float functionOverallFactor = 0.08f;
    public static Vector2 EfficiencyParameters(float advanceRatio)
    {
        float AR = advanceRatio / functionOverallFactor;

        float xa = AR + Mathf.Exp(-AR);
        float a = (M.Pow(xa, 4) - xa * xa) / 3f + 2f;
        float b = 4f / 3f * AR + 2f / (3f * (Mathf.Abs(AR) + 1f));

        return new Vector2(a, b);
    }

    public float EngineDrivenThrust()
    {
        float rpsFactor = Mathf.Clamp01(1.5f * engine.RadPerSec / engine.Preset.NominalRadPerSec);
        return engine.BrakePower * PowerToThrust(RadPerSec, data.signedTas.Get, BladeAngle) * efficiency * rpsFactor;
    }

    public float AdvanceRatio() { return AdvanceRatio(RadPerSec, data.signedTas.Get); }
    public float AdvanceRatio(float radPerSec, float signedTas)
    {
        if (radPerSec == 0f) return 0f;

        return signedTas / (radPerSec * radius);
    }
    public float AdvanceRatio(float bladeAngleDeg) { return Mathf.Tan(bladeAngleDeg * Mathf.Deg2Rad); }
    public float OptimalBladeAngle(float advanceRatio) { return Mathf.Atan(advanceRatio) * Mathf.Rad2Deg; }
    public float OptimalBladeAngle(float radPerSec, float signedTas) { return OptimalBladeAngle(AdvanceRatio(radPerSec, signedTas)); }

    public float BladeSpeedSquared(float radPerSec, float signedTas)
    {
        return new Vector2(radPerSec * radius, signedTas).sqrMagnitude;// new Vector2(radPerSec * radius, tas).sqrMagnitude;
    }

    public float AirSpeed(float advanceRatio, float radPerSec)
    {
        if (radPerSec == 0f) return 0f;

        return advanceRatio * radPerSec * radius;
    }

    public float TwoPitchAdvanceRatioTrigger()
    {
        float nominalRps = engine.PistonPreset.NominalRadPerSec * ReductionGear;
        float airSpeed = AirSpeed(AdvanceRatio(minPitch), nominalRps);
        float maxAirspeed = AirSpeed(AdvanceRatio(maxPitch), nominalRps);
        while (airSpeed < maxAirspeed)
        {
            float minPitchEfficiency = PowerToThrust(nominalRps, airSpeed, minPitch);
            float maxPitchEfficiency = PowerToThrust(nominalRps, airSpeed, maxPitch);

            if (maxPitchEfficiency > minPitchEfficiency) return AdvanceRatio(nominalRps, airSpeed);

            airSpeed += 0.5f;
        }
        return AdvanceRatio(nominalRps, maxAirspeed);
    }


    //TORQUE
    private void ComputeTorqueCoefficent()
    {
        PistonEnginePreset preset = engine.PistonPreset;
        float engineRps = engine.Preset.NominalRadPerSec;
        float powerAtOptimalConditions = preset.Power(0, EngineRunMode.Continuous, 0f, engineRps);
        float torqueAtOptimalConditions = engine.Torque(powerAtOptimalConditions, engineRps) * engine.PistonPreset.PropellerReductionGear;
        float speedSquared = BladeSpeedSquared(engineRps * ReductionGear, aircraft.stats.altitudeZeroMaxSpeed);

        DragCoefficient = torqueAtOptimalConditions / (speedSquared * radius);
    }
    public float CurrentMotionTorque()
    {
        return MotionTorque(BladeAngle, RadPerSec, data.tas.Get);
    }
    public float MotionTorque(float bladeAngle, float radPerSec, float signedTas)
    {
        float optimalBladeForGivenAdvanceRatio = OptimalBladeAngle(radPerSec, signedTas);
        float bladeCoeff = TorqueBladeCoefficient(bladeAngle, optimalBladeForGivenAdvanceRatio);
        float cd = DragCoefficient * bladeCoeff;

        return -BladeSpeedSquared(radPerSec, data.tas.Get) * cd * radius / engine.PistonPreset.PropellerReductionGear;
    }
    private float TorqueBladeCoefficient(float currentAlpha, float optimalAlpha)
    {
        if (currentAlpha < optimalAlpha)
        {
            float reverseEfficiency = 1f - M.Pow(optimalAlpha / 90f - 1f, 2);
            return (currentAlpha - optimalAlpha) * 0.04f * reverseEfficiency + 1f;
        }
        else return (currentAlpha - optimalAlpha) * 0.1f + 1f;
    }
}
