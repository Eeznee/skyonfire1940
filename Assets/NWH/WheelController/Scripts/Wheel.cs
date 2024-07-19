using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace NWH.WheelController3D
{
    /// <summary>
    ///     Contains everything wheel related, including rim and tire.
    /// </summary>
    [Serializable]
    public class Wheel
    {
        /// <summary>
        ///     GameObject representing the visual aspect of the wheel / wheel mesh.
        ///     Should not have any physics colliders attached to it.
        /// </summary>
        [Tooltip(
            "GameObject representing the visual aspect of the wheel / wheel mesh.\r\nShould not have any physics colliders attached to it.")]
        public GameObject visual;

        /// <summary>
        /// Chached value for visual.transform to avoid overhead.
        /// Visual should have been a Transform from the start, but for backwards-compatibility it was left as a GameObject.
        /// </summary>
        [UnityEngine.Tooltip("Chached value for visual.transform to avoid overhead.\r\nVisual should have been a Transform from the start, but for backwards-compatibility it was left as a GameObject.")]
        public Transform visualTransform;

        /// <summary>
        ///     Object representing non-rotating part of the wheel. This could be things such as brake calipers, external fenders,
        ///     etc.
        /// </summary>
        [Tooltip(
            "Object representing non-rotating part of the wheel. This could be things such as brake calipers, external fenders, etc.")]
        public GameObject nonRotatingVisual;

        /// <summary>
        ///     Current angular velocity of the wheel in rad/s.
        /// </summary>
        [Tooltip("    Current angular velocity of the wheel in rad/s.")]
        public float angularVelocity;

        /// <summary>
        ///     Current wheel RPM.
        /// </summary>
        public float rpm
        {
            get { return angularVelocity * 9.55f; }
        }

        /// <summary>
        ///     Forward vector of the wheel in world coordinates.
        /// </summary>
        [Tooltip("    Forward vector of the wheel in world coordinates.")]
        [NonSerialized]
        public Vector3 forward;

        /// <summary>
        ///     Vector in world coordinates pointing to the right of the wheel.
        /// </summary>
        [Tooltip("    Vector in world coordinates pointing to the right of the wheel.")]
        [NonSerialized]
        public Vector3 right;

        /// <summary>
        ///     Wheel's up vector in world coordinates.
        /// </summary>
        [Tooltip("    Wheel's up vector in world coordinates.")]
        [NonSerialized]
        public Vector3 up;

        /// <summary>
        /// Total inertia of the wheel and any attached components.
        /// </summary>
        public float inertia;

        /// <summary>
        /// Inertia of the wheel, without any attached components.
        /// </summary>
        public float baseInertia;

        /// <summary>
        ///     Mass of the wheel. Inertia is calculated from this.
        /// </summary>
        [Tooltip("    Mass of the wheel. Inertia is calculated from this.")]
        public float mass = 20.0f;

        /// <summary>
        ///     Position offset of the non-rotating part.
        /// </summary>
        [Tooltip("    Position offset of the non-rotating part.")]
        [NonSerialized]
        public Vector3 nonRotatingVisualPositionOffset;

        /// <summary>
        ///     Rotation offset of the non-rotating part.
        /// </summary>
        [Tooltip("    Rotation offset of the non-rotating part.")]
        [NonSerialized]
        public Quaternion nonRotatingVisualRotationOffset;

        /// <summary>
        ///     Total radius of the tire in [m].
        /// </summary>
        [Tooltip("    Total radius of the tire in [m].")]
        [Min(0.001f)]
        public float radius = 0.35f;

        /// <summary>
        ///     Current rotation angle of the wheel visual in regards to it's X axis vector.
        /// </summary>
        [Tooltip("    Current rotation angle of the wheel visual in regards to it's X axis vector.")]
        [NonSerialized]
        public float axleAngle;

        /// <summary>
        ///     Width of the tyre.
        /// </summary>
        [Tooltip("    Width of the tyre.")]
        [Min(0.001f)]
        public float width = 0.25f;

        /// <summary>
        ///     Position of the wheel in world coordinates.
        /// </summary>
        [Tooltip("    Position of the wheel in world coordinates.")]
        [NonSerialized]
        public Vector3 worldPosition; // TODO

        /// <summary>
        ///     Position of the wheel in the previous physics update in world coordinates.
        /// </summary>
        [NonSerialized]
        [UnityEngine.Tooltip("    Position of the wheel in the previous physics update in world coordinates.")]
        public Vector3 prevWorldPosition;

        /// <summary>
        ///     Position of the wheel relative to the WheelController transform.
        /// </summary>
        [NonSerialized]
        [UnityEngine.Tooltip("    Position of the wheel relative to the WheelController transform.")]
        public Vector3 localPosition;

        /// <summary>
        ///     Angular velocity during the previus FixedUpdate().
        /// </summary>
        [NonSerialized]
        [UnityEngine.Tooltip("    Angular velocity during the previus FixedUpdate().")]
        public float prevAngularVelocity;

        /// <summary>
        ///     Rotation of the wheel in world coordinates.
        /// </summary>
        [Tooltip("    Rotation of the wheel in world coordinates.")]
        [NonSerialized]
        public Quaternion worldRotation;

        /// <summary>
        /// Local rotation of the wheel.
        /// </summary>
        [NonSerialized] public Quaternion localRotation;


        /// <summary>
        /// Called when either radius or width of the wheel change.
        /// </summary>
        [NonSerialized]
        [UnityEngine.Tooltip("Called when either radius or width of the wheel change.")]
        public UnityEvent onWheelDimensionsChange = new UnityEvent();


        public void Initialize(in WheelController wc)
        {
            Transform controllerTransform = wc.transform;

            // Create an empty wheel visual if not assigned
            if (visual == null)
            {
                visual = CreateEmptyVisual(controllerTransform, wc.spring.maxLength);
            }
            visualTransform = visual.transform;

            // v2.0 or newer requires the wheel visual to be parented directly to the WheelController.
            if (visualTransform.parent != controllerTransform)
            {
                visualTransform.SetParent(controllerTransform);
            }

            // Initialize wheel vectors
            Transform cachedVisualTransform = visual.transform;
            worldPosition = cachedVisualTransform.position;
            localPosition = controllerTransform.InverseTransformPoint(worldPosition);
            localPosition.x = 0;
            localPosition.z = 0;
            worldPosition = controllerTransform.TransformPoint(localPosition);
            up = cachedVisualTransform.up;
            forward = cachedVisualTransform.forward;
            right = cachedVisualTransform.right;

            // Initialize non-rotating visual
            if (nonRotatingVisual != null)
            {
                nonRotatingVisualPositionOffset =
                    visual.transform.InverseTransformPoint(nonRotatingVisual.transform.position);
                nonRotatingVisualRotationOffset = (Quaternion.Inverse(wc.transform.rotation)
                                                  * nonRotatingVisual.transform.rotation);
            }

            UpdatePhysicalProperties();
        }


        /// <summary>
        /// Used to update the wheel parameters (inertia, scale, etc.) after one of the wheel 
        /// dimensions is changed.
        /// </summary>
        public void UpdatePhysicalProperties()
        {
            inertia = 0.5f * mass * radius * radius;
            baseInertia = inertia;
        }


        private GameObject CreateEmptyVisual(in Transform parentTransform, in float springMaxLength)
        {
            GameObject visual = new GameObject($"{parentTransform.name}_emptyVisual");
            visual.transform.parent = parentTransform;
            visual.transform.SetPositionAndRotation(parentTransform.position - parentTransform.up * (springMaxLength * 0.5f),
                parentTransform.rotation);
            return visual;
        }
    }
}