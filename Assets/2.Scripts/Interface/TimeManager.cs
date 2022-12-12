using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Audio;
using System.Collections.Generic;


public class TimeManager : MonoBehaviour
{
    private static float timeScale = 1f;
    public static bool paused;

    public delegate void OnPause();
    public static OnPause OnPauseEvent;

    public void Awake()
    {
        timeScale = 1f;
        OnPauseEvent = null;
        SetPause(false, GameUI.Game);
    }
    private float TimeScaleFactor()
    {
        return Mathf.Log(timeScale, 0.5f) / 5f;
    }
    public static void SetSlowMo(float factor)
    {
        float oldTimeScale = timeScale;
        factor = Mathf.Max(0f, factor);
        timeScale = Mathf.Pow(0.5f, factor * 5f);
        if (oldTimeScale == timeScale) return;

        string txt = timeScale < 0.1f ? "1/" + (1f / timeScale).ToString("0") : timeScale.ToString("0.00");
        Log.Print("Slow Mo : " + txt, "TimeScale");
    }
    private void Update()
    {
        float input = -PlayerActions.instance.actions.General.TimeScaleRelative.ReadValue<float>();
        input *= Time.unscaledDeltaTime * 0.25f;
        SetSlowMo(TimeScaleFactor() + input);

        Time.timeScale = paused ? 0f : timeScale;
        Time.fixedDeltaTime = Time.timeScale * 0.0166667f;
    }
    public static void SetPause(bool _paused, GameUI _ui)
    {
        if (!GameManager.gm.playableScene) return;
        paused = _paused;
        GameManager.gameUI = _ui;
        AudioListener.volume = paused ? 0.2f : 1f;

        if (OnPauseEvent != null) OnPauseEvent();
    }
}
