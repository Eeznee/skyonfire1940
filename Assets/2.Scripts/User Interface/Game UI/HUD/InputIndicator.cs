using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputIndicator : MonoBehaviour
{
    public RectTransform corrected;
    public RectTransform raw;
    public RectTransform yaw;

    private float width;

    private void Start()
    {
        width = GetComponent<RectTransform>().sizeDelta.x;
    }
    private Vector2 Input(bool corrected)
    {
        AircraftAxes axis = corrected ? Player.aircraft.controls.current : Player.aircraft.controls.rawUncorrected;
        return new Vector2(axis.roll,-axis.pitch);
    }
    private void Update()
    {
        if (!Player.aircraft) return;


        corrected.anchoredPosition = Input(true) * width * 0.5f;
        raw.anchoredPosition = Input(false) * width * 0.5f;
        yaw.anchoredPosition = new Vector2(-Player.aircraft.controls.current.yaw, 0f) * width * 0.5f;
    }
}
