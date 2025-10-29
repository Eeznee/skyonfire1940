using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Sof Components/Undercarriage/Simple Link")]
public class SimpleLink : MechanicalLink
{

    public Vector3 axis = Vector3.forward;
    public Vector3 localJointPos = Vector3.right;
    public Link linkTo;


    public override void MechanicalAnimation()
    {
        if (!linkTo.tr || linkTo.tr.root != transform.root) return;

        Vector3 worldAxis = transform.parent.TransformDirection(axis);
        Vector3 jointWorldPos = linkTo.PivotPos;

        transform.localRotation = Quaternion.identity;
        Vector3 jointDirection = jointWorldPos - transform.position;
        float angle = Vector3.SignedAngle(jointDirection, transform.TransformDirection(localJointPos), worldAxis);
        transform.rotation = Quaternion.AngleAxis(-angle, worldAxis) * transform.parent.rotation;
    }
}