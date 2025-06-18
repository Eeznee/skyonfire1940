using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class EnginesAudio : SofComponent
{
    private List<EnginesGroupAudio> groups;

    public override void Initialize(SofModular _complex)
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
                group.Initialize(engine, objectAudio);
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
    private SofSmartAudioSource idleCockpit;
    private SofSmartAudioSource fullCockpit;
    private SofSmartAudioSource idleExternal;
    private SofSmartAudioSource fullExternal;
    private SofSmartAudioSource spatial;
    private ObjectAudio avm;

    private float volumeFadeInIgnition;
    private float rps;
    private float throttle;
    private float invertIdleRPS;
    private float invertFullRPS;
    private bool effectiveBoosting;

    const int frameInterval = 8;
    static int globalEngineGroupId = 0;
    private int thisGroupId;

    public void Initialize(Engine firstEngine, ObjectAudio _avm)
    {
        engines = new List<Engine>();
        preset = firstEngine.Preset;
        avm = _avm;

        thisGroupId = globalEngineGroupId;
        globalEngineGroupId = (globalEngineGroupId + 1) % frameInterval;
    }
    private void Start()
    {
        invertIdleRPS = 1f / preset.IdleRadPerSec;
        invertFullRPS = 1f / preset.NominalRadPerSec;

        idleCockpit = new SofSmartAudioSource(avm, preset.IdleAudioCockpit, SofAudioGroup.Cockpit, false, null);
        fullCockpit = new SofSmartAudioSource(avm, preset.FullAudioCockpit, SofAudioGroup.Cockpit, false, null);
        idleExternal = new SofSmartAudioSource(avm, preset.IdleAudioExtSelf, SofAudioGroup.External, true, null);
        fullExternal = new SofSmartAudioSource(avm, preset.FullAudioExtSelf, SofAudioGroup.External, true, UpdateAudio);

        spatial = new SofSmartAudioSource(avm, preset.SpatialAudio, SofAudioGroup.External, true, null);

        fullExternal.source.minDistance = idleExternal.source.minDistance = 300f;
        spatial.source.minDistance = 600f;

        foreach (Engine engine in engines)
            engine.OnIgnition += OnEngineIgnition;
    }

    public void OnEngineIgnition(Engine engine)
    {
        if (engine.Class == EngineClass.JetEngine) AudioSource.PlayClipAtPoint(preset.IgnitionClip, transform.position, 1f);

        else StartCoroutine(IgnitionCoroutine(engine));
    }
    public IEnumerator IgnitionCoroutine(Engine engine)
    {
        AudioSource.PlayClipAtPoint(preset.PreIgnitionClip, transform.position, 1f);

        while (engine.RadPerSec < PistonEngine.preIgnitionRadPerSec)
        {
            yield return null;
        }

        AudioSource.PlayClipAtPoint(preset.IgnitionClip, transform.position, 1f);
    }


    public void UpdateAudio()
    {
        if (thisGroupId != Time.frameCount % frameInterval) return;

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
        if (volumeFadeInIgnition < 1f)
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

            float spacialVolume = volumeFadeInIgnition * 5f * Mathf.InverseLerp(300f * 300f, 2000f * 2000f, (transform.position - SofAudioListener.Position).sqrMagnitude);
            spatial.source.volume = spacialVolume;

            if (idleVolume > 0.05f != idleExternal.source.isPlaying)
            {
                idleCockpit.source.enabled = idleVolume > 0.05f;
                idleExternal.source.enabled = idleVolume > 0.05f;
            }
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
