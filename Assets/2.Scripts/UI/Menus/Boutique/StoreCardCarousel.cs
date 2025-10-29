using UnityEngine;
using UnityEngine.EventSystems;

public class StoreCardCarousel : MonoBehaviour, IDragHandler, IEndDragHandler
{
    private StoreCard[] storedCardsInOrder;
    private int currentStoreCardId;
    private StoreCard CurrentCard => storedCardsInOrder[currentStoreCardId];


    public float dragSensitivity = 0.02f;


    public float backgroundAlphaMultiplier = 0.5f;
    public float baseOffset = 400f;
    public float scaleDown = 0.7f;
    public float swapOffset = 20f;
    private float offsetFactor;

    public float spring = 1f;
    public float damper = 1f;

    private float currentAnimationLevel;
    private int currentFrontCard;

    private float velocity = 0f;
    private bool dragged;

    public void OnDrag(PointerEventData eventData)
    {
        dragged = true;

        currentAnimationLevel += -eventData.delta.x * dragSensitivity / Screen.width;
        AnimateInstant(currentAnimationLevel);
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        currentStoreCardId = Mathf.Clamp(Mathf.RoundToInt(currentAnimationLevel), 0, storedCardsInOrder.Length - 1);
        velocity = 0f;
        dragged = false;
    }

    private void Awake()
    {
        storedCardsInOrder = GetComponentsInChildren<StoreCard>();
        currentStoreCardId = 0;
        dragged = false;

        offsetFactor = baseOffset / (1f - scaleDown);

        AnimateInstant(0f);
        ReorderCards(currentFrontCard);
    }

    private void Update()
    {
        if (dragged) return;

        float force = (currentStoreCardId - currentAnimationLevel) * spring;
        velocity += force * Time.unscaledDeltaTime;
        velocity = Mathf.MoveTowards(velocity, 0f, damper * Mathf.Abs(velocity) * Time.unscaledDeltaTime);
        currentAnimationLevel += velocity * Time.unscaledDeltaTime;

        AnimateInstant(currentAnimationLevel);
    }
    public void SelectCard(int index)
    {
        currentStoreCardId = index;
    }
    public void SelectCard(StoreCard card)
    {
        for(int i = 0; i < storedCardsInOrder.Length; i++)
        {
            if(storedCardsInOrder[i] == card)
            {
                currentStoreCardId = i;
            }
        }
    }
    private void AnimateInstant(float animationLevel)
    {
        currentAnimationLevel = animationLevel;

        int newFrontCard = Mathf.Clamp(Mathf.RoundToInt(currentAnimationLevel), 0, storedCardsInOrder.Length - 1);
        if (currentFrontCard != newFrontCard)
            ReorderCards(newFrontCard);

        for (int i = 0; i < storedCardsInOrder.Length; i++)
        {
            StoreCard card = storedCardsInOrder[i];
            float distance = Mathf.Abs(i - currentAnimationLevel);
            float sign = Mathf.Sign(i - currentAnimationLevel);

            card.SetFilterAlpha(1f - Mathf.Pow(backgroundAlphaMultiplier, distance));
            card.SetScale(Mathf.Pow(scaleDown, distance));

            float xPos = offsetFactor * sign * (1f - Mathf.Pow(scaleDown, distance));
            if (distance < 1f && currentAnimationLevel > 0f && currentAnimationLevel < storedCardsInOrder.Length - 1)
            {
                float t = Mathf.Abs(distance - 0.5f) * 2f;
                t = Mathv.SmoothStep(t, 4);
                xPos = Mathf.Lerp(sign * swapOffset, xPos, t);
            }
            card.SetRectXPosition(xPos);

            card.purchaseButton.interactable = i == currentFrontCard;
        }
    }
    private void ReorderCards(int frontCardId)
    {
        currentFrontCard = frontCardId;

        for (int i = currentFrontCard; i < storedCardsInOrder.Length; i++)
            storedCardsInOrder[i].transform.SetAsFirstSibling();
        for (int i = currentFrontCard - 1; i >= 0; i--)
            storedCardsInOrder[i].transform.SetAsFirstSibling();
    }
}
