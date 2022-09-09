using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
public class PlayerCamera : MonoBehaviour
{
    //References
    public static PlayerCamera instance;
    public Transform camTr;
    public Camera cam;
    public CustomCam[] customs;
    public static CustomCam customCam;

    //Parameters
    [Header("Distances/FOV")]
    public float externalDistance = 18f;
    public float fieldOfView = 60f;
    public float zoomedFieldOfView = 25f;
    const float minFov = 1f;
    const float maxFov = 85f;
    [Header("Rotation")]
    public static float touchSensitivity = 1f;
    private bool inverted;
    private float sensPilot = 1f;
    private float sensGunner = 0.5f;
    public static float mouseSensitivity = 2f;
    public static float smoothSpeed = 5f;
    private Vector2 rotations;

    CustomCam external;
    CustomCam firstPerson;
    public CustomCam free;
    CustomCam bombSight;

    //Cam state
    public static int viewMode = 0;
    public static bool dynamic = false;
    public static float fov = 60f;

    public static Vector3 directionInput;           //Aircraft and turret guns will track this direction when controlled by the player
    public static Transform camTracker;             //Camera will smoothly rotate to match this transform, local to the player when not using tracking controls
    public static bool Zoomed() { return fov < (instance.fieldOfView + instance.zoomedFieldOfView) * 0.5f; }
    void Awake()
    {
        touchSensitivity = PlayerPrefs.GetFloat("TouchSensitivity", 1f);
        sensPilot = PlayerPrefs.GetFloat("PilotSensitivity", sensPilot);
        sensGunner = PlayerPrefs.GetFloat("GunnerSensitivity", sensGunner);
        camTracker = new GameObject("Camera Tracker").transform;

        Actions actions = GameManager.gm.actions;
        actions.General.Aim.performed += _ => fov = Zoomed() ? fieldOfView : zoomedFieldOfView;
        actions.General.ZoomAxis.performed += zoom => SetFov(Mathf.InverseLerp(-1f, 1f, zoom.ReadValue<float>()));
        actions.General.LookAround.canceled += _ => camTracker.forward = directionInput;
        actions.General.ResetCamera.performed += _ => TryResetView();
        actions.Pilot.Dynamic.performed += _ => dynamic = !dynamic;
        actions.General.ToggleViewMode.performed += _ => SetView(viewMode == 0 ? 1 : 0);
        actions.General.CustomCam1.performed += _ => SetView(-1);
        actions.General.CustomCam2.performed += _ => SetView(-2);
        actions.General.CustomCam3.performed += _ => SetView(-3);
        actions.General.CustomCam4.performed += _ => SetView(-4);
        actions.General.CustomCam5.performed += _ => SetView(-5);
        actions.General.CustomCam6.performed += _ => SetView(-6);

        inverted = PlayerPrefs.GetInt("InvertTouch", 0) == 1;

        dynamic = false;
        rotations = Vector2.zero;
        instance = this;
        cam = Camera.main;
        camTr = cam.transform;

        external = new CustomCam(CamPosition.ObjectRelative, CamDirection.Game, CamUp.ObjectRelative, PlayerIs.PosTarget, true);
        firstPerson = new CustomCam(CamPosition.FirstPerson, CamDirection.Game, CamUp.ObjectRelative, PlayerIs.PosTarget, true);
        free = new CustomCam(CamPosition.Free, CamDirection.Free, CamUp.World, PlayerIs.None, true);
        bombSight = new CustomCam(CamPosition.ObjectRelative, CamDirection.Bombsight,CamUp.ObjectRelative, PlayerIs.PosTarget, false);

        customs = new CustomCam[6];
        viewMode = 0;
        customCam = external;
    }
    private float FovFactor()
    {
        return 1f - Mathf.Log(fov / minFov, 2)/ Mathf.Log(maxFov / minFov, 2);
    }
    private void SetFov(float factor)
    {
        factor = Mathf.Max(0f, factor);
        fov = minFov * Mathf.Pow(2f, Mathf.Log(maxFov / minFov, 2) * (1f-factor));
    }
    void LateUpdate()
    {
#if MOBILE_INPUT
        dynamic = true;
#endif
        Vector2 cameraInput = Sensitivity() * GameManager.gm.actions.General.Camera.ReadValue<Vector2>() / 10f;
        if (inverted) cameraInput = -cameraInput;
        SofAircraft pa = GameManager.ogPlayer.aircraft;

        UpdateGameCams();

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fov, Time.unscaledDeltaTime * 10f);

        CustomCam ccam = customCam;
        bool lookAround = true;
#if !UNITY_MOBILE
        if (GameManager.gameUI != GameUI.Game && !Input.GetKey(KeyCode.Mouse0)) lookAround = false;
        float zoomRelativeInput = GameManager.gm.actions.General.ZoomRelativeAxis.ReadValue<float>() * Time.unscaledDeltaTime * 0.2f;
        SetFov(zoomRelativeInput + FovFactor());
#endif

        if (GameManager.seatInterface == SeatInterface.Bombardier) {    //The bombsight case
            ccam = bombSight;
            bombSight.relativePos = pa.transform.InverseTransformPoint(pa.bombSight.zoomedPOV.position);
            cam.fieldOfView = GameManager.ogPlayer.aircraft.bombSight.fov;
            camTracker.rotation = DefaultRotation(ccam);
            lookAround = false;
        }
        else
        {
            if (!lookAround) cameraInput = Vector2.zero;

            if (ccam.WorldLookAround()) WorldLookAround(cameraInput, DefaultRotation(ccam));
            else RelativeLookAround(cameraInput, DefaultRotation(ccam));
        }

        if (ccam.Player() != GameManager.player.sofObj && ccam.Player() == null) GameManager.PlayerNull();
        else if (ccam.Player() != GameManager.player.sofObj) GameManager.SetPlayer(ccam.Player(),GameManager.seatInterface == SeatInterface.Bombardier);

        //Rotate using Cam Tracker
        if (ccam.smooth) transform.rotation = Mathv.Damp(transform.rotation, camTracker.rotation, SmoothSpeed(), false);
        else transform.rotation = camTracker.rotation;

        //Position
        transform.position = Position(ccam);
        camTr.localPosition = new Vector3(0f, ccam.height, -ccam.distance);

        //Prevents ground clipping
        LayerMask mask = LayerMask.GetMask("Terrain", "Default", "Water");
        float dis = new Vector2(ccam.height, ccam.distance).magnitude + 0.05f;
        if (Physics.Raycast(transform.position, camTr.position - transform.position, out RaycastHit hit, dis, mask))
            camTr.position = hit.point + camTr.forward * 0.05f + Vector3.up * 0.05f;

        //Setup Direction Input
        if (GameManager.gm.actions.General.LookAround.ReadValue<float>() < 0.5f) directionInput = camTracker.forward;
        else if (GameManager.gm.actions.Pilot.Pitch.phase == InputActionPhase.Started) directionInput = GameManager.player.tr.forward;
    }
    public float SmoothSpeed()
    {
        return customCam.pos == CamPosition.FirstPerson ? smoothSpeed * 2f : smoothSpeed;
    }
    public float Sensitivity()
    {
        float sens = mouseSensitivity * cam.fieldOfView / fieldOfView;
        bool gunner = GameManager.seatInterface == SeatInterface.Gunner;
        sens *= gunner ? sensGunner : sensPilot;
        return sens;
    }
    private Vector3 Position(CustomCam cam)
    {
        switch (cam.pos)
        {
            case CamPosition.FirstPerson:
                CrewMember crew = viewMode > 0 ? GameManager.player.crew : cam.posTarget.crew[0];
                return crew.transform.position + crew.Seat().headLookDirection * 0.05f;
            case CamPosition.ObjectRelative:
                return cam.posTarget.transform.TransformPoint(cam.relativePos);
            case CamPosition.FlatRelative:
                return cam.posTarget.transform.position + cam.relativePos;
            case CamPosition.Free:
                return GameManager.gm.mapTr.TransformPoint(cam.worldPos);
        }
        return Vector3.zero;
    }
    private Vector3 Up(CustomCam cam)
    {
        switch (cam.up)
        {
            case CamUp.World: return Vector3.up;
            case CamUp.ObjectRelative:
                SofObject target;
                if (cam.pos == CamPosition.Free) target = cam.dirTarget;
                else target = cam.posTarget;
                return target ? target.transform.up : Vector3.up;
        }
        return Vector3.up;
    }
    private Vector3 Direction(CustomCam cam)
    {
        switch (cam.dir)
        {
            case CamDirection.LookAt:
                return cam.dirTarget.transform.position - Position(cam);
            case CamDirection.ObjectRelative:
                return cam.dirTarget.transform.forward;
            case CamDirection.Free:
                return camTr.forward;
            case CamDirection.Game:
                return GameManager.ogPlayer.crew.Seat().DefaultDirection();
            case CamDirection.Bombsight:
                return GameManager.ogPlayer.aircraft.bombSight.zoomedPOV.forward;
        }
        return Vector3.forward;
    }
    private Quaternion DefaultRotation(CustomCam cam)
    {
        return Quaternion.LookRotation(Direction(cam), Up(cam)) * Quaternion.Euler(0f,0f,cam.tilt);
    }
    private void WorldLookAround(Vector2 inputs, Quaternion defaultRotation)
    {
        if (GameManager.gameUI == GameUI.PauseMenu) return;
        Vector3 up = defaultRotation * Vector3.up;
        //if (Vector3.Angle(camTracker.forward, up) < 10f) up = camTracker.up;
        camTracker.rotation = Quaternion.LookRotation(camTracker.forward, up);
        camTracker.Rotate(Vector3.up * inputs.x);
        camTracker.Rotate(Vector3.right * inputs.y);
    }
    private void RelativeLookAround(Vector2 inputs, Quaternion defaultRotation)
    {
        if (GameManager.gameUI == GameUI.PauseMenu) return;
        float yLimit = customCam.pos == CamPosition.FirstPerson ? 84f : 180f;
        rotations.x += Mathf.Sign(rotations.y - 90f) * -inputs.x;
        rotations.y = Mathf.Clamp(rotations.y + inputs.y, -yLimit, yLimit);
        camTracker.rotation = defaultRotation;
        camTracker.Rotate(Vector3.up * rotations.x);
        camTracker.Rotate(Vector3.right * rotations.y);
    }
    private void TryResetView()
    {
        if (GameManager.seatInterface == SeatInterface.Pilot && viewMode != 2)
            ResetView(false);
    }
    private void UpdateGameCams()
    {
        firstPerson.SetTarget(GameManager.ogPlayer.sofObj);
        external.SetTarget(GameManager.ogPlayer.sofObj);
        external.up = dynamic ? CamUp.ObjectRelative : CamUp.World;
        external.distance = -GameManager.ogPlayer.sofObj.viewPoint.z;
        external.height = GameManager.ogPlayer.sofObj.viewPoint.y;
        bombSight.SetTarget(GameManager.ogPlayer.sofObj);
    }
    public void ResetView(bool instant)
    {
        UpdateGameCams();

        camTracker.rotation = DefaultRotation(customCam);
        rotations = Vector2.zero;
        if (instant) instance.transform.rotation = camTracker.rotation;
    }
    private CustomCam CamByViewMode(int vm)
    {
        switch (vm)
        {
            case 0: return external;
            case 1: return firstPerson;
            case 2: return free;
            case 3: return bombSight;
            default: int i = -vm - 1;
                if (customs[i] == null) customs[i] = new CustomCam(-vm);
                return customs[i];
        }
    }
    public void SetView(int vm) //Negative are the custom cams, 0 is external, 1 is first person, 2 is free
    {
        int oldVm = viewMode;
        customCam = CamByViewMode(vm);
        viewMode = vm;
        customCam.Initialize();

        if (GameManager.seatInterface == SeatInterface.Bombardier)
        {
            if (customCam.Player() == null) GameManager.PlayerNull();
            else GameManager.SetPlayer(customCam.Player(), GameManager.seatInterface == SeatInterface.Bombardier);
        }

        if (viewMode < 0 || oldVm < 0) ResetView(true);
    }
}
