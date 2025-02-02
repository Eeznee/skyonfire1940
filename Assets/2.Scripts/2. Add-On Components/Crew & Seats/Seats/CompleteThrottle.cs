using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct CompleteThrottle
{
    private float Value { get; set; }
    public bool WEP { get; private set; }
    public float TrueThrottle { get; private set; }
    public float TargetRadPerSec { get; private set; }

    const float minimumTrueThrottle = 0.05f;


    public CompleteThrottle(float _throttleInput, Engine engine)
    {
        WEP = _throttleInput > 1f;

        Value = Mathf.Clamp01(_throttleInput);

        if (WEP)
        {
            TrueThrottle = engine.Preset.WEPValue;
            TargetRadPerSec = engine.Preset.WEPrps;
        }
        else
        {
            TrueThrottle = (Value + minimumTrueThrottle) / (1f + minimumTrueThrottle);
            TargetRadPerSec = Mathf.Lerp(engine.Preset.idleRPS, engine.Preset.fullRps, Value);
        }
    }

    public static CompleteThrottle GetThrottleValueFromMultipleEngines(Engine[] engines)
    {
        CompleteThrottle t = new CompleteThrottle(0f, engines[0]);
        t.WEP = false;

        foreach (Engine engine in engines)
        {
            t.Value = Mathf.Max(t.Value, engine.Throttle);
            t.TrueThrottle = Mathf.Max(t.TrueThrottle, engine.Throttle.TrueThrottle);
            t.WEP = t.WEP || engine.Throttle.WEP;
        }
        return t;
    }

    public static implicit operator float(CompleteThrottle throttle)
    {
        return throttle.Value;
    }
}
