using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Marker : MonoBehaviour
{
    public Image reticle;
    public Image arrow;
    public Text infos;

    public float offsetFromBorder = 24f;
    public float minReticleSize = 20f;
    public float maxReticleSize = 100f;

    protected SofObject target;
    protected RectTransform rect;

    protected Vector3 ScreenSize => new(Screen.width, Screen.height, 0f);
    protected Vector3 HalvedScreenSize => new(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
    protected virtual Vector3 TargetPos => target.transform.position;
    protected virtual float TargetDimensions => 10f;

    public Rect ReticleBound => RectTransformExtensions.CalculateBoundingBox(reticle.rectTransform);
    public Rect TextBound => RectTransformExtensions.CalculateBoundingBox(infos.rectTransform);
    public Rect CombinedBound => RectTransformExtensions.CalculateBoundingBox(infos.rectTransform, reticle.rectTransform);

    public bool TargetIsPlayer => target == Player.sofObj;
    public bool TeamMarkersEnabled => target.CompareTag(Player.Tag) ? MarkersManager.AlliedMarkersEnabled : MarkersManager.EnnemiesMarkersEnabled;
    public float SqrDistance => (target.transform.position - SofCamera.tr.position).sqrMagnitude;

    public bool IsOnScreen { get; private set; }

    [HideInInspector] public float reticleOverlapOpacity;
    [HideInInspector] public float textOverlapOpacity;


    const float maximumVisibleDistance = 10000f;
    const float maximumVisibleDistanceOffScreen = 3000f;

    public bool ShouldBeVisible()
    {
        if (TargetIsPlayer) return false;
        if (!TeamMarkersEnabled) return false;

        float maxDistance = IsOnScreen ? maximumVisibleDistance : maximumVisibleDistanceOffScreen;
        float maxDistanceSqr = maxDistance * maxDistance;
        return SqrDistance < maxDistanceSqr;
    }

    public virtual void Init(SofObject _target)
    {
        target = _target;
        rect = GetComponent<RectTransform>();
    }
    private void LateUpdate()
    {
        Vector3 rawScreenPos = SofCamera.cam.WorldToScreenPoint(TargetPos);
        rect.position = MarkerPosition(rawScreenPos, out bool targetIsOnScreen, out float angle);
        arrow.rectTransform.rotation = Quaternion.Euler(0f, 0f, angle);

        IsOnScreen = targetIsOnScreen;

        SetSize(rawScreenPos);
        SetVisibility();

        UpdateColorAndOpacity();

        float distance = (SofCamera.cam.transform.position - target.tr.position).magnitude;
        infos.text = TextToShow(distance);
    }

    protected void SetSize(Vector3 rawScreenPos)
    {
        Vector3 offsetTargetPos = TargetPos + SofCamera.tr.right * TargetDimensions;
        Vector3 offsetScreenPos = SofCamera.cam.WorldToScreenPoint(offsetTargetPos);

        float reticleSize = (rawScreenPos - offsetScreenPos).magnitude;
        reticleSize = Mathf.Clamp(reticleSize, minReticleSize, maxReticleSize);
        reticle.rectTransform.sizeDelta = Vector2.one * reticleSize;
        infos.rectTransform.anchoredPosition = Vector2.right * reticleSize;
    }
    protected Vector3 MarkerPosition(Vector3 screenPoint, out bool targetIsInSights, out float angle)
    {
        bool isContainedWithinScreen = screenPoint.IsContainedWithinDisplay();
        bool isFacingForward = screenPoint.z > 0f;
        targetIsInSights = isContainedWithinScreen && isFacingForward;

        if (!isFacingForward)
        {
            screenPoint *= -1f;
            screenPoint += ScreenSize;
        }
        screenPoint.z = 0f;

        angle = Vector3.SignedAngle(Vector3.up, screenPoint - HalvedScreenSize, Vector3.forward);

        if (!targetIsInSights) ClampOffScreen(ref screenPoint, angle);

        return screenPoint;
    }
    protected void ClampOffScreen(ref Vector3 screenPoint, float angle)
    {
        screenPoint -= HalvedScreenSize;

        float divX = (HalvedScreenSize.x - offsetFromBorder) / Mathf.Abs(screenPoint.x);
        float divY = (HalvedScreenSize.y - offsetFromBorder) / Mathf.Abs(screenPoint.y);

        if (divX < divY)
        {
            screenPoint.x = Mathf.Sign(screenPoint.x) * (HalvedScreenSize.x - offsetFromBorder);
            screenPoint.y = Mathf.Tan(Mathf.Deg2Rad * (angle + 90f)) * screenPoint.x;
        }
        else
        {
            screenPoint.y = Mathf.Sign(screenPoint.y) * (HalvedScreenSize.y - offsetFromBorder);
            screenPoint.x = -Mathf.Tan(Mathf.Deg2Rad * angle) * screenPoint.y;
        }
        screenPoint += HalvedScreenSize;
    }
    protected void UpdateColorAndOpacity()
    {
        Color color = MarkerColor();

        color.a = reticleOverlapOpacity;
        reticle.color = arrow.color = color;

        color.a = textOverlapOpacity;
        infos.color = color;
    }
    protected virtual Color MarkerColor()
    {
        if (target.destroyed) return Color.black;

        Color color = target.CompareTag(Player.Tag) ? Color.blue : Color.red;

        return color;
    }
    protected virtual string TextToShow(float distance)
    {
        string name = target.name;
        string distanceTxt = (UnitsConverter.distance.Multiplier * distance).ToString("0.00") + " " + UnitsConverter.distance.Symbol;
        return name + "\n" + distanceTxt;
    }
    protected void SetVisibility()
    {
        infos.enabled = IsOnScreen && ShouldBeVisible();
        reticle.enabled = IsOnScreen && ShouldBeVisible();
        arrow.enabled = !IsOnScreen && !target.destroyed && ShouldBeVisible();
    }
}
