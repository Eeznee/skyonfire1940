using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnitableExtension : MonoBehaviour
{
    private SofModule module;
    private ModuleMaterial material;

    public bool burning { private set; get; }

    private ParticleSystem burningEffect;
    private AudioSource burningAudio;

    private void Awake()
    {
        module = GetComponent<SofModule>();
        material = module.material;

        if (!material.ignitable) Destroy(this);
    }

    public void TryBurn(float caliber, float fireCoeff)
    {
        if (!material.ignitable || burning || module.structureDamage > 0.8f) return;

        float roundCoeff = fireCoeff * caliber * caliber / 60f;
        float burnChance = (1f - Mathv.SmoothStart(module.structureDamage, 1)) * material.burningChance * roundCoeff;
        if (Random.value < burnChance)
        {
            burning = true;
            if (module.sofObject) module.sofObject.burning = true;

            burningEffect = Instantiate(material.burningEffect, transform);
            burningEffect.Play();

            burningAudio = burningEffect.GetComponent<AudioSource>();
            burningAudio.Play();

            InvokeRepeating("Burning", burningTickInterval, burningTickInterval);
        }
    }
    const float burningTickInterval = 0.5f;
    public virtual void Burning()
    {
        if (!burning) return;

        foreach (SofModule otherModules in module.complex.modules)
        {
            if (!otherModules) continue;
            float distanceSqr = (otherModules.transform.position - transform.position).sqrMagnitude;
            distanceSqr = Mathf.Max(distanceSqr, 4f);
            float damage = burningTickInterval * 0.07f / distanceSqr;
            otherModules.BurnDamage(damage);
        }
    }

    public void StopBurning()
    {
        if (!burning) return;

        burning = false;

        Destroy(burningEffect.gameObject);

        CancelInvoke();
    }
}
