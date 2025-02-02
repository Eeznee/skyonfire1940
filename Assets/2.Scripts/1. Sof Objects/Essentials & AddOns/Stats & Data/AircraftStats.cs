using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AircraftStats
{
    public SofAircraft aircraft { get; private set; }
    public Wing rootWing { get; private set; }
    public Airfoil airfoil { get; private set; }
    public float wingSpan { get; private set; }
    public float wingsArea { get; private set; }
    public float wingsIncidence { get; private set; }
    public float altitudeZeroMaxSpeed { get; private set; }
    public float wingLoading { get; private set; }
    public float totalAreaCd { get; private set; }


    private float[] maxSpeedsAt1000Intervals;

    private float rollRateSpeedCoefficent;
    private float turningRadiusCoefficent;
    private float negTurningRadiusCoefficent;

    public float TurningRadius => turningRadiusCoefficent * aircraft.rb.mass;
    public float NegTurningRadius => negTurningRadiusCoefficent * aircraft.rb.mass;
    public float MaxTurnRate => 360f * aircraft.data.gsp.Get / (TurningRadius * 2f * Mathf.PI);
    public float MaxNegTurnRate => 360f * aircraft.data.gsp.Get / (NegTurningRadius * 2f * Mathf.PI);


    public AircraftStats(SofAircraft _aircraft)
    {
        aircraft = _aircraft;

        ComputeWingStats();
        ComputeMinTakeOffSpeed();
        ComputeTotalAreaCd();
        ComputeRollRate();
        ComputeTurningRadius();

        maxSpeedsAt1000Intervals = new float[10];
        for (int i = 0; i < maxSpeedsAt1000Intervals.Length; i++)
            maxSpeedsAt1000Intervals[i] = MaxSpeed(i * 1000f, 1f);

        altitudeZeroMaxSpeed = MaxSpeed(0f, 1f);
    }
    private void ComputeWingStats()
    {
        Wing[] wings = aircraft.GetComponentsInChildren<Wing>();
        wingsArea = 0f;
        wingSpan = 0f;
        foreach (Wing wing in wings)
        {
            if (!wing.parent)
            {
                wingsArea += wing.EntireWingArea;
                rootWing = wing;
            }
            if (!wing.child)
            {
                Vector3 tipPos = wing.quad.topAeroPos.WorldPos;
                wingSpan = Mathf.Max(aircraft.tr.InverseTransformPoint(tipPos).x * 2f, wingSpan);
            }
        }

        wingLoading = aircraft.LoadedMass.mass / wingsArea;
        airfoil = rootWing.airfoil;
        wingsIncidence = rootWing.shape.incidence;
    }
    private void ComputeRollRate()
    {
        Wing[] wings = aircraft.GetComponentsInChildren<Wing>();
        float totalWingsCoeff = 0f;
        float totalAileronStrength = 0f;
        AircraftAxes fullRoll = new AircraftAxes(0f, 1f, 0f);

        foreach (Wing wing in wings)
        {
            Vector3 worldAeroCenter = wing.quad.centerAero.WorldPos;
            float distanceFromCog = aircraft.tr.InverseTransformPoint(worldAeroCenter).x;
            distanceFromCog = Mathf.Abs(distanceFromCog);

            float airfoilGradient = wing.airfoil.Gradient();

            float coeff = airfoilGradient * wing.area * distanceFromCog * distanceFromCog;
            totalWingsCoeff += coeff;

            float aileronStrength = Mathf.Abs(wing.ControlSurfaceEffect(fullRoll));
            totalAileronStrength += aileronStrength * airfoilGradient * wing.area * distanceFromCog;
        }

        totalWingsCoeff /= wingSpan * 0.5f;

        float equilibrumRollAngle = totalAileronStrength / totalWingsCoeff;
        rollRateSpeedCoefficent = equilibrumRollAngle / (0.5f * wingSpan);
    }
    private void ComputeTurningRadius()
    {
        float cl = airfoil.Coefficients(airfoil.PeakAlpha() - 3f).y;
        float liftForce = cl * 0.5f * aircraft.data.density.Get * wingsArea;

        float negCl = airfoil.Coefficients(airfoil.LowAlpha() + 2f).y;
        float negLiftForce = negCl * 0.5f * aircraft.data.density.Get * wingsArea;

        turningRadiusCoefficent = 1f / liftForce;
        negTurningRadiusCoefficent = Mathf.Abs(1f / negLiftForce);
    }

    private void ComputeMinTakeOffSpeed()
    {
        //TODO compute min takeoffspeed
        
        //float weight = aircraft.rb.mass * -Physics.gravity.y;
        //float density = Aerodynamics.seaLvlDensity;

        airfoil?.Coefficients(10f);
    }
    private void ComputeTotalAreaCd()
    {
        totalAreaCd = 0f;
        foreach (SofAirframe airframe in aircraft.GetComponentsInChildren<SofAirframe>())
        {
            totalAreaCd += airframe.AreaCd();
        }

    }
    public float MaxSpeed(float altitude, float throttle)
    {
        Engine[] engines = aircraft.GetComponentsInChildren<Engine>();
        EnginePreset preset = engines[0].Preset;
        float totalDrag = totalAreaCd * 0.5f * Aerodynamics.GetAirDensity(altitude);
        bool jet = preset.type == EnginePreset.Type.Jet;

        if (jet)
        {
            float relativeAirDensity = Aerodynamics.GetAirDensity(altitude) * Aerodynamics.invertSeaLvlDensity;
            float thrust = throttle * preset.maxThrust * engines.Length * relativeAirDensity;
            return Mathf.Sqrt(thrust / totalDrag);
        }
        else
        {
            float engineMaxThrust = preset.gear1.Evaluate(altitude) * 745.7f * aircraft.GetComponentInChildren<Propeller>().efficiency;
            float totalThrust = engineMaxThrust * throttle * engines.Length;
            return Mathf.Pow(totalThrust / totalDrag, 0.333333f);
        }
    }

    public float RollRateCurrentSpeed()
    {
        return rollRateSpeedCoefficent * aircraft.data.tas.Get;
    }
    public float RollRate(float TAS)
    {
        return rollRateSpeedCoefficent * TAS;
    }

}
