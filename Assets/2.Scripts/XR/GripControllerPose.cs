using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(HandGrip))]
public class GripControllerPose : MonoBehaviour
{
    public InputActionProperty grip;
    public InputActionProperty triggerTouched;
    public InputActionProperty trigger;
    public InputActionProperty thumbInTouched;
    public InputActionProperty thumbOutTouched;
    

    HandGrip handGrip;

    void Start()
    {
        handGrip = GetComponent<HandGrip>();
    }
    //Fine is 0.7 Trigger 0.7 thumb Down 1.0 Thumb In
    void Update()
    {
        float thumbTouched = Mathf.Clamp01(thumbInTouched.action.ReadValue<float>() + thumbOutTouched.action.ReadValue<float>());
        handGrip.grip = grip.action.ReadValue<float>();
        handGrip.trigger = triggerTouched.action.ReadValue<float>() * 0.55f + trigger.action.ReadValue<float>() * 0.45f;
        handGrip.thumbIn = thumbInTouched.action.ReadValue<float>();
        handGrip.thumbDown = thumbTouched * Mathf.Lerp(0.7f, 1f, trigger.action.ReadValue<float>());
    }
}
