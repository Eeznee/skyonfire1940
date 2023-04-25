using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComplexAero : Aero
{
    private Wing wing;
    private WingSkin skin;

    private float controlSqrt;
    private float totalArea;
    private float flapsInput;

    private bool Overlaps(AirframeBase airframe)
    {
        airframe.CalculateAerofoilStructure();
        return Quad.Overlap(quad, airframe.aero.quad) > 0.5f;
    }
    public ComplexAero(AirframeBase _airframe, Quad _quad, Airfoil _airfoil) : base(_airframe, _quad, _airfoil)
    {
        wing = airframe.GetComponent<Wing>();
        totalArea = wing ? wing.totalArea : airframe.area;
        if (wing) skin = wing.skin;
        foreach (Slat s in tr.GetComponentsInChildren<Slat>()) if (Overlaps(s)) { slat = s; slat.foil = airframe.foil; }
        foreach (Flap f in tr.GetComponentsInChildren<Flap>()) if (Overlaps(f)) { flap = f; flap.foil = airframe.foil; }
        foreach (ControlSurface c in tr.GetComponentsInChildren<ControlSurface>()) if (Overlaps(c)) control = c;
        if (control)
        {
            control.foil = airframe.foil;
            control.CalculateAerofoilStructure();
            controlSqrt = Mathf.Sqrt(control.aero.quad.MidChord / quad.MidChord);
        }
    }
    public override Vector2 Coefficients(float aoa)
    {
        if (slat && !slat.ripped)
            aoa -= slat.extend * slat.aoaEffect * Mathf.InverseLerp(15f, 15f + slat.aoaEffect * 2f, aoa);

        if (airfoil)
        {
            bool hasFlaps = airframe.aircraft && flap && !flap.ripped;
            Vector2 coeffs = hasFlaps ? airfoil.Coefficients(aoa,airframe.aircraft.flaps.state) : airfoil.Coefficients(aoa);

            if (control && !control.ripped)
                coeffs.y += controlSqrt * airfoil.Gradient() * control.sinControlAngle;

            float wingSpan = airframe.aircraft ? airframe.aircraft.wingSpan : 5f;
            if (wing) coeffs.x += coeffs.y * coeffs.y * totalArea * 2f / (wingSpan * wingSpan * Mathf.PI * wing.oswald);    //Induced Drag
            coeffs.x *= airframe.data.groundEffect.Get;

            return coeffs;
        }
        return base.Coefficients(aoa);
    }
    public override float Integrity()
    {
        if (skin) return skin.Integrity;
        return base.Integrity();
    }
    public override float ApplyForces()
    {
        flapsInput = airframe.aircraft ? airframe.aircraft.flapsInput : 0f;
        return base.ApplyForces();
    }
}