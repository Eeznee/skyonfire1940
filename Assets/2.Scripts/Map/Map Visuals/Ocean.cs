using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ocean : MonoBehaviour
{
    public Material simple;
    public Material complex;

    public Renderer seaFloorRenderer;

    void OnEnable()
    {
        SofSettingsSO.OnUpdateSettings += UpdateMaterial;
        UpdateMaterial();
    }
    private void OnDisable()
    {
        SofSettingsSO.OnUpdateSettings -= UpdateMaterial;
    }
    private void UpdateMaterial()
    {
        Material mat = SofSettingsSO.CurrentSettings.graphicsPreset >= 2 ? complex : simple;
        GetComponent<Renderer>().sharedMaterial = mat;
    }
    private void Update()
    {
        if (!SofCamera.tr) return;
        bool newActive = SofCamera.tr.position.y < 10f;
        if (newActive != seaFloorRenderer.enabled)
        {
            ;
            seaFloorRenderer.enabled = newActive;
        }
    }
}
