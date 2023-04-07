using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunsGroupAudio : MonoBehaviour
{
    public GunPreset preset;
    public GunAudioSample sample;
    public List<Gun> guns;
    private AVM avm;

    private SofAudio outSource;
    private SofAudio inSource;

    private bool firing = false;

    public void Initialize(Gun firstGun, AVM _avm)
    {
        guns = new List<Gun>();
        guns.Add(firstGun);
        preset = firstGun.gunPreset;
        avm = _avm;
        sample = preset.audioSamples[0];
    }
    private void Start()
    {
        outSource = new SofAudio(avm, null, SofAudioGroup.External, true);
        inSource = new SofAudio(avm, null, SofAudioGroup.Cockpit, false);

        foreach(Gun gun in guns)
            gun.OnFireEvent += ShotFired;
    }
    private int GunsFiring()
    {
        int amount = 0;
        foreach (Gun gun in guns) if (gun && gun.Firing()) amount++;
        return amount;
    }
    IEnumerator AudioCycle()
    {
        //Guns Started Firing
        firing = true;
        yield return null;
        sample = GunAudioSample.GetBestSample(preset.audioSamples, GunsFiring());
        outSource.source.clip = sample.autoOut;
        inSource.source.clip = sample.autoIn;
        outSource.Play();
        inSource.Play();
        float delay = Random.Range(0f, Mathf.Min(outSource.source.clip.length, inSource.source.clip.length));
        delay -= delay % (60f / preset.FireRate);
        outSource.source.time = inSource.source.time = delay;
        outSource.source.volume = inSource.source.volume = 1f;

        do yield return new WaitForSeconds(60f / preset.FireRate);
        while (GunsFiring() > 0);

        //Guns Stopped Firing

        PlayEndClips();
        float volume = outSource.source.volume;
        while (volume > 0f)
        {
            volume -= Time.deltaTime * preset.FireRate / 60f;
            volume = Mathf.Clamp01(volume);
            outSource.source.volume = inSource.source.volume = volume;
            yield return null;
        }
        outSource.Stop();
        inSource.Stop();
        firing = false;
    }
    private void PlayEndClips()
    {
        if (sample.endOut) avm.external.global.PlayOneShot(sample.endOut);
        if (sample.endIn) avm.cockpit.local.PlayOneShot(sample.endIn);
    }
    private void ShotFired()
    {
        if (preset.singleShotsAudio)
            PlayEndClips();
        else if (!firing) StartCoroutine(AudioCycle());
    }
}
public class GunsAudio : AudioVisual
{
    private List<GunsGroupAudio> groups;

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);

        if (firstTime)
        {
            groups = new List<GunsGroupAudio>();
            Gun[] allGuns = sofObject.GetComponentsInChildren<Gun>();
            for (int i = 0; i < allGuns.Length; i++)
            {
                Gun gun = allGuns[i];
                GunPreset preset = gun.gunPreset;

                bool isNewPreset = true;
                foreach (GunsGroupAudio group in groups)
                    if (group.preset == preset) { isNewPreset = false; group.guns.Add(gun); }
                if (isNewPreset)
                {
                    GunsGroupAudio newGroup = gameObject.AddComponent<GunsGroupAudio>();
                    newGroup.Initialize(gun, avm);
                    groups.Add(newGroup);
                }
            }
        }
    }
}
