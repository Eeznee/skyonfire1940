using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class NonLinearityProcessor : InputProcessor<float>
{
    [Tooltip("Number to change the curvature of the input")]
    public float nonLinearity = 0f;

#if UNITY_EDITOR
    static NonLinearityProcessor()
    {
        Initialize();
    }
#endif

    [RuntimeInitializeOnLoadMethod]
    static void Initialize()
    {
        InputSystem.RegisterProcessor<NonLinearityProcessor>();
    }

    public override float Process(float value, InputControl control)
    {
        if(nonLinearity == 0f) return value;

        float abs = Mathf.Abs(value);
        float sign = Mathf.Sign(value);

        if (nonLinearity > 0f)
        {
            float func = abs * abs;
            return Mathf.Lerp(abs, func , nonLinearity) * sign;
        }
        else
        {
            float func = 1f - (abs - 1f) * (abs - 1f);
            return Mathf.Lerp(abs, func, -nonLinearity) * sign;
        }
    }
}