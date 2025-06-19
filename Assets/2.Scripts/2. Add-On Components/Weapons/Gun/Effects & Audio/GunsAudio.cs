using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunsGroupAudio : MonoBehaviour
{
    public GunPreset preset;
    public GunAudioSample sample;
    public List<Gun> guns;
    private ObjectAudio avm;

    private SofSmartAudioSource outSource;
    private SofSmartAudioSource inSource;

    private bool firing = false;

    public void Initialize(Gun firstGun, ObjectAudio _avm)
    {
        guns = new List<Gun>();
        guns.Add(firstGun);
        preset = firstGun.gunPreset;
        avm = _avm;
        sample = preset.audioSamples[0];
    }
    private void Start()
    {
        outSource = new SofSmartAudioSource(avm, null, SofAudioGroup.External, true, null);
        inSource = new SofSmartAudioSource(avm, null, SofAudioGroup.Cockpit, false, null);

        outSource.CancelDoppler();
        inSource.CancelDoppler();

        foreach (Gun gun in guns)
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
        outSource.source.enabled = true;
        inSource.source.enabled = true;

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

        do
        {
            float totalVibrations = 0f;

            foreach(Gun gun in guns)
            {
                if (gun && !gun.gunPreset.singleShotsAudio && gun.Firing())
                {
                    float vibrationsPower = preset.FireRate * preset.ammunition.mass * 0.06f;
                    float sqrDistance = (SofCamera.tr.position - gun.tr.position).magnitude;
                    totalVibrations += vibrationsPower / Mathf.Max(sqrDistance, 1f);
                }

                totalVibrations = Mathf.Log(totalVibrations + 1f, 10f);
            }
            if (avm.sofModular == Player.modular) VibrationsManager.SendVibrations(totalVibrations, 0.15f);

            yield return new WaitForSeconds(60f / preset.FireRate);
        }
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
        outSource.source.enabled = false;
        inSource.source.enabled = false;
        firing = false;
    }
    private void PlayEndClips()
    {
        if (sample.endOut) avm.globalExternalClipsPlayer.PlayOneShot(sample.endOut, 1f);
        if (sample.endIn) avm.localCockpitClipsPlayer.PlayOneShot(sample.endIn, 1f);
    }
    private void ShotFired(float delay)
    {
        if (preset.singleShotsAudio)
        {
            PlayEndClips();
            if (avm.sofModular == Player.modular) VibrationsManager.SendVibrations(1f, 0.25f);
        }
        else if (!firing) StartCoroutine(AudioCycle());
    }
}
public class GunsAudio : SofComponent
{
    private List<GunsGroupAudio> groups;

    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);

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
                newGroup.Initialize(gun, objectAudio);
                groups.Add(newGroup);
            }
        }
    }
}
