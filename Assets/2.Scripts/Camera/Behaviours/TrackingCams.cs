using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TrackCam : CameraLogic
{
    public override string Name => "Tracking";
    public override CamPos BasePosMode { get { return CamPos.Tracking; } }
    public override CamDir BaseDirMode { get { return CamDir.Tracking; } }
    public override CamAdjustment Adjustment { get { return CamAdjustment.Position; } }
    public override CamUp UpMode { get { return CamUp.Custom; } }
    public override bool FollowBaseDir { get { return true; } }

    public override Vector3 DefaultStartingPos()
    {
        return subCam.Target().tr.position + subCam.Target().tr.up * 5f;
    }
}
public class FlyByCam : CameraLogic
{
    public override string Name => "FlyBy";
    public override CamPos BasePosMode { get { return CamPos.World; } }
    public override CamDir BaseDirMode { get { return CamDir.FlyBy; } }
    public override CamUp UpMode { get { return CamUp.World; } }
    public override CamAdjustment Adjustment { get { return CamAdjustment.Position; } }

    public override bool FollowBaseDir { get { return true; } }

    public override Vector3 DefaultStartingPos()
    {
        return subCam.Target().tr.position + subCam.Target().tr.forward * 500f;
    }
}