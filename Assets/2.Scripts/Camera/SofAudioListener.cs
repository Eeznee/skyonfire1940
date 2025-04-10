using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SofAudioListener : MonoBehaviour
{
    public AudioMixerGroup external;
    public AudioMixerGroup cockpit;
    public AudioMixerGroup persistent;
    public AudioMixerGroup boostedEngineCockpit;
    public AudioMixerGroup boostedEngineExternal;

    public static SofAudioListener instance;
    public static AudioListener listener;
    public static Vector3 position;
    public static Transform tr;
    AudioMixer mixer;
    float cockpitRatio = 1f;

    public static bool AttachedToSofObject(SofObject sofObj) { return tr && sofObj.tr.root == tr.root; }

    private void OnEnable()
    {
        instance = this;
        tr = transform;
        listener = gameObject.AddComponent<AudioListener>();
        mixer = GameManager.gm.mixer;
        StartCoroutine(FadeVolumeIn());

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
        if (!SofCamera.subCam.targetsPlayer) return SofCamera.subCam.Target().tr;
        return Player.crew.tr;
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
            tr.parent = CurrentParent();
            tr.localPosition = Vector3.zero;
            tr.localRotation = Quaternion.identity;
        }
        position = tr.position;
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
}
