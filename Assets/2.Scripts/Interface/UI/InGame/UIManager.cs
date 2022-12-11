using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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

    public List<DynamicUI> dynamicUis;
    private CrewMember crew;

    IndicatorsList currentIndicator;

    private void Start()
    {
#if MOBILE_INPUT
        if (PlayerPrefs.GetInt("TiltInput", 1) == 1) TimeManager.SetPause(true,GameUI.Game);
#endif
        dynamicUis = new List<DynamicUI>(GetComponentsInChildren<DynamicUI>(true));
        PlayerManager.OnPlayerChangeEvent += ResetInterface;
        TimeManager.OnPauseEvent += ResetInterface;
    }

    public void ResetInterface()
    {
        seatInterface = GameManager.seatInterface;
        crew = PlayerManager.player.crew;
        foreach (DynamicUI dui in dynamicUis) dui.ResetProperties();

#if !MOBILE_INPUT
        bool cursor = seatInterface == SeatInterface.Bombardier || GameManager.gameUI != GameUI.Game;
        Cursor.visible = cursor;
        Cursor.lockState = cursor ? CursorLockMode.None : CursorLockMode.Locked;
#endif

        currentIndicator = null;
        switch (seatInterface)
        {
            case SeatInterface.Pilot:currentIndicator = pilotIndicators; break;
            case SeatInterface.Gunner: currentIndicator = gunnerIndicators; break;
            case SeatInterface.Bombardier:currentIndicator = bomberIndicators; break;
        }
        gameMenu.SetActive(GameManager.gameUI == GameUI.Game);
        pauseMenu.SetActive(GameManager.gameUI == GameUI.PauseMenu);
        camerasEditor.SetActive(GameManager.gameUI == GameUI.CamerasEditor);
        photoMode.SetActive(GameManager.gameUI == GameUI.PhotoMode);
    }
    //Used by UI buttons
    public void SetPause(bool pause)
    {
        TimeManager.SetPause(pause, pause ? GameUI.PauseMenu : GameUI.Game);
    }
    public void CreateMarker(SofAircraft aircraft)
    {
        AircraftMarker marker = Instantiate(aircraftMarkerPrefab, gameMenu.transform);
        marker.Init(aircraft);
        dynamicUis.Add(marker);
        marker = Instantiate(aircraftMarkerPrefab, camerasEditor.transform);
        marker.Init(aircraft);
        dynamicUis.Add(marker);
    }
    void Update()
    {
        if (currentIndicator) indicators.text = currentIndicator.Text();
    }
}
