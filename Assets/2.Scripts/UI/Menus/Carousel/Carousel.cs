using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class Carousel : MonoBehaviour
{
    public float automaticSwapInterval = 10f;

    public CarouselSO carouselData;
    public ScrollRect scrollRect;
    public Toggle toggleTemplate;
    public RawImage rawImageTemplate;
    public ToggleGroup toggleGroup;
    public GameObject storePanel;
    public GameObject patchNotes;

    private List<RawImage> images = new List<RawImage>();
    private List<CarouselSO.Item> items = new List<CarouselSO.Item>();
    private List<Toggle> radioButtons = new List<Toggle>();

    private int currentIndex;

    private float lastTimeSwapped = 0f;

    private int ItemsAmount => carouselData.items.Length;

    private void Awake()
    {
        images = new List<RawImage>();
        radioButtons = new List<Toggle>();

        foreach (CarouselSO.Item item in carouselData.items)
            CreateIndividualItem(item);

        Destroy(rawImageTemplate.gameObject);
        Destroy(toggleTemplate.gameObject);

        SelectIndex(UnityEngine.Random.Range(0, ItemsAmount));
        MagnetToTargetAnimation(true);
    }
    private void CreateIndividualItem(CarouselSO.Item item)
    {
        RawImage image = Instantiate(rawImageTemplate, scrollRect.content);
        image.texture = item.image;
        images.Add(image);

        image.GetComponentInChildren<Text>().text = item.footerText;

        items.Add(item);

        Toggle radioButton = Instantiate(toggleTemplate, toggleGroup.transform);
        radioButton.group = toggleGroup;
        radioButtons.Add(radioButton);
        radioButton.onValueChanged.AddListener(delegate { OnRadioButtonToggled(); });
    }
    private void OnRadioButtonToggled()
    {
        int newIndex = 0;

        for (int i = 0; i < radioButtons.Count; i++)
            if (radioButtons[i].isOn) newIndex = i;

        SelectIndex(newIndex);
    }
    private void SelectIndex(int newIndex)
    {
        currentIndex = Mathf.Clamp(newIndex, 0, carouselData.items.Length - 1);

        for (int i = 0; i < radioButtons.Count; i++)
            radioButtons[i].SetIsOnWithoutNotify(i == currentIndex);

        lastTimeSwapped = Time.unscaledTime;
    }

    private void Update()
    {
        if((Time.unscaledTime - lastTimeSwapped) > automaticSwapInterval)
        {
            SelectIndex((currentIndex + 1) % ItemsAmount);
        }

        MagnetToTargetAnimation(false);
    }

    private float velocity = 0f;
    private void MagnetToTargetAnimation(bool instant)
    {
        float targetPos = (float)currentIndex / (ItemsAmount - 1);

        float x;
        if (instant) 
        {
            x = targetPos;
            velocity = 0f;
        }
        else
        {
            x = Mathf.SmoothDamp(scrollRect.horizontalNormalizedPosition, targetPos, ref velocity, 0.1f, Mathf.Infinity, Time.unscaledDeltaTime);
        }

        scrollRect.horizontalNormalizedPosition = x;
    }

    public void Click()
    {
        CarouselSO.Item item = items[currentIndex];

        if (item.url == "Patch Notes")
        {
            transform.parent.gameObject.SetActive(false);
            patchNotes.SetActive(true);
            return;
        }

        if (!string.IsNullOrEmpty(item.url))
        {
            Application.OpenURL(item.url);
            return;
        }



        else
        {
            transform.parent.gameObject.SetActive(false);
            storePanel.SetActive(true);
            storePanel.GetComponentInChildren<StoreCardCarousel>(true).SelectCard(item.storeCardId);
        }
    }
}
