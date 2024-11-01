using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Subsurface : ShapedAirframe
{
    public override float HpPerSquareMeter => ModulesHPData.controlHpPerSq;

    protected ShapedAirframe parentShapedFrame;
    protected IAirfoil airfoil;
    public override IAirfoil Airfoil => airfoil;

    public override void SetReferences(SofComplex _complex)
    {
        parentShapedFrame = transform.parent.GetComponentInParent<ShapedAirframe>(true);
        if(parentShapedFrame && airfoil == null) airfoil = parentShapedFrame.Airfoil;

        base.SetReferences(_complex);
    }
}
