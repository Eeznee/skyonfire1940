using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GunsAudio : AudioVisual
{
    private bool firing = false;
    private bool smoothOut = false;

    public Gun[] references;

    public AudioClip gunLoop;
    public AudioClip gunEnd;
    public AudioClip cockpitGunLoop;
    public AudioClip cockpitGunEnd;

    public bool vibrate = false;
    public float vibrationsIntensity = 0.2f;

    private bool singleFire;
    private SofAudio gunSource;
    private SofAudio cockpitGunSource;

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d,firstTime);

        if (firstTime)
        {
            if (references.Length == 0) Debug.LogError("This Gun Audio has no reference gun", this);

            if (vibrate) foreach(Gun gun in references) gun.OnFireEvent += Vibrate;

            singleFire = references[0].gunPreset.FireRate <= 150f;
            if (singleFire)
            {
                references[0].OnFireEvent += PlaySingleShot;
            } else
            {
                gunSource = new SofAudio(avm, gunLoop, SofAudioGroup.External, true);
                cockpitGunSource = new SofAudio(avm, cockpitGunLoop, SofAudioGroup.Cockpit, false);
            }
        }
    }
    private void PlaySingleShot()
    {
        avm.external.global.PlayOneShot(gunLoop);
        avm.cockpit.local.PlayOneShot(cockpitGunLoop);
    }
    private void Vibrate()
    {
        VibrationsManager.SendVibrations(vibrationsIntensity, 0.15f, aircraft);
    }
    
    public void Update()
    {
        if (Time.timeScale == 0f || singleFire) return;

        bool updateFiring = false;
        foreach (Gun gun in references)
            updateFiring = gun.Firing() || updateFiring;
        
        if (updateFiring != firing) Trigger(updateFiring);

        if (!firing && smoothOut)
        {
            float volume = Mathf.Clamp01(gunSource.source.volume - Time.deltaTime * references[0].gunPreset.FireRate / 60f);
            gunSource.source.volume = cockpitGunSource.source.volume = volume;
            if (volume <= 0f)
            {
                gunSource.Stop();
                cockpitGunSource.Stop();
                smoothOut = false;
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

            smoothOut = true;
        } else
        {
            avm.external.global.PlayOneShot(gunEnd);
            avm.cockpit.local.PlayOneShot(cockpitGunEnd);
        }
    }
}
