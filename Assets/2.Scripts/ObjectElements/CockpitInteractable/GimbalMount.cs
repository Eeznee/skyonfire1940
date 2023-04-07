using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GimbalMount : CockpitInteractable
{
    public Turret turret;
    private Vector3 pistolGripOffset;

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        pistolGripOffset = gripDefaultPos;
        pistolGripOffset.z = 0f;
    }
    protected override void VRInteraction(Vector3 gripPos,Quaternion gripRot)
    {
        turret.SetDirectionSemi(transform.position - gripPos + transform.TransformDirection(pistolGripOffset));
        bool guns = SofVrRig.instance.Trigger(xrGrab) > 0.1f;
        turret.Operate(guns, false);
    }

    private void Update()
    {
        CockpitInteractableUpdate();
    }
}
