using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AmbientAudio : AudioVisual
{
    public AudioClip ruralClip;

    SofAudio rural;

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        if (firstTime)
        {
            rural = new SofAudio(avm, ruralClip, SofAudioGroup.External, false);
        }
    }

    public void Update()
    {
        if (aircraft != PlayerManager.player.aircraft) return;
        float targetVolume = Mathf.InverseLerp(50f, 0f, data.relativeAltitude) * 0.1f;
        rural.source.volume = Mathf.MoveTowards(rural.source.volume, targetVolume, Time.deltaTime);
    }
}
