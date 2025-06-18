using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RDG;

public class VibrationsManager : MonoBehaviour
{
    private bool active;
    public static float intensity;
    static float targetIntensity = 0f;
    static float duration = 1f;
    static float currentDuration = 0f;

    const float intensityToVibration = 0.01f;

    private void Start()
    {
        active = PlayerPrefs.GetInt("Vibrations", 0) == 1;
        intensity = 0f;
        currentDuration = 0f;
    }
    void LateUpdate()
    {
        if (Time.timeScale == 0f) return;
        intensity = targetIntensity * Mathf.Sqrt(Mathf.Max(currentDuration, 0f) / duration);
        if (currentDuration > 0f)
        {
            SofCamera.tr.position += Random.insideUnitSphere * intensity * intensityToVibration;
            currentDuration -= Time.deltaTime;
#if MOBILE_INPUT
            if (active) Vibration.Vibrate((int)(currentDuration * 1000f), (int)(intensity * 255f));
#endif
        }
    }

    public static void SendVibrations(float i, float d)
    {
        if (i >= intensity && i > 0.05f)
        {
            targetIntensity = Mathf.Clamp01(i);
            duration = currentDuration = Mathf.Max(d, 0.05f);
        }
    }
}
