using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComplexAeroSurface : AeroSurface
{
    private Wing wing;
    private WingSkin skin;

    private float controlSqrt;
    private float totalArea;

    private bool Overlaps(SofAirframe airframe)
    {
        airframe.UpdateAerofoil();
        return Quad.Overlap(quad, airframe.foilSurface.quad) > 0.5f;
    }
    public ComplexAeroSurface(SofAirframe _airframe, Quad _quad, Airfoil _airfoil) : base(_airframe, _quad, _airfoil)
    {
        wing = attachedAirframe.GetComponent<Wing>();
        totalArea = wing ? wing.EntireWingArea : attachedAirframe.area;
        if (wing) skin = wing.skin;
        Transform subSurfaceParent = wing ? wing.root.tr : tr;
        foreach (Slat s in subSurfaceParent.GetComponentsInChildren<Slat>()) if (Overlaps(s)) { slat = s; }
        foreach (Flap f in subSurfaceParent.GetComponentsInChildren<Flap>()) if (Overlaps(f)) { flap = f; }
        foreach (ControlSurface c in subSurfaceParent.GetComponentsInChildren<ControlSurface>()) if (Overlaps(c)) control = c;
        if (control)
        {
            control.UpdateAerofoil();
            controlSqrt = Mathf.Sqrt(control.foilSurface.quad.MidChord / quad.MidChord);
        }
    }
    public override Vector2 Coefficients(float aoa)
    {
        if (slat && !slat.ripped)
            aoa -= slat.extend * slat.aoaEffect * Mathf.InverseLerp(15f, 15f + slat.aoaEffect * 2f, aoa);

        bool hasFlaps = attachedAirframe.aircraft && flap && !flap.ripped;
        Vector2 coeffs = hasFlaps ? airfoil.Coefficients(aoa, attachedAirframe.aircraft.hydraulics.flaps.state) : airfoil.Coefficients(aoa);

        if (control && !control.ripped)
            coeffs.y += controlSqrt * airfoil.Gradient() * control.sinControlAngle;

        float wingSpan = attachedAirframe.aircraft ? attachedAirframe.aircraft.stats.wingSpan : 5f;
        if (wing) coeffs.x += coeffs.y * coeffs.y * totalArea * 2f / (wingSpan * wingSpan * Mathf.PI * wing.oswald);    //Induced Drag
        coeffs.x *= attachedAirframe.data.groundEffect.Get;

        return coeffs;
    }
    public override float Integrity()
    {
        if (skin) return skin.structureDamage;
        return base.Integrity();
    }
}