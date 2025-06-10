using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torpedo : Detachable
{
    [SerializeField] private float travelSpeedKph = 75f;
    [SerializeField] private float maximumRange = 3000f;

    public float TravelSpeed => travelSpeedKph / 3.6f;

    [SerializeField] private float maxDropSpeedKph = 300f;
    [SerializeField] private float maxDropAltitude = 50f;
    public float MaxDropSpeed => maxDropSpeedKph / 3.6f;

    [SerializeField] private ExplosiveFiller filler;

    [SerializeField] private GameObject splashEffect;
    [SerializeField] private GameObject trailEffect;

    private bool underWater;
    private float currentSpeed;
    const float drag = 0.6f;


    public float maxVerticalSpeed => Physics.gravity.y * Mathf.Sqrt(2f * maxDropAltitude / -Physics.gravity.y);

    protected override void OnDrop()
    {
        base.OnDrop();
        underWater = false;
    }

    protected override void OnGroundContact(float timeSinceDropped)
    {
        if (GameManager.mapTool.HeightAtPoint(tr.position) > 0f)
        {
            Root();
            Destroy(gameObject, 20f);
            return;
        }

        splashEffect = Instantiate(splashEffect, transform.position, Quaternion.identity);

        GetComponent<MeshRenderer>().enabled = false;

        Vector3 horizontalVelocity = inAirVelocity;
        horizontalVelocity.y = 0f;
        bool overSpeed = horizontalVelocity.magnitude > MaxDropSpeed;
        bool overDrop = inAirVelocity.y < maxVerticalSpeed;

        if (overDrop || overSpeed)
        {
            Destroy(gameObject);
            return;
        }

        SetCollider(true);

        Vector3 forward = transform.forward;
        forward.y = 0f;
        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

        underWater = true;

        trailEffect = Instantiate(trailEffect, transform.position, transform.rotation);

        StartCoroutine(UnderwaterTorpedo());
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!underWater) return;

        ShipDamageModel ship = other.GetComponentInParent<ShipDamageModel>();
        if (ship)
        {
            Vector3 detonationPoint = other.ClosestPoint(transform.position);
            ship.DirectTorpedoHit(detonationPoint, filler.TntEquivalent);
        }

        StopAllCoroutines();
        filler.Detonate(transform.position, mass, null);
        Destroy(transform.root.gameObject);


    }
    protected IEnumerator UnderwaterTorpedo()
    {
        Vector3 waterVelocity = startVelocity;
        waterVelocity.y = 0f;
        currentSpeed = waterVelocity.magnitude;
        float timerLimit = maximumRange / TravelSpeed;

        float securityCount = 0f;
        do
        {
            UpdateTorpedoTravel();
            UpdateTorpedoTrail();

            securityCount += Time.deltaTime;
            yield return null;

        } while (securityCount < timerLimit);

        Destroy(transform.root.gameObject);
    }
    private void UpdateTorpedoTravel()
    {
        float acceleration = Mathf.Max(2f, Mathf.Abs(currentSpeed - TravelSpeed) * drag);
        currentSpeed = Mathf.MoveTowards(currentSpeed, TravelSpeed, Time.deltaTime * acceleration);

        Vector3 position = transform.position;
        position += currentSpeed * transform.forward * Time.deltaTime;
        position.y = 0f;
        transform.position = position;
    }

    private void UpdateTorpedoTrail()
    {
        trailEffect.transform.position = transform.position;
    }
}
