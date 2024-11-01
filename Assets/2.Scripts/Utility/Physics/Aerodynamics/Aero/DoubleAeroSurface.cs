using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleAeroSurface : AeroSurface
{
    public SurfaceQuad secondQuad;
    public DoubleAeroSurface(SofAirframe _airframe, IAirfoil _airfoil, SurfaceQuad _quad, SurfaceQuad _secondQuad) : base(_airframe, _quad, _airfoil)
    {
        secondQuad = _secondQuad;
    }
    public override float Area()
    {
        return base.Area() + secondQuad.Area;
    }
}