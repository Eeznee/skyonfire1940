using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Subsurface : ShapedAirframe
{
    public override float HpPerSquareMeter => ModulesHPData.controlHpPerSq;

    protected MainSurface parentSurface;
    protected IAirfoil airfoil;
    public override IAirfoil Airfoil => airfoil;

    public MainSurface Parent => parentSurface;


    public override void SetReferences(SofComplex _complex)
    {
        parentSurface = transform.parent.GetComponentInParent<MainSurface>(true);
        if(parentSurface && airfoil == null) airfoil = parentSurface.Airfoil;

        base.SetReferences(_complex);
    }
#if UNITY_EDITOR
    protected override bool ShowGUI => SofWindow.showWingsOverlay;
#endif
}
