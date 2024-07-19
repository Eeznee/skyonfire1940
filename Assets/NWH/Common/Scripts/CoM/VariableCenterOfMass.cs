using System;
using UnityEngine;
using NWH.Common.Utility;

#if UNITY_EDITOR
using NWH.NUI;
using UnityEditor;
#endif

namespace NWH.Common.CoM
{
    /// <summary>
    /// Script used for adjusting Rigidbody properties at runtime based on
    /// attached IMassAffectors. This allows for vehicle center of mass and inertia changes
    /// as the fuel is depleted, cargo is added, etc. without the need of physically parenting Rigidbodies to the object.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-1000)]
    [RequireComponent(typeof(Rigidbody))]
    public class VariableCenterOfMass : MonoBehaviour
    {
        /// <summary>
        /// Should the default Rigidbody mass be used?
        /// </summary>
        public bool useDefaultMass = true;

        /// <summary>
        /// If true, the script will search for any IMassAffectors attached as a child (recursively)
        /// of this script and use them when calculating mass, center of mass and inertia tensor. 
        /// </summary>
        public bool useMassAffectors = false;

        /// <summary>
        /// Base mass of the object, without IMassAffectors.
        /// </summary>
        [UnityEngine.Tooltip("Base mass of the object, without IMassAffectors.")]
        public float baseMass = 1400f;

        /// <summary>
        /// Total mass of the object with masses of IMassAffectors counted in.
        /// </summary>
        [UnityEngine.Tooltip("Total mass of the object with masses of IMassAffectors counted in.")]
        public float combinedMass = 1400f;

        /// <summary>
        /// Object dimensions in [m]. X - width, Y - height, Z - length.
        /// It is important to set the correct dimensions or otherwise inertia might be calculated incorrectly.
        /// </summary>
        [UnityEngine.Tooltip("Object dimensions in [m]. X - width, Y - height, Z - length.\r\nIt is important to set the correct dimensions or otherwise inertia might be calculated incorrectly.")]
        public Vector3 dimensions = new Vector3(1.8f, 1.6f, 4.6f);

        /// <summary>
        /// When enabled the Unity-calculated center of mass will be used.
        /// </summary>
        [Tooltip(
            "When enabled the Unity-calculated center of mass will be used.")]
        public bool useDefaultCenterOfMass = true;

        /// <summary>
        /// Center of mass of the object. Auto calculated. To adjust center of mass use centerOfMassOffset.
        /// </summary>
        [Tooltip(
            "Center of mass of the rigidbody. Needs to be readjusted when new colliders are added.")]
        public Vector3 centerOfMass = Vector3.zero;

        /// <summary>
        /// Combined center of mass, including the Rigidbody and any IMassAffectors.
        /// </summary>
        public Vector3 combinedCenterOfMass = Vector3.zero;

        /// <summary>
        /// When true inertia settings will be ignored and default Rigidbody inertia tensor will be used.
        /// </summary>
        [UnityEngine.Tooltip("When true inertia settings will be ignored and default Rigidbody inertia tensor will be used.")]
        public bool useDefaultInertia = true;

        /// <summary>
        ///     Vector by which the inertia tensor of the rigidbody will be scaled on Start().
        ///     Due to the uniform density of the rigidbodies, versus the very non-uniform density of a vehicle, inertia can feel
        ///     off.
        ///     Use this to adjust inertia tensor values.
        /// </summary>
        [Tooltip(
            "    Vector by which the inertia tensor of the rigidbody will be scaled on Start().\r\n    Due to the unform density of the rigidbodies, versus the very non-uniform density of a vehicle, inertia can feel\r\n    off.\r\n    Use this to adjust inertia tensor values.")]
        public Vector3 inertiaTensor = new Vector3(1000f, 1000f, 1000f);

        /// <summary>
        /// Total inertia tensor. Includes Rigidbody and IMassAffectors.
        /// </summary>
        public Vector3 combinedInertiaTensor;

        /// <summary>
        /// Objects attached or part of the vehicle affecting its center of mass and inertia.
        /// </summary>
        [NonSerialized] public IMassAffector[] affectors;

        private Rigidbody _rigidbody;



        private void Awake()
        {
            Initialize();
        }


        private void Initialize()
        {
            _rigidbody = GetComponent<Rigidbody>();
            
            if (useDefaultMass) baseMass = _rigidbody.mass;
            if (useDefaultInertia) inertiaTensor = _rigidbody.inertiaTensor;
            if (useDefaultCenterOfMass) centerOfMass = _rigidbody.centerOfMass;
            
            affectors = GetMassAffectors();
            UpdateAllProperties();
        }


        private void OnValidate()
        {
            _rigidbody = GetComponent<Rigidbody>();
            affectors = GetMassAffectors();
        }


        private void FixedUpdate()
        {
            UpdateAllProperties();
        }


        public void UpdateAllProperties()
        {
            if (!useDefaultMass) UpdateMass();
            if (!useDefaultCenterOfMass) UpdateCoM();
            if (!useDefaultInertia) UpdateInertia();
        }


        public void UpdateMass()
        {
            if (useMassAffectors)
            {
                combinedMass = CalculateMass();
            }
            else
            {
                combinedMass = baseMass;
            }
            
            _rigidbody.mass = combinedMass;
        }


        /// <summary>
        /// Calculates and applies the CoM to the Rigidbody.
        /// </summary>
        public void UpdateCoM()
        {
            if (useMassAffectors)
            {
                combinedCenterOfMass = centerOfMass + CalculateRelativeCenterOfMassOffset();
            }
            else
            {
                combinedCenterOfMass = centerOfMass;
            }
            
            _rigidbody.centerOfMass = combinedCenterOfMass;
        }


        /// <summary>
        /// Calculates and applies the inertia tensor to the Rigidbody.
        /// </summary>
        public void UpdateInertia(bool applyUnchanged = false)
        {
            if (useMassAffectors)
            {
                combinedInertiaTensor = inertiaTensor + CalculateInertiaTensorOffset(dimensions);
            }
            else
            {
                combinedInertiaTensor = inertiaTensor;
            }
            
            // Inertia tensor of constrained rigidbody will be 0 which causes errors when trying to set.
            if (combinedInertiaTensor.x > 0 && combinedInertiaTensor.y > 0 && combinedInertiaTensor.z > 0)
            {
                _rigidbody.inertiaTensor = combinedInertiaTensor;
                _rigidbody.inertiaTensorRotation = Quaternion.identity;
            }
        }


        /// <summary>
        /// Updates list of IMassAffectors attached to this object.
        /// Call after IMassAffector has been added or removed from the object.
        /// </summary>
        public IMassAffector[] GetMassAffectors()
        {
            return GetComponentsInChildren<IMassAffector>(true);
        }


        /// <summary>
        /// Calculates the mass of the Rigidbody and attached mass affectors.
        /// </summary>
        public float CalculateMass()
        {
            float massSum = baseMass;
            foreach (IMassAffector affector in affectors)
            {
                if (affector.GetTransform().gameObject.activeInHierarchy)
                {
                    massSum += affector.GetMass();
                }
            }

            return massSum;
        }


        /// <summary>
        /// Calculates the center of mass of the Rigidbody and attached mass affectors.
        /// </summary>
        public Vector3 CalculateRelativeCenterOfMassOffset()
        {
            Vector3 offset = Vector3.zero;

            if (useMassAffectors)
            {
                float massSum = CalculateMass();
                
                for (int i = 0; i < affectors.Length; i++)
                {
                    offset += transform.InverseTransformPoint(affectors[i].GetWorldCenterOfMass()) * (affectors[i].GetMass() / massSum);
                }
            }

            return offset;
        }


        /// <summary>
        /// Calculates the inertia tensor of the Rigidbody and attached mass affectors.
        /// </summary>
        public Vector3 CalculateInertiaTensorOffset(Vector3 dimensions)
        {
            Vector3 affectorInertiaSum = Vector3.zero;
            for (int i = 0; i < affectors.Length; i++) // Skip first (this)
            {
                IMassAffector affector = affectors[i];
                if (affector.GetTransform().gameObject.activeInHierarchy)
                {
                    float mass = affector.GetMass();
                    Vector3 affectorLocalPos = transform.InverseTransformPoint(affector.GetTransform().position);
                    float x = Vector3.ProjectOnPlane(affectorLocalPos, Vector3.right).magnitude * mass;
                    float y = Vector3.ProjectOnPlane(affectorLocalPos, Vector3.up).magnitude * mass;
                    float z = Vector3.ProjectOnPlane(affectorLocalPos, Vector3.forward).magnitude * mass;
                    affectorInertiaSum.x += x * x;
                    affectorInertiaSum.y += y * y;
                    affectorInertiaSum.z += z * z;
                }
            }

            return affectorInertiaSum;
        }


        public static Vector3 CalculateInertia(Vector3 dimensions, float mass)
        {
            float c = (1f / 12f) * mass;
            float Ix = c * (dimensions.y * dimensions.y + dimensions.z * dimensions.z);
            float Iy = c * (dimensions.x * dimensions.x + dimensions.z * dimensions.z);
            float Iz = c * (dimensions.y * dimensions.y + dimensions.x * dimensions.x);

            return new Vector3(Ix, Iy, Iz);
        }


        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Initialize();
                UpdateAllProperties();
            }

            // CoM
            Gizmos.color = Color.yellow;
            Vector3 worldCoM = transform.TransformPoint(centerOfMass);
            Gizmos.DrawSphere(worldCoM, 0.03f);
            Handles.Label(worldCoM, "CoM");

            // Mass Affectors
            Gizmos.color = Color.cyan;

            if (affectors == null) return;
            for (int i = 0; i < affectors.Length; i++)
            {
                if (affectors[i] == null) continue;
                Gizmos.DrawSphere(affectors[i].GetTransform().position, 0.05f);
            }

            // Dimensions
            if (!useDefaultInertia)
            {
                Transform t = transform;
                Vector3 fwdOffset = t.forward * dimensions.z * 0.5f;
                Vector3 rightOffset = t.right * dimensions.x * 0.5f;
                Vector3 upOffset = t.up * dimensions.y * 0.5f;

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(worldCoM + fwdOffset, worldCoM - fwdOffset);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(worldCoM + rightOffset, worldCoM - rightOffset);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(worldCoM + upOffset, worldCoM - upOffset);
            }
#endif
        }

        private void Reset()
        {
            _rigidbody = GetComponent<Rigidbody>();
            Bounds bounds = gameObject.FindBoundsIncludeChildren();
            dimensions = new Vector3(bounds.extents.x * 2f, bounds.extents.y * 2f, bounds.extents.z * 2f);
            Debug.Log($"Detected dimensions of {name} as {dimensions} [m]. If incorrect, adjust manually.");
            if (dimensions.x < 0.001f) dimensions.x = 0.001f;
            if (dimensions.y < 0.001f) dimensions.y = 0.001f;
            if (dimensions.z < 0.001f) dimensions.z = 0.001f;
            centerOfMass = _rigidbody.centerOfMass;
            baseMass = _rigidbody.mass;
            combinedMass = baseMass;
            inertiaTensor = _rigidbody.inertiaTensor;
        }

        public Vector3 GetWorldCenterOfMass()
        {
            return transform.TransformPoint(combinedCenterOfMass);
        }
    }
}


#if UNITY_EDITOR
namespace NWH.Common.CoM
{
    [CustomEditor(typeof(VariableCenterOfMass))]
    public class VariableCenterOfMassEditor : NUIEditor
    {
        public override bool OnInspectorNUI()
        {
            if (!base.OnInspectorNUI())
            {
                return false;
            }

            VariableCenterOfMass vcom = (VariableCenterOfMass)target;
            if (vcom == null)
            {
                drawer.EndEditor();
                return false;
            }

            Rigidbody parentRigidbody = vcom.gameObject.GetComponentInParent<Rigidbody>(true);
            if (parentRigidbody == null)
            {
                drawer.EndEditor();
                return false;
            }

            if (!Application.isPlaying)
            {
                foreach (var o in targets)
                {
                    var t = (VariableCenterOfMass)o;
                    t.affectors = t.GetMassAffectors();
                    t.UpdateAllProperties();
                }
            }

            drawer.BeginSubsection("Mass Affectors");
            if (drawer.Field("useMassAffectors").boolValue)
            {
                if (vcom.affectors != null)
                {
                    if (!Application.isPlaying)
                    {
                        vcom.affectors = vcom.GetMassAffectors();
                    }

                    for (int i = 0; i < vcom.affectors.Length; i++)
                    {
                        IMassAffector affector = vcom.affectors[i];
                        if (affector == null || affector.GetTransform() == null) continue;
                        string positionStr = i == 0 ? "(this)" : $"Position = {affector.GetTransform().localPosition}";
                        drawer.Label($"{affector.GetTransform().name}  |  Mass = {affector.GetMass()}  |  {positionStr}");
                    }
                }
            }
            drawer.EndSubsection();

            // MASS
            drawer.BeginSubsection("Mass");
            if (!drawer.Field("useDefaultMass").boolValue)
            {
                float newMass = drawer.Field("baseMass", true, "kg").floatValue;
                parentRigidbody.mass = newMass;

                if (vcom.useMassAffectors)
                {
                    drawer.Field("combinedMass", false, "kg");
                }
            }
            drawer.EndSubsection();


            // CENTER OF MASS
            drawer.BeginSubsection("Center Of Mass");
            if (!drawer.Field("useDefaultCenterOfMass").boolValue)
            {
                drawer.Field("centerOfMass", true);

                if (vcom.useMassAffectors)
                {
                    drawer.Field("combinedCenterOfMass", false);
                }
            }
            drawer.EndSubsection();


            // INERTIA
            drawer.BeginSubsection("Inertia");
            if (!drawer.Field("useDefaultInertia").boolValue)
            {
                drawer.Field("inertiaTensor", true, "kg m2");
                if (vcom.useMassAffectors)
                {
                    drawer.Field("combinedInertiaTensor", false, "kg m2");
                }

                drawer.BeginSubsection("Calculate Inertia From Dimensions");
                {
                    drawer.Field("dimensions", true, "m");
                    if (drawer.Button("Calculate"))
                    {
                        vcom.inertiaTensor = VariableCenterOfMass.CalculateInertia(vcom.dimensions, parentRigidbody.mass);
                        EditorUtility.SetDirty(vcom);
                    }
                }
            }
            drawer.EndSubsection();

            drawer.EndEditor(this);
            return true;
        }
    }
}
#endif