using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class StoreCard : MonoBehaviour, IPointerClickHandler
{
    public Text descriptionLabel;
    public RawImage showcaseImage;
    public Image filter;
    public Button purchaseButton;
    public Text purchaseButtonText;


    private RectTransform rectTransform;
    private StoreCardCarousel carousel;
    void Start()
    {
        carousel = GetComponentInParent<StoreCardCarousel>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        carousel.SelectCard(this);
    }
    public float Width()
    {
        if (!rectTransform) rectTransform = GetComponent<RectTransform>();

        return rectTransform.sizeDelta.x;
    }

    public void SetFilterAlpha(float alpha)
    {
        Color color = filter.color;
        color.a = alpha;
        filter.color = color;
    }
    public void SetScale(float scale)
    {
        transform.localScale = Vector3.one * scale;
    }
    public void SetRectXPosition(float xPos)
    {
        if(!rectTransform) rectTransform = GetComponent<RectTransform>();

        Vector3 pos = rectTransform.localPosition;
        pos.x = xPos;
        rectTransform.localPosition = pos;
    }
}
