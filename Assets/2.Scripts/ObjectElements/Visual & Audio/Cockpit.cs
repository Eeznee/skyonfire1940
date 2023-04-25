using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cockpit : ObjectElement
{
    public GameObject complexCockpit;
    public Material noCockpitGlass;
    Material originalGlass;
    public Renderer[] glass;
    public Renderer[] internals;

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        if (firstTime)
        {
            originalGlass = glass[0].sharedMaterial;
            complex.lod.OnSwitchEvent += SwitchMode;
        }
    }

    void SwitchMode(int lod)
    {
        foreach (Renderer mr in glass)
        {
            if (!mr) continue;
            mr.sharedMaterial = lod >= 2 ? noCockpitGlass : originalGlass;
            mr.gameObject.layer = lod >= 2 ? 0 : 1;
        }
        foreach (Renderer mr in internals) if (mr && mr.transform.root == transform.root) mr.enabled = lod <= 1;
        complexCockpit.SetActive(lod == 0);
    }
}
