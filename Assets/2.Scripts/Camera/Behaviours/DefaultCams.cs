using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ThirdPersonGameCam : CameraLogic
{
    public override string Name => "Third Person";
    public override CamPos BasePosMode { get { return CamPos.Game; } }
    public override CamDir BaseDirMode { get { return CamDir.SeatAligned; } }
    public override CamUp UpMode { get { return CamUp.Adaptative; } }
    public override bool FollowBaseDir { get { return ControlsManager.CurrentMode() != ControlsMode.Tracking; } }
    public override Vector3 FixedOffset() { return Player.seat.externalViewPoint; }
}
public class FirstPersonGameCam : CameraLogic
{
    public override string Name => "First Person";
    public override CamPos BasePosMode { get { return CamPos.FirstPerson; } }
    public override CamDir BaseDirMode { get { return CamDir.SeatAligned; } }
    public override CamUp UpMode { get { return CamUp.Relative; } }
    public override bool FollowBaseDir { get { return ControlsManager.CurrentMode() != ControlsMode.Tracking; } }
    public override Vector3 FixedOffset() { return -Vector3.forward * 0.04f; }
}
public class FreeCam : CameraLogic
{
    public override string Name => "World Free";
    public override CamPos BasePosMode { get { return CamPos.World; } }
    public override CamDir BaseDirMode { get { return CamDir.World; } }
    public override CamUp UpMode { get { return CamUp.World; } }
    public override bool FollowBaseDir { get { return false; } }
    public override CamAdjustment Adjustment { get { return CamAdjustment.Position; } }
}
public class BombSightCam : CameraLogic
{
    public override string Name => "Bombsight";
    public override CamPos BasePosMode { get { return CamPos.Bomber; } }
    public override CamDir BaseDirMode { get { return CamDir.Bomber; } }
}
