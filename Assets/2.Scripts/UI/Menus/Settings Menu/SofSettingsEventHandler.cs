using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
public class SofSettingsEventHandler : MonoBehaviour
{
    [Header("Graphics")]
    public ToggleGroup graphicsPreset;
    public Slider renderScale;
    public Toggle vsync;
    public Toggle capFrameRate;
    public Slider frameRateLimit;
    public Toggle firstPersonModel;
    public Toggle invertScreenOrientation;

    [Header("Audio")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;

    [Header("Interface")]
    public ToggleGroup altitudeUnit;
    public ToggleGroup distanceUnit;
    public ToggleGroup aircraftSpeedUnit;
    public ToggleGroup climbRateUnit;
    public ToggleGroup markersMode;
    public Toggle vibrations;
    public Toggle hudHiding;
    public Toggle inputBox;
    public Toggle leadIndicator;
    public Toggle showAIBehavior;
    public Toggle gsp;
    public Toggle ias;
    public Toggle altitude;
    public Toggle climbRate;
    public Toggle throttle;
    public Toggle fuel;
    public Toggle ammo;
    public Toggle gforce;
    public Toggle temperature;
    public Toggle heading;

    [Header("Controls")]
    public ToggleGroup pcControlsMode;
    public ToggleGroup mobileControlsMode;
    public ToggleGroup pitchCorrectionMode;
    public Toggle unobtrusiveMouseStick;
    public Toggle invertPitch;
    public Slider tiltSensX;
    public Slider tiltSensY;
    public Slider camSens;
    public Slider mouseStickSens;
    public Toggle firstPersonAiming;
    public Toggle advancedInputBinding;


    public SofSettingsSO Settings => SofSettingsSO.CurrentSettings;


    private DynamicUI[] dynamicUIs;

    private void AddListeners()
    {
        foreach (Toggle toggle in GetComponentsInChildren<Toggle>(true)) toggle.onValueChanged.AddListener(delegate { ValueChanged(); });
        foreach (Slider slider in GetComponentsInChildren<Slider>(true)) slider.onValueChanged.AddListener(delegate { ValueChanged(); });
    }
    private void RemoveListeners()
    {
        foreach (Toggle toggle in GetComponentsInChildren<Toggle>(true)) toggle.onValueChanged.RemoveListener(delegate { ValueChanged(); });
        foreach (Slider slider in GetComponentsInChildren<Slider>(true)) slider.onValueChanged.RemoveListener(delegate { ValueChanged(); });
    }
    private void ValueChanged()
    {
        ApplyUIToSettings();
        SofSettingsSO.ApplyAndUpdateSettings();
    }
    private void OnEnable()
    {
        dynamicUIs = GetComponentsInChildren<DynamicUI>(true);
        UpdateUIValues();
    }
    private void OnDisable()
    {
        SofSettingsSO.Save();
    }
    public void ResetToDefault()
    {
        SofSettingsSO.ResetToDefault();
        UpdateUIValues();
    }
    private void UpdateUIValues()
    {
        RemoveListeners();

        SetToggleGroupIndex(graphicsPreset, Settings.graphicsPreset);
        renderScale.value = Settings.renderScale;
        vsync.isOn = Settings.vsync;
        capFrameRate.isOn = Settings.capFrameRate;
        frameRateLimit.value = Settings.frameRateLimit;
        firstPersonModel.isOn = Settings.firstPersonModel;
        invertScreenOrientation.isOn = Settings.invertScreenOrientation;

        masterVolumeSlider.value = Settings.masterVolume;
        musicVolumeSlider.value = Settings.musicVolume;

        SetToggleGroupIndex(altitudeUnit, Settings.altitudeUnit);
        SetToggleGroupIndex(distanceUnit, Settings.distanceUnit);
        SetToggleGroupIndex(aircraftSpeedUnit, Settings.aircraftSpeedUnit);
        SetToggleGroupIndex(climbRateUnit, Settings.climbRateUnit);
        SetToggleGroupIndex(markersMode, Settings.markersMode);
        vibrations.isOn = Settings.vibrations;
        hudHiding.isOn = Settings.hudHiding;
        inputBox.isOn = Settings.inputBox;
        leadIndicator.isOn = Settings.leadIndicator;
        showAIBehavior.isOn = Settings.showAIBehavior;
        gsp.isOn = Settings.gsp;
        ias.isOn = Settings.ias;
        altitude.isOn = Settings.altitude;
        climbRate.isOn = Settings.climbRate;
        throttle.isOn = Settings.throttle;
        fuel.isOn = Settings.fuel;
        ammo.isOn = Settings.ammo;
        gforce.isOn = Settings.gforce;
        temperature.isOn = Settings.temperature;
        heading.isOn = Settings.heading;

        SetToggleGroupIndex(pcControlsMode, Settings.pcControlsMode);
        SetToggleGroupIndex(mobileControlsMode, Settings.mobileControlsMode);
        SetToggleGroupIndex(pitchCorrectionMode, Settings.pitchCorrectionMode);
        unobtrusiveMouseStick.isOn = Settings.unobtrusiveMouseStick;
        invertPitch.isOn = Settings.invertPitch;
        tiltSensX.value = Settings.tiltSensX;
        tiltSensY.value = Settings.tiltSensY;
        camSens.value = Settings.camSens;
        mouseStickSens.value = Settings.mouseStickSens;
        firstPersonAiming.isOn = Settings.firstPersonAiming;
        advancedInputBinding.isOn = Settings.advancedInputBinding;

        dynamicUIs.ResetProperties();

        AddListeners();
    }
    private void ApplyUIToSettings()
    {
        Settings.graphicsPreset = GetToggleGroupIndex(graphicsPreset);
        Settings.renderScale = renderScale.value;
        Settings.vsync = vsync.isOn;
        Settings.capFrameRate = capFrameRate.isOn;
        Settings.frameRateLimit = Mathf.RoundToInt(frameRateLimit.value);
        Settings.firstPersonModel = firstPersonModel.isOn;
        Settings.invertScreenOrientation = invertScreenOrientation.isOn;

        Settings.masterVolume = masterVolumeSlider.value;
        Settings.musicVolume = musicVolumeSlider.value;

        Settings.altitudeUnit = GetToggleGroupIndex(altitudeUnit);
        Settings.distanceUnit = GetToggleGroupIndex(distanceUnit);
        Settings.aircraftSpeedUnit = GetToggleGroupIndex(aircraftSpeedUnit);
        Settings.climbRateUnit = GetToggleGroupIndex(climbRateUnit);
        Settings.markersMode = GetToggleGroupIndex(markersMode);
        Settings.vibrations = vibrations.isOn;
        Settings.hudHiding = hudHiding.isOn;
        Settings.inputBox = inputBox.isOn;
        Settings.leadIndicator = leadIndicator.isOn;
        Settings.showAIBehavior = showAIBehavior.isOn;
        Settings.gsp = gsp.isOn;
        Settings.ias = ias.isOn;
        Settings.altitude = altitude.isOn;
        Settings.climbRate = climbRate.isOn;
        Settings.throttle = throttle.isOn;
        Settings.fuel = fuel.isOn;
        Settings.ammo = ammo.isOn;
        Settings.gforce = gforce.isOn;
        Settings.temperature = temperature.isOn;
        Settings.heading = heading.isOn;

        Settings.pcControlsMode = GetToggleGroupIndex(pcControlsMode);
        Settings.mobileControlsMode = GetToggleGroupIndex(mobileControlsMode);
        Settings.pitchCorrectionMode = GetToggleGroupIndex(pitchCorrectionMode);
        Settings.unobtrusiveMouseStick = unobtrusiveMouseStick.isOn;
        Settings.invertPitch = invertPitch.isOn;
        Settings.tiltSensX = tiltSensX.value;
        Settings.tiltSensY = tiltSensY.value;
        Settings.camSens = camSens.value;
        Settings.mouseStickSens = mouseStickSens.value;
        Settings.firstPersonAiming = firstPersonAiming.isOn;
        Settings.advancedInputBinding = advancedInputBinding.isOn;

        dynamicUIs.ResetProperties();
    }
    private void SetToggleGroupIndex(ToggleGroup tg,int index)
    {
        if (!tg) return;

        Toggle[] toggles = tg.GetComponentsInChildren<Toggle>();
        index = Mathf.Clamp(index, 0, toggles.Length - 1);
        for (int i = 0; i < toggles.Length; i++)
            toggles[i].isOn = i == index;

        tg.EnsureValidState();
    }
    private int GetToggleGroupIndex(ToggleGroup tg)
    {
        if (!tg) return 0;

        Toggle[] toggles = tg.GetComponentsInChildren<Toggle>();
        for(int i = 0; i < toggles.Length; i++)
        {
            if (toggles[i].isOn) return i;
        }
        return -1;
    }
}
