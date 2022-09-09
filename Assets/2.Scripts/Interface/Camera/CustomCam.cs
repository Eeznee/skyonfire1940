using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum CamPosition
{
    ObjectRelative,
    FlatRelative,
    Free,
    FirstPerson
}
public enum CamDirection
{
    ObjectRelative,
    LookAt,
    Free,
    Game,
    Bombsight
}
public enum CamUp
{
    ObjectRelative,
    World
}
public enum PlayerIs
{
    PosTarget,
    DirTarget,
    None
}
public class CustomCam
{
    public int index = 1;
    public bool smooth = false;
    public SofObject posTarget;
    public SofObject dirTarget;
    public CamPosition pos = CamPosition.Free;
    public CamDirection dir = CamDirection.Free;
    public PlayerIs player;
    public CamUp up = CamUp.World;
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
        }
        return null;
    }
    public bool WorldLookAround()
    {
        if (dir == CamDirection.Free || (GameManager.trackingControl && dir == CamDirection.Game)) return true;
        else return false;
    }
    private SofAircraft LoadAircraft(string squadTag, string wingTag)
    {
        int squad = PlayerPrefs.GetInt(squadTag, 0);
        int wing = PlayerPrefs.GetInt(wingTag, 0);
        if (GameManager.squadrons.Count > squad && GameManager.squadrons[squad].Length > wing) return GameManager.squadrons[squad][wing];
        return GameManager.squadrons[0][0];
    }
    public CustomCam (CamPosition _pos, CamDirection _dir, CamUp _up, PlayerIs _player, bool _smooth)
    {
        pos = _pos;
        dir = _dir;
        up = _up;
        player = _player;
        smooth = _smooth;
    }
    public CustomCam(int i)
    {
        index = i;

        pos = (CamPosition)PlayerPrefs.GetInt("camPosition" + i, 2);
        dir = (CamDirection)PlayerPrefs.GetInt("camDirection" + i, 2);
        up = (CamUp)PlayerPrefs.GetInt("camUp" + i, 0);
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
        PlayerPrefs.SetInt("camUp" + i, (int)up);
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
