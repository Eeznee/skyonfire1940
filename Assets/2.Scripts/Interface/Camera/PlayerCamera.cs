using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
public class PlayerCamera : MonoBehaviour
{
    //References
    public static PlayerCamera instance;
    public static Transform camTr;
    public Camera cam;
    public CustomCam[] customs;
    public static CustomCam customCam;
    public static Vector3 camPos;
    public static Plane[] frustrumPlanes;

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

    //Smoothing
    const float zoomSmoothTime = 0.1f;
    private float zoomVel = 0f;

    //Cam state
    public static int viewMode = 0;
    public static bool dynamic = false;
    public static float fov = 60f;

    public static Vector3 directionInput;           //Aircraft and turret guns will track this direction when controlled by the player
    public static Quaternion camTarget;

    public static bool zoomed;
    void Awake()
    {
        touchSensitivity = PlayerPrefs.GetFloat("TouchSensitivity", 1f);
        sensPilot = PlayerPrefs.GetFloat("PilotSensitivity", sensPilot);
        sensGunner = PlayerPrefs.GetFloat("GunnerSensitivity", sensGunner);

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
        SetView(0);
        customCam = external;
    }
    private void Start()
    {
        Actions actions = PlayerActions.instance.actions;
        Actions.GeneralActions general = actions.General;
        general.Aim.performed += _ => Zoom();
        general.ZoomAxis.performed += zoom => SetFov(Mathf.InverseLerp(-1f, 1f, zoom.ReadValue<float>()));
        general.LookAround.canceled += _ => camTarget = Quaternion.LookRotation(directionInput, Vector3.up);
        general.ResetCamera.performed += _ => TryResetView();
        actions.Pilot.Dynamic.performed += _ => dynamic = !dynamic;
        general.ToggleViewMode.performed += _ => SetView(viewMode == 0 ? 1 : 0);
        general.CustomCam1.performed += _ => SetView(-1);
        general.CustomCam2.performed += _ => SetView(-2);
        general.CustomCam3.performed += _ => SetView(-3);
        general.CustomCam4.performed += _ => SetView(-4);
        general.CustomCam5.performed += _ => SetView(-5);
        general.CustomCam6.performed += _ => SetView(-6);
    }
    private void Zoom()
    {
        zoomed = !zoomed;
        fov = zoomed ? zoomedFieldOfView : fieldOfView;
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
        Vector2 cameraInput = Sensitivity() * PlayerActions.instance.actions.General.Camera.ReadValue<Vector2>() / 10f;
        if (inverted) cameraInput = -cameraInput;

        UpdateGameCams();

        cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, fov,ref zoomVel,zoomSmoothTime,Mathf.Infinity, Time.unscaledDeltaTime);

        CustomCam ccam = customCam;
        bool lookAround = true;
#if !MOBILE_INPUT
        if (GameManager.gameUI != GameUI.Game && PlayerActions.instance.actions.General.UnlockCamera.ReadValue<float>() < 0.5f) lookAround = false;
        float zoomRelativeInput = PlayerActions.instance.actions.General.ZoomRelativeAxis.ReadValue<float>() * Time.unscaledDeltaTime * 0.2f;
        SetFov(zoomRelativeInput + FovFactor());
#endif

        if (GameManager.seatInterface == SeatInterface.Bombardier) {    //The bombsight case
            SofAircraft pa = PlayerManager.player.aircraft;
            ccam = bombSight;
            bombSight.relativePos = pa.transform.InverseTransformPoint(pa.bombSight.zoomedPOV.position);
            cam.fieldOfView = pa.bombSight.fov;
            camTarget = DefaultRotation(ccam);
            lookAround = false;
        }
        else
        {
            if (!lookAround) cameraInput = Vector2.zero;
            if (ccam.WorldLookAround()) WorldLookAround(cameraInput, DefaultRotation(ccam));
            else RelativeLookAround(cameraInput, DefaultRotation(ccam));
        }

        if (ccam.Player() != PlayerManager.player.sofObj && ccam.Player() == null) PlayerManager.PlayerNull();
        else if (ccam.Player() != PlayerManager.player.sofObj) PlayerManager.SetPlayer(ccam.Player(),GameManager.seatInterface == SeatInterface.Bombardier);

        //Rotate using Cam Tracker
        if (ccam.smooth) transform.rotation = Mathv.Damp(transform.rotation, camTarget, SmoothSpeed());
        else transform.rotation = camTarget;

        //Position
        transform.position = Position(ccam);
        camTr.localPosition = new Vector3(0f, ccam.height, -ccam.distance);

        camPos = camTr.position;
        //Prevents ground clipping
        if (camPos.y - GameManager.map.HeightAtPoint(camPos) < ccam.height + ccam.distance)
        {
            LayerMask mask = LayerMask.GetMask("Terrain", "Default", "Water");
            float dis = new Vector2(ccam.height, ccam.distance).magnitude + 0.05f;
            if (Physics.Raycast(transform.position, camTr.position - transform.position, out RaycastHit hit, dis, mask))
            {
                camTr.position = hit.point + camTr.forward * 0.05f + Vector3.up * 0.05f;
            }
        }

        frustrumPlanes = GeometryUtility.CalculateFrustumPlanes(cam);

        //Setup Direction Input
        if (PlayerActions.instance.actions.General.LookAround.ReadValue<float>() < 0.5f) directionInput = camTarget * Vector3.forward;
        else if (PlayerActions.instance.actions.Pilot.Pitch.phase == InputActionPhase.Started) directionInput = PlayerManager.player.tr.forward;
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
                CrewMember crew = viewMode > 0 ? PlayerManager.player.crew : cam.posTarget.crew[0];
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
                return PlayerManager.player.crew.Seat().DefaultDirection();
            case CamDirection.Bombsight:
                return PlayerManager.player.aircraft.bombSight.zoomedPOV.forward;
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
        camTarget = Quaternion.LookRotation(camTarget * Vector3.forward, up);
        camTarget *= Quaternion.Euler(Vector3.up * inputs.x);
        camTarget *= Quaternion.Euler(Vector3.right * inputs.y);
    }
    private void RelativeLookAround(Vector2 inputs, Quaternion defaultRotation)
    {
        if (GameManager.gameUI == GameUI.PauseMenu) return;
        float yLimit = customCam.pos == CamPosition.FirstPerson ? 84f : 180f;
        rotations.x += Mathf.Sign(rotations.y - 90f) * -inputs.x;
        rotations.y = Mathf.Clamp(rotations.y + inputs.y, -yLimit, yLimit);
        camTarget = defaultRotation;
        camTarget *= Quaternion.Euler(Vector3.up * rotations.x);
        camTarget *= Quaternion.Euler(Vector3.right * rotations.y);
    }
    private void TryResetView()
    {
        if (GameManager.seatInterface == SeatInterface.Pilot && viewMode != 2)
            ResetView(false);
    }
    private void UpdateGameCams()
    {
        SofObject player = PlayerManager.player.sofObj;
        if (player == null) return;
        firstPerson.SetTarget(player);
        external.SetTarget(player);
        external.up = dynamic ? CamUp.ObjectRelative : CamUp.World;
        external.distance = -player.viewPoint.z;
        external.height = player.viewPoint.y;
        bombSight.SetTarget(player);
    }
    public void ResetView(bool instant)
    {
        UpdateGameCams();

        camTarget = DefaultRotation(customCam);
        rotations = Vector2.zero;
        if (instant) instance.transform.rotation = camTarget;
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
        if (GameManager.seatInterface == SeatInterface.Bombardier) return;
        int oldVm = viewMode;
        customCam = CamByViewMode(vm);
        viewMode = vm;
        customCam.Initialize();

        if (viewMode < 0 || oldVm < 0) ResetView(true);
    }
}
