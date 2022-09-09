using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PistonEngine))]
public class PistonEngineFX : ObjectElement
{
    private EnginesAudio enginesAudio;
    private PistonEngine engine;
    private EnginePreset preset;
    public Mesh exhaustMesh;
    ParticleSystem ignitionEffect;
    ParticleSystem boostEffect;
    ParticleSystem overHeatEffect;

    const float minPopFrequency = 2f;
    const float popFrequencyGrowth = 0.3f;
    float popCooldown = 0f;


    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        if (firstTime)
        {
            enginesAudio = data.GetComponentInChildren<EnginesAudio>();
            engine = GetComponent<PistonEngine>();
            preset = engine.preset;

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
    }


    private void Update()
    {
        if (complex.lod.LOD() == 3)
        {
            if (complex.lod.Switched())
            {
                boostEffect.Stop();
                ignitionEffect.Stop();
            }
            return;
        }
        if (engine.temperature  > engine.maximumTemperature && engine.Working())
        {
            float excess = engine.temperature - engine.maximumTemperature;
            if (popCooldown < 0f)
            {
                overHeatEffect.Emit(1);
                enginesAudio.Pop();
                float frequency = minPopFrequency + popFrequencyGrowth * excess;
                popCooldown = Random.Range(0.7f, 1.5f) / frequency;
            }
            else popCooldown -= Time.deltaTime;
        }

        bool boost = engine.boosting && engine.boostTime > 0f;
        if (boost && !boostEffect.isPlaying) boostEffect.Play();
        else if (!boost && boostEffect.isPlaying) boostEffect.Stop();

        if (engine.igniting && !ignitionEffect.isPlaying) ignitionEffect.Play();
        else if (!engine.igniting && ignitionEffect.isPlaying) ignitionEffect.Stop();
    }
}
