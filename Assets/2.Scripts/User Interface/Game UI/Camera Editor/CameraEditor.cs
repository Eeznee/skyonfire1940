using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;
public class CameraEditor : MonoBehaviour
{
    public static CameraEditor camEditor;

    private SubCam currentCam = null;
    public Dropdown logics;

    public AircraftSwitcher target;
    public Toggle reverseTrack;
    public Toggle targetsPlayer;
    public Toggle holdPos;
    public Toggle gravity;
    public Toggle smooth;
    public Slider tilt;
    public GameObject trackingWarning;

    private static int lastCustomViewMode = -1;

    public static event Action OnSubcamSettingsChange;


    private void Start()
    {
        int posCount = Enum.GetNames(typeof(CustomCamLogic)).Length;
        logics.options = new List<Dropdown.OptionData>();
        for (int i = 0; i < posCount; i++)
            logics.options.Add(new Dropdown.OptionData(((CustomCamLogic)i).ToString()));
        OnCameraChange();
    }

    private void OnEnable()
    {
        if (SofCamera.subCam != null && SofCamera.viewMode >= 0)
            SofCamera.SwitchViewMode(Mathf.Min(lastCustomViewMode, -1));

        SofCamera.OnSwitchCamEvent += OnCameraChange;
    }
    private void OnDisable()
    {
        SofCamera.OnSwitchCamEvent -= OnCameraChange;
    }
    public void ResetPosition()
    {
        currentCam.Reset();
    }
    public void OnCameraChange()
    {
        if (currentCam == SofCamera.subCam) return;

        if (SofCamera.viewMode >= 0)
        {
            UIManager.SwitchGameUI(GameUI.Game);
            return;
        }

        currentCam = SofCamera.subCam;
        LoadProperties();

        OnSubcamSettingsChanged();

        lastCustomViewMode = SofCamera.viewMode;
    }
    private void Update()
    {
        currentCam.SaveSettings();
    }
    public void OnSubcamSettingsChanged<T>(T fillerVariable) { OnSubcamSettingsChanged(); }
    public void OnSubcamSettingsChanged()
    {
        SendProperties();

        CameraLogic logic = SofCamera.subCam.logic;

        targetsPlayer.gameObject.SetActive(logic.BaseDirMode != CamDir.Tracking);
        reverseTrack.gameObject.SetActive(logic.BaseDirMode == CamDir.Tracking);
        target.gameObject.SetActive(logic.BaseDirMode == CamDir.Tracking || !targetsPlayer.isOn);
        holdPos.gameObject.SetActive(logic.BasePosMode == CamPos.World);
        gravity.gameObject.SetActive(logic.UpMode == CamUp.Custom);
        trackingWarning.SetActive(logic.BaseDirMode == CamDir.Tracking && Player.complex == SofCamera.subCam.trackTarget);

        OnSubcamSettingsChange?.Invoke();
    }

    private void LoadProperties()
    {
        RemoveListeners();

        logics.value = (int)currentCam.customLogicEnum;
        targetsPlayer.isOn = currentCam.targetsPlayer;
        smooth.isOn = currentCam.smooth;
        holdPos.isOn = currentCam.holdPos;
        gravity.isOn = currentCam.gravity;
        reverseTrack.isOn = currentCam.reverseTrack;
        tilt.value = currentCam.tilt;
        target.Current = currentCam.trackTarget.aircraft;

        AddListeners();
    }
    private void SendProperties()
    {
        currentCam.ChangeLogic((CustomCamLogic)logics.value);
        currentCam.targetsPlayer = targetsPlayer.isOn;
        currentCam.smooth = smooth.isOn;
        currentCam.holdPos = holdPos.isOn;
        currentCam.gravity = gravity.isOn;
        currentCam.reverseTrack = reverseTrack.isOn;
        currentCam.tilt = tilt.value;
        currentCam.trackTarget = target.Current;
        currentCam.SaveSettings();
    }
    private void AddListeners()
    {
        logics.onValueChanged.AddListener(OnSubcamSettingsChanged);
        target.onValueChanged.AddListener(OnSubcamSettingsChanged);
        targetsPlayer.onValueChanged.AddListener(OnSubcamSettingsChanged);
        holdPos.onValueChanged.AddListener(OnSubcamSettingsChanged);
        gravity.onValueChanged.AddListener(OnSubcamSettingsChanged);
        reverseTrack.onValueChanged.AddListener(OnSubcamSettingsChanged);
        smooth.onValueChanged.AddListener(OnSubcamSettingsChanged);
        tilt.onValueChanged.AddListener(OnSubcamSettingsChanged);
    }
    private void RemoveListeners()
    {
        logics.onValueChanged.RemoveListener(OnSubcamSettingsChanged);
        target.onValueChanged.RemoveListener(OnSubcamSettingsChanged);
        targetsPlayer.onValueChanged.RemoveListener(OnSubcamSettingsChanged);
        holdPos.onValueChanged.RemoveListener(OnSubcamSettingsChanged);
        gravity.onValueChanged.RemoveListener(OnSubcamSettingsChanged);
        reverseTrack.onValueChanged.RemoveListener(OnSubcamSettingsChanged);
        smooth.onValueChanged.RemoveListener(OnSubcamSettingsChanged);
        tilt.onValueChanged.RemoveListener(OnSubcamSettingsChanged);
    }
}
