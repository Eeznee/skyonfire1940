using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NWH.WheelController3D
{
    public partial class WheelController
    {
        /// <summary>
        ///     Visual representation of the wheel and it's more important Vectors.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            Vector3 tp = transform.position;

            // Draw spring travel
            Gizmos.color = Color.white;
            Vector3 forwardOffset = transform.forward * 0.05f;
            Vector3 springOffset = transform.up * spring.maxLength;

            Gizmos.DrawLine(tp - forwardOffset, tp + forwardOffset);
            Gizmos.DrawLine(tp - springOffset - forwardOffset,
                            tp - springOffset + forwardOffset);
            Gizmos.DrawLine(tp, tp - springOffset);

            // Draw force app. point
            if (wheel.visual != null)
            {
                Gizmos.color = Color.red;
                Vector3 forceAppPoint = wheel.visual.transform.position + Vector3.up * (-wheel.radius + spring.maxLength * forceApplicationPointDistance);
                Gizmos.DrawSphere(forceAppPoint, 0.01f);
                Handles.Label(forceAppPoint, "    Force App. Point");
            }

            // Set dummy variables when in inspector.
            if (!Application.isPlaying)
            {
                if (wheel.visual != null)
                {
                    wheel.worldPosition = wheel.visual.transform.position; // TODO
                    wheel.up = wheel.visual.transform.up;
                    wheel.forward = wheel.visual.transform.forward;
                    wheel.right = wheel.visual.transform.right;
                }
            }

            // Draw wheel
            if (wheel.visual != null)
            {
                Gizmos.color = Color.grey;
                Gizmos.DrawSphere(wheel.worldPosition, 0.02f);

                Vector3 center = wheel.visual.transform.position;
                Vector3 right = wheel.visual.transform.right;
                Vector3 up = wheel.visual.transform.up;
                Vector3 c0 = center - right * wheel.width * 0.5f;
                Vector3 c1 = center + right * wheel.width * 0.5f;
                Handles.DrawWireDisc(c0, right, wheel.radius);
                Handles.DrawWireDisc(c1, right, wheel.radius);
                Handles.DrawLine(c0 - up * wheel.radius, c1 - up * wheel.radius);
                Handles.DrawLine(c0 + up * wheel.radius, c1 + up * wheel.radius);
                Handles.DrawLine(c0 - up * wheel.radius, c0 + up * wheel.radius);
                Handles.DrawLine(c1 - up * wheel.radius, c1 + up * wheel.radius);
            }



            if (Application.isPlaying)
            {
                // Draw wheel anchor normals
                Gizmos.color = Color.green;
                Gizmos.DrawRay(new Ray(wheel.worldPosition, wheel.up * 0.2f));
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(new Ray(wheel.worldPosition, wheel.forward * 0.2f));
                Gizmos.color = Color.red;
                Gizmos.DrawRay(new Ray(wheel.worldPosition, wheel.right * 0.2f));

                if (_isGrounded)
                {
                    //Draw hit forward and sideways
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawRay(wheelHit.point,
                                    _hitForwardDirection * (forwardFriction.force * 1e-4f));

                    Gizmos.color = Color.magenta;
                    Gizmos.DrawRay(wheelHit.point,
                                    _hitSidewaysDirection * (sideFriction.force * 1e-4f));

                    // Draw hit point
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(wheelHit.point, 0.022f);

                    Gizmos.color = Color.white;
                    Gizmos.DrawSphere(wheelHit.point, 0.02f);

                    Gizmos.DrawRay(wheelHit.point, wheelHit.normal * 0.5f);
                }
            }
#endif
        }
    }
}
