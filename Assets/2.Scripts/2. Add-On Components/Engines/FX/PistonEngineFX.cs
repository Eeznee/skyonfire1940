using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PistonEngine))]
public class PistonEngineFX : AudioComponent
{
    private PistonEngine engine;
    private EnginePreset preset;
    public Mesh exhaustMesh;
    ParticleSystem ignitionEffect;
    ParticleSystem boostEffect;
    ParticleSystem overHeatEffect;

    const float minPopFrequency = 2f;
    const float popFrequencyGrowth = 0.3f;
    float popCooldown = 0f;


    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        engine = GetComponent<PistonEngine>();
        preset = engine.Preset;
        ignitionEffect = Instantiate(preset.ignitionEffect, transform);
        boostEffect = Instantiate(preset.boostEffect, transform);
        overHeatEffect = Instantiate(preset.overHeatEffect, transform);

        ParticleSystem.ShapeModule ignitionShape = ignitionEffect.shape;
        ParticleSystem.ShapeModule boostShape = boostEffect.shape;
        ParticleSystem.ShapeModule overHeatShape = overHeatEffect.shape;
        ignitionShape.shapeType = boostShape.shapeType = overHeatShape.shapeType = ParticleSystemShapeType.Mesh;
        ignitionShape.meshShapeType = boostShape.meshShapeType = overHeatShape.meshShapeType = ParticleSystemMeshShapeType.Triangle;
        ignitionShape.mesh = boostShape.mesh = overHeatShape.mesh = exhaustMesh;
    }

    private void Pop()
    {
        avm.persistent.global.PlayOneRandom(preset.enginePops, 0.4f);
    }
    private void Update()
    {
        float excess = engine.Temp.temperature - engine.Temp.maximumTemperature;
        if (excess > 0f && engine.workingAndRunning)
        {
            if (popCooldown < 0f)
            {
                overHeatEffect.Emit(1);
                Pop();
                float frequency = minPopFrequency + popFrequencyGrowth * excess;
                popCooldown = Random.Range(0.7f, 1.5f) / frequency;
            }
            else popCooldown -= Time.deltaTime;
        }

        bool playBoostEffect = engine.Throttle.WEP;
        playBoostEffect &= complex.lod && complex.lod.LOD() <= 2;

        if (playBoostEffect && !boostEffect.isPlaying) boostEffect.Play();
        else if (!playBoostEffect && boostEffect.isPlaying) boostEffect.Stop();

        if (engine.igniting && !ignitionEffect.isPlaying) ignitionEffect.Play();
        else if (!engine.igniting && ignitionEffect.isPlaying) ignitionEffect.Stop();
    }
}
