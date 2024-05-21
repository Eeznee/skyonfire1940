using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Engine;

public class EnginesManager
{
    private SofAircraft aircraft;

    public Engine[] all;

    public EnginesState state;
    public float throttle;
    public bool boost;

    public Engine main { get { return all[0]; } }
    public EnginePreset preset { get { return main.preset; } }
    public EnginesManager(SofAircraft _aircraft)
    {
        aircraft = _aircraft;

        throttle = 0f;
        boost = false;

        all = aircraft.GetComponentsInChildren<Engine>();
    }

    public void Update()
    {
        throttle = 0f;
        bool destroyedEngine = true;
        bool allOn = true;
        bool allOff = true;
        foreach (Engine e in all)
        {
            throttle = Mathf.Max(throttle, e.throttleInput);
            destroyedEngine = destroyedEngine && e.ripped;
            allOn = allOn && e.Working();
            allOff = allOff && !e.Working();
        }
        state = (EnginesState)(destroyedEngine ? 0 : (allOff ? 1 : (allOn ? 2 : 3)));

        if (state == EnginesState.Destroyed && aircraft.data.gsp.Get < 10f) aircraft.destroyed = true;
    }

    public void SetEngines(bool on, bool instant)
    {
        foreach (Engine engine in all)
        {
            engine.throttleInput = throttle;
            engine.Set(on, instant);
        }
        if (Player.aircraft == aircraft) Log.Print((all.Length == 1 ? "Engine " : "Engines ") + (on ? "On" : "Off"), "engines");
    }
    public void SetEngines()
    {
        SetEngines(state == EnginesState.Off, false);
    }
    public void SetThrottle(float thr)
    {
        throttle = Mathf.Clamp01(thr);
        foreach (Engine engine in all) engine.throttleInput = throttle;
    }
    public void TurnOnAllEngines()
    {
        foreach (Engine e in all) if (!e.Working() && e.Functional()) e.Set(true, false);
    }
}
