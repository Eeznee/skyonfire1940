using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseStickSpecific : DynamicUI
{
    public override bool IsActive()
    {
        return GameManager.Controls() == ControlsMode.MouseStick;
    }
}
