using UnityEngine;
using NWH.Common.Vehicles;


namespace NWH.WheelController3D
{
    public partial class WheelController
    {

        [ShowInTelemetry]
        public override float BrakeTorque
        {
            get => brakeTorque;
            set => brakeTorque = value;
        }

        public override float Mass
        {
            get => wheel.mass;
            set
            {
                wheel.mass = Mathf.Clamp(value, 0f, Mathf.Infinity);
                wheel.UpdatePhysicalProperties();
            }
        }

        public override float Radius
        {
            get => wheel.radius;
            set
            {
                wheel.radius = value;
                wheel.UpdatePhysicalProperties();
            }
        }

        public override float Width
        {
            get => wheel.width;
            set
            {
                wheel.width = value;
                wheel.UpdatePhysicalProperties();
            }
        }

        public override float Inertia
        {
            get => wheel.inertia;
            set => wheel.inertia = value < 0f ? 0f : value;
        }


        [ShowInTelemetry]
        public override float RPM
        {
            get => wheel.rpm;
        }

        public override float AngularVelocity
        {
            get => wheel.angularVelocity;
        }

        public override Vector3 WheelPosition
        {
            get => wheel.worldPosition;
        }

        [ShowInTelemetry]
        public override float Load
        {
            get => load;
        }

        public override float MaxLoad
        {
            get => loadRating;
            set => loadRating = value < 0f ? 0f : value;
        }

        [ShowInTelemetry]
        public override bool IsGrounded
        {
            get => _isGrounded;
        }

        public override float SpringMaxLength
        {
            get => spring.maxLength;
            set
            {
                spring.maxLength = value < 0f ? 0f : value;
            }
        }

        public override float SpringMaxForce
        {
            get => spring.maxForce;
            set
            {
                spring.maxForce = value < 0f ? 0f : value;
            }
        }

        [ShowInTelemetry]
        public override float SpringForce
        {
            get => spring.force;
        }

        [ShowInTelemetry]
        public override float SpringLength
        {
            get => spring.length;
        }

        public override float SpringCompression
        {
            get => spring.maxLength == 0f ? 1f
                : (spring.maxLength - spring.length) / spring.maxLength;
        }

        public override float DamperBumpRate
        {
            get => damper.bumpRate;
            set => damper.bumpRate = value < 0f ? 0f : value;
        }

        public override float DamperReboundRate
        {
            get => damper.reboundRate;
            set => damper.reboundRate = value < 0f ? 0f : value;
        } // TODO - finish this

        [ShowInTelemetry]
        public override float DamperForce
        {
            get => damper.force;
        }

        [ShowInTelemetry]
        public override float LongitudinalSlip
        {
            get => forwardFriction.slip;
        }

        [ShowInTelemetry]
        public override float LongitudinalSpeed
        {
            get => forwardFriction.speed;
        }

        [ShowInTelemetry]
        public override float LateralSlip
        {
            get => sideFriction.slip;
        }

        [ShowInTelemetry]
        public override float LateralSpeed
        {
            get => sideFriction.speed;
        }

        public override Vector3 HitPoint
        {
            get
            {
                if (_isGrounded)
                {
                    return wheelHit.point;
                }
                else
                {
                    return Vector3.zero;
                }
            }
        }

        public override Vector3 HitNormal
        {
            get
            {
                if (_isGrounded)
                {
                    return wheelHit.normal;
                }
                else
                {
                    return Vector3.zero;
                }
            }
        }

        public override GameObject WheelVisual
        {
            get => wheel.visual;
            set
            {
                wheel.visual = value;
            }
        }

        public override GameObject NonRotatingVisual
        {
            get => wheel.nonRotatingVisual;
            set
            {
                wheel.nonRotatingVisual = value;
            }
        }

        public override Rigidbody ParentRigidbody
        {
            get => targetRigidbody;
        }

        public override Collider HitCollider
        {
            get => wheelHit.collider;
        }

        public override float ForceApplicationPointDistance
        {
            get => forceApplicationPointDistance;
            set => forceApplicationPointDistance = value;
        }

        public override FrictionPreset FrictionPreset
        {
            get => activeFrictionPreset;
            set => activeFrictionPreset = value;
        }

        public override float LongitudinalFrictionGrip
        {
            get => forwardFriction.grip;
            set => forwardFriction.grip = value < 0f ? 0f : value;
        }

        public override float LongitudinalFrictionStiffness
        {
            get => forwardFriction.stiffness;
            set => forwardFriction.stiffness = value < 0f ? 0f : value;
        }

        public override float LateralFrictionGrip
        {
            get => sideFriction.grip;
            set => sideFriction.grip = value < 0f ? 0f : value;
        }

        public override float LateralFrictionStiffness
        {
            get => sideFriction.stiffness;
            set => sideFriction.stiffness = value < 0f ? 0f : value;
        }

        public override float RollingResistanceTorque
        {
            get => rollingResistanceTorque;
            set => rollingResistanceTorque = value < 0f ? 0f : value;
        }

        public override float FrictionCircleShape
        {
            get => frictionCircleShape;
            set => frictionCircleShape = value < 0f ? 0f : value;
        }

        public override float FrictionCircleStrength
        {
            get => frictionCircleStrength;
            set => frictionCircleStrength = value < 0f ? 0f : value;
        }


        public override void Validate()
        {
            OnValidate();
        }
    }
}
