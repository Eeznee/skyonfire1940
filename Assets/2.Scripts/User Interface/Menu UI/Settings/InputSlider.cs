using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputSlider : MonoBehaviour
{
    public Slider slider;
    public InputField field;

    bool wholeNumber = false;

    public void UpdateField()
    {
        field.text = slider.value.ToString();
    }
    public void UpdateSlider()
    {
        if (wholeNumber) slider.value = int.Parse(field.text);
        else{ int rounded = Mathf.RoundToInt(float.Parse(field.text)*100f);
            slider.value = rounded / 100f;
        }
    }

    private void Start()
    {
        wholeNumber = slider.wholeNumbers;
        field.contentType = wholeNumber ? InputField.ContentType.IntegerNumber : InputField.ContentType.DecimalNumber;
        UpdateField();
    }
}
