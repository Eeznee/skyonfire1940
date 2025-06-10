using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WingTipsTrails : AudioComponent
{
    public TrailRenderer trailReference;
    private TrailRenderer[] tipTrails;
    private List<Wing> wingTips;

    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);
        wingTips = new List<Wing>();
        foreach (Wing wing in transform.root.GetComponentsInChildren<Wing>()) if (!wing.child && wing.parent) wingTips.Add(wing);

        tipTrails = new TrailRenderer[wingTips.Count];
        for (int i = 0; i < wingTips.Count; i++)
        {
            Wing wing = wingTips[i];
            Vector3 tipPos = wing.quad.topAeroPos.WorldPos + tr.right * 0.1f;
            tipTrails[i] = Instantiate(trailReference, tipPos, wing.tr.rotation, wing.tr);
            tipTrails[i].emitting = false;
        }

        aircraft.OnUpdateLOD1 += UpdateWingTipsTrails;
    }

    private void UpdateWingTipsTrails()
    {
        for (int i = 0; i < wingTips.Count; i++)
        {
            if (!tipTrails[0]) continue;
            if (!tipTrails[0].isVisible) continue;

            Wing wing = wingTips[i];

            bool emitting = Player.aircraft;
            emitting &= wing && wing.tr.root == tr.root;
            emitting &= data.ias.Get > 20f;
            emitting &= wing.angleOfAttack * Mathf.Min(data.ias.Get * 0.02f, 1f) > wing.Airfoil.HighPeakAlpha * 0.8f;

            if (tipTrails[i] && tipTrails[i].emitting != emitting) tipTrails[i].emitting = emitting;
        }
    }
}
