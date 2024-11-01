using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComplexAeroSurface
{
    protected Transform tr;
    protected MainSurface surface;

    public ControlSurface control;
    public Flap flap;
    public Slat slat;

    public SurfaceQuad quad;

    private Wing wing;

    private float controlSqrt;
    private float totalArea;

    private bool Overlaps(SofAirframe airframe)
    {
        airframe.UpdateQuad();
        return SurfaceQuad.Overlap(quad, airframe.quad) > 0.5f;
    }
    public ComplexAeroSurface(MainSurface _surface, SurfaceQuad _quad)
    {
        surface = _surface;
        tr = _quad.tr;
        quad = _quad;

        wing = surface.GetComponent<Wing>();
        totalArea = wing ? wing.EntireWingArea : surface.area;
        Transform subSurfaceParent = wing ? wing.root.tr : tr;
        foreach (Slat s in subSurfaceParent.GetComponentsInChildren<Slat>()) if (Overlaps(s)) { slat = s; }
        foreach (Flap f in subSurfaceParent.GetComponentsInChildren<Flap>()) if (Overlaps(f)) { flap = f; }
        foreach (ControlSurface c in subSurfaceParent.GetComponentsInChildren<ControlSurface>()) if (Overlaps(c)) control = c;
        if (control)
            controlSqrt = Mathf.Sqrt(control.quad.MidChord / quad.MidChord);
    }
    public Vector2 Coefficients(float aoa)
    {
        SofAircraft aircraft = surface.aircraft;
        IAirfoil airfoil = surface.Airfoil;

        if (slat && !slat.ripped)
            aoa -= slat.extend * slat.aoaEffect * Mathf.InverseLerp(15f, 15f + slat.aoaEffect * 2f, aoa);

        if (control && !control.ripped)
            aoa += controlSqrt * control.sinControlAngle * Mathf.Rad2Deg;

        bool hasFlaps = aircraft && flap && !flap.ripped;
        Vector2 coeffs = hasFlaps ? airfoil.Coefficients(aoa, aircraft.hydraulics.flaps.state) : airfoil.Coefficients(aoa);

        //if (control && !control.ripped)
        //    coeffs.y += controlSqrt * airfoil.Gradient() * control.sinControlAngle;

        float wingSpan = aircraft ? aircraft.stats.wingSpan : 5f;
        if (wing) coeffs.x += coeffs.y * coeffs.y * totalArea * 2f / (wingSpan * wingSpan * Mathf.PI * wing.oswald);    //Induced Drag
        if(aircraft) coeffs.x *= aircraft.data.groundEffect.Get;

        return coeffs;
    }
}