using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Gun))]
public class GunParticles : MonoBehaviour
{
    private Gun gun;
    private GunPreset preset;

    //References
    private ParticleSystem[] muzzleFlashes;
    private ParticleSystem casings;
    private ParticleSystem.ForceOverLifetimeModule casingsDrag;

    private float multiplier = 3f;

    const float dragConstant = 50f;

    Vector3 ejectionPos { get { return gun.tr.TransformPoint(gun.ejectionPos); } }
    Vector3 muzzlePos { get { return gun.tr.TransformPoint(gun.muzzlePos); } }

    private void Start()
    {
        gun = GetComponent<Gun>();
        preset = gun.gunPreset;
        multiplier = 1f / (preset.ammunition.caliber * dragConstant);

        gun.OnFireEvent += Effect;

        if (preset.casingsFX && gun.ejectCasings && QualitySettings.GetQualityLevel() > 0)
        {
            casings = Instantiate(preset.casingsFX, ejectionPos, gun.tr.rotation, gun.tr).GetComponent<ParticleSystem>();
            casingsDrag = casings.forceOverLifetime;
        }

        muzzleFlashes = Instantiate(preset.FireFX, muzzlePos, gun.tr.rotation, gun.tr).GetComponentsInChildren<ParticleSystem>();
        muzzleFlashes[0].transform.localScale = Vector3.one * Mathf.Pow(preset.ammunition.caliber / 7.62f, 0.8f);
    }

    private void Effect(float delay)
    {
        if (!gun.complex.lod)
        {
            foreach (ParticleSystem ps in muzzleFlashes) ps.Emit(1);
            return;
        }

        if (gun.complex.lod.LOD() <= 1) foreach (ParticleSystem ps in muzzleFlashes) ps.Emit(1);
        if (gun.complex.lod.LOD() == 0) if (casings && gun.ejectCasings)
            {
                casingsDrag.z = -gun.data.ias.Get * gun.data.ias.Get * multiplier;
                casings.Emit(1);
            }
    }
}
