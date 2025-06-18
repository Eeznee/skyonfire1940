using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PistonEngine))]
public class PistonEngineFX : SofComponent
{
    private PistonEngine engine;
    private EnginePreset preset;
    public Mesh exhaustMesh;
    ParticleSystem ignitionEffect;
    ParticleSystem boostEffect;
    ParticleSystem overHeatEffect;

    private AudioClip[] pops;

    public override void SetReferences(SofModular _modular)
    {
        if (aircraft) aircraft.OnUpdateLOD1 -= UpdateEngineFX;
        base.SetReferences(_modular);
    }
    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);
        engine = GetComponent<PistonEngine>();
        preset = engine.Preset;
        ignitionEffect = Instantiate(preset.IgnitionEffect, transform);
        boostEffect = Instantiate(StaticReferences.Instance.engineBoostEffect, transform);
        overHeatEffect = Instantiate(StaticReferences.Instance.engineOverHeatEffect, transform);
        pops = StaticReferences.Instance.engineDamagePops;

        ParticleSystem.ShapeModule ignitionShape = ignitionEffect.shape;
        ParticleSystem.ShapeModule boostShape = boostEffect.shape;
        ParticleSystem.ShapeModule overHeatShape = overHeatEffect.shape;
        ignitionShape.shapeType = boostShape.shapeType = overHeatShape.shapeType = ParticleSystemShapeType.Mesh;
        ignitionShape.meshShapeType = boostShape.meshShapeType = overHeatShape.meshShapeType = ParticleSystemMeshShapeType.Triangle;
        ignitionShape.mesh = boostShape.mesh = overHeatShape.mesh = exhaustMesh;

        engine.OnDirectDamage += OnEngineDamage;

        aircraft.OnUpdateLOD1 += UpdateEngineFX;
    }

    const float popDamageThreshold = 0.001f;

    private float damageTracker = 0f;
    private void OnEngineDamage(float damage)
    {
        damageTracker -= damage;
        if(damageTracker < 0f )
        {
            damageTracker = popDamageThreshold * Random.Range(0.7f,1.5f);

            overHeatEffect.Emit(1);
            objectAudio.globalExternalClipsPlayer.PlayOneRandom(pops, 0.4f);
        }
    }

    private void UpdateEngineFX()
    {
        bool playBoostEffect = engine.BoostIsEffective;
        playBoostEffect &= sofModular.lod && sofModular.lod.LOD() <= 2;

        if (playBoostEffect && !boostEffect.isPlaying) boostEffect.Play();
        else if (!playBoostEffect && boostEffect.isPlaying) boostEffect.Stop();


        bool playIgnitionEffect = engine.Igniting && engine.RadPerSec > PistonEngine.preIgnitionRadPerSec * 2f;

        if (playIgnitionEffect && !ignitionEffect.isPlaying) ignitionEffect.Play();
        else if (!playIgnitionEffect && ignitionEffect.isPlaying) ignitionEffect.Stop();
    }
}
