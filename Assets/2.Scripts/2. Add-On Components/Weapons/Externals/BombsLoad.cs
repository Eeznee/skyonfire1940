using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[AddComponentMenu("Sof Components/Weapons/Heavy Ordnance/Bombs Load")]
public class BombsLoad : OrdnanceLoad
{
    public bool bombBay;
    public Bomb bombRef;

    private Bomb[] bombs;

    public override float SingleMass => bombRef ? bombRef.mass : base.SingleMass;
    protected override void Clear()
    {
        base.Clear();
        if (bombs == null) return;
        foreach (Bomb b in bombs) if (b != null && b.transform.root == transform.root) Destroy(b.gameObject);
    }
    public override void Rearm()
    {
        base.Rearm();

        bombs = new Bomb[launchPositions.Length];

        for (int i = 0; i < launchPositions.Length; i++)
        {
            Vector3 pos = transform.TransformPoint(launchPositions[i]);
            bombs[i] = Instantiate(bombRef, pos, transform.rotation, transform);
            bombs[i].SetInstanciatedComponent(sofModular);
        }
    }

    public override bool Launch(float delayFuse)
    {
        if (fireIndex >= launchPositions.Length) return false;
        if (bombBay && aircraft.hydraulics.bombBay.state < 1f) return false;

        bombs[fireIndex].Drop();

        base.Launch(delayFuse);

        return true;
    }
}