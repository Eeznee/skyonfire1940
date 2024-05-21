using UnityEngine;
using System;
using UnityEngine.InputSystem;
using UnityEngine.Audio;
using System.Collections.Generic;


public class TimeManager : MonoBehaviour
{
    private static float timeScale = 1f;
    public static float invertFixedDelta = 1f;
    public static bool paused;
    public static float fixedTimeDifference { get; private set; }

    public static event Action OnPauseEvent;

    public void Awake()
    {
        timeScale = 1f;
#if MOBILE_INPUT
        SetPause(PlayerPrefs.GetInt("TiltInput", 1) == 1);
#else
        SetPause(false);
#endif
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
        float input = -PlayerActions.menu.TimeScaleRelative.ReadValue<float>();
        input *= Time.unscaledDeltaTime * 0.25f;
        SetSlowMo(TimeScaleFactor() + input);

        Time.timeScale = paused ? 0f : timeScale;
        Time.fixedDeltaTime = Time.timeScale * 0.0166667f ;
        invertFixedDelta = 1f / Time.fixedDeltaTime;
        fixedTimeDifference = Time.time - Time.fixedTime;
    }
    public static void SetPause(bool _paused)
    {
        if (!GameManager.gm.playableScene) return;
        paused = _paused;

        OnPauseEvent?.Invoke();
    }
}
