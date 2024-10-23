using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Subsurface : ShapedAirframe
{
    public override float HpPerSquareMeter => ModulesHPData.controlHpPerSq;

    protected ShapedAirframe parentShapedFrame;
    public override void SetReferences(SofComplex _complex)
    {
        parentShapedFrame = transform.parent.GetComponent<ShapedAirframe>();
        base.SetReferences(_complex);
    }
    public override void UpdateAerofoil()
    {
        if(foil == null) foil = parentShapedFrame.foil;
        base.UpdateAerofoil();
    }
}
