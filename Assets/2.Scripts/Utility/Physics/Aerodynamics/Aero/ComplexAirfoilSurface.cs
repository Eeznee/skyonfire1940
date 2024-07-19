using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComplexAirfoilSurface : AirfoilSurface
{
    private Wing wing;
    private WingSkin skin;

    private float controlSqrt;
    private float totalArea;
    private float flapsInput;

    private bool Overlaps(SofAirframe airframe)
    {
        airframe.UpdateAerofoil();
        return Quad.Overlap(quad, airframe.foilSurface.quad) > 0.5f;
    }
    public ComplexAirfoilSurface(SofAirframe _airframe, Quad _quad, Airfoil _airfoil) : base(_airframe, _quad, _airfoil)
    {
        wing = airframe.GetComponent<Wing>();
        totalArea = wing ? wing.totalArea : airframe.area;
        if (wing) skin = wing.skin;
        Transform subSurfaceParent = wing ? wing.root.tr : tr;
        foreach (Slat s in subSurfaceParent.GetComponentsInChildren<Slat>()) if (Overlaps(s)) { slat = s; slat.foil = airframe.foil; }
        foreach (Flap f in subSurfaceParent.GetComponentsInChildren<Flap>()) if (Overlaps(f)) { flap = f; flap.foil = airframe.foil; }
        foreach (ControlSurface c in subSurfaceParent.GetComponentsInChildren<ControlSurface>()) if (Overlaps(c)) control = c;
        if (control)
        {
            control.foil = airframe.foil;
            control.UpdateAerofoil();
            controlSqrt = Mathf.Sqrt(control.foilSurface.quad.MidChord / quad.MidChord);
        }
    }
    public override Vector2 Coefficients(float aoa)
    {
        if (slat && !slat.ripped)
            aoa -= slat.extend * slat.aoaEffect * Mathf.InverseLerp(15f, 15f + slat.aoaEffect * 2f, aoa);

        if (airfoil)
        {
            bool hasFlaps = airframe.aircraft && flap && !flap.ripped;
            Vector2 coeffs = hasFlaps ? airfoil.Coefficients(aoa, airframe.aircraft.hydraulics.flaps.state) : airfoil.Coefficients(aoa);

            if (control && !control.ripped)
                coeffs.y += controlSqrt * airfoil.Gradient() * control.sinControlAngle;

            float wingSpan = airframe.aircraft ? airframe.aircraft.stats.wingSpan : 5f;
            if (wing) coeffs.x += coeffs.y * coeffs.y * totalArea * 2f / (wingSpan * wingSpan * Mathf.PI * wing.oswald);    //Induced Drag
            coeffs.x *= airframe.data.groundEffect.Get;

            return coeffs;
        }
        return base.Coefficients(aoa);
    }
    public override float Integrity()
    {
        if (skin) return skin.structureDamage;
        return base.Integrity();
    }
    public override float ApplyForces()
    {
        flapsInput = airframe.aircraft && airframe.aircraft.hydraulics.flaps ? airframe.aircraft.hydraulics.flaps.state : 0f;
        return base.ApplyForces();
    }
}