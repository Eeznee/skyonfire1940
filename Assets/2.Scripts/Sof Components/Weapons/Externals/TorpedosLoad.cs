using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Sof Components/Weapons/Heavy Ordnance/Torpedos Load")]
public class TorpedosLoad : OrdnanceLoad
{
    public bool bombBay;
    public Torpedo torpedoRef;

    private Torpedo[] torpedos;

    public override float SingleMass => torpedoRef? torpedoRef.mass : base.SingleMass;
    protected override void Clear()
    {
        base.Clear();
        if (torpedos == null) return;
        foreach (Torpedo t in torpedos) if (t != null && t.transform.root == transform.root) Destroy(t.gameObject);
    }
    public override void Rearm()
    {
        base.Rearm();

        torpedos = new Torpedo[launchPositions.Length];

        for (int i = 0; i < launchPositions.Length; i++)
        {
            Vector3 pos = transform.TransformPoint(launchPositions[i]);
            torpedos[i] = Instantiate(torpedoRef, pos, transform.rotation, transform);
            torpedos[i].SetInstanciatedComponent(sofModular);
        }
    }

    public override bool CanLaunch
    {
        get
        {
            if (bombBay && aircraft.hydraulics.bombBay.state < 1f) return false;
            return base.CanLaunch;
        }
    }

    public override void Launch(float delayFuse)
    {
        torpedos[fireIndex].Drop();

        base.Launch(delayFuse);

        if(aircraft == Player.aircraft)
        {
            foreach(SofAircraft squadAircraft in GameManager.squadrons[aircraft.SquadronId])
            {
                if (squadAircraft == Player.aircraft) continue;
                squadAircraft.armament.DropTorpedo();
            }
        }
    }
}