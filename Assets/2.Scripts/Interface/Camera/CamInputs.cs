using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamInputs : MonoBehaviour
{
    private static float sensitivity = 1f;
    private static float sensGunner = 1f;
    private static bool inverted;
    public static float touchSensitivity = 1f;
    private void Awake()
    {
        touchSensitivity = PlayerPrefs.GetFloat("TouchSensitivity", 1f);
        sensitivity = PlayerPrefs.GetFloat("Sensitivity", sensitivity);
        sensGunner = PlayerPrefs.GetFloat("GunnerSensitivity", sensGunner);
        inverted = PlayerPrefs.GetInt("InvertTouch", 0) == 1;
    }
    public static bool CameraUnlocked()
    {
        if (GameManager.Controls() == ControlsMode.MouseStick) return PlayerCamera.lookAround;
#if !MOBILE_INPUT
        if (GameManager.gameUI != GameUI.Game) return PlayerActions.General().UnlockCamera.ReadValue<float>() > 0.5f;
#endif
        return true;
    }
    public static float Sensitivity()
    {
        float sens = PlayerCamera.cam.fieldOfView / CameraFov.fov;
        sens *= PlayerManager.seatInterface == SeatInterface.Gunner ? sensGunner : sensitivity;
        return sens;
    }
    public static Vector2 CameraInput()
    {
        if (!CameraUnlocked()) return Vector2.zero;
        Vector2 cameraInput = Sensitivity() * PlayerActions.General().Camera.ReadValue<Vector2>() * 0.1f;
        if (inverted) cameraInput = -cameraInput;
        return cameraInput;
    }
}
