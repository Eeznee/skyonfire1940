using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private float dragCoeff;
    private float counter = 0f;
    private float lifeTimeCounter = 0f;
    private float detonationTime = 0f;

    private Vector3[] worldPos;
    private Vector3 previousPos;
    private Vector3 initPosition;

    private Vector3 velocity;
    private Vector3 initDir;
    private float initSpeed;

    public void Setup(ProjectileProperties properties)
    {
        p = properties;
        if (p.bulletHits) p.filler.fx = p.bulletHits.explosion;
        gameObject.layer = 10;

        //Add Collider
        box = gameObject.AddComponent<BoxCollider>();
        box.isTrigger = true;
        float size = Time.fixedUnscaledDeltaTime * (properties.baseVelocity + 300f);
        box.size = new Vector3(0.1f, 0.1f, size);
        box.center = new Vector3(0f, 0f, size/2f);
        box.enabled = false;
    }
    public void InitializeTrajectory(Vector3 vel, Vector3 _tracerDir, Collider ignore)
    {
        counter = 0f;
        previousPos = initPosition = transform.position;
        velocity = vel;
        initSpeed = vel.magnitude;
        initDir = transform.forward = vel / initSpeed;
        tracerDir = _tracerDir;
        dragCoeff = Mathv.SmoothStart(p.diameter / 2000f, 2) * Mathf.PI * 0.1f / p.mass;
        lifetime = Mathf.Lerp(3f, 10f, Mathf.InverseLerp(7.62f, 40f, p.diameter));
        if (p.diameter == 9f) lifetime = 2f;
        points = Mathf.RoundToInt(lifetime * pointsPerSecond);
        worldPos = Ballistics.BallisticPath(initPosition, initDir, initSpeed, dragCoeff, points, lifetime);
        if (p.fuze > 0f) StartFuze(p.fuze);

        if (inert && fired) { inert.enabled = false; fired.enabled = true; }

        box.enabled = true;
        ignoreCollider = ignore;
    }
    private void UpdateTrajectory()
    {
        counter += Time.fixedDeltaTime;
        lifeTimeCounter += Time.fixedDeltaTime;
        if (lifeTimeCounter > lifetime) { Destroy(gameObject); return; }
        int prevIndex = Mathf.FloorToInt(counter * points / lifetime);
        if (prevIndex + 1 >= points) { Destroy(gameObject); return; }
        Vector3 prev = worldPos[prevIndex];
        Vector3 next = worldPos[prevIndex + 1];
        Vector3 pos = Vector3.Lerp(prev, next, counter * points / lifetime - prevIndex);
        transform.position += pos - previousPos;
        transform.forward = pos - previousPos;
        previousPos = pos;

        velocity = initDir / (dragCoeff * counter + 1f / initSpeed) + Physics.gravity * counter;
    }
    private void FixedUpdate()
    {
        UpdateTrajectory();

        if (transform.position.y < 0f) CollideWater();
        if (detonationTime != 0f && Time.time > detonationTime) Detonate(transform.position,null);
    }
    public void Ricochet(RaycastHit hit, float vel)
    {
        transform.position = hit.point;
        float alpha = Vector3.Angle(transform.forward, -hit.normal);
        float chance = Mathf.InverseLerp(noRicochetAlpha, ricochetAlpha, alpha);
        if (Random.value < chance)
        {
            transform.forward = Vector3.Reflect(transform.forward, hit.normal);
            transform.rotation = Ballistics.Spread(transform.rotation, 7f);
            InitializeTrajectory(transform.forward * vel * 0.4f * Mathf.Sin(alpha * Mathf.Deg2Rad),transform.forward,ignoreCollider);
        }
        else Destroy(gameObject);
    }
    public void StartFuze(float t) { detonationTime = Time.time + t * Random.Range(0.98f, 1.02f); }
    public void RaycastDamage(Vector3 velocity, Vector3 targetVelocity, float range)
    {
        Vector3 relativeVelocity = velocity - targetVelocity;

        RaycastHit[] hits = Ballistics.RaycastAndSort(transform.position, relativeVelocity, range, LayerMask.GetMask("SofObject"));
        if (hits.Length == 0) return;


        if (p.explosive) { Detonate(hits[0].point, hits[0].collider.transform.root); return; }
        if (p.bulletHits) p.bulletHits.AircraftHit(p.incendiary && !p.explosive, hits[0]);
        float sqrVelocity = relativeVelocity.sqrMagnitude;

        foreach (RaycastHit h in hits)
        {
            if (!TryPenPart(h.collider.GetComponent<Part>(), h, ref sqrVelocity)) { return; }
        }

        float spread = Mathf.Lerp(5f,0f,sqrVelocity / relativeVelocity.sqrMagnitude);
        Vector3 vel = Mathf.Sqrt(sqrVelocity) * (Ballistics.Spread(transform.rotation, spread) * Vector3.forward);
        InitializeTrajectory(vel, Vector3.zero,ignoreCollider);
    }
    void OnTriggerEnter(Collider obj)
    {
        if (obj.gameObject.layer != 11)  //fixed objects have been hit
            CollideTerrain(obj);
        else if (obj != ignoreCollider)
            RaycastDamage(velocity, obj.transform.root.GetComponent<Rigidbody>().velocity, 35f);
    }
    private bool TryPenPart(Part part, RaycastHit hit, ref float sqrVelocity)
    {
        if (part == null) return true;
        float penetrationPower = p.basePenetration * sqrVelocity / (p.baseVelocity * p.baseVelocity);
        float alpha = Vector3.Angle(-hit.normal, transform.forward);
        float armor = Random.Range(0.8f, 1.2f) * part.material.armor / Mathf.Cos(alpha * Mathf.Deg2Rad);
        if (penetrationPower > armor)//If penetration occurs
        {
            //part.Damage(p.mass * sqrVelocity  / 2000f, p.diameter, p.FireChance());
            part.Damage(p.diameter * p.diameter / 30f, p.diameter, p.FireChance());
            armor += Random.Range(0.8f, 1.2f) * part.material.totalThickness;
            sqrVelocity *= 1f - armor / penetrationPower;
            return sqrVelocity > 0f;
        }
        else //Try ricochet if penetration fails
        {
            Ricochet(hit, Mathf.Sqrt(sqrVelocity));
            return false;
        }
    }
    private void CollideTerrain(Collider obj)
    {
        Ray ray = new Ray(transform.position, transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, 20f, LayerMask.GetMask("Default", "Terrain"))) return;

        SofSimple sofSimple = hit.collider.transform.parent ? hit.collider.transform.parent.GetComponent<SofSimple>() : null;
        if (sofSimple && sofSimple.bulletAffected)
            sofSimple.BulletDamage(p.mass * velocity.sqrMagnitude / 2f);

        if (p.explosive) Detonate(hit.point, obj.transform);
        else
        {
            if (p.bulletHits) p.bulletHits.CreateHit(obj.sharedMaterial.name, hit.point, Quaternion.LookRotation(hit.normal), null);
            Ricochet(hit, velocity.magnitude);
        }
    }
    private void CollideWater()
    {
        if (p.explosive) Detonate(transform.position - transform.position.y * Vector3.up, null);
        else if (p.bulletHits) p.bulletHits.CreateHit("Water", Vector3.Scale(transform.position, new Vector3(1f, 0f, 1f)), Quaternion.identity, null);
        Destroy(gameObject);
    }
    public void Detonate(Vector3 pos, Transform tr) { if (p.explosive) p.filler.Detonate(pos, p.mass, tr); Destroy(gameObject); }
}

/*
private float fuzeDisSquared;
public void SetFuze(float dis)
{
    if (explosive == null) return;
    dis *= Random.Range(0.9f, 1.1f);
    fuzeDisSquared = dis * dis;
}
        if (fuzeDisSquared > 50f * 50f)
    {
        float dis = (transform.position - initPosition).sqrMagnitude;
        if (dis > fuzeDisSquared) SelfDestruct(false);
    }
    else if (counter > lifetime)
        SelfDestruct(false);
*/