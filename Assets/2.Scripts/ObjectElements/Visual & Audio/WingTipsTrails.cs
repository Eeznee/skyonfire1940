using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WingTipsTrails : AudioVisual
{
    public TrailRenderer trailReference;
    private TrailRenderer[] tipTrails;
    private List<Wing> wingTips;

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        if (firstTime)
        {
            wingTips = new List<Wing>();
            foreach (Wing wing in transform.root.GetComponentsInChildren<Wing>()) if (!wing.child && wing.parent) wingTips.Add(wing);

            tipTrails = new TrailRenderer[wingTips.Count];
            for (int i = 0; i < wingTips.Count; i++)
            {
                Wing wing = wingTips[i];
                Vector3 tipPos = (wing.split ? wing.splitAero : wing.aero).quad.TopAeroPos(true) + tr.right * 0.1f;
                tipTrails[i] = Instantiate(trailReference, tipPos, wing.tr.rotation, wing.tr);
                tipTrails[i].emitting = false;
            }
        }
    }

    private void Update()
    {
        for (int i = 0; i < wingTips.Count; i++)
        {
            Wing wing = wingTips[i];

            bool emitting = PlayerManager.player.aircraft;
            emitting &= data.ias.Get > 20f;
            emitting &= wing.alpha * Mathf.Min(data.ias.Get * 0.02f, 1f) > wing.foil.airfoilSim.maxAlpha * 0.8f;
            emitting &= complex.lod.LOD() <= 2;
            emitting &= wing && wing.tr.root == tr.root;
            if (tipTrails[i] && tipTrails[i].emitting != emitting) tipTrails[i].emitting = emitting;
        }
    }
}
