using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Audio;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public enum Device
{
    PC,
    Mobile,
    VR
}

public class GameManager : MonoBehaviour
{
    public static Device device;
    public bool playableScene = true;
    public bool vr = false;
    public UIManager mobileUI;
    public UIManager pcUI;
    public GameObject playerCamera;
    public SofAudioListener listener;
    public GameObject vrRig;
    public MapData mapData;
    public Airfield[] airfields;
    public AudioMixer mixer;

    public static bool war {  get; private set; }
    public static GameManager gm { get; private set; }
    public static UIManager ui { get; private set; }
    public static Weather weather { get; private set; }
    public static MapTool map { get; private set; }
    public Map mapmap { get; private set; }

    public static List<SofAircraft[]> squadrons = new List<SofAircraft[]>(0);
    public static List<SofObject> sofObjects = new List<SofObject>(0);
    public static List<SofAircraft> axisAircrafts = new List<SofAircraft>(0);
    public static List<SofAircraft> allyAircrafts = new List<SofAircraft>(0);

    public UniversalRenderPipelineAsset urpAsset;

    public static Vector3 refPos;

    private void Awake()
    {
        //Application.targetFrameRate = PlayerPrefs.GetInt("TargetFrameRate", 60);
        Application.targetFrameRate = 120;
        QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel()));
        float renderScale = PlayerPrefs.GetFloat("RenderScale", 100f) / 100f;
        ((UniversalRenderPipelineAsset)QualitySettings.renderPipeline).renderScale = renderScale;
#if MOBILE_INPUT
        device = Device.Mobile;
#else 
        device = Device.PC;
#endif
        if (vr) device = Device.VR;
        UnitsConverter.Initialize();
        gameObject.AddComponent<ControlsManager>();

        if (!playableScene)
        {
            PlayerPrefs.SetFloat("CockpitVolume", -80f);
            PlayerPrefs.SetFloat("ExternalVolume", -80f);
        }

        gm = GetComponent<GameManager>();
        weather = GetComponent<Weather>();
        map = GetComponent<MapTool>();
        mapmap = FindObjectOfType<Map>();
        StaticReferences.Instance.defaultAircrafts.UpdateCards();

        InputSystem.settings.SetInternalFeatureFlag("DISABLE_SHORTCUT_SUPPORT", true);

        squadrons = new List<SofAircraft[]>(0);
        sofObjects = new List<SofObject>(0);
        axisAircrafts = new List<SofAircraft>(0);
        allyAircrafts = new List<SofAircraft>(0);

        if (playableScene)
            CreateGameEnvironment();
    }
    private void CreateGameEnvironment()
    {
        gameObject.AddComponent<TimeManager>();

        CreateMission();
        gameObject.AddComponent<Player>();

        if (vr) Instantiate(vrRig);
        else
        {
            Instantiate(playerCamera);
#if MOBILE_INPUT
            ui = Instantiate(mobileUI);
#else
            ui = Instantiate(pcUI);
#endif
        }
        listener = Instantiate(listener);
    }
    private void Start()
    {
        Physics.autoSyncTransforms = false;
    }
    private void CreateMission()
    {
        PlayerPrefs.SetInt("FirstGame", 1);
        war = PlayerPrefs.GetInt("War", 0) == 1;

        int squads = PlayerPrefs.GetInt("SquadronsAmount");
        for (int i = 0; i < squads; i++)
            SpawnSquadron(Game.Squadron.LoadSquadron(i));
    }
    public static void SpawnSquadron(Game.Squadron squad)
    {
        SofAircraft[] squadronAircrafts = new SofAircraft[squad.amount];
        for (int i = 0; i < squad.amount; i++) squadronAircrafts[i] = SpawnAircraft(squad, i);
        squadrons.Add(squadronAircrafts);
    }
    public static SofAircraft SpawnAircraft(Game.Squadron squad, int wing)
    {
        GameObject plane = squad.aircraftCard.aircraft;
        if (squad.airfield >= 0)    //Airfield Spawn
        {
            Transform tr = gm.airfields[squad.airfield].GetNextSpawn();
            Vector3 pos = tr.position;
            pos.y = map.HeightAtPoint(pos);
            pos += plane.transform.localPosition;
            plane = Instantiate(plane, pos, tr.rotation * plane.transform.localRotation);
        }
        else                      //Airspawn
        {
            plane = Instantiate(plane, squad.startPosition, Quaternion.Euler(0f, -squad.startHeading, 0f));
            plane.transform.Translate(squad.aircraftCard.formation.aircraftPositions[wing], Space.Self);
        }

        SofAircraft aircraft = plane.GetComponent<SofAircraft>();
        aircraft.squadron = squad;
        aircraft.placeInSquad = wing;
        return aircraft;
    }
    
    public static void ScreenShot()
    {
        int screenshots = PlayerPrefs.GetInt("Screenshots", 0);
        ScreenCapture.CaptureScreenshot("Capture" + screenshots + ".png", 2);
        screenshots++;
        PlayerPrefs.SetInt("Screenshots", screenshots);
    }
}
