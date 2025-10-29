using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.XR.Interaction.Toolkit;
//using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.SceneManagement;

//[RequireComponent(typeof(InputActionManager))]
public class SofVrRig : MonoBehaviour
{
    public static SofVrRig instance;
    //public XRGrabInteractable xrGrab;
    //private XRActions actions;

    //public XRDirectInteractor rightHand;
    //public XRDirectInteractor rightIndex;
    //public XRDirectInteractor leftHand;
    //public XRDirectInteractor leftIndex;

    [HideInInspector] public CockpitInteractable rightHandTarget;
    [HideInInspector] public CockpitInteractable leftHandTarget;

    [SerializeField] private HandGrip xrRightGrip;
    [SerializeField] private HandGrip xrLeftGrip;
    [HideInInspector] public HandGrip rightHandGrip;
    [HideInInspector] public HandGrip leftHandGrip;

    [HideInInspector] public Vector3 rightHandDelta;
    [HideInInspector] public Vector3 leftHandDelta;
    private Vector3 previousRightHandPos = Vector3.zero;
    private Vector3 previousLeftHandPos = Vector3.zero;

    private SofObject currentObject;
    /*
public static void EnableVR(SofObject obj)
{
    CockpitInteractable[] interactables = obj.GetComponentsInChildren<CockpitInteractable>();
    foreach (CockpitInteractable c in interactables) c.EnableVR(instance.xrGrab);
    instance.actions.Enable();
}
public static void DisableVR(SofObject obj)
{
    CockpitInteractable[] interactables = obj.GetComponentsInChildren<CockpitInteractable>();
    foreach (CockpitInteractable c in interactables) c.DisableVR();
    instance.actions.Disable();
}

public float Grip(XRGrabInteractable grab)
{
    IXRInteractable ixr = grab;
    if (ixr == rightHand.firstInteractableSelected) return actions.RightHand.GripValue.ReadValue<float>();
    if (ixr == leftHand.firstInteractableSelected) return actions.LeftHand.GripValue.ReadValue<float>();
    return 0f;
}
public float Trigger(XRGrabInteractable grab)
{
    IXRInteractable ixr = grab;
    if (ixr == rightHand.firstInteractableSelected) return actions.RightHand.TriggerValue.ReadValue<float>();
    if (ixr == leftHand.firstInteractableSelected) return actions.LeftHand.TriggerValue.ReadValue<float>();
    return 0f;
}
public bool PrimaryButton(XRGrabInteractable grab)
{
    IXRInteractable ixr = grab;
    if (ixr == rightHand.firstInteractableSelected) return actions.RightHand.ButtonIn.ReadValue<float>() > 0.5f;
    if (ixr == leftHand.firstInteractableSelected) return actions.LeftHand.ButtonIn.ReadValue<float>() > 0.5f;
    return false;
}
public bool SecondaryButton(XRGrabInteractable grab)
{
    IXRInteractable ixr = grab;
    if (ixr == rightHand.firstInteractableSelected) return actions.RightHand.ButtonOut.ReadValue<float>() > 0.5f;
    if (ixr == leftHand.firstInteractableSelected) return actions.LeftHand.ButtonOut.ReadValue<float>() > 0.5f;
    return false;
}
public Vector2 Stick(XRGrabInteractable grab)
{
    IXRInteractable ixr = grab;
    if (ixr == rightHand.firstInteractableSelected) return actions.RightHand.Stick.ReadValue<Vector2>();
    if (ixr == leftHand.firstInteractableSelected) return actions.LeftHand.Stick.ReadValue<Vector2>();
    return Vector2.zero;
}

    private void Awake()
    {
        GetReferences();
        actions.LeftHand.Menu.performed += _ => SceneManager.LoadScene("MainMenu");
    }
    private void GetReferences()
    {
        instance = this;

        if (actions == null)
            actions = new XRActions();
    }
    */
    /**
    private void OnEnable()
    {
        Player.OnSeatChange += ResetPlayer;
    }
    private void OnDisable()
    {
        Player.OnSeatChange -= ResetPlayer;
    }
    private void ResetPlayer()
    {
        ResetView();
        if (!currentObject || currentObject != Player.sofObj)
        {
            if (currentObject) DisableVR(currentObject);
            currentObject = Player.sofObj;
            EnableVR(currentObject);
        }
    }

    private void Update()
    {
        rightHandDelta = transform.TransformDirection(rightHand.transform.localPosition - previousRightHandPos);
        previousRightHandPos = rightHand.transform.localPosition;
        leftHandDelta = transform.TransformDirection(leftHand.transform.localPosition - previousLeftHandPos);
        previousLeftHandPos = leftHand.transform.localPosition;

        rightHandGrip = rightHandTarget ? rightHandTarget.CurrentGrip() : xrRightGrip;
        leftHandGrip = leftHandTarget ? leftHandTarget.CurrentGrip() : xrLeftGrip;
    }

    public void ResetView()
    {
        GetReferences();
        Transform pov = Player.seat.defaultPOV;
        transform.parent = pov;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        rightHandGrip = xrRightGrip;
        leftHandGrip = xrLeftGrip;
    }
            */
    /*
    public void HandGrab(SelectEnterEventArgs args)
    {
        XRDirectInteractor xrObj = (XRDirectInteractor)args.interactorObject;
        if (xrObj == rightHand || xrObj == rightIndex)
        {
            rightHandTarget = args.interactableObject.colliders[0].GetComponentInParent<CockpitInteractable>();
            rightHand.enabled = xrObj == rightHand;
            rightIndex.enabled = xrObj == rightIndex;
        }
        if (xrObj == leftHand || xrObj == leftIndex)
        {
            leftHandTarget = args.interactableObject.colliders[0].GetComponentInParent<CockpitInteractable>();
            leftHand.enabled = xrObj == leftHand;
            leftIndex.enabled = xrObj == leftIndex;
        }
    }
    public void HandRelease(SelectExitEventArgs args)
    {
        XRDirectInteractor xrObj = (XRDirectInteractor)args.interactorObject;
        if (xrObj == rightHand || xrObj == rightIndex)
        {
            rightHandTarget = null;
            rightHand.enabled = rightIndex.enabled = true;
        }
        if (xrObj == leftHand || xrObj == leftIndex)
        {
            leftHandTarget = null;
            leftHand.enabled = leftIndex.enabled = true;
        }
    }
    */
}
