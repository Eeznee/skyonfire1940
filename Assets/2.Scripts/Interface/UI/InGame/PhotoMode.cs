using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
public class PhotoMode : MonoBehaviour
{
    public InputField tilt;

    private float speed;
    public float minSpeed = 1f;
    public float maxSpeed = 100f;
    private Vector3 speeds = Vector3.zero;
    private Vector3 worldPos = Vector3.zero;

    private void Start()
    {
        speed = minSpeed;
        PlayerActions.instance.actions.General.CameraSpeed.performed += t => ChangeSpeed(t.ReadValue<float>());
    }

    private void OnEnable()
    {
        worldPos = GameManager.gm.mapTr.InverseTransformPoint(PlayerCamera.camTr.position);
    }

    private CustomCam currentCam = null;

    public void ResetPositions()
    {
        worldPos = GameManager.gm.mapTr.InverseTransformPoint(PlayerManager.player.sofObj.transform.position);
    }
    private void ChangeSpeed(float input)
    {
        float f = Mathf.InverseLerp(-1f, 1f, input);
        speed = minSpeed * Mathf.Pow(2f, Mathf.Log(maxSpeed / minSpeed, 2) * f);
    }
    private void Update()
    {
        if (PlayerCamera.viewMode == 2)
        {
            if (currentCam != PlayerCamera.customCam) //Reset
            {
                currentCam = PlayerCamera.customCam;
                tilt.text = PlayerCamera.instance.free.tilt.ToString("0.0");
            }
            //Send values
            PlayerCamera.instance.free.tilt = float.Parse(tilt.text);
            PlayerCamera.instance.free.worldPos = worldPos;
            PlayerCamera.instance.free.freeResetting = false;
        }

        Transform camTr = PlayerCamera.camTr;
        Actions.GeneralActions actions = PlayerActions.instance.actions.General;
        Vector3 moveAxis = new Vector3(actions.CameraHorizontal.ReadValue<Vector2>().x, actions.CameraVertical.ReadValue<float>(), actions.CameraHorizontal.ReadValue<Vector2>().y);
        speeds = Vector3.MoveTowards(speeds, moveAxis, Time.unscaledDeltaTime * 2f);

        Vector3 moveVector = camTr.forward * speeds.z + camTr.right * speeds.x + Vector3.up * speeds.y;
        float actualSpeed = speed;
        if (PlayerActions.instance.actions.General.CameraFast.ReadValue<float>() > 0.5f) actualSpeed = maxSpeed;
        moveVector *= actualSpeed * Time.unscaledDeltaTime;
        worldPos += moveVector;
    }
}
