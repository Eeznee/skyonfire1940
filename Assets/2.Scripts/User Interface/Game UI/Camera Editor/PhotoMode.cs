using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Diagnostics;
public class PhotoMode : MonoBehaviour
{
    public Slider tilt;

    private SubCam photoCam => SofCamera.GetSubCam(2);

    private void OnEnable()
    {
        tilt.value = photoCam.tilt;
        tilt.onValueChanged.AddListener(OnSubcamSettingsChanged);
    }
    private void OnDisable()
    {
        tilt.onValueChanged.RemoveListener(OnSubcamSettingsChanged);
    }

    public void ResetPosition()
    {
        photoCam.ResetPosition();
    }

    public void OnSubcamSettingsChanged<T>(T fillerVariable) { SendProperties();  }
    private void SendProperties()
    {
        photoCam.tilt = tilt.value;
        photoCam.SaveSettings();
    }

    public void OpenScreenshotsLocation()
    {
        Application.OpenURL(Application.persistentDataPath + "/Screenshots/");
    }
}