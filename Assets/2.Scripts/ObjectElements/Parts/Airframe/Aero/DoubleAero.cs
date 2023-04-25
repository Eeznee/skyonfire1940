using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleAero : Aero
{
    public Quad secondQuad;
    public DoubleAero(AirframeBase _airframe, Airfoil _airfoil, Quad _quad, Quad _secondQuad) : base(_airframe, _quad, _airfoil)
    {
        secondQuad = _secondQuad;
    }
    public override float Area()
    {
        return base.Area() + secondQuad.Area;
    }
    public override float ApplyForces()
    {
        ApplyForcesSpecificQuad(secondQuad);
        return base.ApplyForces();
    }
}