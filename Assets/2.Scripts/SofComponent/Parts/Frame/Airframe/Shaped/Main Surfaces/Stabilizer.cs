﻿using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[AddComponentMenu("Sof Components/Aerodynamic Surfaces/Stabilizer")]
public class Stabilizer : MainSurface
{
    private Propeller propeller;

    public override float HpPerSquareMeter => ModulesHPData.stabilizerHpPerSq;


    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);

        Propeller[] allPropellers = aircraft.GetComponentsInChildren<Propeller>();
        propeller = null;

        foreach(Propeller p in allPropellers)
        {
            Vector3 deltaPos = p.localPos - localPos;
            if (deltaPos.z < 0f) continue;
            deltaPos.z = 0f;

            if (deltaPos.magnitude < p.preset.diameter) propeller = p;
        }
    }
    public override float PropSpeedEffect()
    {
        if (!propeller) return 0f;
        if (!aircraft) return 0f;
        if (!data.grounded) return 0f;
        if (data.ias.Get < 0.1f) return 0f;

        float ias = data.ias.Get;
        float densArea = data.density.Get * propeller.preset.Area;

        float formula = Mathf.Abs(2f * propeller.thrust / densArea + ias * ias);
        float speedBoost = Mathf.Sqrt(formula) - ias;

        return speedBoost;
    }
    protected override void FixedUpdate()
    {
        float aoa = ApplyForces();
    }
#if UNITY_EDITOR
    public override void Draw()
    {
        quad.Draw(vertical ? rudderColor : elevatorColor, bordersColor, true);
    }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(Stabilizer)), CanEditMultipleObjects]
public class StabilizerEditor : MainSurfaceEditor
{

}
#endif