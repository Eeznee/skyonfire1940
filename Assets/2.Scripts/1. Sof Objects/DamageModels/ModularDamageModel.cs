using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SofModular))]
public class ModularDamageModel : SofDamageModel
{
    public float radius = 10f;


    public SofModular complex { get; private set; }
    public SphereCollider bubble { get; private set; }

    public override float RaycastDistanceExtension => bubble.radius * 2f;

    protected override void Start()
    {
        base.Start();

        complex = GetComponent<SofModular>();
        SofAircraft aircraft = GetComponent<SofAircraft>();

        bubble = transform.CreateChild("Bubble").gameObject.AddComponent<SphereCollider>();
        bubble.isTrigger = true;
        bubble.radius = aircraft ? aircraft.stats.wingSpan * 0.5f + 5f : radius;
        bubble.gameObject.layer = 10;
    }

    public override void Explosion(Vector3 center, float tnt)
    {
        if (center.y < 1f) tnt *= 0.25f;

        float explosionMaxSqrtDistance = tnt * 500f * 2f + 400f;
        float currentSqrtDistance = (center - transform.position).sqrMagnitude;

        if (explosionMaxSqrtDistance < currentSqrtDistance) return;   //no calculations if too far
        foreach (SofModule m in complex.modules.ToArray()) if (m) m.ExplosionDamage(center, tnt);
    }
    public override HitResult ProjectileRaycast(Vector3 position, Vector3 velocity, ProjectileChart chart)
    {
        if(rb) velocity -= rb.velocity;
        float range = RaycastDistanceExtension + velocity.magnitude * Time.fixedDeltaTime;

        RaycastHit[] hits = Ballistics.RaycastAndSort(position, velocity, range, LayerMask.GetMask("SofComplex"));
        if (hits.Length == 0) return HitResult.NoHit(velocity);

        float sqrVelocity = velocity.sqrMagnitude;
        float initialSqrVelocity = sqrVelocity;

        bool oneConfirmedHit = false;
        RaycastHit firstHit = new RaycastHit();
        RaycastHit lastHit = new RaycastHit();

        foreach (RaycastHit hit in hits)
        {
            SofModule module = hit.collider.GetComponentInParent<SofModule>();
            if (module == null) continue;
            if (module.transform.root != transform.root) continue;

            if (firstHit.collider == null) firstHit = hit;
            lastHit = hit;
            oneConfirmedHit = true;

            float penetrationPower = chart.Pen(sqrVelocity);

            float alpha = Vector3.Angle(-hit.normal, velocity);
            float armor = Random.Range(0.8f, 1.2f) * module.Armor.surfaceArmor / Mathf.Cos(alpha * Mathf.Deg2Rad);


            if (penetrationPower > armor)//If penetration occurs
            {
                module.ProjectileDamage(chart.KineticDamage(sqrVelocity), chart.diameter, chart.fireChance);
                armor += Random.Range(0.8f, 1.2f) * module.Armor.fullPenArmor;
                sqrVelocity *= 1f - armor / penetrationPower;

                if (sqrVelocity <= 0f) return new HitResult(firstHit, lastHit, Vector3.zero, HitSummary.Stopped);
            }
            else return new HitResult(firstHit, lastHit, velocity.normalized * Mathf.Sqrt(sqrVelocity), HitSummary.RicochetChance);
        }

        if (!oneConfirmedHit) return HitResult.NoHit(velocity);

        velocity = velocity.normalized * Mathf.Sqrt(sqrVelocity);
        velocity += rb.velocity;

        return new HitResult(firstHit, lastHit, velocity, HitSummary.Penetration);
    }
}
