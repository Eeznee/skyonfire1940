using UnityEngine;
using UnityEngine.UI;

public enum GameUI
{
    Game,
    PauseMenu,
    CamerasEditor,
    PhotoMode
}

public class UIManager : MonoBehaviour
{
    public GameObject gameMenu;
    public GameObject pauseMenu;
    public GameObject camerasEditor;
    public GameObject photoMode;

    public Text indicators;

    public IndicatorsList pilotIndicators;
    public IndicatorsList gunnerIndicators;
    public IndicatorsList bomberIndicators;

    public AircraftMarker aircraftMarkerPrefab;

    private SeatInterface seatInterface;

    public DynamicUI[] dynamicUIs;
    private CrewMember crew;

    IndicatorsList currentIndicator;

    private void Start()
    {
#if MOBILE_INPUT
        if (PlayerPrefs.GetInt("TiltInput", 1) == 1) GameManager.SetPause(true, false);
#endif
        dynamicUIs = GetComponentsInChildren<DynamicUI>(true);
        ResetInterface();
    }

    public void ResetInterface()
    {
        crew = GameManager.player.crew;
        foreach (DynamicUI dui in dynamicUIs) dui.ResetProperties();

        seatInterface = GameManager.seatInterface;
        pilotIndicators.gameObject.SetActive(false);
        gunnerIndicators.gameObject.SetActive(false);
        bomberIndicators.gameObject.SetActive(false);

#if !MOBILE_INPUT
        bool cursor = seatInterface == SeatInterface.Bombardier || GameManager.gameUI != GameUI.Game;
        Cursor.visible = cursor;
        Cursor.lockState = cursor ? CursorLockMode.None : CursorLockMode.Locked;
#endif

        switch (seatInterface)
        {
            case SeatInterface.Pilot: pilotIndicators.gameObject.SetActive(true); currentIndicator = pilotIndicators; break;
            case SeatInterface.Gunner: gunnerIndicators.gameObject.SetActive(true); currentIndicator = gunnerIndicators; break;
            case SeatInterface.Bombardier: bomberIndicators.gameObject.SetActive(true); currentIndicator = bomberIndicators; break;
        }
        gameMenu.SetActive(GameManager.gameUI == GameUI.Game);
        pauseMenu.SetActive(GameManager.gameUI == GameUI.PauseMenu);
        camerasEditor.SetActive(GameManager.gameUI == GameUI.CamerasEditor);
        photoMode.SetActive(GameManager.gameUI == GameUI.PhotoMode);
    }
    //Used by UI buttons
    public void SetPause(bool pause)
    {
        GameManager.SetPause(pause, pause);
    }
    public void CreateMarker(SofAircraft aircraft)
    {
        AircraftMarker marker = Instantiate(aircraftMarkerPrefab, gameMenu.transform);
        marker.Init(aircraft);
        marker = Instantiate(aircraftMarkerPrefab, camerasEditor.transform);
        marker.Init(aircraft);
    }

    void Update()
    {
        if (seatInterface != GameManager.seatInterface) ResetInterface();
        if (crew != GameManager.player.crew) ResetInterface();

        //User interface
        if (seatInterface == SeatInterface.Empty) return;
        indicators.text = currentIndicator.Text();
    }
}
