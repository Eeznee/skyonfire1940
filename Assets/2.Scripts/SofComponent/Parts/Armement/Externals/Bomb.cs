using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : SofComponent, IMassComponent
{
    public float LoadedMass => mass;
    public float EmptyMass => 0f;

    public float mass;
    public Mesh droppedMesh;
    public ExplosiveFiller filler;

    protected bool dropped = false;

    protected const float securityTimer = 1f;

    public virtual void Drop(float delayFuse, bool bay)
    {
        if (dropped) return;
        dropped = true;
        StartCoroutine(DropSequence(delayFuse));
    }
    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        dropped = false;
        GetComponent<Collider>().enabled = false;
    }
    private void Drop()
    {
        Detach();

        if (droppedMesh) GetComponent<MeshFilter>().sharedMesh = droppedMesh;
        rb.velocity += Random.insideUnitSphere * 0.4f;
        rb.inertiaTensor = Vector3.one * mass;
        GetComponent<Collider>().enabled = true;
    }
    private void FixedUpdate()
    {
        if (dropped)
        {
            if (aircraft) Drop();
            Vector3 forward = Vector3.RotateTowards(transform.forward, rb.velocity.normalized, Time.fixedDeltaTime * 0.5f, 0f);
            transform.rotation = Quaternion.LookRotation(forward, transform.up);
        }
    }
    protected virtual IEnumerator DropSequence(float delayFuse)
    {
        bool safety = Mathf.Abs(rb.velocity.magnitude) < 15f;

        float securityCount = 0f;
        float heightPrediction;
        do
        {
            securityCount += Time.deltaTime;
            heightPrediction = GameManager.map.RelativeHeight(tr.position) + rb.velocity.y * Time.deltaTime;
            yield return null;

        } while (heightPrediction > 3f || Mathf.Abs(rb.velocity.y) < 1f && heightPrediction < 3f);

        if (rb.velocity.y < -10f) Root();            //Bomb is stuck in the ground
        if (securityCount < securityTimer && safety) //Safety Triggered, bomb will not explode
        {
            Destroy(transform.root.gameObject, 20f);
            yield break;
        }
        yield return new WaitForSeconds(delayFuse);
        Detonate();
    }
    protected void Root()
    {
        Vector3 pos = transform.position;
        pos.y = GameManager.map.HeightAtPoint(pos);
        transform.position = pos;
        GetComponent<Collider>().enabled = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.isKinematic = true;
    }
    protected void Detonate()
    {
        filler.Detonate(transform.position, mass, null);
        Destroy(transform.root.gameObject);
    }
}
