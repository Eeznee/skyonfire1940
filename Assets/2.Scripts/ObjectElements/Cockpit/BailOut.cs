using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
public class BailOut : CockpitTwoHands
{
    const float minDis = 0.2f;
    public Transform bailOutPoint;

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
    }
    protected override void VRInteraction(Vector3 gripPos,Quaternion gripRot)
    {
        SofVrRig s = SofVrRig.instance;
        Vector3 delta = Vector3.Lerp(s.rightHandDelta, s.leftHandDelta, 0.5f);
        s.transform.position -= delta;
    }
    protected override void OnRelease()
    {
        base.OnRelease();
        if (GameManager.player.crew.transform.root != GameManager.player.aircraft.transform) return;
        Vector3 camLocal = bailOutPoint.InverseTransformPoint(Camera.main.transform.position);
        if (camLocal.y > 0f) GameManager.player.crew.Bailout();
        else SofVrRig.instance.ResetView();
    }

    private void LateUpdate()
    {
        CockpitInteractableUpdate();
    }
}

