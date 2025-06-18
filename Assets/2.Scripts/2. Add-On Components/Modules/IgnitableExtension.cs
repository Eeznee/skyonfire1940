using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnitableExtension : MonoBehaviour
{
    private SofModule module;
    private IIgnitable iIgnitable;

    public bool burning { private set; get; }

    private ParticleSystem burningEffect;
    private AudioSource burningAudio;

    private void OnEnable()
    {
        iIgnitable = GetComponent<IIgnitable>();
        module = GetComponent<SofModule>();
        module.OnProjectileDamage += TryBurn;
        module.OnRepair += StopBurning;

        if (iIgnitable == null || !iIgnitable.Ignitable) Destroy(this);
    }
    private void OnDisable()
    {
        module.OnProjectileDamage -= TryBurn;
        module.OnRepair -= StopBurning;
    }
    public void TryBurn(float damage, float caliber, float fireCoeff)
    {
        if (burning || module.structureDamage > iIgnitable.MaxStructureDamageToBurn) return;

        float multiplier = fireCoeff * Mathv.SmoothStart(caliber / 7.62f, 2);
        float chanceNotToBurn = 1f - iIgnitable.BurningChance * multiplier;
        if (Random.value > chanceNotToBurn)
        {
            burning = true;
            if (module.sofObject) module.sofObject.StartBurning();

            burningEffect = Instantiate(iIgnitable.BurningEffect, transform);
            burningEffect.Play();

            burningAudio = burningEffect.GetComponent<AudioSource>();
            burningAudio.Play();

            InvokeRepeating("Burning", burningTickInterval, burningTickInterval);
        }
    }
    const float burningTickInterval = 0.5f;
    public void Burning()
    {
        if (!burning) return;

        SofModule[] modules = module.sofModular.modules.ToArray();

        foreach (SofModule moduleToBurn in modules)
        {
            if (!moduleToBurn) continue;
            float distanceSqr = (moduleToBurn.transform.position - transform.position).sqrMagnitude;
            distanceSqr = Mathf.Max(distanceSqr, 4f);
            float damage = burningTickInterval * 0.07f / distanceSqr;
            moduleToBurn.DirectStructuralDamage(damage);
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
