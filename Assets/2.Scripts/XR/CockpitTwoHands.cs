using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CockpitTwoHands : CockpitInteractable
{
    public XRGrabInteractable secondXrGrip;
    public HandGrip secondGrip;

    protected private Vector3 secondXrGripDefaultPos;

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        secondXrGrip.colliders[0].gameObject.layer = secondXrGrip.gameObject.layer = indexSelect ? 14 : 13;
        secondXrGrip.interactionLayerMask = LayerMask.GetMask(indexSelect ? "TriggerGrab" : "GripGrab");
        secondXrGrip.attachEaseInTime = 0f;
        secondXrGripDefaultPos = secondXrGrip.transform.localPosition;
    }
    protected override void OnGrab()
    {
        
        Vector3 average = Vector3.Lerp(secondXrGrip.transform.position, xrGrip.transform.position,0.5f);
        Vector3 averageDefault = Vector3.Lerp(secondXrGripDefaultPos,xrGripDefaultPos, 0.5f);
        data.transform.InverseTransformVector(average - anchor.TransformPoint(averageDefault));
    }
    protected override void CockpitInteractableUpdate()
    {
        if (!xrGrip || !secondXrGrip) return;
        bool selected = xrGrip.isSelected && !(indexSelect && SofVrRig.instance.Grip(xrGrip) > 0.7f);
        selected = selected && secondXrGrip.isSelected && !(indexSelect && SofVrRig.instance.Grip(secondXrGrip) > 0.7f);
        if (selected)
        {
            if (!selectedPrevious) OnGrab();
            Vector3 average = Vector3.Lerp(secondXrGrip.transform.position, xrGrip.transform.position, 0.5f);
            VRInteraction(average - data.transform.TransformVector(gripOffset), xrGrip.transform.rotation);
        }
        else
        {
            if (selectedPrevious) OnRelease();
            xrGrip.transform.localPosition = xrGripDefaultPos;
            secondXrGrip.transform.localPosition = secondXrGripDefaultPos;
        }
        Animate();
        if (GameManager.gm.vr && outline) outline.enabled = ReadySelect();
        selectedPrevious = selected;
    }

    protected override bool ReadySelect()
    {
        return xrGrip.isHovered ||secondXrGrip.isHovered;
    }
}
