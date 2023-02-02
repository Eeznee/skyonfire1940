using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
public class CameraEditor : MonoBehaviour
{
    public Dropdown position;
    public Dropdown direction;
    public Dropdown up;
    public Dropdown player;

    public Toggle freeResetting;
    public Toggle smooth;

    public InputField distance;
    public InputField height;
    public InputField tilt;

    public AircraftSwitcher posTarget;
    public AircraftSwitcher dirTarget;

    public Button[] camButtons;

    private float speed;
    public float minSpeed = 1f;
    public float maxSpeed = 100f;
    private Vector3 speeds = Vector3.zero;
    private Vector3 relativePos = Vector3.zero;
    private Vector3 worldPos = Vector3.zero;

    private void LoadProperties(SubCam cam)
    {
        position.value = (int)cam.pos;
        direction.value = (int)cam.dir;
        up.value = cam.relativeRotation ? 0 : 1;
        player.value = (int)cam.player;

        freeResetting.isOn = cam.freeResetting;
        smooth.isOn = cam.smooth;

        distance.text = cam.distance.ToString("0.00");
        height.text = cam.height.ToString("0.00");
        tilt.text = cam.tilt.ToString("0.0");
        relativePos = cam.relativePos;
        worldPos = cam.worldPos;

        posTarget.current = cam.posTarget.data.aircraft;
        dirTarget.current = cam.dirTarget.data.aircraft;
    }
    private void SendProperties(SubCam cam)
    {
        cam.pos = (CamPosition)position.value;
        cam.dir = (CamDirection)direction.value;
        cam.relativeRotation = up.value == 0;
        cam.player = (PlayerIs)player.value;

        cam.freeResetting = freeResetting.isOn;
        cam.smooth = smooth.isOn;

        cam.distance = float.Parse(distance.text);
        cam.height = float.Parse(height.text);
        cam.tilt = float.Parse(tilt.text);
        cam.relativePos = relativePos;
        cam.worldPos = worldPos;

        cam.posTarget = posTarget.current;
        cam.dirTarget = dirTarget.current;
    }
    private void Start()
    {
        position.options = new List<Dropdown.OptionData> { new Dropdown.OptionData("Relative") , new Dropdown.OptionData("Flat Relative") , 
           new Dropdown.OptionData("Free"), new Dropdown.OptionData("First Person") };
        direction.options = new List<Dropdown.OptionData> { new Dropdown.OptionData("Relative"), new Dropdown.OptionData("Tracking"), new Dropdown.OptionData("Free") };
        up.options = new List<Dropdown.OptionData> { new Dropdown.OptionData("Relative"), new Dropdown.OptionData("World") };
        player.options = new List<Dropdown.OptionData> { new Dropdown.OptionData("Position Target"), new Dropdown.OptionData("Direction Target"), new Dropdown.OptionData("None") };

        speed = minSpeed;
        PlayerActions.General().CameraSpeed.performed += t => ChangeSpeed(t.ReadValue<float>());
    }

    private void OnEnable()
    {
        if (PlayerCamera.viewMode >= 0)
            PlayerCamera.SetView(-1);
    }

    private SubCam currentCam = null;

    public void SaveCurrentCam()
    {
        SendProperties(currentCam);
        currentCam.SaveSettings();
    }

    private void ResetCustomCam()
    {
        currentCam = PlayerCamera.subCam;
        LoadProperties(currentCam);

        foreach(Button button in camButtons) button.interactable = false;
        camButtons[-PlayerCamera.viewMode - 1].interactable = true;
    }
    public void ResetPositions()
    {
        relativePos = Vector3.zero;
        worldPos = GameManager.gm.mapTr.InverseTransformPoint(PlayerCamera.subCam.Player().transform.position);
    }
    private void ChangeSpeed(float input)
    {
        float f = Mathf.InverseLerp(-1f, 1f, input);
        speed = minSpeed * Mathf.Pow(2f, Mathf.Log(maxSpeed / minSpeed, 2) * f);
    }
    private void Update()
    {
        if (PlayerCamera.viewMode < 0)
        {
            if (currentCam != PlayerCamera.subCam)  ResetCustomCam();
            SendProperties(currentCam);
        }

        if (freeResetting.gameObject.activeSelf != (position.value == 2)) freeResetting.gameObject.SetActive(position.value == 2);

        Transform camTr = PlayerCamera.camTr;
        Actions.GeneralActions actions = PlayerActions.General();
        Vector3 moveAxis = new Vector3(actions.CameraHorizontal.ReadValue<Vector2>().x, actions.CameraVertical.ReadValue<float>(), actions.CameraHorizontal.ReadValue<Vector2>().y);
        speeds = Vector3.MoveTowards(speeds, moveAxis, Time.unscaledDeltaTime * 2f);

        Vector3 moveVector = camTr.forward * speeds.z + camTr.right * speeds.x + Vector3.up * speeds.y;
        float actualSpeed = speed;
        if (actions.CameraFast.ReadValue<float>() > 0.5f) actualSpeed = maxSpeed;
        moveVector *= actualSpeed * Time.unscaledDeltaTime;

        if (position.value == 0) relativePos += posTarget.current.transform.InverseTransformDirection(moveVector);
        if (position.value == 1) relativePos += moveVector;
        if (position.value == 2) { 
            worldPos += moveVector;
            Vector3 absWorldPos = GameManager.gm.mapTr.TransformPoint(worldPos);
            relativePos = posTarget.current.transform.InverseTransformPoint(absWorldPos);
        }
    }
}
