using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Gun))]
public class GunFX : MonoBehaviour
{
    private Gun gun;
    private GunPreset preset;
    private Transform ejection;
    private Transform muzzle;

    //References
    private ParticleSystem[] muzzleFlashes;
    private ParticleSystem casings;
    private ParticleSystem.ForceOverLifetimeModule casingsDrag;

    private float multiplier = 1f;

    private int count;
    private const int updateCount = 10;

    private void Start()
    {
        gun = GetComponent<Gun>();
        preset = gun.gunPreset;
        multiplier = 1f / (preset.ammunition.caliber * 150f);
        ejection = gun.ejection;
        muzzle = gun.muzzleEffects;

        gun.OnFireEvent += Effect;

        if (preset.casingsFX && ejection && QualitySettings.GetQualityLevel() > 0)
        {
            casings = Instantiate(preset.casingsFX, ejection.position, ejection.rotation, ejection).GetComponent<ParticleSystem>();
            casingsDrag = casings.forceOverLifetime;
        }

        muzzleFlashes = Instantiate(preset.FireFX, muzzle.position, muzzle.rotation, muzzle).GetComponentsInChildren<ParticleSystem>();
        muzzleFlashes[0].transform.localScale = Vector3.one * Mathf.Pow(preset.ammunition.caliber / 7.62f, 0.8f);
    }

    private void Effect()
    {
        if (gun.complex && gun.complex.lod.LOD() < 2) foreach (ParticleSystem ps in muzzleFlashes) ps.Emit(1);
        if (gun.complex && gun.complex.lod.LOD() == 0) if (casings && ejection)
            {
                casingsDrag.z = -gun.data.ias * gun.data.ias * multiplier;
                casings.Emit(1);
            }
    }
}
