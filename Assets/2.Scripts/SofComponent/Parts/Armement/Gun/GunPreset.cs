using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "New Gun Preset", menuName = "Weapons/Gun")]
public class GunPreset : ScriptableObject
{
    public string Name;
    public float mass = 20f;
    public ModuleMaterial material;
    public AmmunitionPreset ammunition;
    public bool openBolt = false;
    public bool boltCatch = false;
    public bool fullAuto = true;
    public bool singleShotsAudio = false;
    public float FireRate = 600f;
    public float dispersion = 0.05f;
    public float temperaturePerShot = 5f;
    public float coolingFactor = 0.05f;
    public float cyclingTime = 0.3f;
    public GunAudioSample[] audioSamples;

    public GameObject FireFX;
    public GameObject casingsFX;
}
