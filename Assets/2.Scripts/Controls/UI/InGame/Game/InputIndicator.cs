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
        Vector3 ogControls = corrected ? Player.aircraft.controlValue : Player.aircraft.controlUncorrected;
        return new Vector2(ogControls.z,-ogControls.x);
    }
    private void Update()
    {
        if (!Player.aircraft) return;


        corrected.anchoredPosition = Input(true) * width * 0.5f;
        raw.anchoredPosition = Input(false) * width * 0.5f;
        yaw.anchoredPosition = new Vector2(-Player.aircraft.controlValue.y, 0f) * width * 0.5f;
    }
}
