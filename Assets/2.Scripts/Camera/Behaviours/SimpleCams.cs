using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RelativeCam : CameraLogic
{
    public override CamPos BasePosMode { get { return CamPos.SofObject; } }
    public override CamDir BaseDirMode { get { return CamDir.SeatAligned; } }
    public override CamUp UpMode { get { return CamUp.Custom; } }
    public override CamAdjustment Adjustment { get { return CamAdjustment.Offset; } }
    public override bool FollowBaseDir { get { return true; } }

}
public class GyroCam : CameraLogic
{
    public override CamPos BasePosMode { get { return CamPos.SofObject; } }
    public override CamDir BaseDirMode { get { return CamDir.SeatAligned; } }
    public override CamUp UpMode { get { return CamUp.World; } }
    public override CamAdjustment Adjustment { get { return CamAdjustment.Offset; } }
    public override bool FollowBaseDir { get { return false; } }
}
public class GoProCam : CameraLogic
{
    public override CamPos BasePosMode { get { return CamPos.SofObject; } }
    public override CamDir BaseDirMode { get { return CamDir.SeatAligned; } }
    public override CamUp UpMode { get { return CamUp.Custom; } }
    public override CamAdjustment Adjustment { get { return CamAdjustment.Position; } }
    public override bool FollowBaseDir { get { return true; } }

    public override Vector3 DefaultStartingPos()
    {
        return subCam.TargetCrew().Seat.tr.TransformPoint(subCam.TargetCrew().Seat.goProViewPoint);
    }
}
