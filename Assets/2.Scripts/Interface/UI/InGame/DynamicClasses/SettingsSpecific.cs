using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsSpecific : DynamicUI
{
    public string key;
    public int expectedValue = 0;
    public int defaultValue = 0;

    public override bool IsActive()
    {
        return PlayerPrefs.GetInt(key, defaultValue) == expectedValue;
    }
}
