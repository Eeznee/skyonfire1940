using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public const float lifetime = 3f;
    public const int points = 15;

    public AmmunitionPreset ammo;
    public AmmunitionPreset.BulletPreset bullet;
    public GameObject bubbleShotFrom;
    public LineRenderer line;
    public TrailRenderer trail;

    private float counter = 0f;
    private float fuzeDisSquared;
    private float dragCoeff;
    private float tracerOffset;
    private Vector3 tracerDir;

    private Vector3[] worldPos;
    private Vector3 previousPos;
    private Vector3 initPosition;

    private Vector3 velocity;
    private Vector3 initVelocity;
    private Vector3 initVelNormalized;
    private float initSpeed;



    public void SetFuze(float dis)
    {
        if (!bullet.explosive) return;
        dis *= Random.Range(0.9f, 1f);
        fuzeDisSquared = dis * dis;
    }
    public void InitializeTrajectory(Vector3 vel)
    {
        previousPos = initPosition = transform.position;
        initVelocity = velocity = vel;
        initSpeed = initVelocity.magnitude;
        tracerDir = transform.forward;
        initVelNormalized = transform.forward = vel / initSpeed;
        dragCoeff = Mathf.Pow(ammo.caliber / 2000f, 2) * Mathf.PI * 0.1f / (ammo.mass / 1000f);

        //Compute the world poses
        float logConst = Mathf.Log(1f / initSpeed) / dragCoeff;
        worldPos = new Vector3[points];
        for (int i = 0; i < points; i++)
        {
            float t = (float)i / points * lifetime;
            worldPos[i] = initPosition;
            worldPos[i] += Physics.gravity / 2f * t * t;
            worldPos[i] += initVelocity.normalized * (Mathf.Log(dragCoeff * t + 1f / initSpeed) / dragCoeff - logConst);
        }
        float updateDis = initSpeed * Time.fixedDeltaTime;
        tracerOffset = Random.Range(0f, updateDis) + updateDis * 0.2f;
        UpdateTracer();
    }

    void FixedUpdate()
    {
        //Compute position
        counter += Time.fixedDeltaTime;
        int prevIndex = Mathf.FloorToInt(counter * points / lifetime);
        if (prevIndex + 1 >= points) { SelfDestruct(false); return; }
        Vector3 prev = worldPos[prevIndex];
        Vector3 next = worldPos[prevIndex + 1];
        Vector3 pos = Vector3.Lerp(prev, next, counter * points / lifetime - prevIndex);
        transform.position += pos - previousPos;
        transform.forward = pos - previousPos;
        previousPos = pos;

        velocity = initVelNormalized / (dragCoeff * counter + 1 / initSpeed) + Physics.gravity * counter;
        if (transform.position.y < 0f) WaterImpact();

        if (fuzeDisSquared > 50f * 50f)
        {
            float dis = (transform.position - initPosition).sqrMagnitude;
            if (dis > fuzeDisSquared) SelfDestruct(false);
        }
        else if (counter > lifetime)
            SelfDestruct(false);

        UpdateTracer();
    }
    private void UpdateTracer()
    {
        if (line && Time.timeScale != 0f)
        {
            Vector3 tailPos = transform.position + tracerDir * tracerOffset;
            float length = ammo.tracerLength * Time.timeScale + ammo.tracerWidth * 3f;
            Vector3 frontPos = tailPos + tracerDir * length;
            line.SetPosition(0, tailPos);
            line.SetPosition(1, frontPos);

            Vector3 midPos = Vector3.Lerp(tailPos, frontPos, Random.value);
            Vector3 midOffset = Random.insideUnitSphere * ammo.tracerScatter * ammo.tracerWidth * Time.timeScale;
            line.SetPosition(0, tailPos);
            line.SetPosition(1, midPos + midOffset / 2f);
            line.SetPosition(2, midPos - midOffset / 2f);
            line.SetPosition(3, frontPos);
        }
    }

    public void Ricochet(RaycastHit hit, float vel)
    {
        transform.position = hit.point;
        if (bullet.explosive) SelfDestruct(true);
        float alpha = Vector3.Angle(transform.forward, -hit.normal);
        float chance = Mathf.InverseLerp(ammo.noRicochetAlpha, ammo.ricochetAlpha, alpha);
        if (Random.value < chance)
        {
            transform.forward = Vector3.Reflect(transform.forward, hit.normal);
            transform.Rotate(Vector3.forward * Random.Range(-90f, 90f));
            transform.Rotate(Vector3.right * Random.Range(-7f, 7f));
            InitializeTrajectory(transform.forward * vel * 0.7f * Mathf.Sin(alpha * Mathf.Deg2Rad));
        }
        else SelfDestruct(true);
    }

    public void SelfDestruct(bool explodes)
    {
        if ((bullet.explosive && explodes) || bullet.fuze)
        {
            Explosion.ExplosionDamage(transform.position, bullet.tntMass / 1000f, ammo.mass);
            Destroy(Instantiate(ammo.bulletHits.explosiveHit, transform.position, Quaternion.identity), 10f);
        }
        Destroy(gameObject);
    }

    //Compute bullet raycasting damages, return true if something has been hit
    public bool BulletAction(float relativeSpeed, Vector3 direction, float range)
    {


        //Cast and sort hits
        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, direction, range, LayerMask.GetMask("SofObject"));
        if (hits.Length == 0) return false;
        for (int i = 0; i < hits.Length; i++) //Sort hits in order
        {
            int j = i;
            while (j > 0 && hits[j - 1].distance > hits[j].distance)
            {
                RaycastHit save = hits[j];
                hits[j] = hits[j - 1];
                hits[j - 1] = save;
            }
        }

        //Hit audio
        if (GameManager.player.tr == hits[0].collider.transform.root)
            GameManager.player.sofObj.avm.persistent.local.PlayOneShot(ammo.bulletHits.GetClip(bullet.explosive), 1f);


        //Particles
        Destroy(Instantiate(bullet.AircraftHit(), hits[0].point, Quaternion.LookRotation(hits[0].normal), hits[0].collider.transform), 10f);
        Instantiate(ammo.bulletHits.bulletHole, hits[0].point + hits[0].normal.normalized * 0.05f, Quaternion.LookRotation(-hits[0].normal), hits[0].collider.transform);
        for (float debrisChance = ammo.bulletHits.debrisChance; debrisChance > 0f; debrisChance--)
            if (Random.value < debrisChance) Destroy(Instantiate(ammo.bulletHits.debris, hits[0].point, Quaternion.LookRotation(hits[0].normal)), 10f);

        //Kinetic Bullet Damage
        float energy = ammo.mass * relativeSpeed * relativeSpeed / 2000f;
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            Part part = hit.collider.GetComponent<Part>();
            if (part)
            {
                float penetrationPower = bullet.penetration * energy / ammo.KineticEnergy;
                float alpha = Vector3.Angle(-hit.normal, transform.forward);
                if (!part.material) Debug.LogError(part.name + " has no part material");
                float armor = Random.Range(0.8f, 1.2f) * part.material.armor / Mathf.Cos(alpha * Mathf.Deg2Rad);
                //If penetration occurs
                if (penetrationPower > armor)
                {
                    part.Damage(energy / 1000f, ammo.caliber, bullet.fireMultiplier);

                    //Full penetration, reduce energy
                    float fullPenArmor = Random.Range(0.8f, 1.2f) * part.material.totalThickness + armor;
                    energy *= 1f - fullPenArmor / penetrationPower;
                }
                else //Try ricochet if penetration fails
                {
                    Ricochet(hit, relativeSpeed);
                    return true;
                }
            }
            //Destroy bullet if no energy left
            if (energy <= 0f) { transform.position = hits[i].point; SelfDestruct(true); return true; }
            //Compute new bullet velocity
            relativeSpeed = Mathf.Sqrt(energy * 2000f / ammo.mass);
        }
        //Update bullet velocity based on left speed
        InitializeTrajectory(transform.forward * relativeSpeed);
        return true;
    }

    void WaterImpact()
    {
        transform.Translate(transform.position.y * Vector3.down);
        Destroy(Instantiate(ammo.bulletHits.waterHit, transform.position, Quaternion.identity), 10f);
        Destroy(gameObject);
    }
    void OnTriggerEnter(Collider obj)
    {
        float speed = initVelocity.magnitude;

        //Ground or fixed objects have been hit
        if (obj.gameObject.layer != 11)
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;
            if (!Physics.Raycast(ray, out hit, 15f, LayerMask.GetMask("Default", "Terrain"))) return;

            Destroy(Instantiate(bullet.EnvironmentHit(obj), hit.point, Quaternion.LookRotation(hit.normal)), 10f);
            SofSimple sofSimple = hit.collider.transform.parent ? hit.collider.transform.parent.GetComponent<SofSimple>() : null;
            if (sofSimple && sofSimple.bulletAffected)
                sofSimple.BulletDamage(ammo.mass * speed * speed / 2000f);

            Ricochet(hit, velocity.magnitude);
        }

        //Object with complex damage system have been hit
        else if (obj.gameObject != bubbleShotFrom)
        {
            
            Vector3 relativeVelocity = velocity - obj.transform.root.GetComponent<Rigidbody>().velocity;
            float relativeSpeed = relativeVelocity.magnitude;
            BulletAction(relativeSpeed,relativeVelocity , 35f);
        }

    }
}
