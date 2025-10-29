using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.SceneManagement;
public class PauseMenu : MonoBehaviour
{
    public GameObject settings;

    public static bool settingsActive;

    public void Resume()
    {
        UIManager.SwitchGameUI(GameUI.Game);
        TimeManager.SetPause(false);
        if (SofCamera.viewMode != 1 && SofCamera.viewMode != 0) SofCamera.SwitchViewMode(0);

        settingsActive = false;
    }

    public void RestartScene()
    {
        SceneManager.LoadScene("Dover");
    }
    public void Recalibrate()
    {
        TiltInput.Recalibrate();
    }
    public void ExitToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void OpenPhotoMode()
    {
        UIManager.SwitchGameUI(GameUI.PhotoMode);
        SofCamera.SwitchViewMode(2);
    }
    public void OpenCameraEditor()
    {
        UIManager.SwitchGameUI(GameUI.CamEditor);
    }

    public void SetDebugIndicators(bool active)
    {
        DebugIndicators.SetActive(active);
    }
    public void SetSpectatingAI(bool isSpectating)
    {
        Player.controllingPlayer = !isSpectating;
    }


    

    private void Update()
    {
        settingsActive = settings.activeSelf;
    }
}
