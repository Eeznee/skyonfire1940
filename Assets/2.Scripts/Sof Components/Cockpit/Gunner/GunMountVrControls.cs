using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunMountVrControls : CockpitInteractable
{
    public GunMount turret;
    private Vector3 pistolGripOffset;

    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);
        pistolGripOffset = gripDefaultPos;
        pistolGripOffset.z = 0f;
    }
    protected override void VRInteraction(Vector3 gripPos,Quaternion gripRot)
    {
        turret.OperateMainTracking(transform.position - gripPos + transform.TransformDirection(pistolGripOffset));
        //bool guns = SofVrRig.instance.Trigger(xrGrab) > 0.1f;
        //turret.OperateTrigger(guns, false);
    }

    private void Update()
    {
        CockpitInteractableUpdate();
    }
}
