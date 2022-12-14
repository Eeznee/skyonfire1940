using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Features
{
#if UNITY_EDITOR
    public static void DrawControlHandles(Vector3 A, Vector3 B, Vector3 C, Vector3 D, Color face, Color outline)
    {
        Vector3[] v = new Vector3[4];
        v[0] = A;
        v[1] = B;
        v[2] = C;
        v[3] = D;
        Handles.DrawSolidRectangleWithOutline(v, face, outline);
    }
#endif

    public static void PlayerPrefsSetVector3(string key, Vector3 value)
    {
        PlayerPrefs.SetFloat(key + "X", value.x);
        PlayerPrefs.SetFloat(key + "Y", value.y);
        PlayerPrefs.SetFloat(key + "Z", value.z);
    }

    public static Vector3 PlayerPrefsGetVector3(string key, Vector3 defaultValue)
    {
        Vector3 value = defaultValue;
        value.x = PlayerPrefs.GetFloat(key + "X", defaultValue.x);
        value.y = PlayerPrefs.GetFloat(key + "Y", defaultValue.y);
        value.z = PlayerPrefs.GetFloat(key + "Z", defaultValue.z);
        return value;
    }
}
