using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AI
{
    public enum DogfightState
    {
        Neutral,
        Offensive,
        Defensive,
        Engage
    }
    public class GeometricData
    {
        public SofAircraft aircraft;
        public SofAircraft target; 

        const float stateNeutralLimit = 0.3f;
        const float stateEngageDis = 1000f;
        public DogfightState state;
        public Vector3 dir;
        public float distance;
        public float closure;       //How fast a2 is moving towards a1
        public float offAngle;      //Angle of a2 off the nose of a1
        public float crossAngle;    //Angle between a1 nose and a2 nose
        public float aspectAngle;   //Aspect of a2 from a1, 0 = rear , 90 = side, 180 = front
        public float energyDelta;   //Energy advantage of a1 over a2
        public GeometricData(SofAircraft a, SofAircraft t)
        {
            aircraft = a;
            target = t;
            if (target == null) return;

            dir = target.transform.position - aircraft.transform.position;
            distance = dir.magnitude;
            closure = Vector3.Dot(target.rb.velocity - aircraft.rb.velocity, dir/distance);
            offAngle = Vector3.Angle(dir, aircraft.transform.forward);
            crossAngle = Vector3.Angle(aircraft.transform.forward, target.transform.forward);
            aspectAngle = Vector3.Angle(dir, target.transform.forward);
            energyDelta = aircraft.data.energy.Get - target.data.energy.Get;

            float offensiveness = (180f-offAngle - aspectAngle) / 180f; //1f is full offensive, -1f is defensive
            state = offensiveness > stateNeutralLimit ? DogfightState.Offensive : (offensiveness < -stateNeutralLimit ? DogfightState.Defensive : DogfightState.Neutral);
            if (distance > stateEngageDis) state = DogfightState.Engage;
            if (state == DogfightState.Offensive && !aircraft.card.fighter) state = DogfightState.Neutral;
        }

        public bool Collision(float t) { return distance - target.stats.wingSpan + closure * t < 0f; }
    }
    public class GunnerTargetingData
    {
        GunMount turret;
        Rigidbody target;
        public float distance;
        public float closure;
        public Vector3 dir;
        public Vector3 relativeVel;
        public float angularSpeed;
        public float aspectAngle;
        public GunnerTargetingData(GunMount _turret, Rigidbody _target)
        {
            turret = _turret;
            target = _target;

            dir = target.position - turret.transform.position;
            distance = dir.magnitude;
            relativeVel = target.velocity - turret.rb.velocity;
            closure = Vector3.Dot(target.velocity - turret.rb.velocity, dir/distance);
            angularSpeed = Vector3.Angle(dir, dir + target.velocity);
            aspectAngle = Vector3.Angle(dir, target.transform.forward);
        }
    }
    public static bool TargetInSight(Transform pilot, Transform target)
    {
        Vector3 dir = pilot.InverseTransformDirection(target.position - pilot.position).normalized;
        return dir.z > -0.3f && dir.y > -0.2f;
    }
}
