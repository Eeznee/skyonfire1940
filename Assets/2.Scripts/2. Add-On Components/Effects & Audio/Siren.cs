﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Siren : AudioComponent
{
    public AudioClip sirenClip;
    SofAudio siren;
    public Transform[] spinners;

    public float minSpeed = 400f / 3.6f;
    public float minPitch = 0.3f;
    public float maxPitch = 1.2f;
    public float maxVolume = 1f;

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        siren = new SofAudio(avm, sirenClip, SofAudioGroup.Persistent, true);
    }

    // Update is called once per frame
    void Update()
    {
        if (siren.source.isPlaying)
        {
            bool sirenActive = false;
            foreach (Transform t in spinners) sirenActive |= t.root == transform.root;
            if (!sirenActive) siren.source.Stop();
        }
        siren.source.volume = Mathf.Lerp(0f, maxVolume, 2 * (data.ias.Get - minSpeed) / minSpeed);
        siren.source.pitch = Mathf.Lerp(minPitch, maxPitch, 2 * (data.ias.Get - minSpeed) / minSpeed);
        foreach (Transform spinner in spinners)
            if(spinner != null) spinner.Rotate(Vector3.forward * siren.source.volume * 5000f * Time.deltaTime);
    }
}
