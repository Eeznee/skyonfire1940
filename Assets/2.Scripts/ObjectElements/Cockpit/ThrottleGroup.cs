using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ThrottleGroup : CockpitInteractable
{
    public CockpitInteractable[] linked;
    public float minAngle = -20f;
    public float maxAngle = 20f;
    public Vector3 axis = new Vector3(1f, 0f, 0f);

    protected override void VRInteraction(Vector3 gripPos, Quaternion gripRot)
    {
        base.VRInteraction(gripPos, gripRot);
        float angle = Mathv.OptimalLeverRotation(transform, gripPos, axis, transform.parent.up);
        aircraft.SetThrottle(Mathf.Clamp(Mathf.InverseLerp(minAngle, maxAngle, angle), -1f, 1f));
    }
    protected override void Animate()
    {
        
    }

    private void Update()
    {
        CockpitInteractableUpdate();
    }
}
