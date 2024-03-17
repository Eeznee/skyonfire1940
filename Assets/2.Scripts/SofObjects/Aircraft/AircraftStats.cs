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
                Vector3 tipPos = (wing.split ? wing.splitAero : wing.aero).quad.TopAeroPos(true) + wing.tr.right * 0.1f;
                wingSpan = Mathf.Max(aircraft.tr.InverseTransformPoint(tipPos).x * 2f, wingSpan) ;
            }
        }
        airfoil = rootWing.foil;
    }
}
