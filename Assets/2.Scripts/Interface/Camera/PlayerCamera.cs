using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    public static Transform tr;
    public static Transform camTr;
    public static Camera cam;
    public static Vector3 camPos;
    public static SubCam[] customs;
    public static SubCam subCam;
    public static SubCam external;
    public static SubCam firstPerson;
    public static SubCam free;
    public static SubCam bombSight;

    public static bool dynamic = true;
    public static bool wasCockpit = false;
    public static int viewMode = 0;
    public static bool lookAround = false;
    public static Quaternion camTarget;
    public static Vector3 directionInput;           //Aircraft and turret guns will track this direction when controlled by the player
    private static Vector2 rotations;
    private static Vector2 lastRotations;

    public static float smoothSpeed = 5f;

    private void Awake()
    {
        tr = transform;
        cam = GetComponentInChildren<Camera>();
        camTr = cam.transform;

        external = new SubCam(CamPosition.ObjectRelative, CamDirection.Game, dynamic, PlayerIs.Auto, true);
        firstPerson = new SubCam(CamPosition.FirstPerson, CamDirection.Game, true, PlayerIs.Auto, true);
        free = new SubCam(CamPosition.Free, CamDirection.Free,false, PlayerIs.None, true);
        bombSight = new SubCam(CamPosition.Bombsight, CamDirection.Bombsight,true, PlayerIs.Auto, false);

        rotations = Vector2.zero;
        viewMode = 0;
        SetView(0);
        customs = new SubCam[6];
    }
    private void Start() { PlayerManager.OnSeatChangeEvent += OnSeatChange; }
    private void LateUpdate()
    {
        SetPlayer();
        SetRotation();
        SetPosition();
    }
    private void SetPlayer()
    {
        SofObject camPlayer = subCam.Player();
        if (camPlayer != PlayerManager.player.sofObj)
        {
            if (camPlayer == null) PlayerManager.PlayerNull();
            else PlayerManager.SetPlayer(camPlayer);
        }
    }
    private void SetRotation()
    {
        camTarget = subCam.Rotation(ref rotations, camTarget);
        if (subCam.smooth) transform.rotation = Mathv.Damp(transform.rotation, camTarget, subCam.pos == CamPosition.FirstPerson ? smoothSpeed * 2f : smoothSpeed);
        else transform.rotation = camTarget;
        if (!lookAround) directionInput = camTarget * Vector3.forward;
        else if (PlayerManager.seatInterface == SeatInterface.Pilot && PlayerActions.General().Pitch.phase == InputActionPhase.Started) directionInput = PlayerManager.player.tr.forward;
    }
    private void SetPosition()
    {
        transform.position = subCam.Position();
        camTr.localPosition = new Vector3(0f, subCam.height, -subCam.distance);
        if (camPos.y - GameManager.map.HeightAtPoint(camTr.position) < subCam.height + subCam.distance) //Ground clipping
        {
            LayerMask mask = LayerMask.GetMask("Terrain", "Default", "Water");
            float dis = new Vector2(subCam.height, subCam.distance).magnitude + 0.05f;
            if (Physics.Raycast(transform.position, camTr.position - transform.position, out RaycastHit hit, dis, mask))
                camTr.position = hit.point + camTr.forward * 0.1f + Vector3.up * 0.1f;
        }
        camPos = camTr.position;
    }
    public static void ToggleDynamic()
    {
        dynamic = !dynamic;
        external.relativeRotation = dynamic;
    }

    public static void ToggleLookAround(bool _lookAround)
    {
        lookAround = _lookAround;
        if (lookAround) lastRotations = rotations;
        else
        {
            camTarget = Quaternion.LookRotation(directionInput, subCam.Up());
            rotations = lastRotations;
        }
    }
    private void OnSeatChange() {
        if (PlayerManager.seatInterface == SeatInterface.Bombardier && viewMode != 3) { SetView(3); return; }
        if (PlayerManager.seatInterface != SeatInterface.Bombardier && viewMode == 3) { SetView(wasCockpit ? 1 : 0); return; }
        ResetView(true);
    }
    public static void ResetView(bool instant)
    {
        external.distance = -PlayerManager.player.sofObj.viewPoint.z;
        external.height = PlayerManager.player.sofObj.viewPoint.y;
        camTarget = subCam.DefaultRotation();
        if (instant) tr.rotation = camTarget;
        rotations = Vector2.zero;
    }
    private static SubCam CamByViewMode(int vm)
    {
        switch (vm)
        {
            case 0: return external;
            case 1: return firstPerson;
            case 2: return free;
            case 3: return bombSight;
            default:
                int i = -vm - 1;
                if (customs[i] == null) customs[i] = new SubCam(-vm);
                return customs[i];
        }
    }
    public static void SetView(int vm)
    {
        int previous = viewMode;
        subCam = CamByViewMode(vm);
        viewMode = vm;
        subCam.Initialize();
        if (vm == 0) wasCockpit = false;
        else if (vm == 1) wasCockpit = true;

        if ((vm == 0 || vm == 1) && previous != 0 && previous != 1) TimeManager.SetPause(TimeManager.paused, GameUI.Game);

        bool reset = viewMode < 0 || previous < 0 || viewMode == 3 || previous == 3;
        if (reset) ResetView(true);
    }
}
