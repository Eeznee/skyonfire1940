using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RDG;

public class VibrationsManager : MonoBehaviour
{
    public static float intensity;
    static float targetIntensity = 0f;
    static float duration = 1f;
    static float currentDuration = 0f;

    const float intensityToVibration = 0.01f;

    private void Start()
    {
        intensity = 0f;
        currentDuration = 0f;
    }
    void LateUpdate()
    {
        currentDuration -= Time.deltaTime;

        if (Time.timeScale == 0f) return;
        if (currentDuration <= 0f) return;

        intensity = targetIntensity * Mathf.Sqrt(Mathf.Max(currentDuration, 0f) / duration);

        SofCamera.tr.position += Random.insideUnitSphere * intensity * intensityToVibration;

        if(Extensions.IsMobile && SofSettingsSO.CurrentSettings.vibrations) 
            Vibration.Vibrate((int)(currentDuration * 1000f), (int)(intensity * 255f));
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
