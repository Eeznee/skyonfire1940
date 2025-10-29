using UnityEngine;
using UnityEngine.UI;

public class ActivateWithToggle : DynamicUI
{
    public Toggle toggle;

    public bool invert = false;
    public bool enableIfToggleIsInactive = false;


    public override bool IsActive()
    {
        if (!toggle) return enableIfToggleIsInactive;
        if (!toggle.gameObject.activeInHierarchy) return enableIfToggleIsInactive;

        bool active = toggle.isOn;
        if (invert) active = !active;

        return active;
    }
}