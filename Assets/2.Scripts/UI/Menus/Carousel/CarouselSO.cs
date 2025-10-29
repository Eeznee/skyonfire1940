using UnityEngine;
using System;

[CreateAssetMenu(fileName = "new Carousel", menuName = "SOF/Menus/Carousel")]
[Serializable]
public class CarouselSO : ScriptableObject
{
    [Serializable]
    public class Item
    {
        public Texture2D image;
        public string footerText;
        public string url;
        public int storeCardId;
    }

    public Item[] items;
}
