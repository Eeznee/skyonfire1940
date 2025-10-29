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

    public virtual string Name { get { return "Set A Camera Name"; } }

    public virtual CamPos BasePosMode { get { return CamPos.SofObject; } }
    public virtual CamDir BaseDirMode { get { return CamDir.SeatAligned; } }
    public virtual CamUp UpMode { get { return CamUp.Relative; } }
    public virtual bool FollowBaseDir { get { return true; } }
    public virtual CamAdjustment Adjustment { get { return CamAdjustment.None; } }
    public virtual Vector3 FixedOffset() { return Vector3.zero; }
    public virtual Vector3 DefaultStartingPos() { return SofCamera.tr.position; }



    public Vector3 BasePosition()
    {
        switch (BasePosMode)
        {
            case CamPos.Game:

                return (Player.role == SeatRole.Pilot ? subCam.Target().tr : subCam.TargetCrew().Seat.tr).position;

            case CamPos.SofObject:

                return subCam.Target().tr.position;

            case CamPos.Tracking:

                SofObject target = subCam.reverseTrack ? Player.sofObj : subCam.trackTarget;
                return target.tr.position;

            case CamPos.FirstPerson:

                return subCam.TargetCrew().CameraPosition;

            case CamPos.Bomber:

                return subCam.Target().aircraft.bombSight.tr.position;

            case CamPos.World:

                return GameManager.gm.map.transform.position;

        }
        return subCam.Target().tr.position;
    }
    public Vector3 BaseDirection()
    {
        switch (BaseDirMode)
        {
            case CamDir.SeatAligned:

                return subCam.TargetCrew().Seat.LookingDirection;

            case CamDir.Tracking:

                SofObject target = subCam.reverseTrack ? subCam.trackTarget : Player.sofObj;
                return (target.tr.position - SofCamera.tr.position).normalized;

            case CamDir.FlyBy:

                return (subCam.Target().tr.position - SofCamera.tr.position).normalized;

            case CamDir.Bomber:

                return subCam.Target().aircraft.bombSight.tr.forward;

            case CamDir.World:

                return Vector3.forward;

        }
        return subCam.TargetCrew().Seat.LookingDirection;
    }
    public Transform RelativeTransform()
    {
        if (BasePosMode == CamPos.World)
            return GameManager.gm.map.transform;
        return subCam.Target().tr;
    }
    public Vector3 BaseUp()
    {
        if (RelativeUp())
            return Player.seat.CameraUp;
        else 
            return Vector3.up;
    }
    private bool RelativeUp()
    {
        switch (UpMode)
        {
            case CamUp.Relative: return true;

            case CamUp.World: return false;

            case CamUp.Custom: return !subCam.gravity;

            case CamUp.Adaptative:
                return ControlsManager.dynamic || ControlsManager.CurrentMode() != ControlsMode.Tracking;
        }
        return true;
    }
    public Quaternion BaseRotation()
    {
        return Quaternion.LookRotation(BaseDirection(), BaseUp());
    }
}

