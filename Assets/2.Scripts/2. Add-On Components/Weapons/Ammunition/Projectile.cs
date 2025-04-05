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
    [HideInInspector][SerializeField] private ProjectileChart ballisticChart;

    private float initializedAtTime;
    private float despawnTime;
    private float detonationTime = 0f;

    private Vector3 initPos;
    private Vector3 initDir;
    private float initSpeed;
    private Collider ignoreCollider;

    public void Setup(ProjectileProperties _properties)
    {
        tr = transform;
        properties = _properties;
        if (properties.bulletHits) properties.filler.fx = properties.bulletHits.explosion;
        gameObject.layer = 12;

        dragCoeff = ProjectileDragCoeff(properties.diameter, properties.mass);
        ballisticChart = new ProjectileChart(properties.basePenetration, properties.baseVelocity, properties.diameter, properties.FireChance());

        SetupCollider();
    }
    public void StartFuze(float t) { detonationTime = Time.time + t * Random.Range(0.98f, 1.02f); }

    private void SetupCollider()
    {
        box = this.GetCreateComponent<BoxCollider>();
        box.isTrigger = true;
        float size = Time.fixedUnscaledDeltaTime * (properties.baseVelocity + 300f);
        box.size = new Vector3(0.1f, 0.1f, size);
        box.center = new Vector3(0f, 0f, size / 2f);
        box.enabled = false;
    }
    public void InitializeTrajectory(Vector3 _vel, Collider ignore)
    {
        ignoreCollider = ignore;
        initSpeed = _vel.magnitude;
        initDir = tr.forward = _vel / initSpeed;
        initPos = tr.position - GameManager.refPos;

        initializedAtTime = Time.fixedTime;

        despawnTime = Time.time + properties.LifeTime;
        if (properties.fuze > 0f) StartFuze(properties.fuze);

        if (inert && fired) { inert.enabled = false; fired.enabled = true; }

        box.enabled = true;
    }
    public Vector3 Pos(float t)
    {
        t -= initializedAtTime;
        return GameManager.refPos + initPos + BallisticTrajectory(initDir, initSpeed, dragCoeff, t);
    }
    public Vector3 Vel(float t)
    {
        t -= initializedAtTime;
        return initDir / (dragCoeff * t + 1f / initSpeed) + Physics.gravity * t;
    }
    private void FixedUpdate()
    {
        tr.position = Pos(Time.fixedTime);

        if (tr.position.y < 0f) CollideWater();
        if (detonationTime != 0f && Time.time > detonationTime) Detonate(tr.position,null);
        if (Time.time > despawnTime) { Destroy(gameObject); return; }
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
            InitializeTrajectory(tr.forward * speed * 0.4f * Mathf.Sin(alpha * Mathf.Deg2Rad),ignoreCollider);
        }
        else Destroy(gameObject);
    }
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
        Vector3 relativeVelocity = Vel(Time.fixedTime);
        relativeVelocity -= obj.GetComponentInParent<Rigidbody>().velocity;
        StartDamage(relativeVelocity,bubble.bubble.radius * 2f + box.size.z);
    }

    public void StartDamage(Vector3 velocity, float range)
    {
        if (properties.explosive)
        {
            RaycastHit hit;
            if(Physics.Raycast(transform.position, velocity, out hit, range, LayerMask.GetMask("SofComplex")))
                Detonate(hit.point, hit.collider.transform.root);
        }
        else
        {
            HitResult result = RaycastDamage(transform.position, velocity, range, ballisticChart);

            if (result.summary == HitSummary.NoHit) return;

            properties.AircraftHit(result.firstHit);

            if (result.summary == HitSummary.Penetration)
            {
                
                float sqrVelocityLeftRatio = result.velocityLeft.sqrMagnitude / velocity.sqrMagnitude;
                float spread = Mathf.Lerp(5f, 0f,sqrVelocityLeftRatio);
                Vector3 newVelocity = Spread(Quaternion.identity, spread) * result.velocityLeft;
                InitializeTrajectory(newVelocity, ignoreCollider);
            }
            else TryRicochet(result.lastHit, result.velocityLeft.magnitude);
        }
    }
    private void CollideTerrain(Collider obj)
    {
        Ray ray = new Ray(tr.position, tr.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, 20f, LayerMask.GetMask("Default", "Terrain"))) return;

        Vector3 velocity = Vel(Time.fixedTime);
        SofSimple sofSimple = hit.collider.transform.parent ? hit.collider.transform.parent.GetComponent<SofSimple>() : null;
        if (sofSimple && sofSimple.bulletAffected)
            sofSimple.BulletDamage(properties.mass * velocity.sqrMagnitude / 2f);

        if (properties.explosive) Detonate(hit.point, obj.transform);
        else
        {
            if (properties.bulletHits)
            {
                if (obj.sharedMaterial == null)
                {
                    Debug.LogError(obj.gameObject.name + " Does not have a material", obj.gameObject);
                    return;
                }
                properties.bulletHits.CreateHit(obj.sharedMaterial.name, hit.point, Quaternion.LookRotation(hit.normal), null);
            }
            TryRicochet(hit, velocity.magnitude);
        }
    }
    private void CollideWater()
    {
        if (properties.explosive) Detonate(tr.position - tr.position.y * Vector3.up, null);
        else if (properties.bulletHits) properties.bulletHits.CreateHit("Water", Vector3.Scale(tr.position, new Vector3(1f, 0f, 1f)), Quaternion.identity, null);
        Destroy(gameObject);
    }
    public void Detonate(Vector3 pos, Transform tran) { if (properties.explosive) properties.filler.Detonate(pos, properties.mass, tran); Destroy(gameObject); }
}