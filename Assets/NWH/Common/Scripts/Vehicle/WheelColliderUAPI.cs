using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace NWH.Common.Vehicles
{
    [RequireComponent(typeof(WheelCollider))]
    public class WheelColliderUAPI : WheelUAPI
    {
        public GameObject wheelVisual;
        public float width = 0.3f;

        [SerializeField] private WheelCollider _wc;
        [SerializeField] private Rigidbody _rb;

        private WheelHit _wheelHit;
        private bool _isGrounded;
        private Vector3 _rbVelocity;
        private float _forwardSpeed;
        private float _sideSpeed;
        private float _inertia;
        private float _latFrictionStiffness;
        private float _latFrictionGrip;
        private float _lngFrictionStiffness;
        private float _lngFrictionGrip;


        public override float BrakeTorque
        {
            get { return _wc.brakeTorque; }
            set { _wc.brakeTorque = value; }
        }
        public override float Mass
        {
            get { return _wc.mass; }
            set { _wc.mass = value; }
        }

        public override float Inertia
        {
            get { return _inertia; }
            set { Mathf.Clamp(_inertia, 1e-6f, Mathf.Infinity); }
        }

        public override float Radius
        {
            get { return _wc.radius; }
            set { _wc.radius = value; }
        }

        public override float Width
        {
            get { return width; }
            set { width = value; }
        }

        public override float RPM
        {
            get { return _wc.rpm; }
        }

        public override float AngularVelocity
        {
            get { return _wc.rpm * 0.10471975512f; }
        }

        public override Vector3 WheelPosition
        {
            get { return transform.TransformPoint(_wc.center); }
        }

        public override float Load
        {
            get { return _isGrounded ? _wheelHit.force : 0f; }
        }

        public override float MaxLoad
        {
            get { return _wc.forwardFriction.extremumValue; }
            set
            {
                var forwardFriction = _wc.forwardFriction;
                forwardFriction.extremumValue = value;
                forwardFriction.asymptoteValue = forwardFriction.extremumValue * 0.7f; // TODO - could be solved better
                _wc.forwardFriction = forwardFriction;
            }
        }

        public override bool IsGrounded
        {
            get { return _isGrounded; }
        }
        public override float SpringMaxLength
        {
            get { return _wc.suspensionDistance; }
            set { _wc.suspensionDistance = value; }
        }

        public override float SpringMaxForce
        {
            get { return _wc.suspensionSpring.spring; }
            set
            {
                JointSpring suspensionSpring = _wc.suspensionSpring;
                suspensionSpring.spring = value;
                _wc.suspensionSpring = suspensionSpring;
            }
        }

        public override float SpringForce
        {
            get { return _wc.isGrounded ? _wheelHit.force : 0f; }
        }

        public override float SpringLength
        {
            get { return -_wc.center.y; }
        }

        public override float SpringCompression
        {
            get { return SpringLength / SpringMaxLength; }
        }

        public override float DamperBumpRate
        {
            get { return _wc.suspensionSpring.damper; }
            set
            {
                JointSpring suspensionSpring = _wc.suspensionSpring;
                suspensionSpring.damper = value;
                _wc.suspensionSpring = suspensionSpring;
            }
        }

        public override float DamperReboundRate
        {
            get { return DamperBumpRate; }
            set { DamperBumpRate = value; }
        }

        public override float DamperForce
        {
            get { return 0f; }
        }

        public override float LongitudinalSlip
        {
            get { return _isGrounded ? _wheelHit.forwardSlip : 0f; }
        }

        public override float LongitudinalSpeed
        {
            get { return _forwardSpeed; }
        }

        public override float LateralSlip
        {
            get { return _isGrounded ? _wheelHit.sidewaysSlip : 0f; }
        }

        public override float LateralSpeed
        {
            get { return _sideSpeed; }
        }

        public override Vector3 HitPoint
        {
            get { return _isGrounded ? _wheelHit.point : Vector3.zero; }
        }

        public override GameObject WheelVisual
        {
            get { return wheelVisual; }
            set { wheelVisual = value; }
        }

        public override GameObject NonRotatingVisual
        {
            get { return null; }
            set { }
        }

        public override Rigidbody ParentRigidbody
        {
            get { return _rb; }
        }

        public override Vector3 HitNormal
        {
            get
            {
                return _isGrounded ? _wheelHit.normal : Vector3.up;
            }
        }

        public override Collider HitCollider
        {
            get
            {
                return _isGrounded ? _wheelHit.collider : null;
            }
        }

        public override float ForceApplicationPointDistance
        {
            get => _wc.forceAppPointDistance;
            set => _wc.forceAppPointDistance = value;
        }

        public override FrictionPreset FrictionPreset
        {
            get { return null; }
            set { }
        }

        public override float LongitudinalFrictionGrip
        {
            get { return _lngFrictionGrip; }
            set { _lngFrictionGrip = value; }
        }

        public override float LongitudinalFrictionStiffness
        {
            get { return _lngFrictionStiffness; }
            set { _lngFrictionStiffness = value; }
        }

        public override float LateralFrictionGrip
        {
            get { return _latFrictionGrip; }
            set { _latFrictionGrip = value; }
        }

        public override float LateralFrictionStiffness
        {
            get { return _latFrictionStiffness; }
            set { _latFrictionStiffness = value; }
        }

        public override float RollingResistanceTorque { get; set; }
        
        public override float FrictionCircleShape { get; set; }
        
        public override float FrictionCircleStrength { get; set; }
        

        public override void Validate()
        {

        }


        void Initialize()
        {
            _wc = GetComponent<WheelCollider>();
            Debug.Assert(_wc != null, "Can not find WheelCollider. Add WheelCollider to the same object as WheelColliderUAPI.");

            _rb = GetComponentInParent<Rigidbody>();
            Debug.Assert(_rb != null, "Rigidbody not found in parent(s).");

            _wc.mass = 200f;
            _inertia = 0.5f * _wc.mass * _wc.radius * _wc.radius;
        }


        void Reset()
        {
            Initialize();
        }


        void Awake()
        {
            Initialize();
        }


        public void FixedUpdate()
        {
            _isGrounded = _wc.GetGroundHit(out _wheelHit);
            _rbVelocity = _rb.GetPointVelocity(WheelPosition);
            Vector3 localRbVelocity = transform.InverseTransformVector(_rbVelocity);
            _forwardSpeed = localRbVelocity.z;
            _sideSpeed = localRbVelocity.x; // TODO - steering not taken into consideration

            _wc.GetWorldPose(out Vector3 pos, out Quaternion rot);
            wheelVisual.transform.SetPositionAndRotation(pos, rot);
        }
    }

}
