using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MechanicalAudio : AudioVisual
{
    private Wheel[] wheels;
    public AudioClip bendingClip;
    public AudioClip overSpeedClip;
    public AudioClip overSpeedCockpitClip;
    public AudioClip wheelRollClip;

    SofAudio wheelRoll;
    SofAudio bending;
    SofAudio overSpeed;
    SofAudio overSpeedCockpit;

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        if (firstTime)
        {
            wheels = sofObject.GetComponentsInChildren<Wheel>();
            avm = sofObject.avm;
            bending = new SofAudio(avm, bendingClip, SofAudioGroup.Cockpit, false);
            overSpeed = new SofAudio(avm, overSpeedClip, SofAudioGroup.External, false);
            overSpeedCockpit = new SofAudio(avm, overSpeedCockpitClip, SofAudioGroup.Cockpit, false);
            wheelRoll = new SofAudio(avm, wheelRollClip, SofAudioGroup.Cockpit, false);

            overSpeed.source.volume = overSpeedCockpit.source.volume = bending.source.volume = 0f;
        }
    }

    public void Update()
    {
        if (aircraft != PlayerManager.player.aircraft) return;
        //VibrationsManager.SendVibrations(Mathf.InverseLerp(aircraft.maxG * 0.65f, aircraft.maxG, data.gForce), 0.3f, aircraft);
        VibrationsManager.SendVibrations(Mathf.InverseLerp(aircraft.maxSpeed * 0.85f, aircraft.maxSpeed, data.ias.Get), 0.3f, aircraft);
        if (aircraft.data.ias.Get > 50f) VibrationsManager.SendVibrations(Mathf.InverseLerp(16f * 0.9f, 16f * 1f, data.angleOfAttack.Get), 0.3f, aircraft);
        float targetVolume = Mathf.InverseLerp(aircraft.maxG * 0.5f, aircraft.maxG, data.gForce);
        bending.source.volume = Mathf.Lerp(bending.source.volume, targetVolume, Time.deltaTime);
        overSpeed.source.volume = overSpeedCockpit.source.volume = Mathf.InverseLerp(aircraft.maxSpeed * 0.7f, aircraft.maxSpeed * 1.1f, data.ias.Get);

        //Wheels
        bool wheelGrounded = false;
        foreach (Wheel wheel in wheels)
        {
            if (false && wheel.data == data) wheelGrounded = wheelGrounded || wheel.wheel.isGrounded;
        }
        targetVolume = wheelGrounded ? Mathf.InverseLerp(5f, 20f, data.gsp.Get) : 0f;
        wheelRoll.source.volume = Mathf.MoveTowards(wheelRoll.source.volume, targetVolume, Time.deltaTime * 5f);
    }
}
