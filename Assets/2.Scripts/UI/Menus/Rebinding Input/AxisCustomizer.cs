using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine.InputSystem.Controls;


public class AxisCustomizer : MonoBehaviour
{
    [Header("UI References")]
    public Text mainLabel;
    public UIGraphRenderer uiGraph;


    [Header("Axis Input Testing")]
    public GameObject axisTestingWindow;
    public RectTransform axisGraphXIndicator;
    public RectTransform axisGraphYIndicator;
    public RectTransform inputIndicator;
    public Text inputIndicatorLabel;

    [Header("Vector2 Input Testing")]
    public GameObject vector2TestingWindow;
    public RectTransform vector2GraphIndicator;
    public RectTransform inputXIndicator;
    public RectTransform inputYIndicator;
    public Text inputXIndicatorLabel;
    public Text inputYIndicatorLabel;

    [Header("Axis Parameteres")]
    public Toggle invertToggle;
    public Slider scaleSlider;
    public Slider deadzoneSlider;
    public Slider shiftSlider;
    public Slider nonLinearitySlider;

    [Header("Vector2 Parameters")]
    public Toggle invertXToggle;
    public Toggle invertYToggle;
    public Slider scaleXSlider;
    public Slider scaleYSlider;


    [Header("Buttons")]
    public Button saveAndClose;
    public Button reset;

    private InputAction currentAction;
    private InputBinding currentBinding;
    private int currentBindingIndex;
    private AxisControl axisControl;
    private Vector2Control vector2Control;

    private void Awake()
    {
        reset.onClick.AddListener(ResetProcessors);
        saveAndClose.onClick.AddListener(SaveAndClose);

        invertToggle.onValueChanged.AddListener(delegate { UpdateUIGraph(); });
        scaleSlider.onValueChanged.AddListener(delegate { UpdateUIGraph(); });
        deadzoneSlider.onValueChanged.AddListener(delegate { UpdateUIGraph(); });
        shiftSlider.onValueChanged.AddListener(delegate { UpdateUIGraph(); });
        nonLinearitySlider.onValueChanged.AddListener(delegate { UpdateUIGraph(); });

        invertXToggle.onValueChanged.AddListener(delegate { UpdateUIGraph(); });
        invertYToggle.onValueChanged.AddListener(delegate { UpdateUIGraph(); });
        scaleXSlider.onValueChanged.AddListener(delegate { UpdateUIGraph(); });
        scaleYSlider.onValueChanged.AddListener(delegate { UpdateUIGraph(); });
    }

    private void OnEnable()
    {
        UpdateUIGraph();
    }
    private void ResetProcessors()
    {
        invertToggle.isOn = invertXToggle.isOn = invertYToggle.isOn = false;
        scaleSlider.value = scaleXSlider.value = scaleYSlider.value = 1f;
        deadzoneSlider.value = shiftSlider.value = nonLinearitySlider.value = 0f;
    }
    public void LoadBindingAndEnableAxisCustomizer(InputAction action, int bindingId)
    {
        currentAction = action;
        currentBinding = action.bindings[bindingId];
        currentBindingIndex = bindingId;

        axisControl = InputSystem.FindControl(currentBinding.effectivePath) as AxisControl;
        vector2Control = InputSystem.FindControl(currentBinding.effectivePath) as Vector2Control;

        bool vector2 = action.expectedControlType == "Vector2";
        mainLabel.text = InputSystemUtil.DisplayCamelCaseString(action.actionMap.name) + " / " + InputSystemUtil.DisplayCamelCaseString(action.name);
        mainLabel.text += vector2 ? " Dual-Axis" : " Axis";

        invertToggle.gameObject.SetActive(!vector2);
        scaleSlider.gameObject.SetActive(!vector2);
        shiftSlider.gameObject.SetActive(!vector2);
        nonLinearitySlider.gameObject.SetActive(!vector2);
        invertToggle.gameObject.SetActive(!vector2);

        invertXToggle.transform.parent.gameObject.SetActive(vector2);
        scaleXSlider.gameObject.SetActive(vector2);
        scaleYSlider.gameObject.SetActive(vector2);

        deadzoneSlider.gameObject.SetActive(true);

        axisTestingWindow.gameObject.SetActive(!vector2);
        vector2TestingWindow.gameObject.SetActive(vector2);

        if (vector2)
        {
            deadzoneSlider.value = GetStickDeadzoneMin(currentBinding);
            invertXToggle.isOn = GetInvertedX(currentBinding);
            invertYToggle.isOn = GetInvertedY(currentBinding);
            scaleXSlider.value = Mathf.Abs(GetStickScaleWithSign(currentBinding).x);
            scaleYSlider.value = Mathf.Abs(GetStickScaleWithSign(currentBinding).y);
        }
        else
        {
            deadzoneSlider.value = GetDeadzoneMin(currentBinding);
            invertToggle.isOn = GetInverted(currentBinding);
            scaleSlider.value = Mathf.Abs(GetScaleWithSign(currentBinding));
            nonLinearitySlider.value = GetNonLinearity(currentBinding);
            shiftSlider.value = GetValueShift(currentBinding);
        }

        gameObject.SetActive(true);
    }
    public void SaveAndClose()
    {
        string processors = (currentAction.expectedControlType == "Vector2") ? CompleteVector2ProcessorsString() : CompleteAxisProcessorsString();
        currentAction.ApplyBindingOverride(currentBindingIndex, new InputBinding { overridePath = currentBinding.effectivePath, overrideProcessors = processors });
        RebindingManager.instance.CloseAxisCustomizer();
    }
    public string CompleteVector2ProcessorsString()
    {
        Vector2 scale = new Vector2(scaleXSlider.value, scaleYSlider.value);
        if (invertXToggle.isOn) scale.x = -scale.x;
        if (invertYToggle.isOn) scale.y = -scale.y;

        return CompleteVector2ProcessorsString(deadzoneSlider.value, scale);
    }
    public string CompleteVector2ProcessorsString(float deadzone, Vector2 scale)
    {
        string txt = "";
        txt += "StickDeadzone(min=" + deadzone.ToString("0.00") + "),";
        txt += "ScaleVector2(x=" + scale.x.ToString("0.00") + ",y=" + scale.y.ToString("0.00") + ")";

        return txt;
    }
    public string CompleteAxisProcessorsString()
    {
        float scale = scaleSlider.value;
        if (invertToggle.isOn) scale = -scale;
        return CompleteAxisProcessorsString(deadzoneSlider.value, scale, nonLinearitySlider.value, shiftSlider.value);
    }
    public string CompleteAxisProcessorsString(float deadzone, float scale, float nonLinearity, float shift)
    {
        string txt = "";
        txt += "AxisDeadzone(min=" + deadzone.ToString("0.00") + "),";
        txt += "Scale(factor=" + scale.ToString("0.00") + "),";
        txt += "NonLinearity(nonLinearity=" + nonLinearity.ToString("0.00") + "),";
        txt += "ValueShift(valueShift=" + shift.ToString("0.00") + ")";

        return txt;
    }

    const string deadzoneMinPattern = @"AxisDeadzone\([^)]*min=(-?\d+\.?\d*)";
    const string scalePattern = @"Scale\([^)]*factor=(-?\d+\.?\d*)";
    const string nonLinearityPattern = @"NonLinearity\([^)]*nonLinearity=(-?\d+\.?\d*)";
    const string shiftPattern = @"ValueShift\([^)]*valueShift=(-?\d+\.?\d*)";

    public float GetDeadzoneMin(InputBinding binding)
    {
        return GetFloatValue(binding, "AxisDeadzone", deadzoneMinPattern, 0f);
    }
    public float GetScaleWithSign(InputBinding binding)
    {
        return GetFloatValue(binding, "Scale", scalePattern, 1f);
    }
    public float GetNonLinearity(InputBinding binding)
    {
        return GetFloatValue(binding, "NonLinearity", nonLinearityPattern, 0f);
    }
    public float GetValueShift(InputBinding binding)
    {
        return GetFloatValue(binding, "ValueShift", shiftPattern, 0f);
    }
    public bool GetInverted(InputBinding binding)
    {
        float scale = GetScaleWithSign(binding);
        return Mathf.Sign(scale) == -1f;
    }

    const string stickDeadzoneMinPattern = @"StickDeadzone\([^)]*min=(-?\d+\.?\d*)";
    const string scaleVector2XPattern = @"ScaleVector2\([^)]*x=(-?\d+\.?\d*)";
    const string scaleVector2YPattern = @"ScaleVector2\([^)]*y=(-?\d+\.?\d*)";

    public float GetStickDeadzoneMin(InputBinding binding)
    {
        return GetFloatValue(binding, "StickDeadzone", stickDeadzoneMinPattern, 0f);
    }
    public Vector2 GetStickScaleWithSign(InputBinding binding)
    {
        float x = GetFloatValue(binding, "ScaleVector2", scaleVector2XPattern, 1f);
        float y = GetFloatValue(binding, "ScaleVector2", scaleVector2YPattern, 1f);
        return new Vector2(x, y);
    }
    public bool GetInvertedX(InputBinding binding)
    {
        float scale = GetStickScaleWithSign(binding).x;
        return Mathf.Sign(scale) == -1f;
    }
    public bool GetInvertedY(InputBinding binding)
    {
        float scale = GetStickScaleWithSign(binding).y;
        return Mathf.Sign(scale) == -1f;
    }
    public float GetFloatValue(InputBinding binding, string processorName, string pattern, float defaultValue)
    {
        if (string.IsNullOrEmpty(binding.effectiveProcessors) || !binding.effectiveProcessors.Contains(processorName)) return defaultValue;

        Match match = Regex.Match(binding.effectiveProcessors, pattern);

        if (!match.Success)
            return defaultValue;
        else
            return float.Parse(match.Groups[1].Value);
    }
    public void FixAndReorderBinding(InputAction action, InputBinding binding)
    {
        string processors;
        if (action.expectedControlType == "Vector2")
        {
            float deadzone = GetStickDeadzoneMin(binding);
            Vector2 scale = GetStickScaleWithSign(binding);

            processors = CompleteVector2ProcessorsString(deadzone, scale);
        }
        else
        {
            float deadzone = GetDeadzoneMin(binding);
            float scale = GetScaleWithSign(binding);
            float nonLinearity = GetNonLinearity(binding);
            float valueShift = GetValueShift(binding);

            processors = CompleteAxisProcessorsString(deadzone, scale, nonLinearity, valueShift);
        }
        currentAction.ApplyBindingOverride(currentBindingIndex, new InputBinding { overrideProcessors = processors });
    }

    private Vector2 lastVector2;
    private void Update()
    {
        if (currentAction == null || currentBinding == null) return;
        if (currentAction.expectedControlType == "Vector2") 
        {
            if (vector2Control == null) return;

            Vector2 value = vector2Control.ReadValue();

            value = ApplyModifiers(value);
            UpdateVector2TestValues(value);
        }
        else
        {
            if (axisControl == null) return;

            float x = axisControl.ReadValue();
            float y = ApplyModifiers(x);
            Vector2 newVector = new Vector2(x, y);

            if (lastVector2 != newVector)
            {
                UpdateAxisTestValue(newVector);
                lastVector2 = newVector;
            }
        }
    }
    private void UpdateVector2TestValues(Vector2 value)
    {
        Vector3 pos = inputXIndicator.localPosition;
        pos.x = value.x * inputXIndicator.parent.GetComponent<RectTransform>().sizeDelta.x * 0.5f;
        inputXIndicator.localPosition = pos;

        pos = inputYIndicator.localPosition;
        pos.x = value.y * inputYIndicator.parent.GetComponent<RectTransform>().sizeDelta.x * 0.5f;
        inputYIndicator.localPosition = pos;

        float graphRadius = axisGraphYIndicator.parent.GetComponent<RectTransform>().sizeDelta.x * 0.5f;
        pos = vector2GraphIndicator.localPosition;
        pos.x = value.x * graphRadius;
        pos.y = value.y * graphRadius;
        vector2GraphIndicator.localPosition = pos;


        inputXIndicatorLabel.text = "X Value : " + value.x.ToString("0.00");
        inputYIndicatorLabel.text = "Y Value : " + value.y.ToString("0.00");
    }
    private void UpdateAxisTestValue(Vector2 testValues)
    {
        Vector3 pos = inputIndicator.localPosition;
        pos.x = testValues.y * inputIndicator.parent.GetComponent<RectTransform>().sizeDelta.x * 0.5f;
        inputIndicator.localPosition = pos;

        inputIndicatorLabel.text = "Test Value : " + testValues.y.ToString("0.00");

        float graphRadius = axisGraphYIndicator.parent.GetComponent<RectTransform>().sizeDelta.x * 0.5f;

        pos = axisGraphYIndicator.localPosition;
        pos.x = testValues.x * graphRadius;
        axisGraphYIndicator.localPosition = pos;

        Vector2 size = axisGraphYIndicator.sizeDelta;
        size.y = graphRadius - testValues.y * graphRadius;
        axisGraphYIndicator.sizeDelta = size;

        pos = axisGraphXIndicator.localPosition;
        pos.y = testValues.y * graphRadius;
        axisGraphXIndicator.localPosition = pos;

        size = axisGraphXIndicator.sizeDelta;
        size.x = graphRadius + testValues.x * graphRadius;
        axisGraphXIndicator.sizeDelta = size;
    }

    const int pointsCount = 128;
    public void UpdateUIGraph()
    {
        Vector2[] points = new Vector2[pointsCount];
        for (int i = 0; i < pointsCount; i++)
        {
            float x = Mathf.Lerp(-1f, 1f, (float)i / (pointsCount - 1));
            float y = ApplyModifiers(x);

            points[i] = new Vector2(x, y);
        }
        uiGraph.UpdatePlot(points);



    }
    private Vector2 ApplyModifiers(Vector2 value)
    {
        Vector2 final = value;

        float magnitude = value.magnitude;
        magnitude = Mathf.InverseLerp(deadzoneSlider.value, 1f, magnitude);
        value = value.normalized *  magnitude;

        value.x *= scaleXSlider.value;
        value.y *= scaleYSlider.value;

        if (invertXToggle) value.x = -value.x;
        if (invertYToggle) value.y = -value.y;

        value.ClampUnitSquare();
        return value;
    }
    private float ApplyModifiers(float x)
    {
        float y = x;
        y = Mathf.InverseLerp(deadzoneSlider.value, 1f, Mathf.Abs(y)) * Mathf.Sign(y);
        y *= scaleSlider.value;
        if (invertToggle.isOn) y = -y;
        y = ApplyNonLinearity(y, nonLinearitySlider.value);
        y += shiftSlider.value;
        return Mathf.Clamp(y, -1f, 1f);
    }
    public float ApplyNonLinearity(float value, float nonLinearity)
    {
        if (nonLinearity == 0f) return value;

        float abs = Mathf.Abs(value);
        float sign = Mathf.Sign(value);

        if (nonLinearity > 0f)
        {
            float func = abs * abs;
            return Mathf.Lerp(abs, func, nonLinearity) * sign;
        }
        else
        {
            if (abs > 1f) abs = 1f;
            float func = 1f - (abs - 1f) * (abs - 1f);
            return Mathf.Lerp(abs, func, -nonLinearity) * sign;
        }
    }
}
