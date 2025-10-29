using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum SofAudioGroup
{
    External,
    Cockpit,
    Persistent
}
public class ObjectAudio : SofComponent
{
    private SofSmartAudioSource externalClipsPlayer { get; set; }
    private SofSmartAudioSource cockpitClipsPlayer { get; set; }
    private SofSmartAudioSource persistentClipsPlayer { get; set; }

    public GameObject localHolder { get; private set; }
    public GameObject globalHolder { get; private set; }

    private List<SofSmartAudioSource> sofAudios = new List<SofSmartAudioSource>(0);
    private List<SofSmartAudioSource> localSofAudios = new List<SofSmartAudioSource>(0);
    private List<SofSmartAudioSource> globalSofAudios = new List<SofSmartAudioSource>(0);

    public bool AudioListenerIsAttachedToThisObject => SofAudioListener.attachedTransform && transform.IsChildOf(SofAudioListener.attachedTransform);
    public bool ActiveAudio => true;


    public float SqrDistanceToListener => (tr.position - SofAudioListener.instance.transform.position).sqrMagnitude;

    public void AddSofAudio(SofSmartAudioSource sa)
    {
        sofAudios.Add(sa);
        if (sa.global) globalSofAudios.Add(sa);
        else localSofAudios.Add(sa);
    }
    public void RemoveSofAudio(SofSmartAudioSource sa)
    {
        sofAudios.Remove(sa);
    }

    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);
    }
    private void OnEnable()
    {
        SofAudioListener.allObjectAudios.Add(this);
        SofAudioListener.OnAttachToNewTransform += OnAttachListenerNewTransform;
    }
    private void OnDisable()
    {
        SofAudioListener.allObjectAudios.Remove(this);
        SofAudioListener.allowedObjectsAudios.Remove(this);
        SofAudioListener.OnAttachToNewTransform -= OnAttachListenerNewTransform;
    }
    public void PlayRandomClip(AudioClip[] clips, float volume, SofAudioGroup group, bool global)
    {
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        PlayAudioClip(clip, volume, group, global);
    }
    public void PlayAudioClip(AudioClip clip, float volume, SofAudioGroup group, bool global)
    {
        if (group == SofAudioGroup.Cockpit) global = false;

        if (global && !globalHolder.activeInHierarchy) return;
        if (!global && !localHolder.activeInHierarchy) return;

        switch (group)
        {
            case SofAudioGroup.External:
                externalClipsPlayer.PlayOneShot(clip, volume);
                break;
            case SofAudioGroup.Persistent:
                persistentClipsPlayer.PlayOneShot(clip, volume);
                break;
            case SofAudioGroup.Cockpit:
                cockpitClipsPlayer.PlayOneShot(clip, volume);
                break;
        }
    }
    private void Awake()
    {
        localHolder = transform.CreateChild("Local Sounds Holder").gameObject;
        globalHolder = transform.CreateChild("Global Sounds Holder").gameObject;

        externalClipsPlayer = new SofSmartAudioSource(this, null, SofAudioGroup.External, true, null);
        persistentClipsPlayer = new SofSmartAudioSource(this, null, SofAudioGroup.Persistent, true, null);
        cockpitClipsPlayer = new SofSmartAudioSource(this, null, SofAudioGroup.Cockpit, false, null);

        externalClipsPlayer.source.volume = persistentClipsPlayer.source.volume = cockpitClipsPlayer.source.volume = 1f;

        localHolder.SetActive(false);
        globalHolder.SetActive(false);
    }
    public void UpdateSofSmartAudioSources()
    {
        if (globalHolder.activeInHierarchy)
        {
            foreach (SofSmartAudioSource smartAudioSource in globalSofAudios)
            {
                smartAudioSource.updateAudio?.Invoke();
            }
        }
        if (localHolder.activeInHierarchy)
        {
            foreach (SofSmartAudioSource smartAudioSource in localSofAudios)
            {
                smartAudioSource.updateAudio?.Invoke();
            }
        }
    }

    public void OnAttachListenerNewTransform()
    {
        localHolder.SetActive(AudioListenerIsAttachedToThisObject);
        foreach (SofSmartAudioSource sa in sofAudios)
        {
            sa.UpdatePriorityAndSpatialization();
        }

        SofAudioListener.TryToAdd(this);
    }
    public void EnableFadeIn()
    {
        if (!gameObject.activeInHierarchy)
        {
            globalHolder.SetActive(true);
            return;
        }
        StopAllCoroutines();
        StartCoroutine(EnableFadeInCoroutine());
    }
    public void DisableFadeOut()
    {
        if (!gameObject.activeInHierarchy)
        {
            globalHolder.SetActive(false);
            return;
        }
        StopAllCoroutines();
        StartCoroutine(DisableFadeOutCoroutine());
    }

    IEnumerator EnableFadeInCoroutine()
    {
        globalHolder.SetActive(true);

        float volume = 0f;

        foreach (SofSmartAudioSource sa in globalSofAudios)
        {
            sa.v = Mathf.Clamp01(sa.source.volume);
        }
        while (volume < 1f)
        {
            volume = Mathf.Clamp01(volume + Time.unscaledDeltaTime * 0.5f);
            foreach (SofSmartAudioSource sa in globalSofAudios)
            {
                sa.source.volume = Mathf.Min(volume,Mathf.Max(sa.v,sa.source.volume));
            }
            yield return null;
        }
    }
    IEnumerator DisableFadeOutCoroutine()
    {
        float volume = 1f;
        while (volume > 0f)
        {
            volume -= Time.unscaledDeltaTime * 0.5f;
            foreach (SofSmartAudioSource sa in globalSofAudios)
            {
                sa.source.volume = Mathf.Min(sa.source.volume,volume);
            }
            yield return null;
        }

        globalHolder.SetActive(false);
    }

}

