using UnityEngine;
using UnityEngine.InputSystem;
public class CockpitButton : CockpitInteractable
{
    Vector3 openedPos;
    public float maxDistance = 0.02f;
    public bool toggle = false;

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        openedPos = defaultPos + transform.parent.InverseTransformDirection(-transform.forward * maxDistance);
    }
    protected override void VRInteraction(Vector3 gripPos,Quaternion gripRot)
    {
        Vector3 localPos = transform.parent.InverseTransformPoint(gripPos);
        float input = Mathv.InverseLerpVec3(defaultPos, openedPos, localPos);

    }
    protected override void OnRelease()
    {
        base.OnRelease();

    }

    private void Update()
    {
        CockpitInteractableUpdate();
    }
}

