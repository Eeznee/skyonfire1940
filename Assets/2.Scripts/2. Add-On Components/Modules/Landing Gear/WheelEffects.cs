using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelEffects : SofComponent
{
    public ParticleSystem groundFriction;

    private Wheel[] wheels;
    private ParticleSystem[] effects;



    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);

        wheels = _complex.GetComponentsInChildren<Wheel>();
        effects = new ParticleSystem[wheels.Length];

        for (int i = 0; i < wheels.Length; i++)
        {
            effects[i] = Instantiate(groundFriction, wheels[i].tr.position, wheels[i].tr.rotation, wheels[i].tr);
            wheels[i].OnRip += OnWheelRipped;
        }


    }
    private void OnWheelRipped(SofModule wheel)
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            if(wheel == (SofModule)wheels[i])
            {
                effects[i].Stop();
                Destroy(effects[i],5f);
            }
        }
    }
    private void Update()
    {
        if (sofModular.data.relativeAltitude.Get > 15f) return;

        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i] == null || wheels[i].sofModular != sofModular) continue;

            WheelEffect(wheels[i], effects[i]);
        }
    }
    void WheelEffect(Wheel wheel, ParticleSystem frictionFx)
    {

        bool playFrictionFX = wheel.grounded && sofModular.lod.LOD() <= 1;

        bool slipping = Mathf.Abs(wheel.sideSpeed) > 2f;
        bool braking = wheel.BrakesInput() > 0.1f && wheel.brakes != Wheel.BrakeSystem.None;

        playFrictionFX &= slipping || braking;
        
        if (playFrictionFX != frictionFx.isPlaying)
        {
            if (playFrictionFX) frictionFx.Play();
            else frictionFx.Stop();
        }
    }
}
