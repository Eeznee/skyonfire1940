using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Carburetor
{
    private Engine engine;

    private bool noEngineCarburetor;
    private float carburetorState = 1f;
    public bool working { get; private set; }

    public Carburetor(Engine attachedEngine)
    {
        engine = attachedEngine;
        noEngineCarburetor = engine.preset.fuelMixer != EnginePreset.FuelMixerType.Carburetor;
        working = true;
    }

    public void Update(float deltaTime)
    {
        if (noEngineCarburetor) return;

        carburetorState = Mathf.MoveTowards(carburetorState, Mathf.Sign(engine.data.gForce + 0.5f), deltaTime);
        if (working && carburetorState < 0f) VibrationsManager.SendVibrations(0.6f, 0.5f, engine.aircraft);
        working = carburetorState > 0f;
    }
}
