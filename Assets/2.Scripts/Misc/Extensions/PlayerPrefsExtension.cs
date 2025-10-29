using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerPrefsExtension
{
    public static void SetVector3(string key, Vector3 value)
    {
        PlayerPrefs.SetFloat(key + "X", value.x);
        PlayerPrefs.SetFloat(key + "Y", value.y);
        PlayerPrefs.SetFloat(key + "Z", value.z);
    }

    public static Vector3 GetVector3(string key, Vector3 defaultValue)
    {
        Vector3 value = defaultValue;
        value.x = PlayerPrefs.GetFloat(key + "X", defaultValue.x);
        value.y = PlayerPrefs.GetFloat(key + "Y", defaultValue.y);
        value.z = PlayerPrefs.GetFloat(key + "Z", defaultValue.z);
        return value;
    }
}
