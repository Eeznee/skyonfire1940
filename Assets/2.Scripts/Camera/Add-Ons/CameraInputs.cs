using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Camera))]
public class CameraInputs : MonoBehaviour
{
    [SerializeField] private float fieldOfView = 60f;
    [SerializeField] private float zoomedFieldOfView = 25f;
    [SerializeField] private float minSpeed = 3f;
    [SerializeField] private float maxSpeed = 300f;

    private Camera cam;
    public static CameraInputs instance { get; private set; }

    public static float fov = 60f;
    public static bool zoomed;
    public static Plane[] frustrumPlanes;

    const float zoomSmoothTime = 0.1f;
    const float minFov = 1f;
    const float maxFov = 90f;

    private static float sensitivity = 1f;
    private static float sensGunner = 1f;
    private static bool inverted;
    public static float touchSensitivity = 1f;

    public static float speed;

    public static float MaxSpeed { get { return instance.maxSpeed; } }

    private void Start()
    {
        instance = this;
        float playerFov = PlayerPrefs.GetFloat("DefaultFov", 60f);
        float zoomFactor = PlayerPrefs.GetFloat("ZoomFactor", 3f);
        cam = GetComponent<Camera>();
        fov = fieldOfView;
        zoomed = false;
        PlayerActions.cam.Aim.performed += _ => Zoom();
        PlayerActions.cam.Zoom.performed += zoomAxis => SetFov(Mathf.InverseLerp(-1f, 1f, zoomAxis.ReadValue<float>()));
        PlayerActions.cam.Speed.performed += t => ChangeSpeed(t.ReadValue<float>());

        touchSensitivity = PlayerPrefs.GetFloat("TouchSensitivity", 1f);
        sensitivity = PlayerPrefs.GetFloat("Sensitivity", sensitivity);
        sensGunner = PlayerPrefs.GetFloat("GunnerSensitivity", sensGunner);
        inverted = PlayerPrefs.GetInt("InvertTouch", 0) == 1;

        ChangeSpeed(0f);
    }
    private void ProgressiveZoom()
    {
        float input = PlayerActions.cam.ZoomRelativeAxis.ReadValue<float>();
        float zoomRelativeInput = input * Time.unscaledDeltaTime * 0.2f;
        float fovFactor = 1f - Mathf.Log(fov / minFov, 2) / Mathf.Log(maxFov / minFov, 2);
        SetFov(zoomRelativeInput + fovFactor);
    }
    public void Zoom(bool zoomedIn)
    {
        //Log.Print(zoomedIn ? "Camera Zoomed In" : "Camera Zoomed Out", "Camera Zoom");
        zoomed = zoomedIn;
        fov = zoomed ? zoomedFieldOfView : fieldOfView;
    }
    public void Zoom()
    {
        Zoom(!zoomed);
    }
    public void SetFov(float factor)
    {
        factor = Mathf.Max(0f, factor);
        fov = minFov * Mathf.Pow(2f, Mathf.Log(maxFov / minFov, 2) * (1f - factor));
        zoomed = fov <= 0.5f * (zoomedFieldOfView + fieldOfView);
    }
    private float zoomVelocity = 0f;
    private void Update()
    {
        ProgressiveZoom();

        if (Player.role == SeatRole.Bombardier) cam.fieldOfView = Player.aircraft.bombSight.fov;
        else
            cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, fov, ref zoomVelocity, zoomSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);

        frustrumPlanes = GeometryUtility.CalculateFrustumPlanes(cam);
    }
    private void ChangeSpeed(float input)
    {
        float f = Mathf.InverseLerp(-1f, 1f, input);
        speed = minSpeed * Mathf.Pow(2f, Mathf.Log(maxSpeed / minSpeed, 2) * f);
        Log.Print("Camera Speed : " + speed.ToString("0") + " m/s", "CameraSpeed");
    }
    private static bool CameraUnlocked()
    {
#if MOBILE_INPUT
#if UNITY_EDITOR
        return PlayerActions.cam.Unlock.ReadValue<float>() > 0.5f && EventSystem.current.currentSelectedGameObject == null;
#else
        return true;
#endif
#else
        if (ControlsManager.CurrentMode() == ControlsMode.MouseStick) return SofCamera.lookAround;
        if (Player.role == SeatRole.Gunner && ControlsManager.CurrentMode() == ControlsMode.Direct) return SofCamera.lookAround;
        if (UIManager.gameUI != GameUI.Game) return PlayerActions.cam.Unlock.ReadValue<float>() > 0.5f && EventSystem.current.currentSelectedGameObject == null;
        return true;
#endif

    }
    public static float Sensitivity()
    {
        float sens = SofCamera.cam.fieldOfView / instance.fieldOfView;
        sens *= Player.role == SeatRole.Gunner ? sensGunner : sensitivity;
        return sens;
    }
    public static Vector2 CameraInput()
    {
        if (!CameraUnlocked()) return Vector2.zero;
        Vector2 cameraInput = Sensitivity() * PlayerActions.cam.Rotate.ReadValue<Vector2>();
        if (inverted) cameraInput = -cameraInput;
        return cameraInput;
    }
}
