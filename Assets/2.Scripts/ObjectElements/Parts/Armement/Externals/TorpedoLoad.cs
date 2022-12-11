using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorpedoLoad : OrdnanceLoad
{
    public bool bombBay;
    public Bomb bombRef;

    private Bomb[] bombs;

    public override float SingleMass()
    {
        return bombRef.Mass();
    }


    public override void ReloadOrdnance()
    {
        base.ReloadOrdnance();

        foreach (Bomb b in bombs) if (b != null && b.transform.root == transform.root) Destroy(b.gameObject);

        bombs = new Bomb[launchPositions.Length];

        for (int i = 0; i < launchPositions.Length; i++)
        {
            Vector3 pos = transform.TransformPoint(launchPositions[i]);
            bombs[i] = Instantiate(bombRef, pos, transform.rotation, transform);
            bombs[i].Initialize(data, true);
        }
        fireIndex = launchPositions.Length - 1;
    }

    public override bool Launch(float delayFuse)
    {
        if (fireIndex < 0) return false;
        if (bombBay && aircraft.bombBay.state < 1f) return false;

        base.Launch(delayFuse);
        bombs[fireIndex].Drop(5f, bombBay);

        return true;
    }
}
