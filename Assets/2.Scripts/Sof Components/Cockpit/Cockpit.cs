using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cockpit : SofComponent
{
    public GameObject complexCockpit;

    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);
        sofModular.lod.OnSwitchEvent += SwitchMode;
    }

    void SwitchMode(int lod)
    {
        complexCockpit.SetActive(lod == 0);
    }
}
