using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.XR.Interaction.Toolkit;

public class CockpitTwoHands : CockpitInteractable
{
    //public XRGrabInteractable secondXrGrip;
    public HandGrip secondGrip;

    protected private Vector3 secondXrGripDefaultPos;

    /*
    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        secondXrGrip.colliders[0].gameObject.layer = secondXrGrip.gameObject.layer = indexSelect ? 14 : 13;
        secondXrGrip.interactionLayers = LayerMask.GetMask(indexSelect ? "TriggerGrab" : "GripGrab");
        secondXrGrip.attachEaseInTime = 0f;
        secondXrGripDefaultPos = secondXrGrip.transform.localPosition;
    }
    protected override void OnGrab()
    {
        
        Vector3 average = Vector3.Lerp(secondXrGrip.transform.position, xrGrab.transform.position,0.5f);
        Vector3 averageDefault = Vector3.Lerp(secondXrGripDefaultPos,gripDefaultPos, 0.5f);
        data.transform.InverseTransformVector(average - grip.transform.parent.TransformPoint(averageDefault));
    }
    protected override void CockpitInteractableUpdate()
    {
        if (!xrGrab || !secondXrGrip) return;
        bool selected = xrGrab.isSelected && !(indexSelect && SofVrRig.instance.Grip(xrGrab) > 0.7f);
        selected = selected && secondXrGrip.isSelected && !(indexSelect && SofVrRig.instance.Grip(secondXrGrip) > 0.7f);
        if (selected)
        {
            if (!wasSelected) OnGrab();
            Vector3 average = Vector3.Lerp(secondXrGrip.transform.position, xrGrab.transform.position, 0.5f);
            VRInteraction(average - data.transform.TransformVector(gripOffset), xrGrab.transform.rotation);
        }
        else
        {
            if (wasSelected) OnRelease();
            xrGrab.transform.localPosition = gripDefaultPos;
            secondXrGrip.transform.localPosition = secondXrGripDefaultPos;
        }
        wasSelected = selected;
    }
    protected override bool ReadySelect()
    {
        return xrGrab.isHovered ||secondXrGrip.isHovered;
    }
        */
    private void Update()
    {
        CockpitInteractableUpdate();
    }
}
