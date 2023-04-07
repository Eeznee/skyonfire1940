using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFov : MonoBehaviour
{
    [SerializeField] private float fieldOfView = 60f;
    [SerializeField] private float zoomedFieldOfView = 25f;

    private Camera cam;

    public static float fov = 60f;
    public static bool zoomed;
    public static Plane[] frustrumPlanes;

    const float zoomSmoothTime = 0.1f;
    const float minFov = 1f;
    const float maxFov = 90f;

    private void Start()
    {
        float playerFov = PlayerPrefs.GetFloat("DefaultFov", 60f);
        float zoomFactor = PlayerPrefs.GetFloat("ZoomFactor", 3f);
        cam = GetComponent<Camera>();
        PlayerActions.General().Aim.performed += _ => Zoom();
        PlayerActions.General().ZoomAxis.performed += zoomAxis => SetFov(Mathf.InverseLerp(-1f, 1f, zoomAxis.ReadValue<float>()));
    }

    private void Zoom()
    {
        zoomed = !zoomed;
        fov = zoomed ? zoomedFieldOfView : fieldOfView;
    }
    private float FovFactor()
    {
        return 1f - Mathf.Log(fov / minFov, 2) / Mathf.Log(maxFov / minFov, 2);
    }
    private void SetFov(float factor)
    {
        factor = Mathf.Max(0f, factor);
        fov = minFov * Mathf.Pow(2f, Mathf.Log(maxFov / minFov, 2) * (1f - factor));
    }
    private float zoomVelocity = 0f;
    private void Update()
    {
        if (PlayerManager.seatInterface == SeatInterface.Bombardier) cam.fieldOfView = PlayerManager.player.aircraft.bombSight.fov;
        else
        {
            cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, fov, ref zoomVelocity, zoomSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
#if !MOBILE_INPUT
            float zoomRelativeInput = PlayerActions.General().ZoomRelativeAxis.ReadValue<float>() * Time.unscaledDeltaTime * 0.2f;
            SetFov(zoomRelativeInput + FovFactor());
#endif
        }

    frustrumPlanes = GeometryUtility.CalculateFrustumPlanes(cam);
    }
}
