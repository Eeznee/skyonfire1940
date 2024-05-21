using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Processors;

[ExecuteInEditMode]
public class LinkJoint : MonoBehaviour
{

    [System.Serializable]
    public struct Link
    {
        public Transform tr;
        public Vector3 jointPivot;
        
        [HideInInspector] public Vector3 axis;

        public Vector3 PivotWorldDir() { return tr.TransformDirection(jointPivot); }

        public Vector2 CircleCenter(Vector3 normal, Vector3 xAxis, Vector3 yAxis)
        {
            Vector3 circle1Center = Vector3.ProjectOnPlane(tr.position, normal);

            float x = Vector3.Dot(circle1Center,xAxis);
            float y = Vector3.Dot(circle1Center,yAxis);
            return new Vector2(x, y);
        }
        public void RotateToIntersection(Vector3 intersection, Vector3 normal)
        {
            Vector3 fromC2ToIntersection = intersection - Vector3.ProjectOnPlane(tr.position, normal);
            float angle = Vector3.SignedAngle(fromC2ToIntersection, PivotWorldDir(), normal);
            tr.Rotate(normal, -angle, Space.World);
        }
    }
    public bool showIntersectionCircles = false;

    public Link link1;
    public Link link2;

    public Vector3 axis;

    public bool invert = false;
    void Awake()
    {
        axis = axis.normalized;

    }
    void Update()
    {
        MatchLinks();
    }

    private void MatchLinks()
    {
        Vector3 normal = transform.TransformDirection(axis);

        Vector3 jointPivot1 = Vector3.ProjectOnPlane(link1.PivotWorldDir(), normal);
        Vector3 jointPivot2 = Vector3.ProjectOnPlane(link2.PivotWorldDir(), normal);

        Vector3 xAxis = jointPivot1.normalized;
        Vector3 yAxis = Vector3.Cross(xAxis, normal);

        Vector2 c1 = link1.CircleCenter(normal, xAxis, yAxis);
        Vector2 c2 = link2.CircleCenter(normal, xAxis, yAxis);

        Vector2 intersection2d = Intersection(c1, jointPivot1.magnitude, c2, jointPivot2.magnitude);
        Vector3 intersection3d = intersection2d.x * xAxis + intersection2d.y * yAxis;

        link1.RotateToIntersection(intersection3d, normal);
        link2.RotateToIntersection(intersection3d, normal);
    }

    private void OnDrawGizmos()
    {
        if (!showIntersectionCircles) return;

        Features.DrawCircle(link1.tr.position, link1.tr.right, link1.jointPivot.magnitude,Color.red, 64);
        Features.DrawCircle(link2.tr.position, link2.tr.right, link2.jointPivot.magnitude, Color.blue, 64);
        Vector3 intersection = 0.5f * (link1.tr.TransformPoint(link1.jointPivot) + link2.tr.TransformPoint(link2.jointPivot));
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(intersection, 0.02f);
    }

    //https://planetcalc.com/8098/
    private Vector2 Intersection(Vector2 c1, float r1, Vector2 c2, float r2)
    {
        float d = (c1 - c2).magnitude;

        float a = ((r1 * r1) - (r2 * r2) + (d * d)) / (2*d);

        if (a > r1) { r1 = a; }

        float h = Mathf.Sqrt(r1 * r1 - a * a);

        Vector2 P = c1 + a/d * (c2 - c1);

        Vector2 offset = new Vector2(c2.y - c1.y, c1.x - c2.x) * h/d;
        return invert ? P + offset : P - offset;
    }
}
