using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class EnginesAudio : AudioComponent
{
    private List<EnginesGroupAudio> groups;

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);

        groups = new List<EnginesGroupAudio>();
        Engine[] allEngines = sofObject.GetComponentsInChildren<Engine>();

        for (int i = 0; i < allEngines.Length; i++)
        {
            Engine engine = allEngines[i];

            EnginesGroupAudio group = TryToFindExistingGroup(engine);
            if (group == null)
            {
                group = gameObject.AddComponent<EnginesGroupAudio>();
                group.Initialize(engine, avm);
                groups.Add(group);
            }
            group.engines.Add(engine);
        }
    }
    public EnginesGroupAudio TryToFindExistingGroup(Engine engine)
    {
        foreach (EnginesGroupAudio group in groups)
            if (group.preset == engine.Preset)
                return group;

        return null;
    }
}
public class EnginesGroupAudio : MonoBehaviour
{
    //References
    public List<Engine> engines;
    public EnginePreset preset;

    //Sources
    private SofAudio idleCockpit;
    private SofAudio fullCockpit;
    private SofAudio idleExternal;
    private SofAudio fullExternal;
    private SofAudio spatial;
    private ObjectAudio avm;

    private float volumeFadeInIgnition;
    private float rps;
    private float throttle;
    private float invertIdleRPS;
    private float invertFullRPS;
    private bool effectiveBoosting;

    public void Initialize(Engine firstEngine, ObjectAudio _avm)
    {
        engines = new List<Engine>();
        preset = firstEngine.Preset;
        avm = _avm;
    }
    private void Start()
    {
        invertIdleRPS = 1f / preset.IdleRadPerSec;
        invertFullRPS = 1f / preset.NominalRadPerSec;

        idleCockpit = new SofAudio(avm, preset.IdleAudioCockpit, SofAudioGroup.Cockpit, false);
        fullCockpit = new SofAudio(avm, preset.FullAudioCockpit, SofAudioGroup.Cockpit, false);
        idleExternal = new SofAudio(avm, preset.IdleAudioExtSelf, SofAudioGroup.External, true);
        fullExternal = new SofAudio(avm, preset.FullAudioExtSelf, SofAudioGroup.External, true);
        spatial = new SofAudio(avm, preset.SpatialAudio, SofAudioGroup.External, true);
        fullExternal.source.minDistance = idleExternal.source.minDistance = 300f;
        spatial.source.minDistance = 600f;

        foreach (Engine engine in engines)
            engine.OnIgnition += OnEngineIgnition;
    }

    public void OnEngineIgnition(Engine engine)
    {
        if (engine.Class == EngineClass.JetEngine) avm.persistent.global.PlayOneShot(preset.IgnitionClip);

        else StartCoroutine(IgnitionCoroutine(engine));
    }

    public IEnumerator IgnitionCoroutine(Engine engine)
    {
        avm.persistent.global.PlayOneShot(preset.PreIgnitionClip);

        while (engine.RadPerSec < PistonEngine.preIgnitionRadPerSec)
        {
            yield return null;
        }

        avm.persistent.global.PlayOneShot(preset.IgnitionClip);
    }


    void Update()
    {
        float highestRPS = 0f;
        float highestVolume = 0f;
        float highestThrottle = 0f;
        effectiveBoosting = false;
        foreach (Engine engine in engines)
        {
            highestRPS = Mathf.Max(highestRPS, engine.RadPerSec);
            highestThrottle = Mathf.Max(highestThrottle, engine.Throttle);
            highestVolume = Mathf.Max(highestVolume, VolumeFadeInIgnition(engine));
            effectiveBoosting |= engine.BoostIsEffective;
        }

        //if (volume == highestVolume && Mathf.Abs(rps-highestRPS) < rpsThreshold) return;

        rps = highestRPS;
        throttle = highestThrottle;
        volumeFadeInIgnition = highestVolume;
        UpdateVolume();
        UpdatePitch();
        UpdateBoostedMixer();
    }
    private float VolumeFadeInIgnition(Engine engine)
    {
        if (engine.Working) return 1f;
        if (engine.Igniting) return engine.RadPerSec / preset.IdleRadPerSec;
        return 0f;
    }
    private void UpdateVolume()
    {
        if(volumeFadeInIgnition < 1f) //when volume is lower than 1, it means the engine is
        {
            idleCockpit.source.volume = idleExternal.source.volume = volumeFadeInIgnition;
            fullCockpit.source.volume = fullExternal.source.volume = 0f;
            spatial.source.volume = 0f;
        }
        else
        {
            float rpsFactor = Mathf.InverseLerp(preset.IdleRadPerSec, preset.NominalRadPerSec, rps);
            float fullVolumePortion = rpsFactor * 0.5f + throttle * 0.5f;

            float idleVolume = (1f - fullVolumePortion) * volumeFadeInIgnition;
            float fullVolume = fullVolumePortion * volumeFadeInIgnition;

            idleCockpit.source.volume = idleExternal.source.volume = idleVolume;
            fullCockpit.source.volume = fullExternal.source.volume = fullVolume;

            float spacialVolume = volumeFadeInIgnition * 5f * Mathf.InverseLerp(300f * 300f, 2000f * 2000f, (transform.position - SofAudioListener.position).sqrMagnitude);
            spatial.source.volume = spacialVolume;
        }
    }

    private void UpdatePitch()
    {
        if (!avm.aircraft.engines.AtLeastOneEngineOn) return;

        float idlePitch = rps * invertIdleRPS * (TimeManager.paused ? 1f : Time.timeScale);
        idlePitch = Mathf.Sqrt(Mathf.Abs(idlePitch));
        float fullPitch = rps * invertFullRPS * (TimeManager.paused ? 1f : Time.timeScale);

        idleCockpit.source.pitch = idleExternal.source.pitch = idlePitch;
        fullCockpit.source.pitch = fullExternal.source.pitch = fullPitch;
    }

    private void UpdateBoostedMixer()
    {
        if (effectiveBoosting != UsingBoostedMixer)
        {
            AudioMixerGroup cockpitMixer = effectiveBoosting ? SofAudioListener.instance.boostedEngineCockpit : SofAudioListener.instance.cockpit;
            AudioMixerGroup externalMixer = effectiveBoosting ? SofAudioListener.instance.boostedEngineExternal : SofAudioListener.instance.external;

            idleCockpit.source.outputAudioMixerGroup = fullCockpit.source.outputAudioMixerGroup = cockpitMixer;
            idleExternal.source.outputAudioMixerGroup = fullExternal.source.outputAudioMixerGroup = externalMixer;
        }
    }

    private bool UsingBoostedMixer => fullExternal.source.outputAudioMixerGroup == SofAudioListener.instance.boostedEngineExternal;
}
