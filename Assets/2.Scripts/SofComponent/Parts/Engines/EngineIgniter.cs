using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineIgniter
{
    private Engine engine;
    private EnginePreset preset;

    public bool igniting = false;
    public float ignitionState;

    public EngineIgniter(Engine attachedEngine)
    {
        engine = attachedEngine;
        preset = engine.preset;
    }
    public bool CanIgnite() { return engine.rps <= preset.idleRPS / 2f && engine.pumped && engine.Functional() && engine.onInput; }
    public IEnumerator Ignition()
    {
        float delay = Random.Range(0f, 1.5f);
        while (delay > 0f)
        {
            delay -= Time.deltaTime;
            yield return null;
        }
        igniting = true;
        ignitionState = 0f;
        delay = 0f;
        float fromRPS = engine.rps;

        while (delay < preset.ignitionTime)
        {
            ignitionState = delay / preset.ignitionTime;
            float rpsFactor = Mathf.Sin(ignitionState * Mathf.PI / 2f);
            rpsFactor = Mathv.SmoothStart(rpsFactor, 4);
            engine.rps = Mathf.Lerp(fromRPS, preset.idleRPS, rpsFactor);
            delay += Time.deltaTime;
            yield return null;
        }
        igniting = false;
    }
}
