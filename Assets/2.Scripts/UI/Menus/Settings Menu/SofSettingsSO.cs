using UnityEngine;
using System;
using System.IO;
using UnityEngine.Rendering.Universal;

[CreateAssetMenu(fileName = "Game Settings", menuName = "SOF/Menus/Settings")]
[Serializable]
public class SofSettingsSO : ScriptableObject
{
    [Header("Graphics")]
    public int graphicsPreset;
    public float renderScale;
    public bool vsync;
    public bool capFrameRate;
    public int frameRateLimit;
    public bool firstPersonModel;
    public bool invertScreenOrientation;

    [Header("Audio")]
    public float masterVolume;
    public float musicVolume;

    [Header("Interface")]
    public int altitudeUnit;
    public int distanceUnit;
    public int aircraftSpeedUnit;
    public int climbRateUnit;
    public int markersMode;
    public bool vibrations;
    public bool hudHiding;
    public bool inputBox;
    public bool leadIndicator;
    public bool showAIBehavior;
    public bool gsp;
    public bool ias;
    public bool altitude;
    public bool climbRate;
    public bool throttle;
    public bool fuel;
    public bool ammo;
    public bool gforce;
    public bool temperature;
    public bool heading;


    [Header("Controls")]
    public int pcControlsMode;
    public int mobileControlsMode;
    public int pitchCorrectionMode;
    public bool unobtrusiveMouseStick;
    public bool invertPitch;
    public float tiltSensX;
    public float tiltSensY;
    public float camSens;
    public float mouseStickSens;
    public bool firstPersonAiming;
    public bool advancedInputBinding;



    public static void ApplyAndUpdateSettings()
    {
        int targetFrame = CurrentSettings.capFrameRate ? CurrentSettings.frameRateLimit : 60;
        if (Application.targetFrameRate != targetFrame) Application.targetFrameRate = targetFrame;

        if (QualitySettings.vSyncCount != (CurrentSettings.vsync ? 1 : 0)) QualitySettings.vSyncCount = CurrentSettings.vsync ? 1 : 0;

        if (QualitySettings.GetQualityLevel() != CurrentSettings.graphicsPreset) QualitySettings.SetQualityLevel(CurrentSettings.graphicsPreset);

        ((UniversalRenderPipelineAsset)QualitySettings.renderPipeline).renderScale = CurrentSettings.renderScale / 100f;

        Screen.orientation = CurrentSettings.invertScreenOrientation ? ScreenOrientation.LandscapeRight : ScreenOrientation.LandscapeLeft;

        //UNITS
        UnitsConverter.altitude.ChangeOption(CurrentSettings.altitudeUnit);
        UnitsConverter.distance.ChangeOption(CurrentSettings.distanceUnit);
        UnitsConverter.speed.ChangeOption(CurrentSettings.aircraftSpeedUnit);
        UnitsConverter.climbRate.ChangeOption(CurrentSettings.climbRateUnit);

        OnUpdateSettings?.Invoke();
    }

    private static string SaveFilePath => Path.Combine(Application.persistentDataPath, "sofsettings.json");
    public static Action OnUpdateSettings;

    static SofSettingsSO _defaultSettings;
    public static SofSettingsSO DefaultSettings
    {
        get
        {
            if (_defaultSettings == null)
                _defaultSettings = Resources.Load<SofSettingsSO>("Default SOF Settings");
            return _defaultSettings;
        }
    }
    static SofSettingsSO _currentSettings;
    public static SofSettingsSO CurrentSettings
    {
        get
        {
            if (_currentSettings == null)
                _currentSettings = Resources.Load<SofSettingsSO>("SOF Settings");
            return _currentSettings;
        }
    }
    public static void Save()
    {
        string json = JsonUtility.ToJson(CurrentSettings, true);
        File.WriteAllText(SaveFilePath, json);
    }
    public static void Load()
    {
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(DefaultSettings, false), CurrentSettings);

        if (File.Exists(SaveFilePath))
        {
            string json = File.ReadAllText(SaveFilePath);
            JsonUtility.FromJsonOverwrite(json, CurrentSettings);
        }

        if (CurrentSettings.tiltSensX == 0f) CurrentSettings.tiltSensX = 100f;
        if (CurrentSettings.tiltSensY == 0f) CurrentSettings.tiltSensY = 100f;
        if (CurrentSettings.camSens == 0f) CurrentSettings.camSens = 100f;
        if (CurrentSettings.mouseStickSens == 0f) CurrentSettings.mouseStickSens = 100f;

        ApplyAndUpdateSettings();
    }
    public static void ResetToDefault()
    {
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(DefaultSettings, false), CurrentSettings);
        ApplyAndUpdateSettings();
    }
}
