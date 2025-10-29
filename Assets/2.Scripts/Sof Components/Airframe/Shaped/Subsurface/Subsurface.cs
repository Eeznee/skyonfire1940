using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Subsurface : ShapedAirframe
{
    public override float HpPerSquareMeter => ModulesHPData.controlHpPerSq;

    protected MainSurface parentSurface;
    public override IAirfoil Airfoil => virtualAirfoil;

    public MainSurface Parent => parentSurface;


    public override void SetReferences(SofModular _complex)
    {
        parentSurface = transform.parent.GetComponentInParent<MainSurface>(true);
        if(parentSurface && virtualAirfoil == null) virtualAirfoil = parentSurface.Airfoil;

        base.SetReferences(_complex);
    }
#if UNITY_EDITOR
    protected override bool ShowGUI => SofWindow.showWingsOverlay;
#endif
}
