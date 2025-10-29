using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefsSpecific : DynamicUI
{
    public string key;
    public int expectedValue = 0;
    public int defaultValue = 0;
    public bool negate;

    public override bool IsActive()
    {
        if (negate) return PlayerPrefs.GetInt(key, defaultValue) != expectedValue;
        return PlayerPrefs.GetInt(key, defaultValue) == expectedValue;
    }
}
