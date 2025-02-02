using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private float volume;
    private float rps;
    private float invertIdleRPS;
    private float invertFullRPS;

    const float rpsThreshold = 2f;

    public void Initialize(Engine firstEngine, ObjectAudio _avm)
    {
        engines = new List<Engine>();
        engines.Add(firstEngine);
        preset = firstEngine.Preset;
        avm = _avm;
    }
    private void Start()
    {
        invertIdleRPS = 1f / preset.idleRPS;
        invertFullRPS = 1f / preset.fullRps;

        //Audio
        idleCockpit = new SofAudio(avm, preset.idleAudioCockpit, SofAudioGroup.Cockpit, false);
        fullCockpit = new SofAudio(avm, preset.fullAudioCockpit, SofAudioGroup.Cockpit, false);
        idleExternal = new SofAudio(avm, preset.idleAudioExtSelf, SofAudioGroup.External, true);
        fullExternal = new SofAudio(avm, preset.fullAudioExtSelf, SofAudioGroup.External, true);
        spatial = new SofAudio(avm, preset.spatialAudio, SofAudioGroup.External, true);
        fullExternal.source.minDistance = idleExternal.source.minDistance = 300f;
        spatial.source.minDistance = 600f;

        foreach(Engine engine in engines)
            engine.OnIgnition += PlayStartUpAudio;
    }
    public void PlayStartUpAudio() { avm.persistent.global.PlayOneShot(preset.startUpAudio); }

    void Update()
    {
        float highestRPS = 0f;
        float highestVolume = 0f;
        foreach(Engine engine in engines)
        {
            highestRPS = Mathf.Max(highestRPS, engine.radiansPerSeconds);
            highestVolume = Mathf.Max(highestVolume, Volume(engine));
        }

        if (volume == highestVolume && Mathf.Abs(rps-highestRPS) < rpsThreshold) return;

        rps = highestRPS;
        volume = Mathf.MoveTowards(volume, highestVolume, Time.deltaTime);
        UpdateVolume();
        UpdatePitch();
    }
    private float Volume(Engine engine)
    {
        if (engine.workingAndRunning) return 1f;
        if (engine.igniting) return engine.radiansPerSeconds / preset.idleRPS;
        return 0f;
    }
    private void UpdateVolume()
    {
        float idleVolume = Mathf.InverseLerp(preset.fullRps, preset.idleRPS, rps) * volume;
        float fullVolume = volume - idleVolume;
        float spacialVolume = volume * 5f * Mathf.InverseLerp(300f * 300f, 2000f * 2000f, (transform.position - SofAudioListener.position).sqrMagnitude);

        idleCockpit.source.volume = idleExternal.source.volume = idleVolume;
        fullCockpit.source.volume = fullExternal.source.volume = fullVolume;
        spatial.source.volume = spacialVolume;
    }

    private void UpdatePitch()
    {
        if (!avm.aircraft.engines.AtLeastOneEngineOn) return;

        float idlePitch = Mathf.Max(1f, rps * invertIdleRPS) * (TimeManager.paused ? 1f : Time.timeScale);
        float fullPitch = idlePitch * preset.idleRPS * invertFullRPS;
        fullPitch *= (1f + avm.aircraft.engines.Throttle.TrueThrottle) * 0.5f;

        idleCockpit.source.pitch = idleExternal.source.pitch = idlePitch;
        fullCockpit.source.pitch = fullExternal.source.pitch = fullPitch;
    }
}
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
            EnginePreset preset = engine.Preset;

            bool isNewPreset = true;
            foreach (EnginesGroupAudio group in groups)
                if (group.preset == preset) { isNewPreset = false; group.engines.Add(engine); }
            if (isNewPreset)
            {
                EnginesGroupAudio newGroup = gameObject.AddComponent<EnginesGroupAudio>();
                newGroup.Initialize(engine, avm);
                groups.Add(newGroup);
            }
        }
    }
}