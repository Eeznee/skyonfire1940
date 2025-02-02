using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circuit
{
    public Circuit(Transform fxParent, LiquidTank attachedTank)
    {
        holesArea = 0f;
        mainTank = attachedTank;
        leakEffect = Object.Instantiate(mainTank.liquid.leakFx, fxParent.transform);
    }
    public LiquidTank mainTank;

    public float holesArea;
    protected ParticleSystem leakEffect;

    public void Damage(float caliber)
    {
        holesArea += Mathv.SmoothStart(caliber / 2000f, 2) * Mathf.PI;
    }
    public void Leaking(float deltaTime)
    {
        float fill = mainTank.FillRatio;
        float leakRate = holesArea * mainTank.liquid.leakSpeed * 1000f;
        mainTank.ShiftFluidMass(-leakRate * deltaTime);

        if (!leakEffect) return;
        bool leakingFx = fill > 0f && holesArea > 0f;
        if (leakingFx != leakEffect.isPlaying)
        {
            if (leakingFx) leakEffect.Play();
            else leakEffect.Stop();
        }
    }
}