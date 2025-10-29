using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(LensFlareComponentSRP))]
public class SofLensFlare : MonoBehaviour
{
    LensFlareComponentSRP lensFlare;
    void OnEnable()
    {
        lensFlare = GetComponent<LensFlareComponentSRP>();
        SofSettingsSO.OnUpdateSettings += UpdateLensFlareQuality;
        UpdateLensFlareQuality();
    }
    private void OnDisable()
    {
        SofSettingsSO.OnUpdateSettings -= UpdateLensFlareQuality;
    }
    private void UpdateLensFlareQuality()
    {
        lensFlare.useOcclusion = SofSettingsSO.CurrentSettings.graphicsPreset >= 2;
        lensFlare.enabled = SofSettingsSO.CurrentSettings.graphicsPreset >= 2;
    }
}
