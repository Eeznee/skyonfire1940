using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrdnanceLoad : Module
{
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

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        if (firstTime)
            ReloadOrdnance();
    }
    public virtual float SingleMass()
    {
        return 0f;
    }

    public virtual void ReloadOrdnance()
    {
        fireIndex = launchPositions.Length - 1;
        ordnanceMass = SingleMass() * launchPositions.Length;
    }

    public virtual bool Launch(float delayFuse)
    {
        if (fireIndex < 0) return false;

        if (launchClips.Length > 0)
        {
            AudioClip clip = launchClips[Random.Range(0, launchClips.Length)];
            sofObject.avm.persistent.global.PlayOneShot(clip);
        }
        fireIndex--;
        ordnanceMass -= SingleMass();

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
            higherPriority &= lo.fireIndex >= 0;
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
