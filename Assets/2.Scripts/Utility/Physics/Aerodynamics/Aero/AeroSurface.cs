using System.Collections.Generic;
using UnityEngine;



public class AeroSurface
{
    protected Transform tr;
    protected SofAirframe attachedAirframe;
    protected IAirfoil airfoil;

    public ControlSurface control;
    public Flap flap;
    public Slat slat;

    public SurfaceQuad quad;

    public AeroSurface(SofAirframe _airframe, SurfaceQuad _quad, IAirfoil _airfoil)
    {
        attachedAirframe = _airframe;
        tr = _quad.tr;
        quad = _quad;
        airfoil = _airfoil;

        if (airfoil == null) Debug.LogError("A null airfoil was sent to this ");
    }

    public virtual float Area()
    {
        return quad.Area;
    }
    public virtual Vector2 Coefficients(float aoa)
    {
        return airfoil.Coefficients(aoa);
    }
    public virtual float Integrity()
    {
        return attachedAirframe.structureDamage;
    }
}