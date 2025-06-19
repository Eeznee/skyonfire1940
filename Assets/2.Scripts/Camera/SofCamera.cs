using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class SofCamera : MonoBehaviour
{
    public static Transform tr { get; private set; }
    public static Camera cam { get; private set; }
    public static SubCam subCam { get; private set; }
    public static SubCam[] subCams { get; private set; }
    public static int SubCamID(int vm) { return vm < 0 ? -vm + 3 : vm; }
    public static SubCam GetSubCam(int vm) { return subCams[SubCamID(vm)]; }
    public static int viewMode { get; private set; }
    private static int previousViewMode;

    public static Quaternion desiredRotation { get; private set; }
    public static Vector3 directionInput { get; private set; }         //Aircraft and turret guns will track this direction when controlled by the player
    public static Vector3 CurrentDirection => desiredRotation * Vector3.forward;
    public static bool lookAround { get; private set; }
    public static Vector3 Velocity { get; private set; }
    public static Vector3 PositionDelta { get; private set; }

    private static Vector2 axis;
    private static Vector2 savedAxis;

    const float smoothDampSpeed = 10f;

    public static event Action OnSwitchCamEvent;

    private void Start()
    {
        tr = transform;
        cam = GetComponent<Camera>();

        subCam = null;
        subCams = new SubCam[10];
        subCams[0] = new SubCam(new ThirdPersonGameCam(), true, false);
        subCams[1] = new SubCam(new FirstPersonGameCam(), true , false);
        subCams[2] = new SubCam(new FreeCam(), false , false);
        subCams[3] = new SubCam(new BombSightCam(), false, false);

        axis = Vector2.zero;
        lookAround = false;
        viewMode = previousViewMode = 0;

        ResetCamera();
    }
    private void OnEnable() { 
        Player.OnSeatChange += ResetCamera; 
        Player.OnSeatChange += ResetRotation;
        Player.OnCrewChange += ResetRotationInstant; 
    }
    private void OnDisable() { 
        Player.OnSeatChange -= ResetCamera; 
        Player.OnSeatChange -= ResetRotation;
        Player.OnCrewChange -= ResetRotationInstant; 
    }
    private void LateUpdate()
    {
        Vector3 previousPos = transform.position;

        if (subCam.Offset() == Vector3.zero) tr.position = subCam.Position();
        if (UIManager.gameUI != GameUI.Pause)
            desiredRotation = subCam.Rotation(ref axis, desiredRotation);
        tr.rotation = subCam.smooth ? Mathv.Damp(tr.rotation, desiredRotation, smoothDampSpeed, Mathf.Infinity) : desiredRotation;
        if (subCam.Offset() != Vector3.zero) tr.position = subCam.Position();

        if (!lookAround) directionInput = desiredRotation * Vector3.forward;
        else if (PlayerActions.forceCameraPointDirection && Player.role == SeatRole.Pilot) directionInput = Player.tr.forward;

        PositionDelta = transform.position - previousPos;
        Velocity = PositionDelta / Time.unscaledDeltaTime;
    }
    public static void StartLookAround() { lookAround = true; savedAxis = axis; }
    public static void StopLookAround() {
        lookAround = false;
        if (UIManager.gameUI == GameUI.Pause) return;
        desiredRotation = Quaternion.LookRotation(directionInput, subCam.logic.BaseUp());
        axis = savedAxis;
    }
    public static void ResetRotation()
    {
        desiredRotation = subCam.logic.BaseRotation();
        axis = Vector2.zero;
        savedAxis = Vector2.zero;
    }
    public static void ResetRotationInstant()
    {
        ResetRotation();
        tr.rotation = desiredRotation;
    }
    public static void ResetCamera() { SwitchViewMode(viewMode); }
    public static void SwitchViewMode(int vm)
    {
        if (UIManager.gameUI == GameUI.PhotoMode) vm = 2;
        if (Player.role == SeatRole.Bombardier) vm = 3;
        else if (vm == 3) vm = previousViewMode == 1 ? 1 : 0;

        if (vm != viewMode || subCam == null)
        {
            previousViewMode = viewMode;
            viewMode = vm;
            subCam = GetSubCam(vm);
            if (subCam == null) { subCam = new SubCam(-vm); subCams[SubCamID(vm)] = subCam; }

            OnSwitchCamEvent?.Invoke();

            Log.Print("Camera Switch To " + subCam.logName, "Camera Mode");
        }

        bool resetRotation = GetSubCam(viewMode).logic.BaseDirMode != GetSubCam(previousViewMode).logic.BaseDirMode;
        resetRotation |= viewMode == previousViewMode;
        resetRotation &= viewMode != 2;
        if (resetRotation) ResetRotationInstant();

        subCam.OnCameraSwitchedToThis();
    }
}
