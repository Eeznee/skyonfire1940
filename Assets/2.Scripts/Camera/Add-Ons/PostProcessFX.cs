using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class PostProcessFX : MonoBehaviour
{
    public VolumeProfile profile;
    private ColorAdjustments colors;
    private Vignette vignette;
    private ChromaticAberration aberration;
    private LensDistortion lens;

    public RawImage fullScreenFilter;
    public RawImage vignetteFilter;
    public RawImage veins;
    public Text kiaText;

    private bool lightWeight;

    private Color redoutFilter = new Color(1f, 0.25f, 0.25f);
    private Color blackoutFilter = new Color(0.3f, 0.3f, 0.3f);
    private Color damageFilter = new Color(0.65f, 0.2f, 0.2f);
    private Color damageVignette = new Color(0.5f, 0f, 0f);
    private Color damageFilterLW = new Color(0.2f, 0.03f, 0.03f);
    private Color damageVignetteLW = new Color(0.12f, 0.01f, 0.01f);
    //Static colors

    private float damageVignetteIntensity = 0.35f;
    private float damageVignetteIntensityLW = 0.7f;
    private float veinsTransparencyMax = 0.35f;

    private HumanBody body;
    private float blackout;

    void Start()
    {
        profile.TryGet(out colors);
        profile.TryGet(out vignette);
        profile.TryGet(out aberration);
        profile.TryGet(out lens);

        lightWeight = QualitySettings.GetQualityLevel() < 2;
        fullScreenFilter.gameObject.SetActive(lightWeight);
        vignetteFilter.gameObject.SetActive(lightWeight);
        SofCamera.cam.GetComponent<UniversalAdditionalCameraData>().renderPostProcessing = !lightWeight;
        ClearEffects();
    }
    private void ClearEffects()
    {
        if (kiaText.enabled) kiaText.enabled = false;

        Color empty = new Color(0f, 0f, 0f, 0f);

        //Lightweight
        fullScreenFilter.color = veins.color = vignetteFilter.color = empty;

        //Post FX
        colors.colorFilter.value = Color.white;
        vignette.intensity.value = 0f;
        aberration.intensity.value = 0f;
        colors.saturation.value = 0f;
        lens.intensity.value = 0f;
        
    }
    private void UpdateLightWeight()
    {
        //Screen Color Change
        Color fullColor = Color.Lerp(Color.black, Color.red * 0.65f, -blackout * 3f);
        fullColor = Color.Lerp(damageFilterLW, fullColor, Mathf.Abs(blackout));
        fullColor.a = Mathf.Max(Mathf.Abs(blackout), (1f - Player.crew.Integrity) * damageVignetteIntensityLW);
        fullColor = Mathv.CombineColors(fullColor, new Color(0.4f, 0.4f, 0.4f, body.Sickness() * 0.5f));
        if (body.Gloc()) fullColor = Color.black;
        fullScreenFilter.color = Vector4.Lerp(fullScreenFilter.color, fullColor, Time.deltaTime);

        //Vignette
        Color vignetteColor = Color.Lerp(Color.black, Color.red * 0.65f, -blackout * 3f);
        vignetteColor = Color.Lerp(damageVignetteLW, vignetteColor, Mathf.Abs(blackout));
        vignetteColor.a = body.Gloc() ? 0f : fullColor.a;
        vignetteFilter.color = vignetteColor;
    }
    private void UpdatePostFx()
    {
        float crewHealth = Player.crew.Integrity;
        //Vignette
        Color blackOutColor = Color.Lerp(Color.white, blackout > 0f ? Color.black : Color.red, Mathf.Abs(blackout * 2f));
        Color damageColor = Color.Lerp(damageVignette, Color.white, crewHealth);
        vignette.color.value = Color.Lerp(damageColor, blackOutColor, Mathf.Abs(blackout));
        vignette.intensity.value = Mathf.Max(Mathf.Abs(blackout), (1f - crewHealth) * damageVignetteIntensity); ;

        //Color Filter
        Color targetFilter = Color.Lerp(Color.white, blackout > 0f ? blackoutFilter : redoutFilter, blackout > 0f ? blackout * blackout : -blackout * 2f);
        damageColor = Color.Lerp(damageFilter, Color.white, crewHealth);
        targetFilter = Color.Lerp(damageColor, targetFilter, Mathf.Abs(blackout));
        if (body.Gloc()) targetFilter = Color.black;
        colors.colorFilter.value = Vector4.Lerp(colors.colorFilter.value, targetFilter, Time.deltaTime);

        //Black And White
        colors.saturation.value = -100f * Mathf.Clamp01(blackout * Mathf.Abs(blackout) + (1f - crewHealth) / 2f);

        //Pain Effects
        aberration.intensity.value = body.Pain();
        lens.intensity.value = Mathf.Clamp(body.Pain() * -0.3f, -0.5f, 0f);

        //Sickness Effects
        Vector2 center = new Vector2(0.5f, 0.5f);
        center.x += 0.1f * Mathf.Sin(Time.time) * body.Sickness();
        center.y += 0.1f * Mathf.Cos(Mathf.PI * Time.time / 4f) * body.Sickness();
        lens.center.value = center;
        lens.intensity.value += Mathf.Lerp(0f, -0.3f, body.Sickness());
        colors.saturation.value = Mathf.Lerp(colors.saturation.value, Mathf.Min(colors.saturation.value, -50f), body.Sickness());
    }
    void Update()
    {
        if (Player.crew == null || TimeManager.paused || !(SofCamera.viewMode == 0 || SofCamera.viewMode == 1))
        {
            ClearEffects();
            return;
        }

        body = Player.crew.humanBody;
        blackout = Mathf.Clamp(body.Blood() * (1f - body.Stamina()), -1f, 1f);

        float a = veinsTransparencyMax * Mathv.SmoothStart(1f - Player.crew.Integrity, 2);
        veins.color = new Color(veins.color.r, veins.color.g, veins.color.b, a);
        if (kiaText.enabled != Player.crew.ripped) kiaText.enabled = Player.crew.ripped;
        if (lightWeight) UpdateLightWeight();
        else UpdatePostFx();
    }
}
