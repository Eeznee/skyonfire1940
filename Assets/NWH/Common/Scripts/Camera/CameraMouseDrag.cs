using System;
using NWH.Common.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using NWH.NUI;
using UnityEditor;
#endif

namespace NWH.Common.Cameras
{
    /// <summary>
    ///     Camera that can be dragged with the mouse.
    /// </summary>
    public class CameraMouseDrag : VehicleCamera
    {
        public enum POVType { FirstPerson, ThirdPerson }

        /// <summary>
        /// Camera POV type. First person camera will invert controls.
        /// Zoom is not available in 1st person.
        /// </summary>
        [UnityEngine.Tooltip("Camera POV type. First person camera will invert controls.\r\nZoom is not available in 1st person.")]
        public POVType povType = POVType.ThirdPerson;

        /// <summary>
        ///     Can the camera be rotated by the user?
        /// </summary>
        [UnityEngine.Tooltip("    Can the camera be rotated by the user?")]
        public bool allowRotation = true;

        /// <summary>
        ///     Can the camera be panned by the user?
        /// </summary>
        [UnityEngine.Tooltip("    Can the camera be panned by the user?")]
        public bool allowPanning = true;

        /// <summary>
        ///     Distance from target at which camera will be positioned. Might vary depending on smoothing.
        /// </summary>
        [Range(0, 100f)]
        [Tooltip("    Distance from target at which camera will be positioned. Might vary depending on smoothing.")]
        public float distance = 5f;

        /// <summary>
        ///     If true the camera will rotate with the vehicle along the X and Y axis.
        /// </summary>
        [FormerlySerializedAs("followTargetsRotation")]
        [UnityEngine.Tooltip("    If true the camera will rotate with the vehicle along the X and Y axis.")]
        public bool followTargetPitchAndYaw = true;

        /// <summary>
        ///     If true the camera will rotate with the vehicle along the Z axis.
        /// </summary>
        [UnityEngine.Tooltip("    If true the camera will rotate with the vehicle along the Z axis.")]
        public bool followTargetRoll = false;

        /// <summary>
        ///     Maximum distance that will be reached when zooming out.
        /// </summary>
        [Range(0, 100f)]
        [Tooltip("    Maximum distance that will be reached when zooming out.")]
        public float maxDistance = 13.0f;

        /// <summary>
        ///     Minimum distance that will be reached when zooming in.
        /// </summary>
        [Range(0, 100f)]
        [Tooltip("    Minimum distance that will be reached when zooming in.")]
        public float minDistance = 3.0f;

        /// <summary>
        ///     Sensitivity of the middle mouse button / wheel.
        /// </summary>
        [Range(0, 15)]
        [Tooltip("    Sensitivity of the middle mouse button / wheel.")]
        public float zoomSensitivity = 1f;

        /// <summary>
        ///     Smoothing of the camera rotation.
        /// </summary>
        [Range(0, 1)]
        [Tooltip("    Smoothing of the camera rotation.")]
        public float rotationSmoothing = 0.02f;

        /// <summary>
        ///     Maximum vertical angle the camera can achieve.
        /// </summary>
        [Range(-90, 90)]
        [UnityEngine.Tooltip("    Maximum vertical angle the camera can achieve.")]
        public float verticalMaxAngle = 80.0f;

        /// <summary>
        ///     Minimum vertical angle the camera can achieve.
        /// </summary>
        [Range(-90, 90)]
        [UnityEngine.Tooltip("    Minimum vertical angle the camera can achieve.")]
        public float verticalMinAngle = -40.0f;

        /// <summary>
        ///     Sensitivity of rotation input.
        /// </summary>
        [UnityEngine.Tooltip("    Sensitivity of rotation input.")]
        public Vector2 rotationSensitivity = new Vector2(3f, 3f);

        /// <summary>
        ///     Sensitivity of panning input.
        /// </summary>
        [UnityEngine.Tooltip("    Sensitivity of panning input.")]
        public Vector2 panningSensitivity = new Vector2(0.06f, 0.06f);

        /// <summary>
        ///     Initial rotation around the X axis (up/down)
        /// </summary>
        [UnityEngine.Tooltip("    Initial rotation around the X axis (up/down)")]
        public float initXRotation;

        /// <summary>
        ///     Initial rotation around the Y axis (left/right)
        /// </summary>
        [UnityEngine.Tooltip("    Initial rotation around the Y axis (left/right)")]
        public float initYRotation;

        /// <summary>
        ///     Look position offset from the target center.
        /// </summary>
        [UnityEngine.Tooltip("    Look position offset from the target center.")]
        public Vector3 targetPositionOffset = Vector3.zero;

        /// <summary>
        /// Should camera movement on acceleration be used?
        /// </summary>
        [UnityEngine.Tooltip("Should camera movement on acceleration be used?")]
        public bool useShake = true;

        /// <summary>
        ///     Maximum head movement from the initial position.
        /// </summary>
        [Range(0f, 1f)]
        [Tooltip("    Maximum head movement from the initial position.")]
        public float shakeMaxOffset = 0.2f;

        /// <summary>
        ///     How much will the head move around for the given g-force.
        /// </summary>
        [Range(0f, 1f)]
        [Tooltip("    How much will the head move around for the given g-force.")]
        public float shakeIntensity = 0.125f;

        /// <summary>
        ///     Smoothing of the head movement.
        /// </summary>
        [Range(0f, 1f)]
        [Tooltip("    Smoothing of the head movement.")]
        public float shakeSmoothing = 0.3f;

        /// <summary>
        ///     Movement intensity per axis. Set to 0 to disable movement on that axis or negative to reverse it.
        /// </summary>
        [UnityEngine.Tooltip("    Movement intensity per axis. Set to 0 to disable movement on that axis or negative to reverse it.")]
        public Vector3 shakeAxisIntensity = new Vector3(1f, 0.5f, 1f);

        private Vector3 _lookDir;
        private Vector3 _newLookDir;
        private Vector3 _lookDirVel;
        private Vector3 _lookAtPosition;
        private Vector2 _rot;
        private Vector3 _pan;
        private bool    _isFirstFrame;

        private bool _rotationModifier;
        private bool _panningModifier;
        private Vector2 _rotationInput;
        private Vector2 _panningInput;
        private float _zoomInput;

        private Vector3   _acceleration;
        private Vector3   _prevAcceleration;
        private Vector3   _accelerationChangeVelocity;
        private Vector3   _initialPosition;
        private Vector3   _localAcceleration;
        private Vector3   _newPositionOffset;
        private Vector3   _offsetChangeVelocity;
        private Vector3   _positionOffset;
        private Rigidbody _rigidbody;
        private float     _rbSpeed;
        private Vector3   _rbLocalAcceleration;
        private Vector3   _rbLocalVelocity;
        private Vector3   _rbPrevLocalVelocity;

        private bool PointerOverUI
        {
            get
            {
                return EventSystem.current != null &&
                       EventSystem.current.IsPointerOverGameObject();
            }
        }

        private void Start()
        {
            _initialPosition = transform.localPosition;
            _rigidbody = target?.GetComponent<Rigidbody>();

            distance = Mathf.Clamp(distance, minDistance, maxDistance);

            _rot.x = initXRotation;
            _rot.y = initYRotation;
            _isFirstFrame = true;
        }


        private void FixedUpdate()
        {
            if (_rigidbody == null)
            {
                return;
            }

            _rbPrevLocalVelocity = _rbLocalVelocity;
            _rbLocalVelocity = transform.InverseTransformDirection(_rigidbody.velocity);
            _rbLocalAcceleration = (_rbLocalVelocity - _rbPrevLocalVelocity) / Time.fixedDeltaTime;
            _rbSpeed = _rbLocalVelocity.z < 0 ? -_rbLocalVelocity.z : _rbLocalVelocity.z;
        }


        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            // Handle input
            if (!PointerOverUI)
            {
                _rotationInput = InputProvider.CombinedInput<SceneInputProviderBase>(i => i.CameraRotation());
                _panningInput = InputProvider.CombinedInput<SceneInputProviderBase>(i => i.CameraPanning());
                _zoomInput = InputProvider.CombinedInput<SceneInputProviderBase>(i => i.CameraZoom());
                _rotationModifier =
                    InputProvider.CombinedInput<SceneInputProviderBase>(i => i.CameraRotationModifier());
                _panningModifier = InputProvider.CombinedInput<SceneInputProviderBase>(i => i.CameraPanningModifier());

                if (allowRotation && _rotationModifier)
                {
                    _rot.y += _rotationInput.x * rotationSensitivity.x;
                    _rot.x -= _rotationInput.y * rotationSensitivity.y;
                }

                if (allowPanning && _panningModifier)
                {
                    float pX = _panningInput.x * panningSensitivity.x;
                    float pY = _panningInput.y * panningSensitivity.y;
                    _pan -= target.InverseTransformDirection(transform.right * pX);
                    _pan -= target.InverseTransformDirection(transform.up * pY);
                }

                _rot.x = ClampAngle(_rot.x, verticalMinAngle, verticalMaxAngle);

                if (povType == POVType.ThirdPerson && (_zoomInput > 0.0001f || _zoomInput < -0.0001f))
                {
                    distance -= _zoomInput * zoomSensitivity;
                }
            }

            Vector3 forwardVector = followTargetPitchAndYaw ? target.forward : Vector3.forward;
            Vector3 rightVector = followTargetPitchAndYaw ? target.right : Vector3.right;
            Vector3 upVector = followTargetPitchAndYaw ? target.up : Vector3.up;

            _lookAtPosition = target.position +
                              target.TransformDirection(targetPositionOffset + _pan);
            _newLookDir = Quaternion.AngleAxis(_rot.x, rightVector) * forwardVector;
            _newLookDir = Quaternion.AngleAxis(_rot.y, upVector) * _newLookDir;

            _lookDir = _isFirstFrame ?
                _newLookDir :
                Vector3.SmoothDamp(_lookDir, _newLookDir, ref _lookDirVel, rotationSmoothing);
            _lookDir = Vector3.Normalize(_lookDir);

            if (povType == POVType.ThirdPerson)
            {
                distance = povType == POVType.FirstPerson ? 0 : Mathf.Clamp(distance, minDistance, maxDistance);

                Vector3 targetPosition = _lookAtPosition - _lookDir * distance;
                transform.position = targetPosition;
                transform.forward = _lookDir;

                // Check for ground
                if (Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hit, 0.5f))
                {
                    transform.position = hit.point + Vector3.up * 0.5f;
                }

                transform.rotation =
                    Quaternion.LookRotation(_lookDir, followTargetRoll ? target.up : Vector3.up);
            }
            else
            {
                transform.localPosition = _initialPosition + _pan;
                transform.rotation =
                    Quaternion.LookRotation(_lookDir, followTargetRoll ? target.up : Vector3.up);
            }

            // Movement effect
            _prevAcceleration = _acceleration;
            _acceleration = _rbLocalAcceleration;
            _localAcceleration = Vector3.zero;
            if (target != null)
            {
                _localAcceleration = target.TransformDirection(_acceleration);
            }

            if (!_isFirstFrame)
            {
                _newPositionOffset = Vector3.SmoothDamp(_prevAcceleration, _localAcceleration,
                    ref _accelerationChangeVelocity,
                    shakeSmoothing) / 100f * shakeIntensity;
                _newPositionOffset = Vector3.Scale(_newPositionOffset, shakeAxisIntensity);
                _positionOffset = Vector3.SmoothDamp(_positionOffset, _newPositionOffset, ref _offsetChangeVelocity,
                    shakeSmoothing);
                _positionOffset = Vector3.ClampMagnitude(_positionOffset, shakeMaxOffset);
                transform.position -= target.TransformDirection(_positionOffset) *
                                      Mathf.Clamp01(_rbSpeed * 0.5f);
            }

            _isFirstFrame = false;
        }


        public void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(_lookAtPosition, 0.1f);
            Gizmos.DrawRay(_lookAtPosition, _lookDir);
        }


        private void OnEnable()
        {
            _isFirstFrame = true;
        }


        public float ClampAngle(float angle, float min, float max)
        {
            while (angle < -360 || angle > 360)
            {
                if (angle < -360)
                {
                    angle += 360;
                }

                if (angle > 360)
                {
                    angle -= 360;
                }
            }

            return Mathf.Clamp(angle, min, max);
        }
    }
}



#if UNITY_EDITOR

namespace NWH.Common.Cameras
{
    [CustomEditor(typeof(CameraMouseDrag))]
    [CanEditMultipleObjects]
    public class CameraMouseDragEditor : NUIEditor
    {
        public override bool OnInspectorNUI()
        {
            if (!base.OnInspectorNUI())
            {
                return false;
            }

            CameraMouseDrag.POVType povType = ((CameraMouseDrag)target).povType;

            drawer.Field("target");

            drawer.BeginSubsection("POV");
            drawer.Field("povType");
            drawer.EndSubsection();

            if (povType == CameraMouseDrag.POVType.ThirdPerson)
            {
                drawer.BeginSubsection("Distance & Position");
                drawer.Field("distance");
                drawer.Field("minDistance");
                drawer.Field("maxDistance");
                drawer.Field("zoomSensitivity");
                drawer.Field("targetPositionOffset");
                drawer.EndSubsection();
            }

            drawer.BeginSubsection("Rotation");
            drawer.Field("allowRotation");
            drawer.Field("followTargetPitchAndYaw");
            drawer.Field("followTargetRoll");
            drawer.Field("rotationSensitivity");
            drawer.Field("verticalMaxAngle");
            drawer.Field("verticalMinAngle");
            drawer.Field("initXRotation");
            drawer.Field("initYRotation");
            drawer.Field("rotationSmoothing");
            drawer.EndSubsection();

            drawer.BeginSubsection("Panning");
            if (drawer.Field("allowPanning").boolValue)
            {
                drawer.Field("panningSensitivity");
            }
            drawer.EndSubsection();

            drawer.BeginSubsection("Camera Shake");
            drawer.Info("Movement introduced as a result of acceleration.");
            if (drawer.Field("useShake").boolValue)
            {
                drawer.Field("shakeMaxOffset");
                drawer.Field("shakeIntensity");
                drawer.Field("shakeSmoothing");
                drawer.Field("shakeAxisIntensity");
            }
            drawer.EndSubsection();

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
