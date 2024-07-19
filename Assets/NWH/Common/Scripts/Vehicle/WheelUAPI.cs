using UnityEngine;
using UnityEngine.Events;

namespace NWH.Common.Vehicles
{
    public abstract class WheelUAPI : MonoBehaviour
    {
        // INPUTS
        public abstract float BrakeTorque { get; set; }
        public abstract float RollingResistanceTorque { get; set; }


        // PHYSICAL PROPERTIES
        public abstract float Mass { get; set; }
        public abstract float Radius { get; set; }
        public abstract float Width { get; set; }
        public abstract float Inertia { get; set; }
        public abstract float RPM { get; }
        public abstract float AngularVelocity { get; }
        public abstract Vector3 WheelPosition { get; }
        public abstract float Load { get; }
        public abstract float MaxLoad { get; set; }
        public abstract bool IsGrounded { get; }

        // SPRING
        public abstract float SpringMaxLength { get; set; }
        public abstract float SpringMaxForce { get; set; }
        public abstract float SpringForce { get; }
        public abstract float SpringLength { get; }
        public abstract float SpringCompression { get; }


        // DAMPER
        public abstract float DamperBumpRate { get; set; }
        public abstract float DamperReboundRate { get; set; }
        public abstract float DamperForce { get; }


        // FRICTION
        public abstract FrictionPreset FrictionPreset { get; set; }
        public abstract float LongitudinalFrictionGrip { get; set; }
        public abstract float LongitudinalFrictionStiffness { get; set; }
        public abstract float LateralFrictionGrip { get; set; }
        public abstract float LateralFrictionStiffness { get; set; }
        public abstract float ForceApplicationPointDistance { get; set; }


        // LONGITUDINAL FRICTION
        public abstract float LongitudinalSlip { get; }
        public abstract float LongitudinalSpeed { get; }
        public virtual bool IsSkiddingLongitudinally
        {
            get { return NormalizedLongitudinalSlip > 0.35f; }
        }

        public virtual float NormalizedLongitudinalSlip
        {
            get
            {
                float lngSlip = LongitudinalSlip;
                float absLngSlip = lngSlip < 0f ? -lngSlip : lngSlip;
                return absLngSlip < 0f ? 0f : absLngSlip > 1f ? 1f : absLngSlip;
            }
        }


        // LATERAL FRICTION
        public abstract float LateralSlip { get; }
        public abstract float LateralSpeed { get; }
        public virtual bool IsSkiddingLaterally
        {
            get { return NormalizedLateralSlip > 0.35f; }
        }

        public virtual float NormalizedLateralSlip
        {
            get
            {
                float latSlip = LateralSlip;
                float absLatSlip = latSlip < 0f ? -latSlip : latSlip;
                return absLatSlip < 0f ? 0f : absLatSlip > 1f ? 1f : absLatSlip;
            }
        }


        // FRICTION CIRCLE
        public abstract float FrictionCircleShape { get; set; }
        public abstract float FrictionCircleStrength { get; set; }


        // COLLISION
        public abstract Vector3 HitPoint { get; }
        public abstract Vector3 HitNormal { get; }
        public abstract Collider HitCollider { get; }


        // VISUAL
        public abstract GameObject WheelVisual { get; set; }

        public abstract GameObject NonRotatingVisual { get; set; }


        // GENERAL
        public abstract Rigidbody ParentRigidbody { get; }

        public abstract void Validate();
    }
}

