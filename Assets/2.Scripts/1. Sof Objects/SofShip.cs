using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SofShip : SofModular
{
    public float width = 20f;
    public float heightAboveWater = 10f;

    public bool seperateSections = false;
    public float midBowPortion = 0.7f;
    public float midSternPortion = 0.3f;

    public float projectileHp = 3000f;
    public float maxTntKgCharge = 50f;
    public float armorPlatingmm = 10f;

    public float sinkingTime = 30f;
    public float equilibrumRoll = 0f;
    public float equilibrumPitch = 0f;
    public float spring = 1f;
    public float damper = 1f;

    public bool ammoCanDetonate = false;
    public float ammoDetonationChance = 0.2f;
    public float ammoDetonationInstantChance = 0.1f;
    public ExplosiveFiller ammoDetonationCharge;

    public bool canCatchFire = false;
    public float chanceToCatchFire = 0.2f;
    public ShipFlamesFX fireFX;
    private ShipFlamesFX currentFire;

    private float rollAngle;
    private float rollVelocity;
    private float pitchAngle;
    private float pitchVelocity;

    public Bounds MidShipBounds { get; private set; }
    public Bounds FullShipBounds { get; private set; }
    public float BowPoint { get; private set; }
    public float SternPoint { get; private set; }
    public float MidBowPoint { get; private set; }
    public float MidSternPoint { get; private set; }

    [NonSerialized]public float sinkingDepth;
    private bool sinking;
    private bool fullySinked;

    protected override void GameInitialization()
    {
        if (name.EndsWith(')')) name = name[..^4];

        base.GameInitialization();

        rollAngle = 0f;
        rollVelocity = 0f;
        pitchAngle = 0f;
        pitchVelocity = 0f;

        equilibrumRoll = 0f;
        equilibrumPitch = 0f;

        sinkingDepth = 0f;
        sinking = false;

        UpdateBounds();
    }

    public void UpdateBounds()
    {
        BowPoint = -Mathf.Infinity;
        SternPoint = Mathf.Infinity;
        foreach (Collider collider in GetComponentsInChildren<Collider>())
        {
            Vector3 bow = collider.ClosestPointOnBounds(transform.position + transform.forward * 1000f);
            Vector3 stern = collider.ClosestPointOnBounds(transform.position + transform.forward * -1000f);

            bow = transform.InverseTransformPoint(bow);
            stern = transform.InverseTransformPoint(stern);

            if (bow.z > BowPoint) BowPoint = bow.z;
            if (stern.z < SternPoint) SternPoint = stern.z;
        }
        Vector3 size = new Vector3(width, heightAboveWater, BowPoint - SternPoint);
        Vector3 pos = new Vector3(0f, heightAboveWater * 0.5f, (BowPoint + SternPoint) * 0.5f);

        FullShipBounds = new Bounds(pos, size);

        if (seperateSections)
        {
            MidBowPoint = Mathf.Lerp(SternPoint, BowPoint, midBowPortion);
            MidSternPoint = Mathf.Lerp(SternPoint, BowPoint, midSternPortion);

            size.z = MidBowPoint - MidSternPoint;
            pos.z = (MidBowPoint + MidSternPoint) * 0.5f;
            MidShipBounds = new Bounds(pos, size);
        }
        else
        {
            MidBowPoint = BowPoint;
            MidSternPoint = SternPoint;
            MidShipBounds = FullShipBounds;
        }
    }

    public void Update()
    {
        UpdateRollAngle();
        UpdatePitchAngle();

        float yawAngle = transform.rotation.eulerAngles.y;
        Vector3 euler = new Vector3(pitchAngle, yawAngle, rollAngle);
        transform.rotation = Quaternion.Euler(euler);

        Vector3 pos = transform.position;
        pos.y = sinkingDepth;
        transform.position = pos;
    }
    private void UpdateRollAngle()
    {
        float springForce = (equilibrumRoll - rollAngle) * spring;
        float damperForce = -rollVelocity * damper;

        float rollForce = springForce + damperForce;

        rollVelocity += rollForce * Time.deltaTime;
        rollAngle += rollVelocity * Time.deltaTime;
    }
    private void UpdatePitchAngle()
    {
        float springForce = (equilibrumPitch - pitchAngle) * spring * 0.5f;
        float damperForce = -pitchVelocity * damper * 2f;

        float pitchForce = springForce + damperForce;

        pitchVelocity += pitchForce * Time.deltaTime;
        pitchAngle += pitchVelocity * Time.deltaTime;

        pitchAngle = Mathf.Clamp(pitchAngle, -89f, 89f);
    }
    string RemoveNumberSuffix(string name)
    {
        // Check for the pattern "(number)" at the end of the string
        // This regex looks for:
        //   \(     - an opening parenthesis (escaped)
        //   \d+    - one or more digits
        //   \)     - a closing parenthesis (escaped)
        //   $      - end of the string
        System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(name, @"\(\d+\)$");

        if (match.Success)
        {
            return name.Substring(0, name.Length - match.Length);
        }
        else
        {
            return name; // No suffix found, return original name
        }
    }
    public void DestroyShip(Vector3 finalDamagePosition)
    {
        if (destroyed == true) return;


        Log.Print(name + " sunk", "ship sunk");

        destroyed = true;

        Vector3 localPos = transform.InverseTransformPoint(finalDamagePosition);

        if (seperateSections && localPos.z < MidSternPoint) StartCoroutine(StartSinkingAnimation(0, -1));
        else if (seperateSections && localPos.z > MidBowPoint) StartCoroutine(StartSinkingAnimation(0, 1));
        else if (localPos.x < width && finalDamagePosition.y < 2f) StartCoroutine(StartSinkingAnimation(-1, 0));
        else if (localPos.x > -width && finalDamagePosition.y < 2f) StartCoroutine(StartSinkingAnimation(1, 0));
        else
        {
            StartCoroutine(StartSinkingAnimation(0, 0));
        }


        if (ammoCanDetonate)
        {
            if (UnityEngine.Random.value < ammoDetonationChance)
            {
                if (UnityEngine.Random.value < ammoDetonationInstantChance)
                {
                    StartCoroutine(DetonateAmmo(0.1f));
                }
                else
                {
                    StartCoroutine(DetonateAmmo(UnityEngine.Random.Range(0.1f, sinkingTime)));
                }
            }
        }

        if (canCatchFire)
        {
            if (UnityEngine.Random.value < chanceToCatchFire)
            {
                StartFire(localPos.z);
            }
        }


    }
    private IEnumerator DetonateAmmo(float delay)
    {
        yield return new WaitForSeconds(delay);
        ammoDetonationCharge.Detonate(transform.position, ammoDetonationCharge.mass * 2f, null);
    }
    public void StartFire(float zPosition)
    {
        if (!canCatchFire || !fireFX || currentFire != null) return;
        StartCoroutine(FireCoroutine(zPosition));
    }
    private IEnumerator FireCoroutine(float zPosition)
    {
        currentFire = Instantiate(fireFX);
        currentFire.SetparticleSystem(this, zPosition);

        while (!fullySinked)
        {
            yield return null;
        }

        currentFire.StopFire();
    }
    private IEnumerator StartSinkingAnimation(int rollSinking, int pitchSinking)
    {
        if (sinking) yield break;

        sinking = true;

        float sinkTimer = 0f;
        float pitchAngle = UnityEngine.Random.Range(45f, 90f);

        if (pitchSinking != 0) sinkingTime *= 1.5f;

        while (sinkTimer < sinkingTime)
        {
            float t = sinkTimer / sinkingTime;

            if (rollSinking != 0)
            {
                equilibrumRoll = Mathf.Lerp(0f, 180f * Mathf.Sign(rollSinking), Mathf.Clamp01(M.Pow(t, 2)));
            }
            if (pitchSinking != 0)
            {
                equilibrumPitch = Mathf.Lerp(0f, pitchAngle * Mathf.Sign(pitchSinking), Mathf.Clamp01(t));
            }

            sinkingDepth = -t * heightAboveWater;
            if (rollSinking != 0) sinkingDepth = 0f;

            sinkTimer += Time.deltaTime;

            yield return null;
        }

        fullySinked = true;

        while (sinkingDepth > -50f)
        {
            float speed = pitchSinking == 0 ? 0.25f : 1.5f;
            sinkingDepth -= Time.deltaTime * speed;
            yield return null;
        }
    }



#if UNITY_EDITOR
    protected void OnDrawGizmos()
    {
        UpdateBounds();

        Handles.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        Handles.color = Color.yellow;

        Vector3 size = new Vector3(width, heightAboveWater, BowPoint - SternPoint);
        Vector3 pos = new Vector3(0f, heightAboveWater * 0.5f, (BowPoint + SternPoint) * 0.5f);
        Handles.DrawWireCube(pos, size);

        if (seperateSections)
        {
            Handles.color = Color.green;

            size = new Vector3(width, heightAboveWater, MidBowPoint - MidSternPoint);
            pos = new Vector3(0f, heightAboveWater * 0.5f, (MidBowPoint + MidSternPoint) * 0.5f);
            Handles.DrawWireCube(pos, size);
        }

    }
#endif
}
