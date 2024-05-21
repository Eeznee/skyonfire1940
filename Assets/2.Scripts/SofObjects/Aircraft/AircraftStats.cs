using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AircraftStats
{
    public float wingSpan;
    public float wingsArea;
    public Wing rootWing;
    public Airfoil airfoil;

    public AircraftStats(SofAircraft aircraft)
    {
        Wing[] wings = aircraft.GetComponentsInChildren<Wing>();
        wingsArea = 0f;
        wingSpan = 0f;
        foreach (Wing wing in wings)
        {
            if (!wing.parent)
            {
                wingsArea += wing.totalArea;
                rootWing = wing;
            }
            if (!wing.child)
            {
                Vector3 tipPos = (wing.split ? wing.splitFoilSurface : wing.foilSurface).quad.TopAeroPos(true) + wing.tr.right * 0.1f;
                wingSpan = Mathf.Max(aircraft.tr.InverseTransformPoint(tipPos).x * 2f, wingSpan) ;
            }
        }
        airfoil = rootWing.foil;
    }

    public float TotalAreaCd(SofAircraft aircraft)
    {
        float areaCd = 0f;
        foreach (SofAirframe airframe in aircraft.GetComponentsInChildren<SofAirframe>())
            areaCd += airframe.AreaCd();
        return areaCd;
    }
    public float MaxSpeed(SofAircraft aircraft, float totalAreaCd, float altitude, float throttle)
    {
        Engine[] engines = aircraft.engines.all;
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
            float totalPower = throttle * engines.Length * engines[0].preset.gear1.Evaluate(altitude) * 745.7f * aircraft.GetComponentInChildren<Propeller>().efficiency;
            return Mathf.Pow(totalPower / totalDrag, 0.333333f);
        }
    }
}
