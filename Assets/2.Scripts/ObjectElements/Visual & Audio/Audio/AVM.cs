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
public class SofAudioType
{
    public SofAudioGroup group;
    public AudioMixerGroup mixer;
    public SofAudio local;
    public SofAudio global;

    public SofAudioType(SofAudioGroup g, AudioMixerGroup m,AVM avm)
    {
        group = g;
        mixer = m;
        local = new SofAudio(avm, null, g, false);
        global = new SofAudio(avm, null, g, true);
        local.Stop();
        global.Stop();
        local.source.playOnAwake = global.source.playOnAwake = false;
        local.source.volume = global.source.volume = 1f;
    }
}
public class AVM : ObjectElement
{
    [HideInInspector] public SofAudioType external;
    [HideInInspector] public SofAudioType cockpit;
    [HideInInspector] public SofAudioType persistent;

    [HideInInspector] public GameObject localHolder;
    [HideInInspector] public GameObject globalHolder;

    private List<SofAudio> sofAudios = new List<SofAudio>(0);
    private List<SofAudio> sofAudiosCockpit = new List<SofAudio>(0);

    [HideInInspector] public bool localActive;
    private bool use3dSound = false;

    public void AddSofAudio(SofAudio sa)
    {
        sofAudios.Add(sa);
        if (sa.group == SofAudioGroup.Cockpit) sofAudiosCockpit.Add(sa);
    }

    private void Awake()
    {
        localHolder = new GameObject("Local Sounds Holder");
        globalHolder = new GameObject("Global Sounds Holder");
        localHolder.transform.SetParent(transform);
        globalHolder.transform.SetParent(transform);
        localHolder.transform.localPosition = globalHolder.transform.localPosition = Vector3.zero;

        SofAudioListener l = GameManager.gm.listener;
        external = new SofAudioType(SofAudioGroup.External, l.external,this);
        cockpit = new SofAudioType(SofAudioGroup.Cockpit, l.cockpit, this);
        persistent = new SofAudioType(SofAudioGroup.Persistent, l.persistent, this);

        use3dSound = true;
    }
    private void Start()
    {
        UpdatePlayer(PlayerManager.player.sofObj == sofObject);
    }
    private void Update()
    {
        bool newUse3dSound = PlayerManager.player.sofObj != sofObject || (GameManager.gm.vr ? false : PlayerCamera.customCam.pos == CamPosition.Free);
        if (newUse3dSound != use3dSound)
            UpdatePlayer(newUse3dSound);
    }
    public void UpdatePlayer(bool newUse3dSound)
    {
        use3dSound = newUse3dSound;
        localActive = !use3dSound;
        localHolder.SetActive(localActive);
        foreach (SofAudio sa in sofAudios) sa.source.spatialBlend = sa.source.dopplerLevel = use3dSound ? 1f : 0f;
    }
}

