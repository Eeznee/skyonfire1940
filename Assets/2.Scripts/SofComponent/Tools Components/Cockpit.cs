using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cockpit : SofComponent
{
    public GameObject complexCockpit;

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        complex.lod.OnSwitchEvent += SwitchMode;
    }

    void SwitchMode(int lod)
    {
        complexCockpit.SetActive(lod == 0);
    }
}
