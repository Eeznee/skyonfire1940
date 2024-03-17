using UnityEngine;

public enum CustomCamLogic
{
    Relative,
    GoPro,
    Gyro,
    Track,
    FlyBy,
    Free,
}
public enum CamPos
{
    Game,
    SofObject,
    Tracking,
    FirstPerson,
    World,
    Bomber
}
public enum CamDir
{
    SeatAligned,
    Tracking,
    FlyBy,
    World,
    Bomber
}
public enum CamUp
{
    Relative,
    World,
    Custom,
    Adaptative
}

public enum CamAdjustment
{
    Position,
    Offset,
    None
}

public abstract class CameraLogic
{
    public static CameraLogic Create(CustomCamLogic picked)
    {
        switch (picked)
        {
            case CustomCamLogic.Relative:
                return new RelativeCam();
            case CustomCamLogic.GoPro:
                return new GoProCam();
            case CustomCamLogic.Gyro:
                return new GyroCam();
            case CustomCamLogic.Free:
                return new FreeCam();
            case CustomCamLogic.Track:
                return new TrackCam();
            case CustomCamLogic.FlyBy:
                return new FlyByCam();
            default: return null;
        }
    }

    public SubCam subCam;

    public virtual CamPos BasePosMode { get { return CamPos.SofObject; } }
    public virtual CamDir BaseDirMode { get { return CamDir.SeatAligned; } }
    public virtual CamUp UpMode { get { return CamUp.Relative; } }
    public virtual bool FollowBaseDir { get { return true; } }
    public virtual CamAdjustment Adjustment { get { return CamAdjustment.None; } }
    public virtual Vector3 FixedOffset()
    {
        return Vector3.zero;
    }
    public virtual Vector3 DefaultStartingPos()
    {
        return SofCamera.tr.position;
    }
    public Vector3 BasePosition()
    {
        switch (BasePosMode)
        {
            case CamPos.Game:
                return (Player.seatInterface == SeatInterface.Pilot ? subCam.Target().tr : subCam.TargetCrew().Seat.tr).position;
            case CamPos.SofObject:
                return subCam.Target().tr.position;
            case CamPos.Tracking:
                SofObject target = subCam.reverseTrack ? Player.sofObj : subCam.trackTarget;
                return target.tr.position;
            case CamPos.FirstPerson:
                return subCam.TargetCrew().EyesPosition();
            case CamPos.Bomber:
                return subCam.Target().aircraft.bombSight.zoomedPOV.position;
            case CamPos.World:
                return GameManager.gm.mapTr.position;
        }
        return Vector3.zero;
    }
    public Vector3 BaseDirection()
    {
        switch (BaseDirMode)
        {
            case CamDir.SeatAligned:
                return subCam.TargetCrew().Seat.DefaultDirection();
            case CamDir.Tracking:
                SofObject target = subCam.reverseTrack ? subCam.trackTarget : Player.sofObj;
                return (target.tr.position - SofCamera.tr.position).normalized;
            case CamDir.FlyBy:
                return (subCam.Target().tr.position - SofCamera.tr.position).normalized;
            case CamDir.Bomber:
                return subCam.Target().aircraft.bombSight.zoomedPOV.forward;
            case CamDir.World:
                return Vector3.forward;
        }
        return subCam.TargetCrew().Seat.DefaultDirection();
    }
    public Transform RelativeTransform()
    {
        if (BasePosMode == CamPos.World)
            return GameManager.gm.mapTr;
        return subCam.Target().tr;
    }
    public Vector3 Up()
    {
        bool relative = true;
        if (UpMode == CamUp.World) relative = false;
        if (UpMode == CamUp.Custom) relative = !subCam.gravity;
        if (UpMode == CamUp.Adaptative) relative = PlayerActions.dynamic || GameManager.Controls() != ControlsMode.Tracking;
        return relative ? subCam.Target().tr.up : Vector3.up;
    }
    public Quaternion BaseRotation()
    {
        return Quaternion.LookRotation(BaseDirection(), Up());
    }
}

