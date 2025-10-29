using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaCanvas : MonoBehaviour
{
    public bool invert = false;

    private RectTransform rectTransform;

    void OnEnable()
    {
        rectTransform = GetComponent<RectTransform>();
        UpdateSafeArea();

        SofSettingsSO.OnUpdateSettings += UpdateSafeArea;
    }
    private void OnDisable()
    {
        SofSettingsSO.OnUpdateSettings -= UpdateSafeArea;
    }


    private void UpdateSafeArea()
    {
        Rect safeArea = Screen.safeArea;

        safeArea.y = 0f;
        safeArea.height = Screen.height;

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;


        if (invert)
        {
            float screenSizeReduction = anchorMax.x - anchorMin.x;

            anchorMin.x = -anchorMin.x / screenSizeReduction;
            anchorMax.x = 1f + (1f - anchorMax.x) / screenSizeReduction;

        }

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;

        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
