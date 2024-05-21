using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Vectors
{
    public static float Path(float x, params Vector2[] vectors)
    {
        for(int i = 1; i < vectors.Length; i++)
        {
            Vector2 prev = vectors[i - 1];
            Vector2 next = vectors[i];  
            
            if (x >= prev.x && x <= next.x)
            {
                float t = (x - prev.x) / (next.x - prev.x);
                return t * (next.y - prev.y) + prev.y;
            }
        }
        Debug.LogError("Vectors are not sorted");
        return 0f;
    }
}
