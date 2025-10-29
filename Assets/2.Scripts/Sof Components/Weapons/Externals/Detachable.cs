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

    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);
        dropped = false;
        SetCollider(true);
    }
    public virtual void Drop()
    {
        if (dropped) return;

        OnDrop();

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

        sofModular.RemoveComponentRoot(this);

        StartCoroutine(DropSequence());

        SetCollider(false);

        if (droppedMesh) GetComponent<MeshFilter>().sharedMesh = droppedMesh;
    }
    protected virtual IEnumerator DropSequence()
    {
        yield return null;
        inAirVelocity = startVelocity;
        float timeSinceDrop = 0f;
        float heightPrediction = GameManager.mapTool.RelativeHeight(tr.position) + rb.linearVelocity.y * Time.deltaTime;

        while (heightPrediction > 0f)
        {
            inAirVelocity = startVelocity + Physics.gravity * timeSinceDrop;
            transform.position += inAirVelocity * Time.deltaTime;

            Vector3 forward = Vector3.RotateTowards(transform.forward, inAirVelocity.normalized, Time.deltaTime * 0.5f, 0f);
            transform.rotation = Quaternion.LookRotation(forward, transform.up);

            timeSinceDrop += Time.deltaTime;
            heightPrediction = GameManager.mapTool.RelativeHeight(tr.position) + rb.linearVelocity.y * Time.deltaTime;
            yield return null;
        }

        OnGroundContact(timeSinceDrop);
    }

    protected virtual void OnGroundContact(float timeSinceDrop)
    {
        Root();
    }
    protected void Root()
    {
        Vector3 pos = transform.position;
        pos.y = GameManager.mapTool.HeightAtPoint(pos);
        transform.position = pos;
    }
    protected void SetCollider(bool enabled)
    {
        if (GetComponent<Collider>()) GetComponent<Collider>().enabled = enabled;
    }
}
