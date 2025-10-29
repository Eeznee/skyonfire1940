using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SofShip))]
public class ShipDamageModel : SofDamageModel
{
    private SofShip ship;
    private SofAircraft lastAircraftThatCrashedInto;

    private bool cancelExplosionAfterTorpedoHit;
    private float integrity;

    const float extremitiesDamageMultiplier = 0.5f;
    const float explosionHalfDamageDistance = 8f;


    protected override void Start()
    {
        base.Start();

        integrity = 1f;
        ship = GetComponent<SofShip>();
        lastAircraftThatCrashedInto = null;
    }
    public void InflictDamage(float integrityDamage, Vector3 damagePosition)
    {
        integrity -= integrityDamage;
        if(integrity < 0f)
        {
            ship.DestroyShip(damagePosition);
        }
    }

    public override HitResult ProjectileRaycast(Vector3 position, Vector3 velocity, ProjectileChart chart)
    {
        Vector3 direction = velocity.normalized;

        int mask = LayerMask.GetMask("Default");
        float range = velocity.magnitude * Time.fixedDeltaTime;

        bool miss = !Physics.Raycast(position, velocity, out RaycastHit hit, range, mask);

        if (miss)
            return HitResult.NoHit(velocity);

        bool otherColliderHit = hit.collider.GetComponentInParent<SofDamageModel>() != this;

        if (otherColliderHit)
            return HitResult.NoHit(velocity);

        float penetrationPower = chart.Pen(velocity.sqrMagnitude);
        float alpha = Vector3.Angle(-hit.normal, velocity);
        float armor = Random.Range(0.8f, 1.2f) * ship.armorPlatingmm / Mathf.Cos(alpha * Mathf.Deg2Rad);

        foreach (CrewMember crew in ship.crew)
        {
            if (crew.Seat.shipGunnerProtectedFromStun) continue;

            Vector3 crossProduct = Vector3.Cross(crew.transform.position - position, direction);
            float sqrDistanceToCrew = crossProduct.sqrMagnitude;
            if (sqrDistanceToCrew < 3f * 3f)
            {
                crew.stunDuration += chart.KineticDamage(velocity.sqrMagnitude) * 0.1f;
            }
        }

        if (penetrationPower > armor)
        {
            InflictDamage(chart.KineticDamage(velocity.sqrMagnitude) / ship.projectileHp, hit.point);

            return new HitResult(hit, hit, Vector3.zero, HitSummary.Stopped);
        }

        return new HitResult(hit, hit, Vector3.zero, HitSummary.RicochetChance);
    }

    public void DirectTorpedoHit(Vector3 detonationPoint, float tntEquivalent)
    {
        cancelExplosionAfterTorpedoHit = true;

        Vector3 localDetonationPoint = transform.InverseTransformPoint(detonationPoint);
        float zPosition = localDetonationPoint.z;

        bool extremitiesHit = zPosition < ship.SternPoint || zPosition > ship.BowPoint;

        if (extremitiesHit) tntEquivalent *= extremitiesDamageMultiplier;

        InflictDamage(tntEquivalent / ship.maxTntKgCharge, detonationPoint);
    }


    public void AircraftCrashDamage(SofAircraft aircraft)
    {
        if (lastAircraftThatCrashedInto == aircraft) return;

        float kineticEnergy = aircraft.rb.linearVelocity.sqrMagnitude * 0.5f * aircraft.rb.mass;
        float damage = kineticEnergy * Ballistics.kineticEnergyToDamage;

        InflictDamage(damage / ship.projectileHp, aircraft.transform.position);

        lastAircraftThatCrashedInto = aircraft;
    }

    public float TNTEquivalentWithDistance(float sqrDistance, float tnt)
    {
        return Mathf.Min(tnt / (sqrDistance / M.Pow(explosionHalfDamageDistance, 2) + 1f), tnt); ;
    }

    public override void Explosion(Vector3 center, float tnt)
    {
        if (cancelExplosionAfterTorpedoHit)
        {
            cancelExplosionAfterTorpedoHit = false;
            return;
        }

        Vector3 localizedCenter = transform.InverseTransformPoint(center);
        
        Vector3 closestMidShipPoint = ship.MidShipBounds.ClosestPoint(localizedCenter);

        float sqrDistance = (closestMidShipPoint - localizedCenter).sqrMagnitude;

        float tntEquivalentAfterDistance = TNTEquivalentWithDistance(sqrDistance, tnt);

        if (tntEquivalentAfterDistance > 0.001f)
        {
            InflictDamage(tntEquivalentAfterDistance / ship.maxTntKgCharge, center);
            foreach (CrewMember crew in ship.crew)
            {
                if (crew.Seat.shipGunnerProtectedFromStun) continue;

                float sqrDistanceToCrew = (center - crew.transform.position).sqrMagnitude;
                float distanceTntEquivalent = TNTEquivalentWithDistance(sqrDistanceToCrew, tnt);
                crew.stunDuration += distanceTntEquivalent * 200f;
            }

        }
    }
}
