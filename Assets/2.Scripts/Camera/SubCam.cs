using UnityEngine;

public class SubCam
{
    public int index = 1;
    public CustomCamLogic customLogicEnum;
    public CameraLogic logic;

    //settings
    public bool reverseTrack;
    public bool targetsPlayer = true;
    public bool smooth = false;
    public bool holdPos = false;
    public bool gravity = false;
    public float tilt = 0f;

    public Vector3 customPos;
    public Vector3 customOffset;

    public SofComplex trackTarget;

    public SofObject Target()
    {
        return TargetCrew().sofObject;
    }
    public CrewMember TargetCrew()
    {
        if (targetsPlayer) return Player.crew;
        return trackTarget.crew[0];
    }

    public void MovePositionAndOffset()
    {
        if (logic.Adjustment == CamAdjustment.None) return;

        Actions.CameraActions actions = PlayerActions.cam;

        float speed = CameraInputs.speed;
        if (actions.MaxSpeed.ReadValue<float>() > 0.5f) speed = CameraInputs.MaxSpeed;

        Vector3 input = new Vector3(actions.MoveHorizontal.ReadValue<Vector2>().x, actions.MoveVertical.ReadValue<float>(), actions.MoveHorizontal.ReadValue<Vector2>().y);
        input *= speed * Time.unscaledDeltaTime;

        if (logic.Adjustment == CamAdjustment.Position)
        {
            Quaternion fromCurrentToDefault = Quaternion.Inverse(logic.RelativeTransform().rotation) * SofCamera.tr.rotation;
            customPos += fromCurrentToDefault * input;
        } 
        else if (logic.Adjustment == CamAdjustment.Offset)
        {
            customOffset.z += input.z;
            if (customOffset.z > -2f) customOffset.z = -2f;
            customOffset.y += input.y;
            float upLimit = Mathf.Max(5f, -customOffset.z * 0.5f);
            customOffset.y = Mathf.Clamp(customOffset.y, -upLimit, upLimit);
        }
    }
    public Vector3 Offset()
    {
        return logic.Adjustment == CamAdjustment.Offset ? customOffset : logic.FixedOffset();
    }
    public Vector3 Position()
    {
        if (UIManager.gameUI == GameUI.CamEditor || UIManager.gameUI == GameUI.PhotoMode) MovePositionAndOffset();

        return CameraOperations.Position(logic,customPos,Offset());
    }
    public Quaternion Rotation(ref Vector2 axis, Quaternion currentRotation)
    {
        if (Player.seatInterface == SeatInterface.Bombardier) return logic.BaseRotation();

        Quaternion rotation;

        if (logic.FollowBaseDir)
            rotation = CameraOperations.RotateRelative(ref axis, CameraInputs.CameraInput().Rotate(-tilt), logic.BaseRotation());
        else
            rotation = CameraOperations.RotateWorld(currentRotation, logic.BaseRotation());

        rotation *= Quaternion.AngleAxis(tilt, Vector3.forward);
        return rotation;
    }
    private SofAircraft LoadAircraft(string squadTag, string wingTag)
    {
        int squad = PlayerPrefs.GetInt(squadTag, 0);
        int wing = PlayerPrefs.GetInt(wingTag, 0);
        if (GameManager.squadrons.Count > squad && GameManager.squadrons[squad].Length > wing) return GameManager.squadrons[squad][wing];
        return GameManager.squadrons[0][0];
    }
    public void ResetNoneHoldPos()
    {
        if (logic.BasePosMode == CamPos.World && !holdPos) Reset();
    }
    public void Reset()
    {
        if (logic.Adjustment == CamAdjustment.Position)
            customPos = logic.RelativeTransform().InverseTransformPoint(logic.DefaultStartingPos());
        if (logic.Adjustment == CamAdjustment.Offset)
            customOffset = TargetCrew().Seat.externalViewPoint;
    }
    public void ChangeLogic(CustomCamLogic newLogic)
    {
        if (newLogic == customLogicEnum && logic != null) return;
        customLogicEnum = newLogic;
        ChangeLogic(CameraLogic.Create(newLogic));
    }
    public void ChangeLogic(CameraLogic newLogic)
    {
        bool firstLogic = logic == null;
        logic = newLogic;
        logic.subCam = this;
        if (!firstLogic) Reset();
    }
    public SubCam(CameraLogic _logic, bool _smooth, bool _holdPos)
    {
        ChangeLogic(_logic);
        smooth = _smooth;
        holdPos = _holdPos;
        tilt = 0f;
    }
    public SubCam(int i)
    {
        index = i;

        targetsPlayer = PlayerPrefs.GetInt("camTargetsPlayer" + i, 1) == 1;
        reverseTrack = PlayerPrefs.GetInt("camReverseTrack" + i, 0) == 1;
        smooth = PlayerPrefs.GetInt("camSmooth" + i, 1) == 1;
        holdPos = PlayerPrefs.GetInt("camHoldPos" + i, 0) == 1;
        gravity = PlayerPrefs.GetInt("camGravity" + i, 0) == 1;
        tilt = PlayerPrefs.GetFloat("camTilt" + i, 0f);

        customOffset = Features.PlayerPrefsGetVector3("camOffset" + i, Player.seat.externalViewPoint);
        customPos = Features.PlayerPrefsGetVector3("camPos" + i, SofCamera.tr.position);

        trackTarget = LoadAircraft("targetSquad" + i, "targetWing" + i);

        ChangeLogic((CustomCamLogic)PlayerPrefs.GetInt("camBehaviour" + i, 0));
    }
    public void SaveSettings()
    {
        int i = index;

        PlayerPrefs.SetInt("camBehaviour" + i, (int)customLogicEnum);
        PlayerPrefs.SetInt("camTargetsPlayer" + i, targetsPlayer ? 1 : 0);
        PlayerPrefs.SetInt("camReverseTrack" + i, reverseTrack ? 1 : 0);
        PlayerPrefs.SetInt("camSmooth" + i, smooth ? 1 : 0);
        PlayerPrefs.SetInt("camHoldPos" + i, holdPos ? 1 : 0);
        PlayerPrefs.SetInt("camGravity" + i, gravity ? 1 : 0);
        PlayerPrefs.SetFloat("camTilt" + i, tilt);

        Features.PlayerPrefsSetVector3("camOffset" + i, customOffset);
        Features.PlayerPrefsSetVector3("camPos" + i, customPos);

        if (trackTarget.aircraft)
        {
            PlayerPrefs.SetInt("targetSquad" + i, trackTarget.aircraft.squadronId);
            PlayerPrefs.SetInt("targetWing" + i, trackTarget.aircraft.placeInSquad);
        }
    }
}
