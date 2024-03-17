using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkJoint : MonoBehaviour
{
    [System.Serializable]
    public struct Link
    {
        public Transform tr;
        public Vector3 jointPivot;
        
        [HideInInspector] public Vector3 axis;

        public Vector2 CircleCenter(Vector3 normal, Vector3 xAxis, Vector3 yAxis)
        {
            Vector3 circle1Center = Vector3.ProjectOnPlane(tr.position, normal);

            float x = Vector3.Dot(circle1Center,xAxis);
            float y = Vector3.Dot(circle1Center,yAxis);
            return new Vector2(x, y);
        }
    }
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
        link1.tr.localRotation = Quaternion.identity;
        link2.tr.localRotation = Quaternion.identity;

        Vector3 normal = transform.TransformDirection(axis);

        Vector3 jointPivot1 = Vector3.ProjectOnPlane(link1.tr.TransformDirection(link1.jointPivot), normal);
        Vector3 jointPivot2 = Vector3.ProjectOnPlane(link2.tr.TransformDirection(link2.jointPivot),normal);

        Vector3 xAxis = jointPivot1.normalized;
        Vector3 yAxis = Vector3.Cross(xAxis, normal);

        Vector2 c1 = link1.CircleCenter(normal, xAxis,yAxis);
        Vector2 c2 = link2.CircleCenter(normal, xAxis,yAxis);

        float r1 = jointPivot1.magnitude;
        float r2 = jointPivot2.magnitude;

        Vector2 intersection2d = Intersection(c1, r1, c2, r2);
        Vector3 intersection = intersection2d.x * xAxis + intersection2d.y * yAxis;

        Vector3 fromC1ToIntersection = intersection - Vector3.ProjectOnPlane(link1.tr.position, normal);
        Vector3 fromC2ToIntersection = intersection - Vector3.ProjectOnPlane(link2.tr.position, normal);
        float angle1 = Vector3.SignedAngle(fromC1ToIntersection, jointPivot1,normal);
        float angle2 = Vector3.SignedAngle(fromC2ToIntersection, jointPivot2,normal);

        link1.tr.Rotate(normal,-angle1, Space.World);
        link2.tr.Rotate(normal,-angle2, Space.World);
    }
    //https://planetcalc.com/8098/
    private Vector2 Intersection(Vector2 c1, float r1, Vector2 c2, float r2)
    {
        float d = (c1 - c2).magnitude;

        float a = ((r1 * r1) - (r2 * r2) + (d * d)) / (2*d);

        float h = Mathf.Sqrt(r1 * r1 - a * a);

        Vector2 P = c1 + a/d * (c2 - c1);

        Vector2 offset = new Vector2(c2.y - c1.y, c1.x - c2.x) * h/d;
        return invert ? P + offset : P - offset;
    }
}
