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

    void Start()
    {
        originalGlass = glass[0].material;
    }

    void SwitchMode(int mode)
    {
        foreach (Renderer mr in glass)
        {
            mr.sharedMaterial = mode >= 2 ? noCockpitGlass : originalGlass;
            mr.gameObject.layer = mode >= 2 ? 0 : 1;
        }
        foreach (Renderer mr in internals) if (mr && mr.transform.root == transform.root) mr.enabled = mode <= 1;
        complexCockpit.SetActive(mode == 0);
    }

    void Update()
    {
        if (complex.lod.Switched())
        {
            SwitchMode(complex.lod.LOD());
        }
    }
}
