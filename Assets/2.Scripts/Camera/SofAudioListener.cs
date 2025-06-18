using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SofAudioListener : MonoBehaviour
{
    public static List<ObjectAudio> allowedObjectsAudios = new List<ObjectAudio>();
    public static List<ObjectAudio> allObjectAudios = new List<ObjectAudio>();
    public const int maxAllowedObjectsAudios = 5;
    public const float sqrDistanceLimit = 3000f * 3000f;

    public AudioMixerGroup external;
    public AudioMixerGroup cockpit;
    public AudioMixerGroup persistent;
    public AudioMixerGroup boostedEngineCockpit;
    public AudioMixerGroup boostedEngineExternal;

    public static SofAudioListener instance;
    public static AudioListener listener;

    public static Vector3 Position => tr.position;
    public static Transform tr;
    public static AudioSource localSource;
    AudioMixer mixer;
    float cockpitRatio = 1f;

    public static Action OnAttachToNewTransform;

    public static Transform attachedTransform => tr ? tr.parent : tr;

    private int count;

    private void OnEnable()
    {
        instance = this;
        tr = transform;
        listener = this.GetCreateComponent<AudioListener>();
        mixer = GameManager.gm.mixer;
        StartCoroutine(FadeVolumeIn());

        localSource = gameObject.AddComponent<AudioSource>();
        localSource.spatialBlend = 0f;

        allowedObjectsAudios = new List<ObjectAudio>();

        AudioListener.volume = 1f;
        TimeManager.OnPauseEvent += OnPause;
    }
    private void OnDisable()
    {
        TimeManager.OnPauseEvent -= OnPause;
    }
    public static AudioMixerGroup GetAudioMixer(SofAudioGroup group)
    {
        switch (group)
        {
            case SofAudioGroup.Cockpit: return GameManager.gm.listener.cockpit;
            case SofAudioGroup.External: return GameManager.gm.listener.external;
            case SofAudioGroup.Persistent: return GameManager.gm.listener.persistent;
        }
        
        return null;
    }
    private Transform CurrentParent()
    {
        if (SofCamera.subCam.logic.BasePosMode == CamPos.World) return SofCamera.tr;
        return SofCamera.subCam.Target().transform;
    }
    private void OnPause()
    {
        AudioListener.volume = TimeManager.paused ? 0.2f : 1f;
    }
    void Update()
    {
        //Cockpit volume
        bool firstPerson = GameManager.gm.vr || SofCamera.viewMode == 1 || SofCamera.viewMode == 3;
        float targetRatio = firstPerson ? Player.seat.CockpitAudioRatio : 0f;
        cockpitRatio = Mathf.MoveTowards(cockpitRatio, targetRatio, 5f * Time.deltaTime);
        mixer.SetFloat("CockpitVolume", Mathf.Log10(cockpitRatio + 0.0001f) * 20);
        float externalVol = Mathf.Log10(1f - cockpitRatio + 0.0001f) * 20;
        mixer.SetFloat("ExternalVolume", externalVol);

        mixer.SetFloat("Pitch", Mathf.Max(1/32f,Time.timeScale));

        if (tr.parent != CurrentParent())
        {
            AttachToNewParent();
        }
        tr.rotation = SofCamera.tr.rotation;

        foreach(ObjectAudio objectAudio in allowedObjectsAudios)
        {
            objectAudio.UpdateSofSmartAudioSources();
        }

        count = (count + 1) % allObjectAudios.Count;
        TryToAdd(allObjectAudios[count]);

        allowedObjectsAudios.Sort((a1, a2) => a1.SqrDistanceToListener.CompareTo(a2.SqrDistanceToListener));
    }

    private void AttachToNewParent()
    {
        enabled = false;

        tr.parent = CurrentParent();
        tr.localPosition = Vector3.zero;

        foreach (ObjectAudio objectAudio in allowedObjectsAudios)
        {
            objectAudio.DisableFadeOut();
        }
        allowedObjectsAudios = new List<ObjectAudio>();

        enabled = true;

        OnAttachToNewTransform?.Invoke();
    }


    const float speed = 3f;

    IEnumerator FadeVolumeIn()
    {
        float volume = -40f;
        mixer.GetFloat("MasterVolume", out float finalVolume);

        while (volume < finalVolume)
        {
            volume = Mathf.MoveTowards(volume, finalVolume, Time.deltaTime * 80f * speed);
            mixer.SetFloat("MasterVolume", volume);
            yield return null;
        }
    }

    public static void TryToAdd(ObjectAudio objectAudio)
    {
        if (!objectAudio.gameObject.activeInHierarchy) return;
        if (allowedObjectsAudios.Contains(objectAudio)) return;

        if (allowedObjectsAudios.Count < maxAllowedObjectsAudios)
        {
            allowedObjectsAudios.Add(objectAudio);
            objectAudio.EnableFadeIn();
            return;
        }

        float sqrDistance = objectAudio.SqrDistanceToListener;

        if (sqrDistance > sqrDistanceLimit) return;

        if (sqrDistance < allowedObjectsAudios[^1].SqrDistanceToListener)
        {
            allowedObjectsAudios[^1].DisableFadeOut();
            allowedObjectsAudios.RemoveAt(allowedObjectsAudios.Count - 1);

            allowedObjectsAudios.Add(objectAudio);
            objectAudio.EnableFadeIn();

            allowedObjectsAudios.Sort((a1, a2) => a1.SqrDistanceToListener.CompareTo(a2.SqrDistanceToListener));
        }
    }
}
