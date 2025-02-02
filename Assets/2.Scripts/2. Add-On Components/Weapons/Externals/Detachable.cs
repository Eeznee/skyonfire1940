using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Detachable : SofComponent
{

    public float mass;
    public Mesh droppedMesh;

    protected bool dropped = false;

    protected float SecurityTimer => 1f;


    protected Vector3 startVelocity;
    protected Vector3 startPosition;
    protected Quaternion startRotation;
    protected float startTime;

    protected Vector3 inAirVelocity;

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        dropped = false;
        if(GetComponent<Collider>()) GetComponent<Collider>().enabled = true;
    }
    public virtual void Drop()
    {
        if (dropped) return;

        OnDrop();

    }
    private void OnTriggerExit(Collider other)
    {

    }
    protected virtual void OnDrop()
    {
        dropped = true;

        startPosition = transform.position;
        startRotation = transform.rotation;
        startVelocity = rb.GetPointVelocity(startPosition);
        startVelocity += Random.insideUnitSphere * 0.4f;
        startTime = Time.time;

        transform.parent = null;

        complex.RemoveComponentRoot(this);

        StartCoroutine(DropSequence());

        if (droppedMesh) GetComponent<MeshFilter>().sharedMesh = droppedMesh;
    }
    protected virtual IEnumerator DropSequence()
    {
        yield return null;
        inAirVelocity = startVelocity;
        float timeSinceDrop = 0f;
        float heightPrediction = GameManager.map.RelativeHeight(tr.position) + rb.velocity.y * Time.deltaTime;

        while (heightPrediction > 3f || Mathf.Abs(inAirVelocity.y) < 1f && heightPrediction < 3f)
        {
            inAirVelocity = startVelocity + Physics.gravity * timeSinceDrop;
            transform.position += inAirVelocity * Time.deltaTime;

            Vector3 forward = Vector3.RotateTowards(transform.forward, inAirVelocity.normalized, Time.deltaTime * 0.5f, 0f);
            transform.rotation = Quaternion.LookRotation(forward, transform.up);

            timeSinceDrop += Time.deltaTime;
            heightPrediction = GameManager.map.RelativeHeight(tr.position) + rb.velocity.y * Time.deltaTime;
            yield return null;
        }

        OnGroundContact(timeSinceDrop);
    }

    protected virtual void OnGroundContact(float timeSinceDrop) 
    {

    }

    protected void Root()
    {
        Vector3 pos = transform.position;
        pos.y = GameManager.map.HeightAtPoint(pos);
        transform.position = pos;
        GetComponent<Collider>().enabled = false;
    }
}
