using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelEffects : SofComponent
{
    public ParticleSystem groundFriction;

    private Wheel[] wheels;
    private ParticleSystem[] effects;



    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);

        wheels = _complex.GetComponentsInChildren<Wheel>();
        effects = new ParticleSystem[wheels.Length];

        for (int i = 0; i < wheels.Length; i++)
        {
            //effects[i] = 
        }
    }

}
