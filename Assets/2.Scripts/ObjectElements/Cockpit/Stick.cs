using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
public class Stick : CockpitInteractable
{
    public Transform pitch;
    public Transform roll;
    public InputActionProperty rudderBrake;
    public float maxPitch = 10f;
    public float maxRoll = 10f;

    public Vector3 pitchAxis = new Vector3(1f,0f,0f);
    public Vector3 rollAxis = new Vector3(0f, 0f, 1f);
    protected override void VRInteraction(Vector3 gripPos, Quaternion gripRot)
    {
        Vector3 axis = Vector3.zero;

        float angle = Mathv.OptimalLeverRotation(pitch, gripPos, pitchAxis, pitch.parent.up);
        axis.x = Mathf.Clamp(-angle / maxPitch, -1f, 1f);

        angle = Mathv.OptimalLeverRotation(roll, gripPos, rollAxis, roll.parent.up);
        axis.z = Mathf.Clamp(-angle / maxRoll, -1f, 1f);

        axis.y = -rudderBrake.action.ReadValue<Vector2>().x;
        aircraft.brake = Mathf.Abs(rudderBrake.action.ReadValue<Vector2>().y);

        aircraft.SetControls(axis,false, false);

        bool secondaryFire = SofVrRig.instance.Trigger(xrGrip) > 0.9f || SofVrRig.instance.PrimaryButton(xrGrip);
        bool primaryFire = SofVrRig.instance.Trigger(xrGrip) > 0.1f;
        if (primaryFire) aircraft.FirePrimaries();
        if (secondaryFire) aircraft.FireSecondaries();
    }
    protected override void Animate()
    {
        if (!aircraft) return;
        pitch.localRotation = Quaternion.AngleAxis(-aircraft.controlInput.x * maxPitch, pitchAxis);
        roll.localRotation = Quaternion.AngleAxis(-aircraft.controlInput.z * maxRoll, rollAxis);
    }
    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
    }

    private void Update()
    {
        CockpitInteractableUpdate();
    }
}

