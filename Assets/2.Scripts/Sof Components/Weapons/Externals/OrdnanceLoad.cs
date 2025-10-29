using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class OrdnanceLoad : SofComponent, IMassComponent
{
    public float RealMass => SingleMass * Mathf.Max(launchPositions.Length - fireIndex);
    public float LoadedMass => SingleMass * launchPositions.Length;
    public float EmptyMass => 0f;
    public virtual float SingleMass => 0f;

    public Vector3[] launchPositions;
    public AudioClip[] launchClips;

    [HideInInspector] public OrdnanceLoad pairedOrdnanceLaunch;
    [HideInInspector] public int priority;

    private float ordnanceMass = 0f;
    protected int fireIndex;


    public Action OnOrdnanceLaunched;


    public void InterpolatePositions()
    {
        Vector3 first = launchPositions[0];
        Vector3 last = launchPositions[launchPositions.Length - 1];
        for (int i = 0; i < launchPositions.Length; i++)
        {
            launchPositions[i] = Vector3.Lerp(first, last, (float)i / (launchPositions.Length - 1));
        }
    }

    protected virtual void Clear()
    {
        fireIndex = launchPositions.Length;
    }
    public override void Rearm()
    {
        base.Rearm();
        Clear();
        fireIndex = 0;
        ordnanceMass = SingleMass * launchPositions.Length;
    }

    public virtual void Launch(float delayFuse)
    {
        //Mass
        Mass mass = new Mass(SingleMass, localPos + launchPositions[fireIndex]);
        if (sofComplex) sofComplex.ShiftMass(-mass);
        ordnanceMass -= SingleMass;

        //Audio
        if (launchClips.Length > 0) sofModular.objectAudio.PlayRandomClip(launchClips, 1f, SofAudioGroup.Persistent, true);

        fireIndex++;
        OnOrdnanceLaunched?.Invoke();
    }

    public virtual bool CanLaunch => aircraft && fireIndex < launchPositions.Length;

    public bool AttemptLaunch(float delayFuse, OrdnanceLoad originalLauncher)
    {
        if (!CanLaunch || originalLauncher == this) return false;

        Launch(delayFuse);
        if (originalLauncher == null) originalLauncher = this;
        if (pairedOrdnanceLaunch) pairedOrdnanceLaunch.AttemptLaunch(delayFuse, originalLauncher);

        return true;
    }

    public static void LaunchOptimal(OrdnanceLoad[] loads, float fuze)
    {
        OrdnanceLoad load = OptimalLoad(loads);
        if (load) load.AttemptLaunch(fuze, null);
    }

    public static OrdnanceLoad OptimalLoad(OrdnanceLoad[] loads)
    {
        float maxMass = 0f;
        int maxPriority = -1;
        int chosen = 0;
        for (int i = 0; i < loads.Length; i++)
        {
            OrdnanceLoad lo = loads[i];

            bool higherPriority = lo.priority > maxPriority;
            higherPriority |= lo.ordnanceMass > maxMass && lo.priority == maxPriority;
            higherPriority &= lo.fireIndex < lo.launchPositions.Length;
            if (higherPriority)
            {
                chosen = i;
                maxMass = lo.ordnanceMass;
                maxPriority = lo.priority;
            }
        }
        if (maxPriority == -1) return null;
        return loads[chosen];
    }
}
