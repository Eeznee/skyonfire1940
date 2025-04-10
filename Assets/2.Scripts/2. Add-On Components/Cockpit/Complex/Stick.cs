using UnityEngine;
using UnityEngine.InputSystem;
public class Stick : CockpitInteractable
{
    public Transform pitch;
    public Transform roll;
    public float maxPitch = 10f;
    public float maxRoll = 10f;

    public Vector3 pitchAxis = new Vector3(1f,0f,0f);
    public Vector3 rollAxis = new Vector3(0f, 0f, 1f);

    /*
    protected override void VRInteraction(Vector3 gripPos, Quaternion gripRot)
    {
        float pitchAngle = Mathv.OptimalLeverRotation(pitch, gripPos, pitchAxis, pitch.parent.up);
        float rollAngle = Mathv.OptimalLeverRotation(roll, gripPos, rollAxis, roll.parent.up);

        AircraftAxes axes = AircraftAxes.zero;
        axes.pitch = Mathf.Clamp(-pitchAngle / maxPitch, -1f, 1f);
        axes.roll = Mathf.Clamp(-rollAngle / maxRoll, -1f, 1f);
        axes.yaw = -SofVrRig.instance.Stick(xrGrab).x;
        aircraft.controls.SetTargetInput(axes, PitchCorrectionMode.Clamped);

        aircraft.controls.brake = SofVrRig.instance.Stick(xrGrab).y;

        bool secondaryFire = SofVrRig.instance.Trigger(xrGrab) > 0.9f || SofVrRig.instance.PrimaryButton(xrGrab);
        bool primaryFire = SofVrRig.instance.Trigger(xrGrab) > 0.1f;
        if (primaryFire) aircraft.armament.FirePrimaries();
        if (secondaryFire) aircraft.armament.FireSecondaries();
    }
    */
    private void Update()
    {
        CockpitInteractableUpdate();
        if (!aircraft) return;
        pitch.localRotation = Quaternion.AngleAxis(-aircraft.controls.current.pitch * maxPitch, pitchAxis);
        roll.localRotation = Quaternion.AngleAxis(-aircraft.controls.current.roll * maxRoll, rollAxis);
    }
}

