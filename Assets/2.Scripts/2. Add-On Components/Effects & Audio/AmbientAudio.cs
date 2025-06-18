using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AmbientAudio : SofComponent
{
    public AudioClip ruralClip;

    SofSmartAudioSource rural;

    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);
        rural = new SofSmartAudioSource(objectAudio, ruralClip, SofAudioGroup.External, false, UpdateAudio);
    }
    public void UpdateAudio()
    {
        float targetVolume = Mathf.InverseLerp(50f, 0f, data.relativeAltitude.Get) * 0.1f;
        rural.source.volume = Mathf.MoveTowards(rural.source.volume, targetVolume, Time.deltaTime);
    }
}
