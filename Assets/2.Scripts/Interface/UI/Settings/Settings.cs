using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public class Settings : MonoBehaviour
{
    public void Quality(int id)
    {
        PlayerPrefs.SetInt("Quality", id);
        QualitySettings.SetQualityLevel(id);
    }

    public void SetTargetFramerate(int target)
    {
        PlayerPrefs.SetInt("TargetFrameRate", target);
        Application.targetFrameRate = target;
    }
}
