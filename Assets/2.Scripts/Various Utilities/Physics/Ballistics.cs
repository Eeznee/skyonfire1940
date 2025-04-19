using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Ballistics
{
    [System.Serializable]
    public struct ProjectileChart
    {
        public float mass;
        public float basePenetration;
        public float atBaseVelocity;

        public float diameter;
        public float fireChance;

        public ProjectileChart(float _mass, float _basePen, float _baseVel, float _diameter, float _fireChance)
        {
            mass = _mass;
            basePenetration = _basePen;
            atBaseVelocity = _baseVel;
            diameter = _diameter;
            fireChance = _fireChance;
        }
        public float KineticEnergy(float sqrVelocity)
        {
            return mass * sqrVelocity * 0.5f;
        }
        public float KineticEnergy(Vector3 velocity)
        {
            return KineticEnergy(velocity.sqrMagnitude);
        }
        const float energyToDamage = 0.001f;
        public float KineticDamage(float sqrVelocity)
        {
            return KineticEnergy(sqrVelocity) * energyToDamage;
        }
        public float KineticDamage(Vector3 velocity)
        {
            return KineticDamage(velocity.sqrMagnitude);
        }
        public float Pen(float sqrVelocity)
        {
            return basePenetration * sqrVelocity / (atBaseVelocity * atBaseVelocity);
        }
        public float Pen(Vector3 velocity)
        {
            return Pen(velocity.sqrMagnitude);
        }
    }
    public enum HitSummary
    {
        NoHit,
        Penetration,
        Stopped
    }
    public struct HitResult
    {
        public RaycastHit firstHit;
        public RaycastHit lastHit;
        public Vector3 velocityLeft;
        public HitSummary summary;

        public HitResult(RaycastHit _firstHit, RaycastHit _lastHit, Vector3 _velocityLeft, HitSummary _summary)
        {
            firstHit = _firstHit;
            lastHit = _lastHit;
            velocityLeft = _velocityLeft;
            summary = _summary;
        }

        public static HitResult NoHit(Vector3 velocity) { return new HitResult(new RaycastHit(), new RaycastHit(), velocity, HitSummary.NoHit); }
    }
    public static Quaternion Spread(Quaternion rotation, float maxAngle)
    {
        rotation *= Quaternion.Euler(0f, 0f, Random.Range(-90, 90));
        rotation *= Quaternion.Euler(Random.Range(-1f, 1f) * maxAngle, 0f, 0f);
        return rotation;
    }
    public static float ProjectileDragCoeff(float diameter, float mass)
    {
        return M.Pow(diameter * 0.001f * 0.5f, 2) * Mathf.PI * 0.1f / mass;
    }
    public static float ApproximatePenetration(float mass, float vel, float diameter)
    {
        return mass * vel * vel / (Mathf.Pow(diameter, 1.5f) * 35f);
    }
    public static Vector3 BallisticTrajectory(Vector3 dir, float speed, float dragCoeff, float time)
    {
        float ballisticDistance = Mathf.Log(dragCoeff * time * speed + 1f) / dragCoeff;
        return ballisticDistance * dir + Physics.gravity * 0.5f * time * time;
    }
    public static RaycastHit[] RaycastAndSort(Vector3 pos, Vector3 dir, float range, int layerMask)
    {
        RaycastHit[] hits = Physics.RaycastAll(pos, dir, range, layerMask);
        for (int i = 0; i < hits.Length - 1; i++) //Sort hits in order
            for (int j = 0; j < hits.Length - i - 1; j++)
                if (hits[j].distance > hits[j + 1].distance) { RaycastHit jplus1 = hits[j + 1]; hits[j + 1] = hits[j]; hits[j] = jplus1; }
        return hits;
    }
    public static HitResult RaycastDamage(Vector3 position, Vector3 velocity, float range, ProjectileChart chart)
    {
        RaycastHit[] hits = RaycastAndSort(position, velocity, range, LayerMask.GetMask("SofComplex"));
        if (hits.Length == 0) return HitResult.NoHit(velocity);

        float sqrVelocity = velocity.sqrMagnitude;
        float initialSqrVelocity = sqrVelocity;

        bool oneConfirmedHit = false;
        RaycastHit firstHit = new RaycastHit();
        RaycastHit lastHit = new RaycastHit();

        foreach (RaycastHit hit in hits)
        {
            SofModule module = hit.collider.GetComponent<SofModule>();
            if (module == null) continue;

            if (firstHit.collider == null) firstHit = hit;
            lastHit = hit;
            oneConfirmedHit = true;

            float penetrationPower = chart.Pen(sqrVelocity);

            float alpha = Vector3.Angle(-hit.normal, velocity);
            float armor = Random.Range(0.8f, 1.2f) * module.Armor.surfaceArmor / Mathf.Cos(alpha * Mathf.Deg2Rad);


            if (penetrationPower > armor)//If penetration occurs
            {
                module.ProjectileDamage(chart.KineticDamage(initialSqrVelocity), chart.diameter, chart.fireChance);
                armor += Random.Range(0.8f, 1.2f) * module.Armor.fullPenArmor;
                sqrVelocity *= 1f - armor / penetrationPower;

                if (sqrVelocity <= 0f) return new HitResult(firstHit, lastHit, Vector3.zero, HitSummary.Stopped);
            }
            else return new HitResult(firstHit, lastHit, velocity.normalized * Mathf.Sqrt(sqrVelocity), HitSummary.Stopped);
        }

        if (!oneConfirmedHit) return HitResult.NoHit(velocity);

        velocity = velocity.normalized * Mathf.Sqrt(sqrVelocity);
        return new HitResult(firstHit, lastHit, velocity, HitSummary.Penetration);
    }
    public static float ExplosionRangeSimple(float kgTnt)
    {
        return Mathf.Sqrt(kgTnt);
    }
    public static float HalfExplosionRangeSimple(float kgTnt)
    {
        return Mathf.Sqrt(kgTnt) * 2f;
    }
    const float explosionRangeFactor = 30f;
    public static float MaxExplosionDamageRange(float kgTnt)
    {
        return Mathf.Sqrt(kgTnt) * explosionRangeFactor;
    }
    public static float MaxExplosionDamageRangeSqrt(float kgTnt)
    {
        return kgTnt * explosionRangeFactor * explosionRangeFactor;
    }
    public static float InterceptionTime(float shotSpeed, Vector3 relativePos, Vector3 relativeVel)
    {
        float velocitySquared = relativeVel.sqrMagnitude;
        if (velocitySquared < 0.001f)
            return 0f;

        float a = velocitySquared - shotSpeed * shotSpeed;

        //handle similar velocities
        if (Mathf.Abs(a) < 0.001f)
        {
            float t = -relativePos.sqrMagnitude / (2f * Vector3.Dot(relativeVel, relativePos));
            return Mathf.Max(t, 0f); //don't shoot back in time
        }

        float b = 2f * Vector3.Dot(relativeVel, relativePos);
        float c = relativePos.sqrMagnitude;
        float determinant = b * b - 4f * a * c;

        if (determinant > 0f)
        { //determinant > 0; two intercept paths (most common)
            float t1 = (-b + Mathf.Sqrt(determinant)) / (2f * a),
                    t2 = (-b - Mathf.Sqrt(determinant)) / (2f * a);
            if (t1 > 0f)
            {
                if (t2 > 0f)
                    return Mathf.Min(t1, t2); //both are positive
                else
                    return t1; //only t1 is positive
            }
            else
                return Mathf.Max(t2, 0f); //don't shoot back in time
        }
        else if (determinant < 0f) //determinant < 0; no intercept path
            return 0f;
        else //determinant = 0; one intercept path, pretty much never happens
            return Mathf.Max(-b / (2f * a), 0f); //don't shoot back in time
    }
}
