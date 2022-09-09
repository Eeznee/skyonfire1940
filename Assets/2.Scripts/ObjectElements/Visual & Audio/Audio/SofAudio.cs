using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SofAudio
{
    public AudioSource source;
    public SofAudioGroup group;
    private bool global;
    private bool playAtStart;

    public SofAudio(AVM avm,  AudioClip clip,SofAudioGroup g, bool _global, bool play)
    {
        global = _global;
        group = g;
        
        GameObject holder = global ? avm.globalHolder : avm.localHolder;
        source = holder.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = 1f;
        source.loop = true;
        source.minDistance = 300f;
        source.maxDistance = 2000f;
        source.outputAudioMixerGroup = SofAudioListener.GetAudioMixer(group);
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.spatialBlend = source.dopplerLevel = global ? 1f : 0f;
        if (source.clip) source.time = Random.Range(0f, clip.length);
        if (play && holder.activeSelf) source.PlayDelayed(0.05f);

        avm.AddSofAudio(this);
    }

    public void PlayOneShot(AudioClip clip, float volume)
    {
        if (!source.gameObject.activeInHierarchy) return;
        source.PlayOneShot(clip, volume);
    }
    public void PlayOneShot(AudioClip clip)
    {
        PlayOneShot(clip, 1f);
    }
    public void PlayOneRandom(AudioClip[] clips,float volume)
    {
        if (!source.gameObject.activeInHierarchy) return;
        int index = Random.Range(0, clips.Length);
        source.PlayOneShot(clips[index],volume);
    }
    public void PlayOneRandom(AudioClip[] clips)
    {
        PlayOneRandom(clips, 1f);
    }
    public void Play()
    {
        if (source.enabled && source.gameObject.activeInHierarchy) source.Play();
    }
    public void Stop()
    {
        if (source.enabled && source.gameObject.activeInHierarchy) source.Stop();
    }
}
