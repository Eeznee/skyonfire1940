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
    private ParticleSystem.VelocityOverLifetimeModule casingsVelocity;

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

        if (preset.casingsFX && gun.ejectCasings)
        {

            Quaternion rotation = gun.tr.rotation;
            casings = Instantiate(preset.casingsFX, ejectionPos, gun.tr.rotation, gun.tr).GetComponent<ParticleSystem>();
            casingsDrag = casings.forceOverLifetime;
            casingsVelocity = casings.velocityOverLifetime;
        }

        muzzleFlashes = Instantiate(preset.FireFX, muzzlePos, gun.tr.rotation, gun.tr).GetComponentsInChildren<ParticleSystem>();
        muzzleFlashes[0].transform.localScale = Vector3.one * Mathf.Pow(preset.ammunition.caliber / 7.62f, 0.8f);
    }

    private void Effect(float delay)
    {
        if (!gun.sofModular.lod)
        {
            foreach (ParticleSystem ps in muzzleFlashes) ps.Emit(1);
            return;
        }

        if (gun.sofModular.lod.LOD() <= 1) foreach (ParticleSystem ps in muzzleFlashes) ps.Emit(1);

        if (gun.sofModular.lod.LOD() == 0 && SofSettingsSO.CurrentSettings.graphicsPreset > 0 && gun.ejectCasings)
        {
            EjectCasing();
        }

    }
    private void EjectCasing()
    {
        Vector3 drag = gun.tr.InverseTransformDirection(gun.sofModular.tr.forward) * -M.Pow(gun.data.ias.Get, 2) * multiplier;
        casingsDrag.x = drag.x;
        casingsDrag.y = drag.y;
        casingsDrag.z = drag.z;

        Vector3 vel = gun.ejectionVector;
        casingsVelocity.x = vel.x;
        casingsVelocity.y = vel.y;
        casingsVelocity.z = vel.z;

        casings.Emit(1);
    }
}
