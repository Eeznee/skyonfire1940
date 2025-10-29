using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SofSettingsSpecific : DynamicUI
{
    public enum Settings
    {
        MobileControlsMode,
        AdvancedRebinding
    }

    public Settings settingsToCheck;

    public int expectedInt = 0;
    public bool invert = false;


    public override bool IsActive()
    {
        int currentValue = CurrentSettingsValue();

        if (invert) return currentValue != expectedInt;
        else return currentValue == expectedInt;
    }


    public int CurrentSettingsValue()
    {
        switch (settingsToCheck)
        {
            case Settings.MobileControlsMode:
                return SofSettingsSO.CurrentSettings.mobileControlsMode;

            case Settings.AdvancedRebinding:
                return Extensions.IsMobile ? (SofSettingsSO.CurrentSettings.advancedInputBinding ? 1 : 0) : 1;
            default:
                return 0;
        }
    }
}
