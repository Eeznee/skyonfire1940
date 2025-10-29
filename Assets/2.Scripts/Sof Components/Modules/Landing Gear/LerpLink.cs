using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Sof Components/Undercarriage/Lerp Link")]
public class LerpLink : MechanicalLink
{
    [Range(0f,1f)]
    public float lerp = 0.5f;
    public Link link1;
    public Link link2;
    public Vector3 offsetPos;


    public override void MechanicalAnimation()
    {
        if (!link1.tr || !link2.tr || link1.tr.root != transform.root || link2.tr.root != transform.root) return;

        transform.rotation = Quaternion.Lerp(link1.tr.rotation, link2.tr.rotation, lerp);
        transform.position = Vector3.Lerp(link1.PivotPos, link2.PivotPos, lerp) + transform.TransformDirection(offsetPos);
    }
}