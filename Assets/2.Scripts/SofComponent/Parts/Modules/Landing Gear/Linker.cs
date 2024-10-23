using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Sof Components/Undercarriage/Bars Linker")]
public class Linker : MonoBehaviour
{

    [System.Serializable]
    public struct Link
    {
        public Transform tr;
        public Vector3 jointPivot;
        [HideInInspector] public float length;

        public Vector3 pos => tr.position;

        public void RotateToIntersection(Vector3 intersection, Vector3 normal)
        {
            tr.localRotation = Quaternion.identity;
            Vector3 fromC2ToIntersection = intersection - tr.position;
            float angle = Vector3.SignedAngle(fromC2ToIntersection, tr.TransformDirection(jointPivot), normal);
            tr.Rotate(normal, -angle, Space.World);
        }
    }
    public bool showIntersectionCircles = false;

    public Link link1;
    public Link link2;

    public Vector3 axis = Vector3.right;

    public bool invert = false;
    void Awake()
    {
        PrecomputeValues();
    }
    void PrecomputeValues()
    {
        link1.length = link1.jointPivot.magnitude;
        link2.length = link2.jointPivot.magnitude;
    }
    void Update()
    {
        if (!Application.isPlaying) PrecomputeValues();

        if (link1.tr && link2.tr && link1.tr.root == link2.tr.root)
            MatchLinks();
    }

    private void MatchLinks()
    {
        IntersectionsSolutionsCircle(out Vector3 center, out float radius);

        Vector3 worldAxis = transform.TransformDirection(axis);

        Vector3 solutionsDirection = Vector3.Cross(worldAxis, link2.pos - link1.pos).normalized;
        if (invert) solutionsDirection = -solutionsDirection;
        Vector3 solutionPoint = solutionsDirection * radius + center;

        link1.RotateToIntersection(solutionPoint, worldAxis);
        link2.RotateToIntersection(solutionPoint, worldAxis);
    }

    private void IntersectionsSolutionsCircle(out Vector3 center, out float radius)
    {
        float a1 = link1.length;
        float a2 = link2.length;
        float L = (link1.pos - link2.pos).magnitude;
        if (a1 + a2 < L) L = a1 + a2;
        if (a1 > a2 + L) a2 = a1 - L;
        if (a2 > a1 + L) a1 = a2 - L;

        float area = Mathv.TriangleArea(a1, a2, L);
        float height = 2f * area / L;

        float distanceFromLink1 = Mathf.Sqrt(a1 * a1 - height * height);

        center = Vector3.LerpUnclamped(link1.pos, link2.pos, distanceFromLink1 / L);
        radius = height;
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showIntersectionCircles) return;
        if (!link1.tr || !link2.tr) return;

        Vector3 worldAxis = transform.TransformDirection(axis);


        Vector3 intersection = 0.5f * (link1.tr.TransformPoint(link1.jointPivot) + link2.tr.TransformPoint(link2.jointPivot));
        IntersectionsSolutionsCircle(out Vector3 center, out float radius);
        Vector3 solutionsDirection = Vector3.Cross(worldAxis, link2.pos - link1.pos).normalized;
        if (invert) solutionsDirection = -solutionsDirection;
        Vector3 solution = center + solutionsDirection * radius;

        //AdvancedGizmos();

        Handles.color = Color.red;
        Handles.DrawWireDisc(link1.pos, worldAxis, link1.length);
        Handles.DrawLine(link1.pos, solution);
        Handles.color = Color.blue;
        Handles.DrawWireDisc(link2.pos, worldAxis, link2.length);
        Handles.DrawLine(link2.pos, solution);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(solution, 0.02f);
    }
    private void AdvancedGizmos()
    {
        Vector3 worldAxis = transform.TransformDirection(axis);
        IntersectionsSolutionsCircle(out Vector3 center, out float radius);

        Vector3 circleNormal = (link2.pos - link1.pos).normalized;
        Handles.color = Color.green;
        Handles.DrawWireDisc(center, circleNormal, radius);

        Gizmos.color = Color.magenta;
        Vector3 solutionsDirection = Vector3.Cross(worldAxis, link2.pos - link1.pos).normalized;
        if (invert) solutionsDirection = -solutionsDirection;
        Gizmos.DrawRay(center, solutionsDirection * radius);
    }
#endif
}