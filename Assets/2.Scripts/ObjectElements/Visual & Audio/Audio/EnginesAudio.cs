using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnginesAudio : AudioVisual
{
    //References
    Engine[] engines;
    EnginePreset preset;

    //Sources
    SofAudio idleCockpit;
    SofAudio fullCockpit;
    SofAudio idleExternal;
    SofAudio fullExternal;
    SofAudio spatial;

    float counter = 0f;
    const float togglingInterval = 0.15f;
    bool[,] enginesPreviousStates;

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        if (firstTime)
        {
            engines = aircraft.engines;
            enginesPreviousStates = new bool[aircraft.engines.Length, 2];
            preset = engines[0].preset;

            //Audio
            idleCockpit = new SofAudio(avm, preset.idleAudioCockpit, SofAudioGroup.Cockpit, false, true);
            fullCockpit = new SofAudio(avm, preset.fullAudioCockpit, SofAudioGroup.Cockpit, false, true);
            idleExternal = new SofAudio(avm, preset.idleAudioExtSelf, SofAudioGroup.External, true, true);
            fullExternal = new SofAudio(avm, preset.fullAudioExtSelf, SofAudioGroup.External, true, true);
            spatial = new SofAudio(avm, preset.spatialAudio, SofAudioGroup.External, true, true);
            spatial.source.minDistance = 600f;
        }
    }
    public void Toggle(bool on)
    {
        if (counter > 0f) return;
        avm.persistent.global.PlayOneShot(on ? preset.startUpAudio : preset.shutDownAudio);
        counter = togglingInterval;
    }

    public void Pop()
    {
        avm.persistent.global.PlayOneRandom(preset.enginePops, 0.4f);
    }

    void Update()
    {
        counter -= Time.deltaTime;
        float highestRPS = 0f;
        float highestVolume = 0f;
        for(int i = 0; i < engines.Length; i++)
        {
            Engine engine = engines[i];
            highestRPS = Mathf.Max(highestRPS, engine.rps);
            highestVolume = Mathf.Max(highestVolume, engine.trueEngineVolume);

            //Ignition and Shut Off
            if (engine.igniting && !enginesPreviousStates[i,0]) Toggle(true);
            if (!engine.Working() && enginesPreviousStates[i, 1]) Toggle(false);
            enginesPreviousStates[i, 0] = engine.igniting;
            enginesPreviousStates[i, 1] = engine.Working();
        }

        //Volume
        float idleVolume = Mathf.InverseLerp(preset.fullRps, preset.idleRPS, highestRPS) * highestVolume;
        float fullVolume = highestVolume - idleVolume;
        float spacialVolume = highestVolume * 5f * Mathf.InverseLerp(300f, 2000f, Vector3.Distance(transform.position, SofAudioListener.listener.transform.position));
        idleCockpit.source.volume = idleExternal.source.volume = idleVolume;
        fullCockpit.source.volume = fullExternal.source.volume = fullVolume;
        spatial.source.volume = spacialVolume;

        //Pitch
        float idlePitch = Mathf.Max(1f,highestRPS / preset.idleRPS) * (GameManager.paused ? 1f : Mathf.Pow(Time.timeScale, 0.25f));
        float fullPitch = idlePitch * preset.idleRPS / preset.fullRps;
        if (aircraft.boost) fullPitch *= 1.03f;
        if ((int)aircraft.enginesState <= 1) return;
        idleCockpit.source.pitch = idleExternal.source.pitch = idlePitch;
        fullCockpit.source.pitch = fullExternal.source.pitch = fullPitch;
    }
}
