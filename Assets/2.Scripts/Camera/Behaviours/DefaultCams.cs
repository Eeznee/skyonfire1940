using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ThirdPersonGameCam : CameraLogic
{
    public override Vector3 FixedOffset() { return Player.seat.externalViewPoint; } 
    public override CamUp UpMode { get { return CamUp.Adaptative; } }
    public override bool FollowBaseDir { get { return GameManager.Controls() != ControlsMode.Tracking; } }
}
public class FirstPersonGameCam : CameraLogic
{
    public override CamPos BasePosMode { get { return CamPos.FirstPerson; } }
    public override Vector3 FixedOffset() { return -Vector3.forward * 0.04f; }
    public override bool FollowBaseDir { get { return GameManager.Controls() != ControlsMode.Tracking; } }
}
public class FreeCam : CameraLogic
{
    public override CamPos BasePosMode { get { return CamPos.World; } }
    public override CamDir BaseDirMode { get { return CamDir.World; } }
    public override CamAdjustment Adjustment { get { return CamAdjustment.Position; } }
    public override bool FollowBaseDir { get { return false; } }
    public override CamUp UpMode { get { return CamUp.World; } }
}
public class BombSightCam : CameraLogic
{
    public override CamPos BasePosMode { get { return CamPos.Bomber; } }
    public override CamDir BaseDirMode { get { return CamDir.Bomber; } }
}
