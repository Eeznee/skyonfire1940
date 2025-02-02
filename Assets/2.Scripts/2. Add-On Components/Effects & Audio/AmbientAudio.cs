using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AmbientAudio : AudioComponent
{
    public AudioClip ruralClip;

    SofAudio rural;

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        rural = new SofAudio(avm, ruralClip, SofAudioGroup.External, false);
    }

    public void Update()
    {
        if (aircraft != Player.aircraft) return;
        float targetVolume = Mathf.InverseLerp(50f, 0f, data.relativeAltitude.Get) * 0.1f;
        rural.source.volume = Mathf.MoveTowards(rural.source.volume, targetVolume, Time.deltaTime);
    }
}
