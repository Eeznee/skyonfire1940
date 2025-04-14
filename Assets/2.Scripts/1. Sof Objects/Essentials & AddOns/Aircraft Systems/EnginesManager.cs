using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnginesManager
{
    private SofAircraft aircraft;

    public Engine[] AllEngines { get; private set; }
    public PistonEngine[] AllPistonEngines { get; private set; }
    public JetEngine[] AllJetEngines { get; private set; }
    public Propeller[] Propellers { get; private set; }
    public bool AtLeastOneEngineOn { get; private set; }
    public CompleteThrottle Throttle { get; private set; }

    public Engine Main => AllEngines[0];
    public EnginePreset Preset => Main.Preset;


    public EnginesManager(SofAircraft _aircraft)
    {
        aircraft = _aircraft;

        AllEngines = aircraft.GetComponentsInChildren<Engine>();
        AllPistonEngines = aircraft.GetComponentsInChildren<PistonEngine>();
        AllJetEngines = aircraft.GetComponentsInChildren<JetEngine>();
        Propellers = aircraft.GetComponentsInChildren<Propeller>();

        aircraft.OnInitialize += OnInitialize;
    }

    private void OnInitialize()
    {
        SetThrottleAllEngines(aircraft.GroundedStart ? 0f : 1f, false);
        SetEngines(!aircraft.GroundedStart, true);

        aircraft.OnFixedUpdate += OnFixedUpdate;
    }
    public void OnFixedUpdate()
    {
        //Throttle = CompleteThrottle.GetThrottleValueFromMultipleEngines(AllEngines);

        bool allEnginesDestroyed = true;
        bool oneEngineOn = false;
        foreach (Engine e in AllEngines)
        {
            allEnginesDestroyed = allEnginesDestroyed && e.ripped;
            oneEngineOn = oneEngineOn || e.Working;
        }
        AtLeastOneEngineOn = oneEngineOn;

        if (allEnginesDestroyed && aircraft.data.gsp.Get < 10f) aircraft.destroyed = true;
    }

    public void SetEngines(bool on, bool instant)
    {
        foreach (Engine engine in AllEngines)
        {
            engine.SetAutomated(on, instant);
        }
        if (Player.aircraft == aircraft) Log.Print((AllEngines.Length == 1 ? "Engine " : "Engines ") + (on ? "On" : "Off"), "engines");
    }


    public void ToggleSetEngines()
    {
        //One by one engine ignition
        if(aircraft.data.relativeAltitude.Get < 25f)
        {
            for (int i = 0; i < AllEngines.Length; i++)
            {
                Engine engine = AllEngines[i];
                if (engine.CanBeIgnited)
                {
                    engine.SetAutomated(true, false);
                    if (Player.aircraft == aircraft) Log.Print("Engine nº" + (i + 1).ToString() + " Ignited", "engines");
                    return;
                }
            }
        }

        SetEngines(!AtLeastOneEngineOn, false);
    }
    public void SetThrottleAllEngines(float thr, bool allowWEP)
    {
        if (!allowWEP) thr = Mathf.Clamp(thr, 0f, 1f);

        foreach (Engine engine in AllEngines) engine.SetThrottle(thr);

        Throttle = new CompleteThrottle(allowWEP ? thr : Mathf.Clamp01(thr));// CompleteThrottle.GetThrottleValueFromMultipleEngines(AllEngines);
    }
}
