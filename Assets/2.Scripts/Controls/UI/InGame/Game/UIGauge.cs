using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGauge : MonoBehaviour
{
    public float minDegree;
    public float maxDegree;
    public float minValue = 0;
    public float maxValue = 100;
    public string format;

    public bool touchControl;

    public int markings = 50;
    public int labeledMarkingsRate = 10;

    public bool customMarkings;
    public string[] labels;

    public RectTransform indicator;
    public Image baseMarking;
    public Image baseLabeledMarking;
    public Text baseLabel;


    void Start()
    {
        if (customMarkings)
        {
            for (int i = 0; i < labels.Length; i++)
            {
                float angle = Mathf.Lerp(minDegree, maxDegree, (float)i/(labels.Length-1));
                Vector3 position = Vector3.zero;
                position.x = Mathf.Cos(angle * Mathf.Deg2Rad);
                position.y = Mathf.Sin(angle * Mathf.Deg2Rad);

                Image marking = Instantiate(baseMarking, transform);
                marking.transform.localPosition = position * baseMarking.transform.localPosition.x;
                marking.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
                Text label = Instantiate(baseLabel, transform);
                label.transform.localPosition = position * baseLabel.transform.localPosition.x;
                label.text = labels[i];
            }
        }
        else
        {
            for (int i = 0; i <= markings; i++)
            {
                float angle = Mathf.Lerp(minDegree, maxDegree, (float)i / markings);
                Vector3 position = Vector3.zero;
                position.x = Mathf.Cos(angle * Mathf.Deg2Rad);
                position.y = Mathf.Sin(angle * Mathf.Deg2Rad);
                if (i % labeledMarkingsRate == 0)
                {
                    Image marking = Instantiate(baseLabeledMarking, transform);
                    marking.transform.localPosition = position * baseLabeledMarking.transform.localPosition.x;
                    marking.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
                    Text label = Instantiate(baseLabel, transform);
                    label.transform.localPosition = position * baseLabel.transform.localPosition.x;
                    float value = Mathf.Lerp(minValue, maxValue, (float)i / markings);
                    label.text = value.ToString(format);
                }
                else
                {
                    Image marking = Instantiate(baseMarking, transform);
                    marking.transform.localPosition = position * baseMarking.transform.localPosition.x;
                    marking.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
                }
            }
        }

        Destroy(baseMarking);
        Destroy(baseLabeledMarking);
        Destroy(baseLabel);
    }

    private void LateUpdate()
    {
        if (!touchControl) return;
        float angle = indicator.localRotation.eulerAngles.z;
        angle = 0f;
        //indicator.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void SetIndicatorValue(float value)
    {
        float ratio = Mathf.InverseLerp(minValue, maxValue, value);
        float angle = Mathf.Lerp(minDegree,maxDegree,ratio);
        indicator.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}
