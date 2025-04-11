using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunnerJoystickSpecific : DynamicUI
{
    public bool gunnerNoJoystick = false;
    public override bool IsActive()
    {
        if (gunnerNoJoystick)
        {
            return ControlsManager.CurrentMode() == ControlsMode.Tracking;
        }
        else
        {
            return ControlsManager.CurrentMode() != ControlsMode.Tracking;
        }

    }
}
