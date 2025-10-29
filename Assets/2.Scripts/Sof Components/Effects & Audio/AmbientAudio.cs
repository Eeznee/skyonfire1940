using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AmbientAudio : SofComponent
{
    public AudioClip ruralClip;
    public AudioClip oceanAmbient;
    SofSmartAudioSource ambientAudioSource;


    private bool currentlyPlayingOcean;

    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);
        ambientAudioSource = new SofSmartAudioSource(objectAudio, ruralClip, SofAudioGroup.External, false, UpdateAudio);
        currentlyPlayingOcean = false;
    }
    public void UpdateAudio()
    {
        float relativeAltitude = data.relativeAltitude.Get;
        float targetVolume = Mathf.InverseLerp(50f, 0f, relativeAltitude) * 0.1f;

        ambientAudioSource.source.volume = Mathf.MoveTowards(ambientAudioSource.source.volume, targetVolume, Time.deltaTime);

        if (targetVolume > 0f)
        {
            bool ocean = Mathf.Abs(relativeAltitude - data.altitude.Get) < 1f;

            if(ocean != currentlyPlayingOcean)
            {
                currentlyPlayingOcean = ocean;
                ambientAudioSource.source.clip = ocean ? oceanAmbient : ruralClip;
                ambientAudioSource.Play();
            }
        }
    }
}
