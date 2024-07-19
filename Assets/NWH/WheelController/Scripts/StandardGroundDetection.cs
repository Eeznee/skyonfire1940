using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using NWH.NUI;
#endif


namespace NWH.WheelController3D
{
    public class StandardGroundDetection : GroundDetectionBase
    {
        /// <summary>
        /// When true multiple casts are used at all times.
        /// Increases the ground detection quality at the cost of performance (~30% slower).
        /// </summary>
        public bool forceMulticast;

        private RaycastHit _nearestHit;
        private RaycastHit _castResult;
        private WheelController _wheelController;
        private Transform _transform;

#if WC3D_DEBUG
        private List<WheelCastResult> _wheelCastResults = new List<WheelCastResult>();
        private List<WheelCastInfo> _wheelCasts = new List<WheelCastInfo>();

        /// <summary>
        /// Used for debug gizmo drawing only.
        /// Holds ray/sphere cast data.
        /// </summary>
        [System.Serializable]
        private struct WheelCastInfo
        {
            public WheelCastInfo(Type castType, Vector3 origin, Vector3 direction,
                float distance, float radius, float width)
            {
                this.castType = castType;
                this.origin = origin;
                this.direction = direction;
                this.distance = distance;
                this.radius = radius;
                this.width = width;
            }

            public enum Type
            {
                Ray,
                Sphere
            }

            public Type castType;
            public Vector3 origin;
            public Vector3 direction;
            public float distance;
            public float radius;
            public float width;
        }

        /// <summary>
        /// Used for debug gizmo drawing only.
        /// Holds ray/sphere cast data.
        /// </summary>
        [System.Serializable]
        private struct WheelCastResult
        {
            public WheelCastResult(Vector3 point, Vector3 normal, WheelCastInfo castInfo)
            {
                this.point = point;
                this.normal = normal;
                this.castInfo = castInfo;
            }

            public Vector3 point;
            public Vector3 normal;
            public WheelCastInfo castInfo;
        }
#endif

        private void Awake()
        {
            _wheelController = GetComponent<WheelController>();
        }


        public override bool WheelCast(in Vector3 origin, in Vector3 direction, in float distance, in float radius, in float width, ref WheelHit wheelHit, LayerMask layerMask)
        {
            _transform = transform;

            bool initQueriesHitTriggers = Physics.queriesHitTriggers;
            bool initQueriesHitBackfaces = Physics.queriesHitBackfaces;
            Physics.queriesHitTriggers = false;
            Physics.queriesHitBackfaces = false;


#if WC3D_DEBUG
            _wheelCastResults.Clear();
            _wheelCasts.Clear();
#endif

            bool isValid = false;
            if (forceMulticast)
            {
                // Ground can sometimes not be found by a single sphere because any collider that the sphere is already touching at
                // the start of the cast is ignored. Therefore, use multicast to cover this case.
                if (WheelCastMultiSphere(origin, direction, distance, radius, width, ref _castResult, layerMask))
                {
                    isValid = true;
                }
            }
            if (WheelCastSingleSphere(origin, direction, distance, radius, width, ref _castResult, layerMask))
            {
                if (IsInsideWheel(_castResult.point, origin, radius, width))
                {
                    isValid = true;
                }
                else
                {
                    if (WheelCastMultiSphere(origin, direction, distance, radius, width, ref _castResult, layerMask))
                    {
                        isValid = true;
                    }
                }
            }


            if (isValid)
            {
                wheelHit.point = _castResult.point;
                wheelHit.normal = _castResult.normal;
                wheelHit.collider = _castResult.collider;
            }

            Physics.queriesHitTriggers = initQueriesHitTriggers;
            Physics.queriesHitBackfaces = initQueriesHitBackfaces;

            return isValid;
        }


        private bool WheelCastSingleSphere(in Vector3 origin, in Vector3 direction, in float distance, in float radius, in float width, ref RaycastHit hit, LayerMask layerMask)
        {
#if WC3D_DEBUG
            WheelCastInfo castInfo = new WheelCastInfo(WheelCastInfo.Type.Sphere,
              origin, direction, distance, radius, width);
            _wheelCasts.Add(castInfo);
#endif

            if (Physics.SphereCast(origin, radius, direction, out hit, distance, layerMask))
            {
#if WC3D_DEBUG
                _wheelCastResults.Add(new WheelCastResult(hit.point, hit.normal, castInfo));
#endif

                return true;
            }

            return false;
        }


        private bool WheelCastMultiSphere(in Vector3 origin, in Vector3 direction, in float distance, in float radius, in float width, ref RaycastHit hit, LayerMask layerMask)
        {
            float nearestDistance = 1e10f;

            float castRadius = width * 0.5f;
            bool useRaycast = width == 0 ? true : width * 5f < radius;
            float zRange = 2f * radius;

            int zSteps = 11;
            if (!useRaycast)
            {
                zSteps = Mathf.RoundToInt((radius / castRadius) * 2f);
                zSteps = zSteps % 2 == 0 ? zSteps + 1 : zSteps; // Ensure there is always a center sphere (odd number of spheres).
            }
            float stepAngle = 180f / (zSteps - 1);

            Vector3 up = _transform.up;
            Vector3 right = _transform.right;
            Vector3 forwardOffset = _transform.forward * radius;

            Quaternion xStepQuaternion = Quaternion.AngleAxis(stepAngle, right);
            Quaternion xRotationQuaternion = Quaternion.identity;

            for (int z = 0; z < zSteps; z++)
            {
                Vector3 castOrigin = origin + xRotationQuaternion * forwardOffset;

#if WC3D_DEBUG
                WheelCastInfo castInfo = new WheelCastInfo(useRaycast ? WheelCastInfo.Type.Ray : WheelCastInfo.Type.Sphere,
                    castOrigin, direction, distance, castRadius, width);
                _wheelCasts.Add(castInfo);
#endif

                RaycastHit castHit;
                bool hasHit = useRaycast ?
                    Physics.Raycast(castOrigin, direction, out castHit, distance + castRadius, layerMask) :
                    Physics.SphereCast(castOrigin, castRadius, direction, out castHit, distance, layerMask);

                if (hasHit)
                {
                    Vector3 hitLocalPoint = _transform.InverseTransformPoint(castHit.point);
                    float sine = hitLocalPoint.z / radius;
                    sine = sine < -1f ? -1f : sine > 1f ? 1f : sine;
                    float hitAngle = Mathf.Asin(sine);
                    float potentialWheelPosition = hitLocalPoint.y + radius * Mathf.Cos(hitAngle);
                    hit.distance = -potentialWheelPosition;

#if WC3D_DEBUG
                    _wheelCastResults.Add(new WheelCastResult(castHit.point, castHit.normal, castInfo));
#endif

                    if (hit.distance < nearestDistance)
                    {
                        nearestDistance = hit.distance;
                        _nearestHit = castHit;
                    }
                }

                xRotationQuaternion *= xStepQuaternion;
            }

            if (nearestDistance < 1e9f)
            {
                hit = _nearestHit;
                return true;
            }

            return false;
        }


        private bool IsInsideWheel(in Vector3 point, in Vector3 wheelPos, in float radius, in float width)
        {
            Vector3 offset = point - wheelPos;
            Vector3 localOffset = _transform.InverseTransformVector(offset);
            float halfWidth = width * 0.5f;
            if (localOffset.x >= -halfWidth && localOffset.x <= halfWidth
                && localOffset.z <= radius && localOffset.z >= -radius)
            {
                return true;
            }

            return false;
        }


        private void OnDrawGizmos()
        {
#if WC3D_DEBUG
            foreach (WheelCastInfo wheelCast in _wheelCasts)
            {
                Gizmos.color = Color.cyan;
                if (wheelCast.castType == WheelCastInfo.Type.Sphere)
                {
                    Gizmos.DrawWireSphere(wheelCast.origin, wheelCast.radius);
                }
                else
                {
                    Gizmos.DrawCube(wheelCast.origin, new Vector3(0.01f, 0.05f, 0.01f));
                }

                Gizmos.DrawRay(wheelCast.origin, wheelCast.direction * wheelCast.distance);
            }

            foreach (WheelCastResult result in _wheelCastResults)
            {
                bool isInsideWheel = IsInsideWheel(result.point, result.castInfo.origin,
                    result.castInfo.radius, result.castInfo.width);
                Gizmos.color = isInsideWheel ? Color.green : Color.yellow;
                Gizmos.DrawWireSphere(result.point, 0.02f);
                Gizmos.DrawRay(result.point, result.normal * 0.1f);
            }
#endif
        }
    }
}



#if UNITY_EDITOR
namespace NWH.WheelController3D
{
    /// <summary>
    ///     Editor for WheelController.
    /// </summary>
    [CustomEditor(typeof(StandardGroundDetection))]
    [CanEditMultipleObjects]
    public class StandardGroundDetectionEditor : NUIEditor
    {
        public override bool OnInspectorNUI()
        {
            if (!base.OnInspectorNUI()) return false;

            drawer.Field("forceMulticast");

            drawer.EndEditor(this);
            return true;
        }
    }
}
#endif
