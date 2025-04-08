using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(LensFlareComponentSRP))]
public class SofLensFlare : MonoBehaviour
{
    LensFlareComponentSRP lensFlare;

    void Start()
    {
        lensFlare = GetComponent<LensFlareComponentSRP>();

        int qualityLevel = QualitySettings.GetQualityLevel();
        lensFlare.useOcclusion = qualityLevel == 3;
        lensFlare.enabled = qualityLevel == 3;
    }
}
