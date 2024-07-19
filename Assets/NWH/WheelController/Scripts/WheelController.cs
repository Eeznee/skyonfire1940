using NWH.Common.Vehicles;
using System;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using NWH.NUI;
#endif


namespace NWH.WheelController3D
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(100)]
    public partial class WheelController : WheelUAPI
    {
        [Tooltip("    Instance of the spring.")]
        [SerializeField]
        public Spring spring = new Spring();

        [Tooltip("    Instance of the damper.")]
        [SerializeField]
        public Damper damper = new Damper();

        [Tooltip("    Instance of the wheel.")]
        [SerializeField]
        public Wheel wheel = new Wheel();

        [Tooltip("    Side (lateral) friction info.")]
        [SerializeField]
        public Friction sideFriction = new Friction();

        [Tooltip("    Forward (longitudinal) friction info.")]
        [SerializeField]
        public Friction forwardFriction = new Friction();

        /// <summary>
        ///     Contains data about the ground contact point. 
        ///     Not valid if !_isGrounded.
        /// </summary>
        [Tooltip("    Contains point in which wheel touches ground. Not valid if !_isGrounded.")]
        [NonSerialized]
        private WheelHit wheelHit;

        /// <summary>
        ///     Current active friction preset.
        /// </summary>
        [Tooltip("    Current active friction preset.")]
        [SerializeField]
        private FrictionPreset activeFrictionPreset;

        /// <summary>
        ///     Brake torque applied to the wheel in Nm.
        ///     Must be positive.
        /// </summary>
        [Tooltip("    Brake torque applied to the wheel in Nm.")]
        private float brakeTorque;

        /// <summary>
        ///     Tire load in Nm.
        /// </summary>
        [Tooltip("    Tire load in Nm.")]
        [NonSerialized]
        private float load;

        /// <summary>
        ///     Maximum load the tire is rated for in [N]. 
        ///     Used to calculate friction. Default value is adequate for most cars but 
        ///     larger and heavier vehicles such as semi trucks will use higher values.
        ///     A good rule of the thumb is that this value should be 2x the Load (Debug tab) 
        ///     while vehicle is stationary.
        /// </summary>
        [SerializeField]
        private float loadRating = 5400;

        /// <summary>
        ///     Constant torque acting similar to brake torque.
        ///     Imitates rolling resistance.
        /// </summary>
        [Range(0, 500)]
        [Tooltip("    Constant torque acting similar to brake torque.\r\n    Imitates rolling resistance.")]
        public float rollingResistanceTorque = 30f;

        /// <summary>
        /// Higher the number, higher the effect of longitudinal friction on lateral friction.
        /// If 1, when wheels are locked up or there is wheel spin it will be impossible to steer.
        /// If 0 doughnuts or power slides will be impossible.
        /// The 'accurate' value is 1 but might not be desirable for arcade games.
        /// </summary>
        [Tooltip("Higher the number, higher the effect of longitudinal friction on lateral friction.\r\n" +
            "If 1, when wheels are locked up or there is wheel spin it will be impossible to steer." +
            "\r\nIf 0 doughnuts or power slides will be impossible.\r\n" +
            "The 'accurate' value is 1 but might not be desirable for arcade games.")]
        [Range(0, 1)]
        [SerializeField]
        private float frictionCircleStrength = 1f;

        /// <summary>
        /// Higher values have more pronounced slip circle effect as the lateral friction will be
        /// decreased with smaller amounts of longitudinal slip (wheel spin).
        /// Realistic is ~1.5-2.
        /// </summary>
        [Range(0.0001f, 3f)]
        [SerializeField]
        [Tooltip("Higher values have more pronounced slip circle effect as the lateral friction will be\r\ndecreased with smaller amounts of longitudinal slip (wheel spin).\r\nRealistic is ~1.5-2.")]
        private float frictionCircleShape = 0.9f;

        /// <summary>
        ///     True if wheel touching ground.
        /// </summary>
        [Tooltip("    True if wheel touching ground.")]
        private bool _isGrounded;

        /// <summary>
        ///     Rigidbody to which the forces will be applied.
        /// </summary>
        [Tooltip("    Rigidbody to which the forces will be applied.")]
        [SerializeField]
        private Rigidbody targetRigidbody;

        /// <summary>
        /// Distance as a percentage of the max spring length. Value of 1 means that the friction force will
        /// be applied 1 max spring length above the contact point, and value of 0 means that it will be applied at the
        /// ground level. Value can be >1.
        /// Can be used instead of the anti-roll bar to prevent the vehicle from tipping over in corners
        /// and can be useful in low framerate applications where anti-roll bar might induce jitter.
        /// </summary>
        [Tooltip("Distance as a percentage of the max spring length. Value of 1 means that the friction force will\r\nbe applied 1 max spring length above the contact point, and value of 0 means that it will be applied at the\r\nground level. Value can be >1.\r\nCan be used instead of the anti-roll bar to prevent the vehicle from tipping over in corners\r\nand can be useful in low framerate applications where anti-roll bar might induce jitter.")]
        public float forceApplicationPointDistance = 0.8f;

        /// <summary>
        /// Disables the motion vectors on the wheel visual to prevent artefacts due to 
        /// the wheel rotation when using PostProcessing.
        /// </summary>
        [Tooltip("Disables the motion vectors on the wheel visual to prevent artefacts due to \r\nthe wheel rotation when using PostProcessing.")]
        public bool disableMotionVectors = true;

        /// <summary>
        /// Scales the forces applied to other Rigidbodies. Useful for interacting
        /// with lightweight objects and prevents them from flying away or glitching out.
        /// </summary>
        [Tooltip("Scales the forces applied to other Rigidbodies. Useful for interacting\r\nwith lightweight objects and prevents them from flying away or glitching out.")]
        public float otherBodyForceScale = 1f;

        /// <summary>
        /// The percentage this wheel is contributing to the total vehicle load bearing.
        /// </summary>
        public float loadContribution = 0.25f;


        private Vector3 _hitLocalPoint;
        private Vector3 _hitForwardDirection;
        private Vector3 _hitSidewaysDirection;
        private Vector3 _frictionForce;
        private Vector3 _suspensionForce;
        private float _dt;
        private Vector3 _transformPosition;
        private Vector3 _transformUp;
        private GroundDetectionBase _groundDetection;


        private bool _lowSpeedReferenceIsSet;
        private Vector3 _lowSpeedReferencePosition;


        private void Awake()
        {
            targetRigidbody = GetComponentInParent<Rigidbody>();
        }


        private void Start()
        {
            // Cache frequently used values
            _dt = Time.fixedDeltaTime;

            // Sets the defaults if needed.
            SetRuntimeDefaults();

            // Initialize the wheel
            wheel.Initialize(this);

            // Initialize spring length to starting value.
            if (spring.maxLength > 0) spring.length = -transform.InverseTransformPoint(wheel.visualTransform.position).y;

            // Initialize ground detection
            _groundDetection = GetComponent<GroundDetectionBase>();
            if (_groundDetection == null) _groundDetection = gameObject.AddComponent<StandardGroundDetection>();
            wheelHit = new WheelHit();
        }


        private void FixedUpdate()
        {
            if (!isActiveAndEnabled) return;

            _dt = Time.fixedDeltaTime;

            // Optimization. Ideally visual should be a Transform but backwards compatibility is required.
            wheel.visualTransform = wheel.visual.transform;

            // Update cached and previous values
            _transformPosition = transform.position;
            _transformUp = transform.up;
            wheel.prevWorldPosition = wheel.worldPosition;
            wheel.prevAngularVelocity = wheel.angularVelocity;
            spring.prevLength = spring.length;
            spring.prevVelocity = spring.compressionVelocity;

            _isGrounded = FindTheHitPoint();

            bool bottomMeshColliderEnabled = false;

            // Check for hit above axle and enable the collider if there is one
            bool hitAboveAxle = _hitLocalPoint.y > wheel.localPosition.y;
            if (_isGrounded && hitAboveAxle)
                bottomMeshColliderEnabled = true;


            if (!bottomMeshColliderEnabled)
                UpdateSpringAndDamper();

            UpdateWheelValues();
            UpdateHitVariables();
            UpdateFriction();
        }



        private bool FindTheHitPoint()
        {
            float offset = wheel.radius * 1.1f;
            float length = wheel.radius * 2.2f + spring.maxLength;

            Vector3 origin = _transformPosition + _transformUp * offset;
            Vector3 direction = -_transformUp;

            bool hasHit = _groundDetection.WheelCast(origin, direction, length, wheel.radius, wheel.width, ref wheelHit, LayerMask.GetMask("Terrain"));

            if (hasHit)
                _hitLocalPoint = transform.InverseTransformPoint(wheelHit.point);

            return hasHit;
        }


        private void UpdateSpringAndDamper()
        {
            float localAirYPosition = wheel.localPosition.y - _dt * spring.maxLength * 5f;

            if (_isGrounded)
            {
                float sine = Mathf.Clamp(_hitLocalPoint.z / wheel.radius, -1f, 1f);
                float hitAngle = Mathf.Asin(sine);
                float localGroundedYPosition = _hitLocalPoint.y + wheel.radius * Mathf.Cos(hitAngle);
                wheel.localPosition.y = Mathf.Max(localGroundedYPosition, localAirYPosition);
            }
            else
                wheel.localPosition.y = localAirYPosition;

            spring.length = -wheel.localPosition.y;

            if (spring.length <= 0f || spring.maxLength == 0f)
            {
                spring.extensionState = Spring.ExtensionState.BottomedOut;
                spring.length = 0;
            }
            else if (spring.length >= spring.maxLength)
            {
                spring.extensionState = Spring.ExtensionState.OverExtended;
                spring.length = spring.maxLength;
                _isGrounded = false;
            }
            else
            {
                spring.extensionState = Spring.ExtensionState.Normal;
            }

            spring.compressionVelocity = (spring.prevLength - spring.length) / _dt;
            spring.compression = spring.maxLength == 0 ? 1f : (spring.maxLength - spring.length) / spring.maxLength;
            spring.force = _isGrounded ? spring.maxForce * spring.forceCurve.Evaluate(spring.compression) : 0f;
            damper.force = _isGrounded ? damper.CalculateDamperForce(spring.compressionVelocity) : 0f;

            if (_isGrounded)
            {
                if (spring.maxLength > 0f)
                {
                    load = spring.force + damper.force;
                    load = load < 0f ? 0f : load;
                    _suspensionForce = load * wheelHit.normal;
                    targetRigidbody.AddForceAtPosition(_suspensionForce, _transformPosition);
                }
                else
                {
                    load = loadRating;
                    _suspensionForce = Vector3.zero;
                }
            }
            else
            {
                load = 0;
                _suspensionForce = Vector3.zero;
            }
        }


        private void UpdateHitVariables()
        {
            if (_isGrounded)
            {
                Vector3 pointVelocity = targetRigidbody.GetPointVelocity(wheelHit.point);
                _hitForwardDirection = Vector3.Normalize(Vector3.Cross(wheelHit.normal, -wheel.right));
                _hitSidewaysDirection = Quaternion.AngleAxis(90f, wheelHit.normal) * _hitForwardDirection;

                forwardFriction.speed = Vector3.Dot(pointVelocity, _hitForwardDirection);
                sideFriction.speed = Vector3.Dot(pointVelocity, _hitSidewaysDirection);
            }
            else
            {
                forwardFriction.speed = 0f;
                sideFriction.speed = 0f;
            }
        }


        /// <summary>
        /// Updates the wheel positions and rotations.
        /// </summary>
        private void UpdateWheelValues()
        {
            // Update wheel position
            wheel.localPosition.y = -spring.length;
            wheel.worldPosition = transform.TransformPoint(wheel.localPosition);

            // Update rotations
            wheel.axleAngle = wheel.axleAngle % 360.0f + wheel.angularVelocity * Mathf.Rad2Deg * _dt;

            Quaternion _worldBaseRotation = transform.rotation;
            wheel.localRotation = Quaternion.AngleAxis(wheel.axleAngle, Vector3.right);
            wheel.worldRotation = transform.rotation * wheel.localRotation;

            // Update directions
            wheel.up = _worldBaseRotation * Vector3.up;
            wheel.forward = _worldBaseRotation * Vector3.forward;
            wheel.right = _worldBaseRotation * Vector3.right;

            // Apply transforms
            wheel.visualTransform.SetPositionAndRotation(wheel.worldPosition, wheel.worldRotation);

            if (wheel.nonRotatingVisual != null)
            {
                Vector3 position = wheel.visualTransform.position + _worldBaseRotation * wheel.nonRotatingVisualPositionOffset;
                Quaternion rotation = _worldBaseRotation * wheel.nonRotatingVisualRotationOffset;
                wheel.nonRotatingVisual.transform.SetPositionAndRotation(position, rotation);
            }
        }
        protected virtual void UpdateFriction()
        {
            forwardFriction.force = 0;
            sideFriction.force = 0;

            float invDt = 1f / Time.fixedDeltaTime;
            float invRadius = 1f / wheel.radius;
            float inertia = wheel.inertia;
            float invInertia = 1f / wheel.inertia;

            float loadClamped = Mathf.Clamp(load, 0f, loadRating);
            float forwardLoadFactor = loadClamped * 1.35f;
            float sideLoadFactor = loadClamped * 1.9f;

            float slipLoadModifier = 1f - Mathf.Clamp01(load / loadRating) * 0.4f;

            float mass = targetRigidbody.mass;
            float absForwardSpeed = Mathf.Abs(forwardFriction.speed);
            float absSideSpeed = Mathf.Abs(sideFriction.speed);

            float forwardSpeedClamp = 1.5f * (_dt / 0.005f);
            forwardSpeedClamp = forwardSpeedClamp < 1.5f ? 1.5f : forwardSpeedClamp > 10f ? 10f : forwardSpeedClamp;
            float clampedAbsForwardSpeed = absForwardSpeed < forwardSpeedClamp ? forwardSpeedClamp : absForwardSpeed;

            // Calculate effect of camber on friction
            float camberFrictionCoeff = Vector3.Dot(wheel.up, wheelHit.normal);


            // *******************************
            // ******** LONGITUDINAL ********* 
            // *******************************
            // In this version of the friction friction itself and angular velocity are independent.
            // This results in a somewhat reduced physical accuracy and ignores the tail end of the friction curve
            // but gives better results overall with the most common physics update rates (33Hz - 100Hz) since
            // there is no circular dependency between the angular velocity / slip and force which makes it stable
            // and removes the need for iterative methods. Since the stable state is achieved within one frame it can run 
            // with as low as needed physics update.

            // T = r * F
            // F = T / r;

            // *** FRICTION ***
            float peakForwardFrictionForce = activeFrictionPreset.BCDE.z * forwardLoadFactor * forwardFriction.grip;
            float combinedBrakeTorque = Mathf.Max(0f, brakeTorque + rollingResistanceTorque);
            float combinedBrakeForce = combinedBrakeTorque * invRadius * Mathf.Sign(-forwardFriction.speed);
            float maxForwardForce = peakForwardFrictionForce;
            forwardFriction.force = Mathf.Clamp(combinedBrakeForce,-maxForwardForce,maxForwardForce);

            // *** ANGULAR VELOCITY ***

            // Brake force
            bool wheelIsBlocked = false;
            if (_isGrounded)
            {
                float combinedWheelForce = combinedBrakeForce;

                float absWheelForceClamp = Mathf.Abs(wheel.angularVelocity) * inertia * invRadius * invDt;
                float wheelForceClampOverflow = Mathf.Max(0f, Mathf.Abs(combinedWheelForce) - absWheelForceClamp);
                combinedWheelForce = Mathf.Clamp(combinedWheelForce, -absWheelForceClamp, absWheelForceClamp);

                wheel.angularVelocity += combinedWheelForce * wheel.radius * invInertia * _dt;

                float noSlipAngularVelocity = forwardFriction.speed * invRadius;
                float angularVelocityError = wheel.angularVelocity - noSlipAngularVelocity;
                float angularVelocityCorrectionForce = -angularVelocityError * inertia * invRadius * invDt;
                angularVelocityCorrectionForce = Mathf.Clamp(angularVelocityCorrectionForce, -maxForwardForce, maxForwardForce);

                wheelIsBlocked = brakeTorque > 0f &&  wheelForceClampOverflow > Mathf.Abs(angularVelocityCorrectionForce);
                if (wheelIsBlocked)
                    wheel.angularVelocity = 0f;
                else
                    wheel.angularVelocity += angularVelocityCorrectionForce * wheel.radius * invInertia * _dt;
            }
            else
            {
                float maxBrakeTorque = Mathf.Abs(wheel.angularVelocity * inertia);
                float clampedBrakeTorque = Mathf.Clamp(combinedBrakeTorque, -maxBrakeTorque, maxBrakeTorque);
                wheel.angularVelocity -= Mathf.Sign(wheel.angularVelocity) * clampedBrakeTorque * invInertia * _dt;
            }



            float absAngularVelocity = wheel.angularVelocity < 0 ? -wheel.angularVelocity : wheel.angularVelocity;


            // Calculate slip based on the corrected angular velocity
            forwardFriction.slip = (forwardFriction.speed - wheel.angularVelocity * wheel.radius) / clampedAbsForwardSpeed;
            forwardFriction.slip *= forwardFriction.stiffness * slipLoadModifier;

            sideFriction.slip = (Mathf.Atan2(sideFriction.speed, clampedAbsForwardSpeed) * Mathf.Rad2Deg) * 0.01111f;
            sideFriction.slip *= sideFriction.stiffness * slipLoadModifier;

            // *******************************
            // ********** LATERAL ************ 
            // *******************************
            float peakSideFrictionForce = activeFrictionPreset.BCDE.z * sideLoadFactor * sideFriction.grip;
            sideFriction.force = -Mathf.Sign(sideFriction.slip) * activeFrictionPreset.Curve.Evaluate(Mathf.Abs(sideFriction.slip)) * sideLoadFactor * sideFriction.grip * camberFrictionCoeff;

            // *******************************
            // ******* ANTI - CREEP **********
            // *******************************

            // Get the error to the reference point and apply the force to keep the wheel at that point
            if (_isGrounded && absForwardSpeed < 0.12f && absSideSpeed < 0.12f)
            {
                Vector3 currentPosition = _transformPosition - _transformUp * (spring.length + wheel.radius);

                if (!_lowSpeedReferenceIsSet)
                {
                    _lowSpeedReferenceIsSet = true;
                    _lowSpeedReferencePosition = currentPosition;
                }
                else
                {
                    Vector3 referenceError = _lowSpeedReferencePosition - currentPosition;
                    Vector3 correctiveForce = invDt * loadContribution * mass * referenceError;

                    if (wheelIsBlocked && absAngularVelocity < 0.5f)
                        forwardFriction.force += Vector3.Dot(correctiveForce, _hitForwardDirection);

                    sideFriction.force += Vector3.Dot(correctiveForce, _hitSidewaysDirection);
                }
            }
            else
                _lowSpeedReferenceIsSet = false;


            // Clamp the forces once again, this time ignoring the force clamps as the anti-creep forces do not cause jitter,
            // so the forces are limited only by the surface friction.
            forwardFriction.force = Mathf.Clamp(forwardFriction.force, -peakForwardFrictionForce, peakForwardFrictionForce);
            sideFriction.force = Mathf.Clamp(sideFriction.force, -peakSideFrictionForce, peakSideFrictionForce);


            // *******************************
            // ********* SLIP CIRCLE ********* 
            // *******************************
            if (frictionCircleStrength > 0 && (absForwardSpeed > 2f || absAngularVelocity > 4f))
            {
                float forwardSlipPercent = forwardFriction.slip / activeFrictionPreset.peakSlip;
                float sideSlipPercent = sideFriction.slip / activeFrictionPreset.peakSlip;
                float slipCircleLimit = Mathf.Sqrt(forwardSlipPercent * forwardSlipPercent + sideSlipPercent * sideSlipPercent);
                if (slipCircleLimit > 1f)
                {
                    float beta = Mathf.Atan2(sideSlipPercent, forwardSlipPercent * frictionCircleShape);
                    float sinBeta = Mathf.Sin(beta);
                    float cosBeta = Mathf.Cos(beta);

                    float f = Mathf.Abs(forwardFriction.force) * cosBeta * cosBeta + Mathf.Abs(sideFriction.force) * sinBeta * sinBeta;

                    float invSlipCircleCoeff = 1f - frictionCircleStrength;

                    forwardFriction.force = invSlipCircleCoeff * forwardFriction.force - frictionCircleStrength * f * cosBeta;
                    sideFriction.force = invSlipCircleCoeff * sideFriction.force - frictionCircleStrength * f * sinBeta;
                }
            }


            // Apply the forces
            if (_isGrounded)
            {
                _frictionForce = _hitSidewaysDirection * sideFriction.force + _hitForwardDirection * forwardFriction.force;

                // Avoid adding calculated friction when using native friction
                Vector3 forcePosition = wheelHit.point + _transformUp * forceApplicationPointDistance * spring.maxLength;
                targetRigidbody.AddForceAtPosition(_frictionForce, forcePosition);
            }
            else
            {
                _frictionForce = Vector3.zero;
            }
        }
        private void OnValidate()
        {
            // Check for existing colliders
            if (!Application.isPlaying && wheel.visual != null && wheel.visual.GetComponentsInChildren<Collider>().Length > 0)
            {
                Debug.LogWarning($"{name}: Visual object already contains a Collider. Visual should have no colliders attached to it or its children" +
                    $" as they can prevent the wheel from functioning properly.");
            }

            if (targetRigidbody != null)
            {
                string prefix = $"{targetRigidbody.name} > {name}:";

                // Check parent scale
                Debug.Assert(transform.localScale == Vector3.one, $"{prefix} WheelController parent Rigidbody scale is not 1. Rigidbody transform scale should be [1,1,1].");

                // Load rating
                float minLoadRating = targetRigidbody.mass * -Physics.gravity.y * 0.05f;
                float maxLoadRating = targetRigidbody.mass * -Physics.gravity.y * 4f;
                if (loadRating < minLoadRating)
                {
                    Debug.LogWarning($"{prefix} Load rating of the tyre might be too low. This can cause the vehicle to slide around. Current: {loadRating}, min. recommended: {minLoadRating}.");
                }
                else if (loadRating > maxLoadRating)
                {
                    Debug.LogWarning($"{prefix} Load rating of the tyre might be too high. This can cause the vehicle friction to not work properly. Current: {loadRating}, max. recommended: {maxLoadRating}.");
                }

                // Has suspension
                if (spring.length > 0)
                {
                    float minForce = targetRigidbody.mass * -Physics.gravity.y * 0.25f;
                    if (spring.maxForce < minForce)
                    {
                        Debug.LogWarning($"{prefix} spring.maxForce is most likely too low for the given Rigidbody mass. Current: {spring.maxForce}, min. recommended: {minForce}, recommended: {minForce * 3f}" +
                            $"With the current values the suspension might not be strong enough to support the weight of the vehicle and might bottom out.");
                    }

                    float minLength = Time.fixedDeltaTime;
                    if (spring.maxLength < minLength)
                    {
                        Debug.LogWarning($"{prefix} spring.maxLength is shorter than recommended for the given Time.fixedDeltaTime. Current: {spring.maxLength}, min. recommended: {minLength}. With " +
                            $"the current values the suspension might bottom out frequently and cause a harsh ride.");
                    }

                    // TODO - validate damper
                }
            }
        }


        private void Reset()
        {
            SetRuntimeDefaults();

            // Assume 4 as the component count might be wrong at this
            // point and wheels added at a later time.
            int wheelCount = 4;

            float gravity = -Physics.gravity.y;
            float weightPerWheel = targetRigidbody.mass * gravity / wheelCount;

            spring.maxForce = weightPerWheel * 6f;
            damper.bumpRate = weightPerWheel * 0.15f;
            damper.reboundRate = weightPerWheel * 0.15f;
            loadRating = weightPerWheel * 2f;
        }


        /// <summary>
        ///     Sets default values if they have not already been set.
        ///     Gets called each time Reset() is called in editor - such as adding the script to a GameObject.
        /// </summary>
        /// <param name="reset">Sets default values even if they have already been set.</param>
        /// <param name="findWheelVisuals">Should script attempt to find wheel visuals automatically by name and position?</param>
        public void SetRuntimeDefaults(bool reset = false, bool findWheelVisuals = true)
        {
            // Find parent Rigidbody
            if (targetRigidbody == null) targetRigidbody = gameObject.GetComponentInParent<Rigidbody>();
            Debug.Assert(targetRigidbody != null, "Parent does not contain a Rigidbody.");

            if (wheel == null || reset) wheel = new Wheel();
            if (spring == null || reset) spring = new Spring();
            if (damper == null || reset) damper = new Damper();
            if (forwardFriction == null || reset) forwardFriction = new Friction();
            if (sideFriction == null || reset) sideFriction = new Friction();
            if (activeFrictionPreset == null || reset)
                activeFrictionPreset = Resources.Load<FrictionPreset>("Wheel Controller 3D/Defaults/DefaultFrictionPreset");
            if (spring.forceCurve == null || spring.forceCurve.keys.Length == 0 || reset)
                spring.forceCurve = GenerateDefaultSpringCurve();
        }


        private AnimationCurve GenerateDefaultSpringCurve()
        {
            AnimationCurve ac = new AnimationCurve();
            ac.AddKey(0.0f, 0.0f);
            ac.AddKey(1.0f, 1.0f);
            return ac;
        }


        /// <summary>
        /// Places the WheelController roughly to the position it should be in, in relation to the wheel visual (if assigned).
        /// </summary>
        public void PositionToVisual()
        {
            if (wheel.visual == null)
            {
                Debug.LogError("Wheel visual not assigned.");
                return;
            }

            Rigidbody rb = GetComponentInParent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError("Rigidbody not found in parent.");
                return;
            }

            int wheelCount = GetComponentInParent<Rigidbody>().GetComponentsInChildren<WheelController>().Length;
            if (wheelCount == 0) return;

            // Approximate static load on the wheel.
            float approxStaticLoad = (rb.mass * -Physics.gravity.y) / wheelCount;

            // Approximate the spring travel, not taking spring curve into account.
            float approxSpringTravel = Mathf.Clamp01(approxStaticLoad / spring.maxForce) * spring.maxLength;

            // Position the WheelController transform above the wheel.
            transform.position = wheel.visual.transform.position + rb.transform.up * (spring.maxLength - approxSpringTravel);
        }
    }
}



#if UNITY_EDITOR
namespace NWH.WheelController3D
{
    /// <summary>
    ///     Editor for WheelController.
    /// </summary>
    [CustomEditor(typeof(WheelController))]
    [CanEditMultipleObjects]
    public class WheelControllerEditor : NUIEditor
    {
        public override bool OnInspectorNUI()
        {
            if (!base.OnInspectorNUI()) return false;

            WheelController wc = target as WheelController;
            if (wc == null) return false;

            float logoHeight = 40f;
            Rect texRect = drawer.positionRect;
            texRect.height = logoHeight;
            drawer.DrawEditorTexture(texRect, "Wheel Controller 3D/Editor/logo_wc3d", ScaleMode.ScaleToFit);
            drawer.Space(logoHeight + 4);


            int tabIndex = drawer.HorizontalToolbar("wc3dMenu",
                                     new[] { "Wheel", "Suspension", "Friction", "Misc", "Debug" }, true, true);

            if (tabIndex == 0) // WHEEL
            {
                drawer.BeginSubsection("Wheel");
                drawer.Field("targetRigidbody");
                drawer.Field("wheel.radius", true, "m");
                drawer.Field("wheel.width", true, "m");
                drawer.Field("wheel.mass", true, "kg");
                drawer.Field("loadRating", true, "N");
                drawer.Info("It is important to set the load rating correctly as it affects friction drastically.\r\n" +
                    "A value of about 2x of the Load at rest (Debug tab) is a good guidance.");
                drawer.Field("rollingResistanceTorque", true, "Nm");
                drawer.EndSubsection();

                drawer.BeginSubsection("Wheel Model");
                drawer.Field("wheel.visual");
                drawer.Field("wheel.nonRotatingVisual", true, "", "Non-Rotating Visual (opt.)");
                drawer.EndSubsection();
            }
            else if (tabIndex == 1) // SUSPENSION
            {
                drawer.BeginSubsection("Spring");
                drawer.Field("spring.maxForce", true, "N@100%");
                if (Application.isPlaying)
                    if (wc != null)
                    {
                        float minRecommended = wc.ParentRigidbody.mass * -Physics.gravity.y / 4f;
                        if (wc.SpringMaxForce < minRecommended)
                            drawer.Info(
                                "MaxForce of Spring is most likely too low for the vehicle mass. Minimum recommended for current configuration is" +
                                $" {minRecommended}N.", MessageType.Warning);
                    }

                if (drawer.Field("spring.maxLength", true, "m").floatValue < Time.fixedDeltaTime * 10f)
                    drawer.Info(
                        $"Minimum recommended spring length for Time.fixedDeltaTime of {Time.fixedDeltaTime} is {Time.fixedDeltaTime * 10f}");

                drawer.Field("spring.forceCurve");
                drawer.Info("X: Spring compression [%], Y: Force coefficient");
                drawer.EndSubsection();

                drawer.BeginSubsection("Damper");
                drawer.Field("damper.bumpRate", true, "Ns/m");
                drawer.Field("damper.slowBump", true, "slope");
                drawer.Field("damper.fastBump", true, "slope");
                drawer.Field("damper.bumpDivisionVelocity", true, "m/s");
                drawer.Space();
                drawer.Field("damper.reboundRate", true, "Ns/m");
                drawer.Field("damper.slowRebound", true, "slope");
                drawer.Field("damper.fastRebound", true, "slope");
                drawer.Field("damper.reboundDivisionVelocity", true, "m/s");
                drawer.EndSubsection();

                drawer.BeginSubsection("General");
                drawer.Field("forceApplicationPointDistance", true, null, "Force App. Point Distance");
                drawer.EndSubsection();
            }
            else if (tabIndex == 2) // FRICTION
            {
                drawer.BeginSubsection("Friction");
                drawer.Field("activeFrictionPreset");
                drawer.EmbeddedObjectEditor<NUIEditor>(((WheelController)target).FrictionPreset,
                                                       drawer.positionRect);

                drawer.BeginSubsection("Friction Circle");
                drawer.Field("frictionCircleStrength", true, null, "Strength");
                drawer.Field("frictionCircleShape", true, null, "Shape");
                drawer.EndSubsection();

                drawer.BeginSubsection("Longitudinal / Forward");
                drawer.Field("forwardFriction.stiffness", true, "x100 %");
                drawer.Field("forwardFriction.grip", true, "x100 %");
                drawer.EndSubsection();

                drawer.BeginSubsection("Lateral / Sideways");
                drawer.Field("sideFriction.stiffness", true, "x100 %");
                drawer.Field("sideFriction.grip", true, "x100 %");
                drawer.EndSubsection();
                drawer.EndSubsection();
            }
            else if (tabIndex == 3) // MISC
            {
                drawer.BeginSubsection("Actions");
                if (drawer.Button("Position To Visual"))
                {
                    foreach (WheelController targetWC in targets)
                    {
                        targetWC.PositionToVisual();
                    }
                }
                drawer.EndSubsection();


                drawer.BeginSubsection("Rendering");
                {
                    drawer.Field("disableMotionVectors");
                }
                drawer.EndSubsection();


                drawer.BeginSubsection("Other");
                {
                    drawer.Field("otherBodyForceScale");
                }
                drawer.EndSubsection();
            }
            else
            {
                drawer.Label($"Is Grounded: {wc.IsGrounded}");
                drawer.Space();

                drawer.Label("Wheel");
                drawer.Label($"\tBrake Torque: {wc.BrakeTorque}");
                drawer.Label($"\tAng. Vel: {wc.AngularVelocity}");

                drawer.Label("Friction");
                drawer.Label($"\tLng. Slip: {wc.LongitudinalSlip}");
                drawer.Label($"\tLng. Speed: {wc.forwardFriction.speed}");
                drawer.Label($"\tLng. Force: {wc.forwardFriction.force}");
                drawer.Label($"\tLat. Slip: {wc.LateralSlip}");
                drawer.Label($"\tLat. Speed: {wc.sideFriction.speed}");
                drawer.Label($"\tLat. Force: {wc.sideFriction.force}");

                drawer.Label("Suspension");
                drawer.Label($"\tLoad: {wc.Load}");
                drawer.Label($"\tSpring Length: {wc.SpringLength}");
                drawer.Label($"\tSpring Force: {wc.spring.force}");
                drawer.Label($"\tSpring Velocity: {wc.spring.compressionVelocity}");
                drawer.Label($"\tSpring State: {wc.spring.extensionState}");
                drawer.Label($"\tDamper Force: {wc.damper.force}");
            }

            drawer.EndEditor(this);
            return true;
        }


        public override bool UseDefaultMargins()
        {
            return false;
        }
    }
}

#endif