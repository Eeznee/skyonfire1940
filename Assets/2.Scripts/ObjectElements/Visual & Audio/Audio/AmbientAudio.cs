using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AmbientAudio : AudioVisual
{
    public AudioClip ruralClip;

    SofAudio rural;

    public void Start()
    {
        avm = sofObject.avm;
        rural = new SofAudio(avm, ruralClip, SofAudioGroup.External,false, true);
        rural.source.volume = 0f;
    }

    public void Update()
    {
        if (aircraft != GameManager.player.aircraft) return;
        float targetVolume = Mathf.InverseLerp(50f, 0f, data.relativeAltitude) * 0.1f;
        rural.source.volume = Mathf.MoveTowards(rural.source.volume, targetVolume, Time.deltaTime);
    }
}
