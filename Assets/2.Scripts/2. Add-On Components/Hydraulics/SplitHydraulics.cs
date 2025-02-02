using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;


[AddComponentMenu("Sof Components/Liquid Systems/Split Hydraulics")]
public class SplitHydraulics : HydraulicSystem
{
    public string[] parameters;
    private HydraulicsCurve[] curves;

    public override void Initialize(SofComplex _complex)
    {
        curves = new HydraulicsCurve[parameters.Length];
        for (int i = 0; i < curves.Length; i++)
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
        if (!animating)
        {
            if (state == 0f || state == 1f)
                for (int i = 0; i < parameters.Length; i++)
                    curves[i].Reset(state == 1f);
        }

        base.AnimateUpdate();
    }
    protected override void ApplyStateAnimator()
    {
        for (int i = 0; i < parameters.Length; i++)
        {
            if (curves != null && curves[i] != null)
            {
                animator.SetFloat(parameters[i], curves[i].ModifiedValue(state));
            }
            else
            {
                animator.SetFloat(parameters[i], state);
            }
        }
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


        firstPoint = new Vector2(Random.Range(0.05f, 0.4f), Random.Range(0.05f, 0.4f));
        secondPoint = new Vector2(Random.Range(firstPoint.x + 0.1f, 0.95f), Random.Range(firstPoint.y + 0.1f, 0.95f));
    }
    public float ModifiedValue(float value)
    {
        float x = value / earlyFinish;
        if (reverse) x -= 1f / earlyFinish - 1f;
        x = Mathf.Clamp01(x);
        return Path(x, Vector2.zero, firstPoint, secondPoint, Vector2.one);
    }

    public static float Path(float x, params Vector2[] vectors)
    {
        for (int i = 1; i < vectors.Length; i++)
        {
            Vector2 prev = vectors[i - 1];
            Vector2 next = vectors[i];

            if (x >= prev.x && x <= next.x)
            {
                float t = (x - prev.x) / (next.x - prev.x);
                return t * (next.y - prev.y) + prev.y;
            }
        }
        Debug.LogError("Vectors are not sorted");
        return 0f;
    }
}