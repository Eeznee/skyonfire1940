using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : Part
{
    public override float EmptyMass() { return 0f; }
    public float explosiveMass;
    public GameObject explosion;
    public GameObject waterSplosion;

    bool dropped = false;
    bool securityOff = false;
    Vector3 up;
    float counter;

    const float securityCounter = 2f;


    //Explosion damage calculations
    

    public void Drop()
    {
        if (dropped) return;

        Detach();
        rb.velocity += Random.insideUnitSphere * 0.4f;
        dropped = true;
        GetComponent<Collider>().enabled = securityOff = false;
        counter = 0f;
        up = transform.up;
        rb.inertiaTensor = new Vector3(emptyMass, emptyMass, emptyMass);
    }
    public override void Initialize(ObjectData d,bool firstTime)
    {
        base.Initialize(d,firstTime);
        if (firstTime)
        {
            dropped = false;
        }
    }

    private void FixedUpdate()
    {
        if (dropped)
        {
            counter += Time.fixedDeltaTime;
            if (counter > securityCounter && !securityOff)
            {
                securityOff = true;
                GetComponent<Collider>().enabled = true;
            }
            Vector3 forward = Vector3.RotateTowards(transform.forward, rb.velocity.normalized, Time.fixedDeltaTime * Mathf.PI * 2f / 10f, 0f);
            transform.rotation = Quaternion.LookRotation(forward, up);

            if (securityOff)
            {
                float height = GameManager.map.HeightAtPoint(transform.position);
                if (transform.position.y < height + 5)
                {
                    transform.position = new Vector3(transform.position.x, height,transform.position.z);
                    Explodes();
                }
            }
        }
    }
    public void Explodes()
    {
        GameObject splosion = transform.position.y < 2f ? waterSplosion : explosion;
        Instantiate(splosion, transform.position, splosion.transform.rotation);
        Explosion.ExplosionDamage(transform.position, explosiveMass, emptyMass);
        Destroy(gameObject);
    }
}
