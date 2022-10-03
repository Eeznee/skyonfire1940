using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GunsAudio : AudioVisual
{
    bool firing = false;

    public Gun[] references;

    public AudioClip gunLoop;
    public AudioClip gunEnd;
    public AudioClip cockpitGunLoop;
    public AudioClip cockpitGunEnd;

    public bool vibrate = false;
    public float vibrationsIntensity = 0.2f;

    SofAudio gunSource;
    SofAudio cockpitGunSource;

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d,firstTime);

        if (firstTime)
        {
            if (references.Length == 0) Debug.LogError("This Gun Audio has no reference gun", this);

            gunSource = new SofAudio(avm, gunLoop, SofAudioGroup.External, true, false);
            cockpitGunSource = new SofAudio(avm, cockpitGunLoop, SofAudioGroup.Cockpit, false, false);
            gunSource.source.dopplerLevel = cockpitGunSource.source.dopplerLevel = 0f;
        }
    }

    public void Update()
    {
        if (Time.deltaTime == 0f) return;

        bool updateFiring = false;
        foreach (Gun gun in references)
            updateFiring = gun.Firing() || updateFiring;
        
        if (updateFiring != firing) Trigger(updateFiring && Time.timeScale > 0f);

        if (firing)
        {
            if (vibrate) VibrationsManager.SendVibrations(vibrationsIntensity, 0.15f, aircraft);
        } else 
        {
            gunSource.source.volume = cockpitGunSource.source.volume = Mathf.Clamp01(gunSource.source.volume - Time.deltaTime * references[0].gunPreset.FireRate/60f);
            if (gunSource.source.volume == 0f)
            {
                gunSource.Stop();
                cockpitGunSource.Stop();
            }
        }
    }

    public void Trigger(bool f)
    {
        firing = f;
        if (firing)
        {
            gunSource.Play();
            cockpitGunSource.Play();
            float delay = Random.Range(0f, Mathf.Min(gunLoop.length,cockpitGunLoop.length));
            if (references[0].gunPreset.FireRate < 400f) delay = 60f / references[0].gunPreset.FireRate;
            gunSource.source.time = cockpitGunSource.source.time = delay;
            gunSource.source.volume = cockpitGunSource.source.volume = 1f;
        } else
        {
            avm.external.global.PlayOneShot(gunEnd);
            avm.cockpit.local.PlayOneShot(cockpitGunEnd);
        }
    }
}
