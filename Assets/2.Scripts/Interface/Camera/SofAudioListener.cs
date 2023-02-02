using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SofAudioListener : MonoBehaviour
{
    public AudioMixerGroup external;
    public AudioMixerGroup cockpit;
    public AudioMixerGroup persistent;

    public static AudioListener listener;
    public static Vector3 position;
    public static Transform tr;
    AudioMixer mixer;
    float cockpitRatio = 1f;

    void Awake()
    {
        mixer = GameManager.gm.mixer;
        StartCoroutine(FadeVolumeIn());
        listener = gameObject.AddComponent<AudioListener>();
        tr = transform;
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
        if (GameManager.gm.vr) return PlayerManager.player.crew.transform;
        if (PlayerCamera.subCam.pos == CamPosition.Free || PlayerManager.player.crew == null)
            return PlayerCamera.camTr;

        return PlayerManager.player.crew.transform;
    }

    void Update()
    {
        //Cockpit volume
        bool firstPerson = GameManager.gm.vr || PlayerCamera.subCam.pos == CamPosition.FirstPerson || PlayerCamera.subCam.pos == CamPosition.Bombsight;
        float targetRatio = firstPerson ? PlayerManager.player.crew.audioCockpitRatio : 0f;
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

    IEnumerator FadeVolumeIn()
    {
        float volume = -80f;
        float finalVolume;
        mixer.GetFloat("MasterVolume", out finalVolume);

        while (volume < finalVolume)
        {
            volume = Mathf.MoveTowards(volume, finalVolume, Time.deltaTime * 80f);
            mixer.SetFloat("MasterVolume", volume);
            yield return null;
        }
    }
}
