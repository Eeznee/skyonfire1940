using UnityEngine;
using UnityEngine.UI;
public class AspectRatioPanelFitter : MonoBehaviour
{
    public float aspectRatioSwitch = 1.5f;
    public float horizontalSpacing = 20f;
    public float verticalSpacing = 20f;

    public RectOffset padding;
    public float AspectRatio => GetComponent<RectTransform>().rect.width / GetComponent<RectTransform>().rect.height;

    void OnEnable()
    {
        RefreshAspectRatio();
    }
    private void RefreshAspectRatio()
    {
        HorizontalOrVerticalLayoutGroup currentLayout = GetComponent<HorizontalOrVerticalLayoutGroup>();
        if(currentLayout) DestroyImmediate(currentLayout);

        bool vertical = AspectRatio < aspectRatioSwitch;

        if (vertical)
            currentLayout = gameObject.AddComponent<VerticalLayoutGroup>();
        else
            currentLayout = gameObject.AddComponent<HorizontalLayoutGroup>();

        currentLayout.spacing = vertical ? verticalSpacing : horizontalSpacing;

        currentLayout.childForceExpandHeight = currentLayout.childControlHeight = true;
        currentLayout.childScaleHeight = currentLayout.childScaleWidth =currentLayout.childForceExpandWidth = currentLayout.childControlWidth = false;

        currentLayout.padding = padding;
        currentLayout.childAlignment = TextAnchor.MiddleCenter;
    }
}
