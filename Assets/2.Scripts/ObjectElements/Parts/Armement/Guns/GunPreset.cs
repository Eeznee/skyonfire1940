using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "New Gun Preset", menuName = "Weapons/Gun")]
public class GunPreset : ScriptableObject
{
	public enum FiringMode
	{
		Auto,
		Single,
	}

	public string Name;
    public float mass = 20f;
    public PartMaterial material;
    public AmmunitionPreset ammunition;
    public FiringMode WeaponType;
    public bool openBolt = false;
    public bool boltCatch = false;
    public float FireRate = 600f;
    public float dispersion = 1f;
    public float overHeatDispersion = 4f;
    public float temperaturePerShot = 5f;
    public float coolingFactor = 0.05f;
    public float cyclingTime = 0.3f;

    public float FireMass { get { return ammunition.mass / (60 / FireRate) * 1000; } }
    public GameObject FireFX;
    public GameObject casingsFX;
}
