using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnginesAudio : AudioVisual
{
    //References
    public Engine[] engines;
    private EnginePreset preset;

    //Sources
    private SofAudio idleCockpit;
    private SofAudio fullCockpit;
    private SofAudio idleExternal;
    private SofAudio fullExternal;
    private SofAudio spatial;



    private float invertIdleRPS;
    private float invertFullRPS;
    private float counter = 0f;
    private const float togglingInterval = 0.15f;
    private bool[,] enginesPreviousStates;


    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        if (firstTime)
        {
            if (engines.Length == 0) engines = aircraft.engines;
            enginesPreviousStates = new bool[aircraft.engines.Length, 2];
            preset = engines[0].preset;
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
    private float previousIdleVolume = 0f;
    private float previousFullVolume = 0f;
    private float previousSpacialVolume = 0f;
    private const float volumeStep = 0.02f;

    private float previousIdlePitch = 0f;
    private float previousFullPitch = 0f;
    private const float pitchStep = 0.02f;
    void Update()
    {
        counter -= Time.deltaTime;
        float highestRPS = 0f;
        float highestVolume = 0f;
        for (int i = 0; i < engines.Length; i++)
        {
            Engine engine = engines[i];
            highestRPS = Mathf.Max(highestRPS, engine.rps);
            highestVolume = Mathf.Max(highestVolume, engine.trueEngineVolume);

            //Ignition and Shut Off
            if (engine.igniting && !enginesPreviousStates[i, 0]) Toggle(true);
            if (!engine.Working() && enginesPreviousStates[i, 1]) Toggle(false);
            enginesPreviousStates[i, 0] = engine.igniting;
            enginesPreviousStates[i, 1] = engine.Working();
        }
        //Volume
        float idleVolume = Mathf.InverseLerp(preset.fullRps, preset.idleRPS, highestRPS) * highestVolume;
        float fullVolume = highestVolume - idleVolume;
        float spacialVolume = highestVolume * 5f * Mathf.InverseLerp(300f * 300f, 2000f * 2000f, (data.position - SofAudioListener.position).sqrMagnitude);

        if (Mathf.Abs(previousIdleVolume - idleVolume) > volumeStep) previousIdleVolume = idleCockpit.source.volume = idleExternal.source.volume = idleVolume;
        if (Mathf.Abs(previousFullVolume - fullVolume) > volumeStep) previousFullVolume = fullCockpit.source.volume = fullExternal.source.volume = fullVolume;
        if (Mathf.Abs(previousSpacialVolume - spacialVolume) > volumeStep) previousSpacialVolume = spatial.source.volume = spacialVolume;

        //Pitch
        float idlePitch = Mathf.Max(1f, highestRPS * invertIdleRPS) * (TimeManager.paused ? 1f : Time.timeScale);
        float fullPitch = idlePitch * preset.idleRPS * invertFullRPS;
        if (aircraft.boost) fullPitch *= 1.03f;
        if ((int)aircraft.enginesState <= 1) return;
        if (Mathf.Abs(previousIdlePitch - idlePitch) > pitchStep) previousIdlePitch = idleCockpit.source.pitch = idleExternal.source.pitch = idlePitch;
        if (Mathf.Abs(previousFullPitch - fullPitch) > pitchStep) previousFullPitch = fullCockpit.source.pitch = fullExternal.source.pitch = fullPitch;
    }
}
