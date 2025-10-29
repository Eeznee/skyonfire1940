using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct CompleteThrottle
{
    private float Value { get; set; }
    public bool Boost { get; private set; }


    public CompleteThrottle(float _throttleInput)
    {
        Boost = _throttleInput > 1f;
        Value = Mathf.Clamp01(_throttleInput);
    }

    public static CompleteThrottle GetThrottleValueFromMultipleEngines(Engine[] engines)
    {
        CompleteThrottle t = new CompleteThrottle(0f);
        t.Boost = false;

        foreach (Engine engine in engines)
        {
            t.Value = Mathf.Max(t.Value, engine.Throttle);
            t.Boost = t.Boost || engine.Throttle.Boost;
        }
        return t;
    }

    public static implicit operator float(CompleteThrottle throttle)
    {
        return throttle.Value;
    }
}
