using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagazineStock : ObjectElement
{
    private List<Magazine> mags = new List<Magazine>(0);
    private bool show = true;
    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        mags = new List<Magazine>(GetComponentsInChildren<Magazine>());
        foreach(Magazine mag in mags)
        {
            mag.attachedStock = this;
        }
    }
    public int MagsCount()
    {
        return mags.Count;
    }
    public Magazine GetMag()
    {
        if (mags.Count == 0) return null;
        int rand = Random.Range(0, mags.Count);
        Magazine mag = mags[rand];
        mags.RemoveAt(rand);
        return mag;
    }
    public Magazine GetMag(Magazine mag)
    {
        if (mags.Remove(mag)) return mag;
        return null;
    }


    void SwitchMode(int mode)
    {
        foreach (Magazine mag in mags)
        {
            mag.rend.enabled = mode == 0;
        }
    }

    void Update()
    {
        if (complex.lod.Switched())
        {
            SwitchMode(complex.lod.LOD());
        }
    }
}
