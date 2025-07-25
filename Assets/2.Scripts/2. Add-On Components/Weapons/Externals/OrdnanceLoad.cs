using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OrdnanceLoad : SofComponent, IMassComponent
{
    public float RealMass => SingleMass * Mathf.Max(launchPositions.Length - fireIndex);
    public float LoadedMass => SingleMass * launchPositions.Length;
    public float EmptyMass => 0f;
    public virtual float SingleMass => 0f;

    public Vector3[] launchPositions;
    public AudioClip[] launchClips;

    [HideInInspector] public OrdnanceLoad symmetrical;
    [HideInInspector] public int priority;
    private float ordnanceMass = 0f;
    protected int fireIndex;

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

    public virtual bool Launch(float delayFuse)
    {
        if (fireIndex >= launchPositions.Length) return false;

        if (launchClips.Length > 0)
        {
            AudioClip clip = launchClips[Random.Range(0, launchClips.Length)];
            sofModular.objectAudio.localClipsPlayer.PlayOneShot(clip);
        }

        Mass mass = new Mass(SingleMass, localPos + launchPositions[fireIndex]);
        if(sofComplex) sofComplex.ShiftMass(-mass);

        fireIndex++;
        ordnanceMass -= SingleMass;


        if (symmetrical) symmetrical.Launch(delayFuse);

        return true;
    }
    public static void LaunchOptimal(OrdnanceLoad[] loads, float fuze)
    {
        OrdnanceLoad load = OptimalLoad(loads);
        if (load) load.Launch(fuze);
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
