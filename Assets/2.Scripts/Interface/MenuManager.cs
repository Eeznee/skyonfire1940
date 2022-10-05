using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Audio;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    public bool vr = false;
    public SofAudioListener listener;
    public GameObject vrRig;
    public AircraftsList list;
    public Transform mapTr;
    public MapData mapData;
    public Airfield[] airfields;
    public Rigidbody mapRb;
    public AudioMixer mixer;

    public static float timeScale = 1f;
    public static bool paused;
    public static GameUI gameUI = GameUI.Game;

    public static bool trackingControl;
    public static bool fullElevator;

    public static bool war;
    public static GameManager gm;
    public static UIManager ui;
    public static Weather weather;
    public static MapTool map;

    public void Awake()
    {
        Application.targetFrameRate = PlayerPrefs.GetInt("TargetFrameRate", Application.targetFrameRate);
        QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel()));
        fullElevator = PlayerPrefs.GetInt("FullElevatorControl", 0) == 1;

        timeScale = 1f;
        UnitsConverter.Initialize();

        PlayerPrefs.SetFloat("CockpitVolume", -80f);
        PlayerPrefs.SetFloat("ExternalVolume", -80f);

        gm = GetComponent<GameManager>();
        weather = GetComponent<Weather>();
        map = GetComponent<MapTool>();
        list.UpdateCards();
    }
    private float TimeScaleFactor()
    {
        return Mathf.Log(timeScale, 0.5f) / 5f;
    }
    private void SetSlowMo(float factor)
    {
        float oldTimeScale = timeScale;
        factor = Mathf.Max(0f, factor);
        timeScale = Mathf.Pow(0.5f, factor * 5f);
        if (oldTimeScale == timeScale) return;

        string txt = timeScale < 0.1f ? "1/" + (1f / timeScale).ToString("0") : timeScale.ToString("0.00");
        Log.Print("Slow Mo : " + txt, "TimeScale");
    }
}
