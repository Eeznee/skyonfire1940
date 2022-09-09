using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class SofVrRig : MonoBehaviour
{
    //References
    public static SofVrRig instance;
    private SofAircraft aircraft;
    private CrewMember crew;
    private Transform camTr;
    private Transform aircraftTr;
    private Transform crewTr;
    private XRRig rig;

    public InputActionProperty rightGripButton;
    public InputActionProperty rightTriggerButton;
    public InputActionProperty rightPrimaryButton;
    public InputActionProperty rightSecondaryButton;

    public InputActionProperty leftGripButton;
    public InputActionProperty leftTriggerButton;
    public InputActionProperty leftPrimaryButton;
    public InputActionProperty leftSecondaryButton;

    public XRDirectInteractor rightHand;
    public XRDirectInteractor rightIndex;
    public XRDirectInteractor leftHand;
    public XRDirectInteractor leftIndex;
    [SerializeField] private HandGrip xrRightGrip;
    [SerializeField] private HandGrip xrLeftGrip;
    [HideInInspector] public HandGrip rightHandGrip;
    [HideInInspector] public HandGrip leftHandGrip;
    [HideInInspector] public XRBaseInteractable rightHandTarget;
    [HideInInspector] public XRBaseInteractable leftHandTarget;
    [HideInInspector] public XRBaseInteractable rightIndexTarget;
    [HideInInspector] public XRBaseInteractable leftIndexTarget;
    [HideInInspector] public Vector3 rightHandDelta;
    [HideInInspector] public Vector3 leftHandDelta;
    private Vector3 previousRightHandPos = Vector3.zero;
    private Vector3 previousLeftHandPos = Vector3.zero;


    public float Grip(XRGrabInteractable grab)
    {
        if (grab == rightHand.selectTarget) return Grip(true);
        if (grab == leftHand.selectTarget) return Grip(false);
        return 0f;
    }
    public float Grip(bool right)
    {
        if (right) return rightGripButton.action.ReadValue<float>();
        return leftGripButton.action.ReadValue<float>();
    }
    public float Trigger(XRGrabInteractable grab)
    {
        if (grab == rightHand.selectTarget) return Trigger(true);
        if (grab == leftHand.selectTarget) return Trigger(false);
        return 0f;
    }
    public float Trigger(bool right)
    {
        if (right) return rightTriggerButton.action.ReadValue<float>();
        return leftTriggerButton.action.ReadValue<float>();
    }
    public bool PrimaryButton(XRGrabInteractable grab)
    {
        if (grab == rightHand.selectTarget) return PrimaryButton(true);
        if (grab == leftHand.selectTarget) return PrimaryButton(false);
        return false;
    }
    public bool PrimaryButton(bool right)
    {
        if (right) return rightPrimaryButton.action.ReadValue<float>() > 0.5f;
        return rightPrimaryButton.action.ReadValue<float>() > 0.5f;
    }
    public bool SecondaryButton(XRGrabInteractable grab)
    {
        if (grab == rightHand.selectTarget) return SecondaryButton(true);
        if (grab == leftHand.selectTarget) return SecondaryButton(false);
        return false;
    }
    public bool SecondaryButton(bool right)
    {
        if (right) return rightSecondaryButton.action.ReadValue<float>() > 0.5f;
        return leftSecondaryButton.action.ReadValue<float>() > 0.5f;
    }

    void Awake()
    {
        GetReferences();
    }
    private XRBaseInteractable GetTarget(XRDirectInteractor interactor)
    {
        List<XRBaseInteractable> targets = new List<XRBaseInteractable>();
        interactor.GetValidTargets(targets);
        return targets.Count > 0 ? targets[0] : null;
    }
    void GetReferences()
    {
        instance = this;
        rig = GetComponent<XRRig>();
        camTr = Camera.main.transform;
        aircraft = GameManager.player.aircraft;
        if (aircraft) aircraftTr = aircraft.transform;
        crew = GameManager.player.crew;
        if (crew) crewTr = crew.transform;
        rightHandTarget = GetTarget(rightHand);
        leftHandTarget = GetTarget(leftHand);
        rightIndexTarget = GetTarget(rightIndex);
        leftIndexTarget = GetTarget(leftIndex);
    }
    void Update()
    {
        GetReferences();
        rightHandDelta = transform.TransformDirection(rightHand.transform.localPosition - previousRightHandPos);
        previousRightHandPos = rightHand.transform.localPosition;
        leftHandDelta = transform.TransformDirection(leftHand.transform.localPosition - previousLeftHandPos);
        previousLeftHandPos = leftHand.transform.localPosition;
    }
    public void HandGrab(SelectEnterEventArgs args)
    {
        HandGrip grip = args.interactable.colliders[0].GetComponent<HandGrip>();

        if (args.interactor == rightHand || args.interactor == rightIndex)
            rightHandGrip = grip;
        if (args.interactor == leftHand || args.interactor == leftIndex)
            leftHandGrip = grip;
    }
    public void HandRelease(SelectExitEventArgs args)
    {
        if (args.interactor == rightHand || args.interactor == rightIndex)
            rightHandGrip = xrRightGrip;
        if (args.interactor == leftHand || args.interactor == leftIndex)
            leftHandGrip = xrLeftGrip;
    }
    public void ResetView()
    {
        GetReferences();
        Transform pov = crew.seats[crew.currentSeat].defaultPOV;
        transform.parent = pov;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        rightHandGrip = xrRightGrip;
        leftHandGrip = xrLeftGrip;
    }
}
