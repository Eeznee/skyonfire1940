using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.InputSystem;

public class BombsightUI : MonoBehaviour
{
    //Wheel settings
    public int[] markings;
    public float span = 270f;
    public float refAngle = 40f;
    public float tanSmoothing = 0.8f;
    private float totalDistance;

    //Traverse indicator
    public float maxTraverseDistance;
    Vector3 defaultTraversePos;

    public RectTransform wheel;
    public RectTransform markingsHolder;
    public RectTransform indicator;
    public RectTransform traverse;
    public Text marking;
    public UIGauge intervalGauge;
    public UIGauge amountGauge;
    public UIGauge modeGauge;
    public UIToggle bombBayToggle;
    public UIToggle bombsAlertToggle;
    public UIToggle releaseSequenceToggle;

    bool bombBayState;

    private void OnEnable()
    {
        if (Player.aircraft) Reload();
    }
    private void Reload()
    {
        Bombsight sight = Player.aircraft.bombSight;
        if (sight == null) return;

        defaultTraversePos = traverse.localPosition;

        for (int i = 0; i < markingsHolder.childCount; i++)
            Destroy(markingsHolder.GetChild(i).gameObject);

        totalDistance = Mathf.Tan(sight.maxAngle * Mathf.Deg2Rad * tanSmoothing) - Mathf.Tan(sight.minAngle * Mathf.Deg2Rad * tanSmoothing);

        for (int i = 0; i < markings.Length; i++)
        {
            float distance = Mathf.Tan(markings[i] * Mathf.Deg2Rad * tanSmoothing) - Mathf.Tan(sight.minAngle * Mathf.Deg2Rad * tanSmoothing);
            float angle = span * distance / totalDistance;

            Vector3 position = Vector3.zero;
            position.x = -Mathf.Cos(angle * Mathf.Deg2Rad) * wheel.sizeDelta.x / 2f;
            position.y = Mathf.Sin(angle * Mathf.Deg2Rad) * wheel.sizeDelta.x / 2f;
            Text mark = Instantiate(marking, markingsHolder);
            mark.gameObject.SetActive(true);
            mark.text = "- " + Mathf.Abs(markings[i]);
            mark.rectTransform.localPosition = position;
            mark.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -angle);
        }
        marking.gameObject.SetActive(false);

        //Bomb bay toggle
        bombBayState = Player.aircraft.hydraulics.bombBay.state == 0f;
        bombBayToggle.Toggle(bombBayState);

        foreach (MaskableGraphic g in GetComponentsInChildren<Graphic>()) g.maskable = false;
    }
    void Update()
    {
        if (!Player.aircraft.bombSight) return;

        Bombsight sight = Player.aircraft.bombSight;
        float wheelAngle = SightToWheelAngle(sight.verticalAngle);
        wheel.rotation = Quaternion.Euler(0f, 0f, wheelAngle + refAngle);
        float indicatorAngle = SightToWheelAngle(sight.HitAnglePrediction());
        indicator.localRotation = Quaternion.Euler(0f, 0f, -indicatorAngle);

        float offset = maxTraverseDistance * -sight.horizontalAngle / sight.maxSideAngle;
        traverse.localPosition = defaultTraversePos + transform.right * offset;

        //Bomb bay Toggle
        bombBayState = Player.aircraft.hydraulics.bombBay.stateInput == 0f;
        bombBayToggle.Toggle(bombBayState);

        //Bombs alert and release sequence
        bombsAlertToggle.Toggle(Player.aircraft.hydraulics.bombBay.state == 1f);
        releaseSequenceToggle.Toggle(sight.releaseSequence);

        //Interval and Amount
        intervalGauge.SetIndicatorValue(sight.intervalSelection);
        amountGauge.SetIndicatorValue(sight.amountSelection);
        modeGauge.SetIndicatorValue(sight.viewMode);
    }

    float SightToWheelAngle(float angle)
    {
        float distance = Mathf.Tan(angle * Mathf.Deg2Rad * tanSmoothing) - Mathf.Tan(Player.aircraft.bombSight.minAngle * Mathf.Deg2Rad * tanSmoothing);
        return span * distance / totalDistance;
    }

    public void DropBombs()
    {
        Player.aircraft.bombSight.StartReleaseSequence();
    }
    public void ToggleBombBay()
    {
        PlayerActions.Action("BombBay");
    }
    public void ToggleMode()
    {
        PlayerActions.Action("BombsightMode");
    }
    public void ToggleQuantity()
    {
        PlayerActions.Action("BombsightQuantity");
    }
    public void ToggleInterval()
    {
        PlayerActions.Action("BombsightInterval");
    }
}
