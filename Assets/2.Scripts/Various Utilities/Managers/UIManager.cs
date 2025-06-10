using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum GameUI
{
    Game,
    Pause,
    CamEditor,
    PhotoMode
}
public class UIManager : MonoBehaviour
{
    public static GameUI gameUI;
    public static UIManager instance;

    public GameObject gameMenu;
    public GameObject pauseMenu;
    public GameObject camerasEditor;
    public GameObject photoMode;

    public Text indicators;

    public IndicatorsList pilotIndicators;
    public IndicatorsList gunnerIndicators;
    public IndicatorsList bomberIndicators;

    private SeatRole seatInterface;

    public List<DynamicUI> dynamicUis;

    private IndicatorsList currentIndicator;

    public static event Action OnUISwitchEvent;

    private void OnEnable()
    {
        instance = this;
        Player.OnSeatChange += ResetInterface;
        SofCamera.OnSwitchCamEvent += ResetInterface;

        dynamicUis = new List<DynamicUI>(GetComponentsInChildren<DynamicUI>(true));
        gameUI = GameUI.Game;
    }
    private void OnDisable()
    {
        Player.OnSeatChange -= ResetInterface;
        SofCamera.OnSwitchCamEvent -= ResetInterface;
    }
    private void GoBackToGameUI() { SwitchGameUI(GameUI.Game); }
    public static void SwitchGameUI(GameUI newUI)
    {
        gameUI = newUI;
        instance.ResetInterface();
        OnUISwitchEvent?.Invoke();
    }
    public void ResetInterface()
    {
        seatInterface = Player.role;
        foreach (DynamicUI dui in dynamicUis) dui.ResetProperties();

#if !MOBILE_INPUT
        bool cursor = seatInterface == SeatRole.Bombardier || gameUI != GameUI.Game;
        Cursor.visible = cursor;
        Cursor.lockState = cursor ? CursorLockMode.None : CursorLockMode.Locked;
#endif
        currentIndicator = null;
        switch (seatInterface)
        {
            case SeatRole.Pilot:currentIndicator = pilotIndicators; break;
            case SeatRole.Gunner: currentIndicator = gunnerIndicators; break;
            case SeatRole.Bombardier:currentIndicator = bomberIndicators; break;
        }
        gameMenu.SetActive(gameUI == GameUI.Game);
        pauseMenu.SetActive(gameUI == GameUI.Pause);
        camerasEditor.SetActive(gameUI == GameUI.CamEditor);
        photoMode.SetActive(gameUI == GameUI.PhotoMode);
    }
    public void SetPause(bool pause)
    {
        TimeManager.SetPause(pause);
    }
    void Update()
    {
        if (currentIndicator) indicators.text = currentIndicator.Text();
    }
}
