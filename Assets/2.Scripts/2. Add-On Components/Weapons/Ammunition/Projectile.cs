using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Ballistics;

public class Projectile : MonoBehaviour
{
    public ProjectileProperties properties;

    [SerializeField] private MeshRenderer fired;
    [SerializeField] private MeshRenderer inert;

    [HideInInspector][SerializeField] private Transform tr;
    [HideInInspector][SerializeField] private BoxCollider box;
    [HideInInspector][SerializeField] private float dragCoeff;
    [HideInInspector][SerializeField] private float lifeTime;
    [HideInInspector][SerializeField] private ProjectileChart ballisticChart;

    [HideInInspector][SerializeField] private int layerMask;
    [HideInInspector][SerializeField] private int nearGroundMask;
    [HideInInspector][SerializeField] private Collider[] overlapColliders;

    private float initializedAtTime;
    private float despawnTime;
    private float detonationTime = 0f;

    private Vector3 initPos;
    private Vector3 initDir;
    private float initSpeed;
    private float invertInitSpeed;
    private SofDamageModel ignoreDamageModel;

    private bool isDestroyed = false;


    public void Setup(ProjectileProperties _properties)
    {
        isDestroyed = false;

        tr = transform;
        properties = _properties;
        if (properties.bulletHits) properties.filler.fx = properties.bulletHits.explosion;
        gameObject.layer = 12;

        lifeTime = properties.LifeTime;
        dragCoeff = properties.DragCoeff;
        ballisticChart = new ProjectileChart(properties.mass, properties.basePenetration, properties.baseVelocity, properties.diameter, properties.FireChance());

        layerMask = LayerMask.GetMask("Bubble");
        nearGroundMask = LayerMask.GetMask("Bubble", "Default", "Terrain");
        overlapColliders = new Collider[1];
    }
    public void InitializeTrajectory(Vector3 _vel, SofDamageModel shooter)
    {
        ResetTrajectory(_vel, shooter);

        if(shooter) SofDamageModelHit(tr.position, _vel, shooter);

        tr.position = Pos(Time.fixedTime);

        despawnTime = Time.time + lifeTime;
        if (properties.fuze > 0f) SetFuze(properties.fuze);
        if (inert && fired) { inert.enabled = false; fired.enabled = true; }
    }
    public void ResetTrajectory(Vector3 _vel, SofDamageModel ignore)
    {
        initializedAtTime = Time.fixedTime;

        ignoreDamageModel = ignore;
        initSpeed = _vel.magnitude;
        invertInitSpeed = 1f / initSpeed;
        initDir = tr.forward = _vel * invertInitSpeed;
        initPos = tr.position - GameManager.refPos;
    }
    public Vector3 Pos(float t)
    {
        t = t + Time.fixedDeltaTime - initializedAtTime;
        return GameManager.refPos + initPos + BallisticTrajectory(initDir, initSpeed, dragCoeff, t);
    }
    public Vector3 Vel(float t)
    {
        t = t + Time.fixedDeltaTime - initializedAtTime;
        return initDir / (dragCoeff * t + invertInitSpeed) + Physics.gravity * t;
    }
    const float noRicochetAlpha = 50f;
    const float ricochetAlpha = 80f;

    const float maxRicochetChance = 0.5f;
    public void TryRicochet(RaycastHit hit, float speed)
    {
        tr.position = hit.point;
        float alpha = Vector3.Angle(tr.forward, -hit.normal);
        float chance = Mathf.InverseLerp(noRicochetAlpha, ricochetAlpha, alpha);
        if (Random.value < chance && Random.value < maxRicochetChance && speed > 50f)
        {
            tr.forward = Vector3.Reflect(tr.forward, hit.normal);
            tr.rotation = Spread(tr.rotation, 7f);
            ResetTrajectory(tr.forward * speed * 0.4f * Mathf.Sin(alpha * Mathf.Deg2Rad), ignoreDamageModel);
        }
        else Destroy(gameObject);
    }


    private void FixedUpdate()
    {
        if (isDestroyed) return;

        Vector3 currentPos = tr.position;
        Vector3 nextPosition = currentPos + Time.fixedDeltaTime * BallisticVelocity(initDir, initSpeed, dragCoeff, Time.fixedTime - initializedAtTime);
        Vector3 delta = nextPosition - currentPos;

        if (detonationTime != 0f && Time.time > detonationTime)
        {
            Vector3 velocity = delta / Time.fixedDeltaTime;
            float dt = Time.time - detonationTime;
            Vector3 detonationPos = currentPos - dt * velocity;
            Detonate(detonationPos, null);
        }

        if (nextPosition.y < 0f) CollideWater();

        int mask = currentPos.y < 500f ? nearGroundMask : layerMask;
        int c = Physics.OverlapSphereNonAlloc(currentPos, initSpeed * Time.fixedDeltaTime, overlapColliders, mask);
        if (c > 0)
        {
            if(Physics.Raycast(currentPos, delta, out RaycastHit hit, initSpeed * Time.fixedDeltaTime, mask))
            {
                SofDamageModel damageModel = hit.collider.GetComponentInParent<SofDamageModel>();

                if (damageModel != ignoreDamageModel)
                {
                    Vector3 vel = delta / Time.fixedDeltaTime;

                    if (properties.explosive)
                    {
                        mask = LayerMask.GetMask("SofComplex", "Default", "Terrain");
                        float range = delta.magnitude + (damageModel ? damageModel.RaycastDistanceExtension : 0f);
                        if (Physics.Raycast(currentPos, delta, out RaycastHit explosiveHit, range, mask))
                        {
                            Detonate(explosiveHit.point, explosiveHit.collider.transform.root);
                            return;
                        }
                    }
                    else if (damageModel)
                    {
                        if (damageModel != ignoreDamageModel)
                        {
                            SofDamageModelHit(currentPos, vel, damageModel);
                        }
                    }
                    else
                    {
                        properties.TerrainHit(hit);
                        TryRicochet(hit, vel.magnitude);
                    }
                }
            }
        }

        tr.position = nextPosition;

        if (Time.time > despawnTime)
        {
            DestroyProjectile();
        }
    }
    public HitResult SofDamageModelHit(Vector3 pos, Vector3 velocity, SofDamageModel damageModel)
    {
        HitResult result = damageModel.ProjectileRaycast(pos, velocity, ballisticChart);

        switch (result.summary)
        {
            case HitSummary.NoHit:

                break;
            case HitSummary.Stopped:
                break;
            case HitSummary.Penetration:

                float velocityLeftRatio = result.velocityLeft.magnitude / velocity.magnitude;
                float spread = Mathf.Lerp(8f, 0f, velocityLeftRatio);
                Vector3 newVelocity = Spread(result.velocityLeft, spread);
                ResetTrajectory(newVelocity, damageModel);

                break;
            case HitSummary.RicochetChance:

                TryRicochet(result.lastHit, result.velocityLeft.magnitude);

                break;
        }

        if(result.summary != HitSummary.NoHit) properties.AircraftHit(result.firstHit);

        return result;
    }
    private void CollideWater()
    {
        if (properties.explosive)
        {
            Detonate(tr.position - tr.position.y * Vector3.up, null);
        }
        else if (properties.bulletHits) properties.bulletHits.CreateHit("Water", Vector3.Scale(tr.position, new Vector3(1f, 0f, 1f)), Quaternion.identity, null);
        DestroyProjectile();
    }
    public void SetFuzeBasedOnDistance(float distance) { detonationTime = Time.time + TimeRequiredToReachDistance(distance, initSpeed, dragCoeff); }
    public void SetFuze(float _timer) { detonationTime = Time.time + _timer; }
    public void Detonate(Vector3 pos, Transform tran) 
    {
        if (properties.explosive) 
            properties.filler.Detonate(pos, properties.mass, tran);

        DestroyProjectile();
    }

    public void DestroyProjectile()
    {
        isDestroyed = true;
        Destroy(gameObject);
    }
}