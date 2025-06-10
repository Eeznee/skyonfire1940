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

    public SofAudioType(SofAudioGroup g, AudioMixerGroup m,ObjectAudio oa)
    {
        group = g;
        mixer = m;
        local = new SofAudio(oa, null, g, false);
        global = new SofAudio(oa, null, g, true);
        local.Stop();
        global.Stop();
        local.source.playOnAwake = global.source.playOnAwake = false;
        local.source.volume = global.source.volume = 1f;
    }
}
public class ObjectAudio : SofComponent
{
    [HideInInspector] public SofAudioType external;
    [HideInInspector] public SofAudioType cockpit;
    [HideInInspector] public SofAudioType persistent;

    [HideInInspector] public GameObject localHolder;
    [HideInInspector] public GameObject globalHolder;

    private List<SofAudio> sofAudios = new List<SofAudio>(0);
    private List<SofAudio> sofAudiosCockpit = new List<SofAudio>(0);

    [HideInInspector] public bool use3dSound = false;

    public bool AttachedListener { get { return SofAudioListener.AttachedToSofObject(sofObject); } }

    public void AddSofAudio(SofAudio sa)
    {
        sofAudios.Add(sa);
        if (sa.group == SofAudioGroup.Cockpit) sofAudiosCockpit.Add(sa);
    }

    private void Awake()
    {
        localHolder = transform.CreateChild("Local Sounds Holder").gameObject;
        globalHolder = transform.CreateChild("Global Sounds Holder").gameObject;

        SofAudioListener l = GameManager.gm.listener;
        external = new SofAudioType(SofAudioGroup.External, l.external,this);
        cockpit = new SofAudioType(SofAudioGroup.Cockpit, l.cockpit, this);
        persistent = new SofAudioType(SofAudioGroup.Persistent, l.persistent, this);

        use3dSound = true;
    }
    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);
        UpdatePlayer();
    }
    private void Update()
    {
        if (AttachedListener == use3dSound)
            UpdatePlayer();
    }
    public void UpdatePlayer()
    {
        use3dSound = !AttachedListener;
        localHolder.SetActive(AttachedListener);
        foreach (SofAudio sa in sofAudios) sa.source.spatialBlend = sa.source.dopplerLevel = use3dSound ? 1f : 0f;
    }
}

