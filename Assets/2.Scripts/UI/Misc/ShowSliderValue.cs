using UnityEngine;
using UnityEngine.UI;

public class ShowSliderValue : MonoBehaviour
{
    public Slider slider;
    public Text text;

    public string format = "0.0";
    public string unit = "";

    private void Start()
    {
        if (!slider || !text) Debug.LogError("Missing reference");

        slider.onValueChanged.AddListener(OnSliderChanged);

        OnSliderChanged(slider.value);
    }
    public string Format(float value)
    {
        if (format == "0.02")
        {
            value = value / 0.02f;
            value = Mathf.Round(value) * 0.02f;
            return value.ToString("0.00");
        }
        else if (format == "0.05")
        {
            value = value / 0.05f;
            value = Mathf.Round(value) * 0.05f;
            return value.ToString("0.00");
        }
        else if (format == "0.2")
        {
            value = value / 0.2f;
            value = Mathf.Round(value) * 0.2f;
            return value.ToString("0.0");
        }
        else if (format == "0.5")
        {
            value = value / 0.5f;
            value = Mathf.Round(value) * 0.5f;
            return value.ToString("0.0");
        }
        else if (format == "2")
        {
            value = value / 2f;
            value = Mathf.Round(value) * 2f;
            return value.ToString("0");
        }
        else if (format == "5")
        {
            value = value / 5f;
            value = Mathf.Round(value) * 5f;
            return value.ToString("0");
        }
        else if (format == "50")
        {
            value = value / 50f;
            value = Mathf.Round(value) * 50f;
            return value.ToString("0");
        }
        return value.ToString(format);
    }
    private void OnSliderChanged(float number)
    {
        string formatted = Format(number);
        number = float.Parse(formatted);

        slider.SetValueWithoutNotify(number);

        if (unit == "AM")
        {
            bool pm = number > 11f && number < 24f;
            number = number % 12;
            if (number == 0f) number = 12f;
            text.text = Format(number) + (pm ? " PM" : " AM");
        }
        else
        {
            text.text = formatted + " " + unit;
        }

    }
}
