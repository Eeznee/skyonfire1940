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
    public float altitudeZeroMaxSpeed { get; private set; }
    public float wingLoading { get; private set; }
    public float totalAreaCd { get; private set; }


    private float[] maxSpeedsAt1000Intervals;


    public AircraftStats(SofAircraft _aircraft)
    {
        aircraft = _aircraft;

        ComputeWingStats();
        ComputeMinTakeOffSpeed();
        ComputeTotalAreaCd();

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
                Vector3 tipPos = wing.OuterQuad.TopAeroPos(true) + wing.tr.right * 0.1f;
                wingSpan = Mathf.Max(aircraft.tr.InverseTransformPoint(tipPos).x * 2f, wingSpan);
            }
        }

        wingLoading = aircraft.LoadedMass.mass / wingsArea;
        airfoil = rootWing.airfoil;
    }
    private void ComputeMinTakeOffSpeed()
    {
        //float weight = aircraft.rb.mass * -Physics.gravity.y;
        //float density = Aerodynamics.seaLvlDensity;

        airfoil?.Coefficients(10f);
    }
    private void ComputeTotalAreaCd()
    {
        totalAreaCd = 0f;
        foreach (SofAirframe airframe in aircraft.GetComponentsInChildren<SofAirframe>())
        {
            //Debug.Log(aircraft.name + "  " + airframe.name + "  " +airframe.AreaCd());
            totalAreaCd += airframe.AreaCd();
        }

    }
    public float MaxSpeed(float altitude, float throttle)
    {
        Engine[] engines = aircraft.GetComponentsInChildren<Engine>();
        float totalDrag = totalAreaCd * 0.5f * Aerodynamics.GetAirDensity(altitude);
        bool jet = engines[0].preset.type == EnginePreset.Type.Jet;
        if (jet)
        {
            float relativeAirDensity = Aerodynamics.GetAirDensity(altitude) * Aerodynamics.invertSeaLvlDensity;
            float thrust = throttle * engines[0].preset.maxThrust * engines.Length * relativeAirDensity;
            return Mathf.Sqrt(thrust / totalDrag);
        }
        else
        {
            float totalThrust = throttle * engines.Length * engines[0].preset.gear1.Evaluate(altitude) * 745.7f * aircraft.GetComponentInChildren<Propeller>().efficiency;
            return Mathf.Pow(totalThrust / totalDrag, 0.333333f);
        }
    }
}
