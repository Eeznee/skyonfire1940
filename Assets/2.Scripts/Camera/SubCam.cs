using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum CamPosition
{
    ObjectRelative,
    FlatRelative,
    Free,
    FirstPerson,
    Bombsight
}
public enum CamDirection
{
    ObjectRelative,
    LookAt,
    Free,
    Game,
    Bombsight
}
public enum PlayerIs
{
    PosTarget,
    DirTarget,
    None,
    Auto
}
public class SubCam
{
    public int index = 1;
    public bool smooth = false;
    public SofObject posTarget;
    public SofObject dirTarget;
    public CamPosition pos = CamPosition.Free;
    public CamDirection dir = CamDirection.Free;
    public PlayerIs player;
    public bool relativeRotation;
    public bool freeResetting;

    public Vector3 worldPos = Vector3.zero;
    public Vector3 relativePos = Vector3.zero;
    public float tilt = 0f;
    public float distance = 0f;
    public float height = 0f;
    public SofObject Player()
    {
        switch (player)
        {
            case PlayerIs.PosTarget: return posTarget;
            case PlayerIs.DirTarget: return dirTarget;
            case PlayerIs.Auto: return PlayerManager.player.sofObj;
        }
        return null;
    }
    public Vector3 Position()
    {
        if (player == PlayerIs.Auto) SetTarget(Player());
        switch (pos)
        {
            case CamPosition.FirstPerson:
                return (PlayerCamera.viewMode > 0 ? PlayerManager.player.crew : posTarget.crew[0]).EyesPosition();
            case CamPosition.ObjectRelative:
                return posTarget.transform.TransformPoint(relativePos);
            case CamPosition.FlatRelative:
                return posTarget.transform.position + relativePos;
            case CamPosition.Free:
                return GameManager.gm.mapTr.TransformPoint(worldPos);
            case CamPosition.Bombsight:
                return PlayerManager.player.aircraft.bombSight.zoomedPOV.position;
        }
        return Vector3.zero;
    }
    public Vector3 Up()
    {
        if (relativeRotation)
        {
            SofObject target;
            target = pos == CamPosition.Free ? dirTarget : posTarget;
            return target ? target.transform.up : Vector3.up;
        }
        else return Vector3.up;
    }
    public Vector3 Direction()
    {
        switch (dir)
        {
            case CamDirection.LookAt:
                return dirTarget.transform.position - Position();
            case CamDirection.ObjectRelative:
                return dirTarget.transform.forward;
            case CamDirection.Free:
                return PlayerCamera.camTr.forward;
            case CamDirection.Game:
                return PlayerManager.player.crew.Seat().DefaultDirection();
            case CamDirection.Bombsight:
                return PlayerManager.player.aircraft.bombSight.zoomedPOV.forward;
        }
        return Vector3.forward;
    }
    public Quaternion DefaultRotation()
    {
        if (player == PlayerIs.Auto) SetTarget(Player());
        return Quaternion.LookRotation(Direction(), Up()) * Quaternion.Euler(0f, 0f, tilt);
    }
    public bool RelativeLookAround()
    {
        if (dir == CamDirection.Game && GameManager.Controls() == ControlsMode.Tracking) return false;
        if (dir == CamDirection.Free) return false;
        return true;
    }
    const float minUp = 15f;
    public Quaternion Rotation(ref Vector2 rotations, Quaternion current)
    {
        if (GameManager.gameUI == GameUI.PauseMenu) return current;

        Vector2 inputs = CamInputs.CameraInput();
        Quaternion defaultRotation = DefaultRotation();
        if (dir == CamDirection.Bombsight) return defaultRotation;
        if (RelativeLookAround())
        {
            float yLimit = pos == CamPosition.FirstPerson ? 84f : 180f;
            rotations.x += Mathf.Sign(90f - Mathf.Abs(rotations.y)) * inputs.x;
            rotations.y = Mathf.Clamp(rotations.y + inputs.y, -yLimit, yLimit);
            current = defaultRotation;
            current *= Quaternion.Euler(Vector3.up * rotations.x);
            current *= Quaternion.Euler(Vector3.right * rotations.y);
            return current;
        }
        else
        {
            Vector3 up = defaultRotation.Up();
            float upAngle = Vector3.Angle(up, current.Forward());

            if (upAngle < minUp || upAngle > 180f - minUp) up = current.Up();
            else
            {
                bool backwards = Vector3.Angle(up, current.Up()) > 90f;
                if (!PlayerCamera.lookAround && Vector3.Angle(defaultRotation.Forward(), current.Forward()) < 90f) backwards = false;
                if (backwards) up = -up;
            }
            current = Quaternion.LookRotation(current * Vector3.forward, up);
            current *= Quaternion.Euler(Vector3.forward * inputs.x * Mathf.Cos(upAngle * Mathf.Deg2Rad));
            current *= Quaternion.Euler(Vector3.up * inputs.x * Mathf.Sin(upAngle * Mathf.Deg2Rad));
            current *= Quaternion.Euler(Vector3.right * inputs.y);
            return current;
        }
    }
    private SofAircraft LoadAircraft(string squadTag, string wingTag)
    {
        int squad = PlayerPrefs.GetInt(squadTag, 0);
        int wing = PlayerPrefs.GetInt(wingTag, 0);
        if (GameManager.squadrons.Count > squad && GameManager.squadrons[squad].Length > wing) return GameManager.squadrons[squad][wing];
        return GameManager.squadrons[0][0];
    }
    public SubCam (CamPosition _pos, CamDirection _dir, bool _relativeRotation, PlayerIs _player, bool _smooth)
    {
        pos = _pos;
        dir = _dir;
        relativeRotation = _relativeRotation;
        player = _player;
        smooth = _smooth;
    }
    public SubCam(int i)
    {
        index = i;

        pos = (CamPosition)PlayerPrefs.GetInt("camPosition" + i, 2);
        dir = (CamDirection)PlayerPrefs.GetInt("camDirection" + i, 2);
        relativeRotation = PlayerPrefs.GetInt("camRelativeRotation" + i, 1) == 1;
        player = (PlayerIs)PlayerPrefs.GetInt("camPlayer" + i, 0);

        freeResetting = PlayerPrefs.GetInt("freeResetting" + i, index % 2) == 1;
        smooth = PlayerPrefs.GetInt("camSmooth" + i, index % 2) == 1;

        distance = PlayerPrefs.GetFloat("camDistance" + i, 0f);
        height = PlayerPrefs.GetFloat("camHeight" + i, 0f);
        tilt = PlayerPrefs.GetFloat("camTilt" + i, 0f);

        relativePos = Features.PlayerPrefsGetVector3("camRelativePos" + i, Vector3.right * 1.2f);
        worldPos = Features.PlayerPrefsGetVector3("camWorldPos" + i, Player() ? Player().transform.position : Vector3.zero);

        posTarget = LoadAircraft("camPosTargetSquad" + i, "camPosTargetWing" + i);
        dirTarget = LoadAircraft("camDirTargetSquad" + i, "camDirTargetWing" + i);
    }
    public void Initialize()
    {
        if (pos == CamPosition.Free && freeResetting) worldPos = GameManager.gm.mapTr.InverseTransformPoint(posTarget.transform.TransformPoint(relativePos));
    }
    public void SaveSettings()
    {
        int i = index;

        PlayerPrefs.SetInt("camPosition" + i, (int)pos);
        PlayerPrefs.SetInt("camDirection" + i, (int)dir);
        PlayerPrefs.SetInt("camRelativeRotation" + i, relativeRotation ? 1 : 0);
        PlayerPrefs.SetInt("camPlayer" + i, (int)player);

        PlayerPrefs.SetInt("freeResetting" + i, freeResetting ? 1 : 0);
        PlayerPrefs.SetInt("camSmooth" + i, smooth ? 1 : 0);
        PlayerPrefs.SetFloat("camDistance" + i, distance);
        PlayerPrefs.SetFloat("camHeight" + i, height);
        PlayerPrefs.SetFloat("camTilt" + i, tilt);

        Features.PlayerPrefsSetVector3("camRelativePos" + i, relativePos);
        Features.PlayerPrefsSetVector3("camWorldPos" + i, worldPos);

        if (posTarget.data.aircraft)
        {
            PlayerPrefs.SetInt("camPosTargetSquad" + i, posTarget.data.aircraft.squadronId);
            PlayerPrefs.SetInt("camPosTargetWing" + i, posTarget.data.aircraft.placeInSquad);
        }
        if (dirTarget.data.aircraft)
        {
            PlayerPrefs.SetInt("camDirTargetSquad" + i, dirTarget.data.aircraft.squadronId);
            PlayerPrefs.SetInt("camDirTargetWing" + i, dirTarget.data.aircraft.placeInSquad);
        }
    }
    public void SetTarget(SofObject sofComplex)
    {
        SetTarget(sofComplex, sofComplex);
    }
    public void SetTarget(SofObject _pos, SofObject _dir)
    {
        posTarget = _pos;
        dirTarget = _dir;
    }

}
