using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SofSmartAudioSource
{
    public AudioSource source;
    public SofAudioGroup group;
    private ObjectAudio avm;
    public bool global;

    public delegate void UpdateAudioDelegate();
    public UpdateAudioDelegate updateAudio;

    private bool useDoppler;

    public float v;

    public SofSmartAudioSource(ObjectAudio _avm,  AudioClip clip,SofAudioGroup g, bool _global, UpdateAudioDelegate updateAudioFunction)
    {
        updateAudio = updateAudioFunction;
        avm = _avm;
        global = _global;
        group = g;
        GameObject holder = global ? avm.globalHolder : avm.localHolder;
        source = holder.AddComponent<AudioSource>();
        source.playOnAwake = true;
        source.clip = clip;
        source.volume = 0f;
        source.loop = true;
        source.minDistance = 300f;
        source.maxDistance = 2000f;
        source.outputAudioMixerGroup = SofAudioListener.GetAudioMixer(group);
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        UpdatePriorityAndSpatialization();
        if (source.clip) source.time = Random.Range(0f, clip.length);
        if (holder.activeInHierarchy) source.Play();
        useDoppler = true;

        avm.AddSofAudio(this);
    }

    public void CancelDoppler()
    {
        useDoppler = false;
        source.dopplerLevel = 0f;
    }

    public void UpdatePriorityAndSpatialization()
    {
        bool audioListenerTarget = avm.AudioListenerIsAttachedToThisObject;
        source.priority = audioListenerTarget ? 0 : 128;
        source.spatialize = !audioListenerTarget;
        source.dopplerLevel = audioListenerTarget ? 0f : (useDoppler ? 1f : 0f);
        source.spatialBlend = audioListenerTarget ? 0f : 1f;
    }

    public bool Enabled()
    {
        return (global ? avm.globalHolder : avm.localHolder).activeInHierarchy;
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
        if (source.enabled && source.gameObject.activeInHierarchy)source.Play();
    }
    public void Stop()
    {
        if (source.enabled && source.gameObject.activeInHierarchy) source.Stop();
    }

    public void Destroy()
    {
        Stop();
        avm.RemoveSofAudio(this);
        Object.Destroy(source);
    }
    
}
