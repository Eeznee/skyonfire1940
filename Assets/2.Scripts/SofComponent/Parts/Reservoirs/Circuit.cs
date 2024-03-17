using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circuit
{
    public Circuit(Transform fxParent, LiquidTank attachedTank)
    {
        holesArea = 0f;
        mainTank = attachedTank;
        leak = Object.Instantiate(mainTank.liquid.leakFx, fxParent.transform);
    }
    private SofModule module;
    public LiquidTank mainTank;

    public float holesArea;
    protected ParticleSystem leak;

    public void Damage(float caliber)
    {
        holesArea += Mathv.SmoothStart(caliber / 2000f, 2) * Mathf.PI;
    }
    public void Leaking(float deltaTime)
    {
        float fill = mainTank.fill;
        float leakRate = holesArea * mainTank.liquid.leakSpeed * 1000f;
        mainTank.ShiftFluidMass(-leakRate * deltaTime);

        if (!leak) return;
        bool leaking = !mainTank.burning && fill > 0f && holesArea > 0f;
        if (leaking != leak.isPlaying)
        {
            if (leaking) leak.Play();
            else leak.Stop();
        }
    }
}