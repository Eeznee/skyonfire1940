using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MechanicalAudio : SofComponent
{
    private Wheel[] wheels;
    public AudioClip bendingClip;
    public AudioClip overSpeedClip;
    public AudioClip overSpeedCockpitClip;
    public AudioClip wheelRollClip;

    SofSmartAudioSource wheelRoll;
    SofSmartAudioSource bending;
    SofSmartAudioSource overSpeed;
    SofSmartAudioSource overSpeedCockpit;

    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);
        wheels = sofModular.GetComponentsInChildren<Wheel>();
        bending = new SofSmartAudioSource(objectAudio, bendingClip, SofAudioGroup.Cockpit, false, UpdateAudioBending);
        overSpeed = new SofSmartAudioSource(objectAudio, overSpeedClip, SofAudioGroup.External, false, UpdateAudioOverspeed);
        overSpeedCockpit = new SofSmartAudioSource(objectAudio, overSpeedCockpitClip, SofAudioGroup.Cockpit, false, UpdateAudioOverspeedCockpit);
        wheelRoll = new SofSmartAudioSource(objectAudio, wheelRollClip, SofAudioGroup.Cockpit, false, UpdateAudioWheels);

        overSpeed.source.volume = overSpeedCockpit.source.volume = bending.source.volume = 0f;
    }
    public void UpdateAudioOverspeed()
    {
        VibrationsManager.SendVibrations(Mathf.InverseLerp(aircraft.SpeedLimitMps * 0.85f, aircraft.SpeedLimitMps, data.ias.Get), 0.3f);
        if (aircraft.data.ias.Get > 50f) VibrationsManager.SendVibrations(Mathf.InverseLerp(14f, 16f, data.angleOfAttack.Get), 0.3f);

        overSpeed.source.volume =Mathf.InverseLerp(aircraft.SpeedLimitMps * 0.7f, aircraft.SpeedLimitMps * 1.1f, data.ias.Get);
    }
    public void UpdateAudioOverspeedCockpit()
    {
        overSpeedCockpit.source.volume = Mathf.InverseLerp(aircraft.SpeedLimitMps * 0.7f, aircraft.SpeedLimitMps * 1.1f, data.ias.Get);
    }
    public void UpdateAudioBending()
    {
        float targetVolume = Mathf.InverseLerp(aircraft.MaxGForce * 0.5f, aircraft.MaxGForce, data.gForce);
        bending.source.volume = Mathf.Lerp(bending.source.volume, targetVolume, Time.deltaTime);
    }
    public void UpdateAudioWheels()
    {
        bool wheelGrounded = false;
        foreach (Wheel wheel in wheels)
        {
            if (false && wheel.data == data) wheelGrounded = wheelGrounded || wheel.grounded;
        }
        float targetVolume = wheelGrounded ? Mathf.InverseLerp(5f, 20f, data.gsp.Get) : 0f;
        wheelRoll.source.volume = Mathf.MoveTowards(wheelRoll.source.volume, targetVolume, Time.deltaTime * 5f);
    }
}
