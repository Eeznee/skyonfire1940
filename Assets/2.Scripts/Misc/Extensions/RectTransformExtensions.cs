using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RectTransformExtensions
{
    public static void SetLeft(this RectTransform rt, float left)
    {
        rt.offsetMin = new Vector2(left, rt.offsetMin.y);
    }

    public static void SetRight(this RectTransform rt, float right)
    {
        rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
    }

    public static void SetTop(this RectTransform rt, float top)
    {
        rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
    }

    public static void SetBottom(this RectTransform rt, float bottom)
    {
        rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
    }

    public static Rect CalculateBoundingBox(params RectTransform[] elements)
    {
        Vector3 min = Vector3.positiveInfinity;
        Vector3 max = Vector3.negativeInfinity;

        foreach (RectTransform rectTransform in elements)
        {
            Vector3[] worldCorners = new Vector3[4];
            rectTransform.GetWorldCorners(worldCorners);

            for (int i = 0; i < 4; i++)
            {
                min = Vector3.Min(min, worldCorners[i]);
                max = Vector3.Max(max, worldCorners[i]);
            }
        }
        return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
    }
    public static float RectsOverlapFactor(Rect rect1, Rect rect2)
    {
        if (!rect1.Overlaps(rect2))
        {
            return 0f; 
        }

        float intersectionWidth = Mathf.Max(0f, Mathf.Min(rect1.xMax, rect2.xMax) - Mathf.Max(rect1.xMin, rect2.xMin));
        float intersectionHeight = Mathf.Max(0f, Mathf.Min(rect1.yMax, rect2.yMax) - Mathf.Max(rect1.yMin, rect2.yMin));

        float intersectionArea = intersectionWidth * intersectionHeight;

        float minArea = Mathf.Min(rect1.width * rect1.height, rect2.width * rect2.height);
        return Mathf.Clamp01(intersectionArea / minArea);
    }
}
