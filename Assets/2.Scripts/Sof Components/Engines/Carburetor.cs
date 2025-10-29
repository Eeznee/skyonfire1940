using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Carburetor
{
    private Engine engine;

    private bool noEngineCarburetor;
    private float carburetorState = 1f;
    public bool carburetorFlowing { get; private set; }

    public Carburetor(Engine attachedEngine)
    {
        engine = attachedEngine;
        noEngineCarburetor = !engine.Preset.UsesCarburetor;
        carburetorFlowing = true;
    }

    public void Update(float deltaTime)
    {
        if (noEngineCarburetor) return;

        carburetorState = Mathf.MoveTowards(carburetorState, Mathf.Sign(engine.data.gForce + 0.5f), deltaTime);

        if (carburetorFlowing && carburetorState < 0f && Player.aircraft == engine.aircraft) VibrationsManager.SendVibrations(0.6f, 0.5f);
        carburetorFlowing = carburetorState > 0f;
    }
}
