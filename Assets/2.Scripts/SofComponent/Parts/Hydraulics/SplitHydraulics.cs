using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class SplitHydraulics : HydraulicSystem
{
    public string[] parameters;
    private HydraulicsCurve[] curves;

    public override void Initialize(SofComplex _complex)
    {
        curves = new HydraulicsCurve[parameters.Length];
        for(int i = 0; i < curves.Length; i++)
            curves[i] = new HydraulicsCurve();

        base.Initialize(_complex);
    }
    private void Update()
    {
        AnimateUpdate();
        AudioUpdate();
    }

    protected override void AnimateUpdate()
    {
        if (state == 0f || state == 1f)
            for (int i = 0; i < parameters.Length; i++)
                curves[i].Reset(state == 1f);

        base.AnimateUpdate();
    }
    protected override void ApplyStateAnimator()
    {
        for (int i = 0; i < parameters.Length; i++)
            animator.SetFloat(parameters[i], curves[i].ModifiedValue(state));
    }
}
public class HydraulicsCurve
{
    private bool reverse;
    private float earlyFinish;

    private Vector2 firstPoint;
    private Vector2 secondPoint;

    public HydraulicsCurve()
    {
        Reset(false);
    }

    public void Reset(bool _reverse)
    {
        earlyFinish = Random.Range(0.6f, 1f);
        reverse = _reverse;

        firstPoint = new Vector2(Random.Range(0.1f, 0.4f), Random.Range(0.1f, 0.4f));
        secondPoint = new Vector2(Random.Range(0.6f, 0.9f), Random.Range(0.6f, 0.9f));
    }
    public float ModifiedValue(float value)
    {
        float x = value / earlyFinish;
        if (reverse) x -= 1f / earlyFinish - 1f;
        x = Mathf.Clamp01(x);
        return Vectors.Path(x, Vector3.zero, firstPoint, secondPoint, Vector3.one);
    }
}