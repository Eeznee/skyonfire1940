using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Camera))]
public class CameraInputs : MonoBehaviour
{
    public static CameraInputs instance { get; private set; }

    private Camera cam;


    public static float fov = 60f;
    public static bool zoomed;
    public const float fieldOfView = 60f;
    public const float zoomedFieldOfView = 25f;
    public const float minFov = 1f;
    public const float maxFov = 90f;
    public const float zoomSmoothTime = 0.1f;

    public static float speed;
    public const float minSpeed = 1f;
    public const float maxSpeed = 300f;


    public static Plane[] frustrumPlanes;
    public static float MaxSpeed { get { return maxSpeed; } }

    private void Start()
    {
        instance = this;
        float playerFov = PlayerPrefs.GetFloat("DefaultFov", 60f);
        float zoomFactor = PlayerPrefs.GetFloat("ZoomFactor", 3f);
        cam = GetComponent<Camera>();
        fov = fieldOfView;
        zoomed = false;

        ControlsManager.cam.ToggleAim.performed += _ => Zoom();
        ControlsManager.cam.HoldAim.performed += _ => Zoom(true);
        ControlsManager.cam.HoldAim.canceled += _ => Zoom(false);

        SetCamSpeed(0.5f);
        Zoom(false);
    }

    private void Update()
    {
        ProgressiveZoom();

        if (Player.role == SeatRole.Bombardier) cam.fieldOfView = Player.aircraft.bombSight.fov;
        else
            cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, fov, ref zoomVelocity, zoomSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);

        frustrumPlanes = GeometryUtility.CalculateFrustumPlanes(cam);
    }
    public static void SetCamSpeed(float factor)
    {
        speed = minSpeed * Mathf.Pow(maxSpeed / minSpeed, factor);
        Log.Print("Camera Speed : " + speed.ToString("0") + " m/s", "CameraSpeed");
    }
    public static float GetCamSpeedFactor()
    {
        return Mathf.Log(speed / minSpeed, maxSpeed / minSpeed);
    }
    private static bool CameraUnlocked()
    {
        if (Extensions.IsMobile)
        {
#if UNITY_EDITOR
            bool evenSystemNoneSelected = EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null;
            return ControlsManager.cam.Unlock.ReadValue<float>() > 0.5f && evenSystemNoneSelected;
#else
            return true;
#endif
        }
        else
        {
            if (ControlsManager.CurrentMode() == ControlsMode.MouseStick) return SofCamera.lookAround;
            if (UIManager.gameUI != GameUI.Game) return ControlsManager.cam.Unlock.ReadValue<float>() > 0.5f && EventSystem.current.currentSelectedGameObject == null;
            if (Player.role == SeatRole.Gunner && ControlsManager.CurrentMode() == ControlsMode.Direct) return SofCamera.lookAround;

            return true;
        }
    }
    public static Vector2 CameraInput()
    {
        if (!CameraUnlocked()) return Vector2.zero;

        Vector2 cameraInput = ControlsManager.cam.Rotate.ReadValue<Vector2>();
        return SofSettingsSO.CurrentSettings.camSens * 0.01f * SofCamera.cam.fieldOfView / fieldOfView * cameraInput;
    }

    //Zoom Logic

    private float zoomVelocity = 0f;
    public void ProgressiveZoom()
    {
        float input = ControlsManager.cam.ZoomRelativeAxis.ReadValue<float>();
        float zoomRelativeInput = input * Time.unscaledDeltaTime * 0.2f;
        float fovFactor = 1f - Mathf.Log(fov / minFov, 2) / Mathf.Log(maxFov / minFov, 2);
        SetFov(zoomRelativeInput + fovFactor);
    }


    private static bool wasThirdPerson = false;
    public static void Zoom(bool zoomedIn)
    {
        if (SofSettingsSO.CurrentSettings.firstPersonAiming && Player.aircraft &&  Player.aircraft.card.fighter)
        {
            bool pilotFighter = Player.role == SeatRole.Pilot && Player.aircraft && Player.aircraft.card.fighter;
            bool gunner = Player.role == SeatRole.Gunner;
            if (pilotFighter || gunner)
            {
                if (SofCamera.viewMode == 0 && !zoomed && zoomedIn)
                {
                    wasThirdPerson = true;
                    SofCamera.SwitchViewMode(1);
                }

                if (wasThirdPerson && SofCamera.viewMode == 1 && zoomed && !zoomedIn)
                {
                    wasThirdPerson = false;
                    SofCamera.SwitchViewMode(3);
                }
            }
        }

        zoomed = zoomedIn;
        fov = zoomed ? zoomedFieldOfView : fieldOfView;
    }
    public static void Zoom()
    {
        Zoom(!zoomed);
    }
    public static void SetFov(float factor)
    {
        factor = Mathf.Clamp01(factor);
        fov = minFov * Mathf.Pow(maxFov / minFov, 1f - factor);
        zoomed = fov <= 0.5f * (zoomedFieldOfView + fieldOfView);
    }
    public static float GetZoomFactor()
    {
        return 1f - Mathf.Log(fov / minFov, maxFov / minFov);
    }
}
