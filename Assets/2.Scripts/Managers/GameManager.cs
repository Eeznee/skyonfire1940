using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Audio;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{
    public bool playableScene = true;
    public bool vr = false;
    public UIManager mobileUI;
    public UIManager pcUI;
    public GameObject playerCamera;
    public SofAudioListener listener;
    public GameObject vrRig;
    public MapData mapData;
    public Airfield[] airfields;


    public static bool war {  get; private set; }
    public static GameManager gm { get; private set; }
    public static UIManager ui { get; private set; }
    public static Weather weather { get; private set; }
    public static MapTool mapTool { get; private set; }
    public Map map { get; private set; }

    public static List<SofModular> crewedModulars = new List<SofModular>(0);
    public static List<SofAircraft[]> squadrons = new List<SofAircraft[]>(0);
    public static List<SofObject> sofObjects = new List<SofObject>(0);
    public static List<SofAircraft> axisAircrafts = new List<SofAircraft>(0);
    public static List<SofAircraft> allyAircrafts = new List<SofAircraft>(0);

    public UniversalRenderPipelineAsset urpAsset;

    public static Vector3 refPos;

    private void Awake()
    {
        SofSettingsSO.Load();

        gm = GetComponent<GameManager>();
        weather = GetComponent<Weather>();
        mapTool = GetComponent<MapTool>();
        map = FindFirstObjectByType<Map>();
        StaticReferences.Instance.defaultAircrafts.UpdateCards();

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        InputSystem.settings.SetInternalFeatureFlag("DISABLE_SHORTCUT_SUPPORT", true);

        squadrons = new List<SofAircraft[]>(0);
        sofObjects = new List<SofObject>(0);
        axisAircrafts = new List<SofAircraft>(0);
        allyAircrafts = new List<SofAircraft>(0);

        crewedModulars = new List<SofModular>(0);
        SofModular[] allModulars = map.GetComponentsInChildren<SofModular>(true);
        foreach(SofModular modular in allModulars) if (modular.crew.Length > 0) crewedModulars.Add(modular);

        if (playableScene) CreateGameEnvironment();
    }
    private void Start()
    {
        Physics.autoSyncTransforms = false;
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
            ui = Instantiate(Extensions.IsMobile ? mobileUI : pcUI);
        }
        listener = Instantiate(listener);
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
            pos.y = mapTool.HeightAtPoint(pos);
            pos += plane.transform.localPosition;
            plane = Instantiate(plane, pos, tr.rotation * plane.transform.localRotation);
        }
        else                      //Airspawn
        {
            Quaternion rotation = Quaternion.Euler(0f, squad.startHeading, 0f);


            Vector3 startPosition = squad.startPosition;
            startPosition.y = Mathf.Max(startPosition.y, 250f);
            startPosition += rotation * squad.aircraftCard.formation.aircraftPositions[wing];

            plane = Instantiate(plane, startPosition, rotation);
        }

        SofAircraft aircraft = plane.GetComponent<SofAircraft>();
        aircraft.squadron = squad;
        aircraft.placeInSquad = wing;
        return aircraft;
    }
}
