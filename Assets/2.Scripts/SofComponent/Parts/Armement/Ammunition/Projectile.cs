using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Ballistics;

public class Projectile : MonoBehaviour //Follows a trajectory using drag, weight and thrust (rocket).
{
    private float lifetime = 30f;
    private int points = 150;
    const float pointsPerSecond = 8f;
    const float noRicochetAlpha = 50f;
    const float ricochetAlpha = 80f;

    public ProjectileProperties p;
    public MeshRenderer fired;
    public MeshRenderer inert;

    [HideInInspector] public Collider ignoreCollider;
    [HideInInspector] public BoxCollider box;

    [HideInInspector] public Vector3 tracerDir;
    [HideInInspector] public float delay;

    private float dragCoeff;
    private float counter = 0f;
    private float lastInitialize = 0f;
    private float detonationTime = 0f;

    [HideInInspector] public Transform tr;
    private Vector3[] worldPos;
    private Vector3 initPos;

    private Vector3 initDir;
    private float initSpeed;

    public void Setup(ProjectileProperties properties)
    {
        tr = transform;
        p = properties;
        if (p.bulletHits) p.filler.fx = p.bulletHits.explosion;
        gameObject.layer = 12;

        //Add Collider
        box = gameObject.AddComponent<BoxCollider>();
        box.isTrigger = true;
        float size = Time.fixedUnscaledDeltaTime * (properties.baseVelocity + 300f);
        box.size = new Vector3(0.1f, 0.1f, size);
        box.center = new Vector3(0f, 0f, size/2f);
        box.enabled = false;
    }
    public void InitializeTrajectory(Vector3 _vel, Vector3 _tracerDir, Collider ignore, float _delay)
    {
        lastInitialize = counter;
        initPos = tr.position - GameManager.refPos;
        initSpeed = _vel.magnitude;
        initDir = tr.forward = _vel / initSpeed;
        tracerDir = _tracerDir;
        delay = _delay;
        dragCoeff = Mathv.SmoothStart(p.diameter / 2000f, 2) * Mathf.PI * 0.1f / p.mass;
        float lifeTimeFactor = Mathf.InverseLerp(4000f, 20000f, p.Energy);
        lifetime = Mathf.Lerp(4f, 10f, lifeTimeFactor);
        points = Mathf.RoundToInt(lifetime * pointsPerSecond);
        worldPos = Ballistics.BallisticPath(initDir, initSpeed, dragCoeff, points, lifetime);
        if (p.fuze > 0f) StartFuze(p.fuze);

        if (inert && fired) { inert.enabled = false; fired.enabled = true; }

        box.enabled = true;
        ignoreCollider = ignore;
    }
    public Vector3 Pos(float t)
    {
        t -= lastInitialize;
        float progress = points * t / lifetime;
        int prevPos = Mathf.Clamp(Mathf.FloorToInt(progress), 0, worldPos.Length - 2);
        Vector3 prev = worldPos[prevPos];
        Vector3 next = worldPos[prevPos + 1];
        float interpolation = progress > points ? progress - points + 1f : progress % 1f;
        Vector3 pos = Vector3.LerpUnclamped(prev, next, interpolation);
        return initPos + GameManager.refPos +pos;
    }
    private Vector3 Vel(float t)
    {
        t -= lastInitialize;
        return initDir / (dragCoeff * t + 1f / initSpeed) + Physics.gravity * t;
    }
    private void FixedUpdate()
    {
        counter += Time.fixedDeltaTime;
        if (counter > lifetime) { Destroy(gameObject); return; }
        Vector3 pos = Pos(counter);
        tr.position = pos;

        if (pos.y < 0f) CollideWater();
        if (detonationTime != 0f && Time.time > detonationTime) Detonate(tr.position,null);
    }
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
            InitializeTrajectory(tr.forward * speed * 0.4f * Mathf.Sin(alpha * Mathf.Deg2Rad),tr.forward,ignoreCollider,delay);
        }
        else Destroy(gameObject);
    }
    public void StartFuze(float t) { detonationTime = Time.time + t * Random.Range(0.98f, 1.02f); }

    void OnTriggerEnter(Collider obj)
    {
        if (obj.gameObject.layer != 10)  //fixed objects have been hit
            CollideTerrain(obj);
        else if (obj != ignoreCollider)
            CollideComplexBubble(obj);
    }
    private void CollideComplexBubble(Collider obj)
    {
        ObjectBubble bubble = obj.GetComponent<ObjectBubble>();
        if (!bubble) return;
        bubble.EnableColliders(false);
        StartDamage(Vel(counter),bubble.bubble.radius * 2f + box.size.z);
    }

    public void StartDamage(Vector3 velocity, float range)
    {
        if (p.explosive)
        {
            RaycastHit hit;
            if(Physics.Raycast(transform.position, velocity, out hit, range, LayerMask.GetMask("SofComplex")))
                Detonate(hit.point, hit.collider.transform.root);
        }
        else
        {
            ProjectileChart chart = new ProjectileChart(p.basePenetration, p.baseVelocity, p.diameter, p.FireChance());

            HitResult result = RaycastDamage(transform.position, velocity, range, chart);

            if (result.summary == HitSummary.NoHit) return;

            if (p.bulletHits) p.bulletHits.AircraftHit(p.incendiary && !p.explosive, result.firstHit);

            if (result.summary == HitSummary.Penetration)
            {
                
                float sqrVelocityLeftRatio = result.velocityLeft.sqrMagnitude / velocity.sqrMagnitude;
                float spread = Mathf.Lerp(5f, 0f,sqrVelocityLeftRatio);
                Vector3 newVelocity = Spread(Quaternion.identity, spread) * result.velocityLeft;
                InitializeTrajectory(newVelocity, Vector3.zero, ignoreCollider, delay);
            }
            else TryRicochet(result.lastHit, result.velocityLeft.magnitude);
        }
    }
    private void CollideTerrain(Collider obj)
    {
        Ray ray = new Ray(tr.position, tr.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, 20f, LayerMask.GetMask("Default", "Terrain"))) return;

        Vector3 velocity = Vel(counter);
        SofSimple sofSimple = hit.collider.transform.parent ? hit.collider.transform.parent.GetComponent<SofSimple>() : null;
        if (sofSimple && sofSimple.bulletAffected)
            sofSimple.BulletDamage(p.mass * velocity.sqrMagnitude / 2f);

        if (p.explosive) Detonate(hit.point, obj.transform);
        else
        {
            if (p.bulletHits) p.bulletHits.CreateHit(obj.sharedMaterial?.name, hit.point, Quaternion.LookRotation(hit.normal), null);
            TryRicochet(hit, velocity.magnitude);
        }
    }
    private void CollideWater()
    {
        if (p.explosive) Detonate(tr.position - tr.position.y * Vector3.up, null);
        else if (p.bulletHits) p.bulletHits.CreateHit("Water", Vector3.Scale(tr.position, new Vector3(1f, 0f, 1f)), Quaternion.identity, null);
        Destroy(gameObject);
    }
    public void Detonate(Vector3 pos, Transform tran) { if (p.explosive) p.filler.Detonate(pos, p.mass, tran); Destroy(gameObject); }
}