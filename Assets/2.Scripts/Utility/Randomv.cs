using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Randomv
{
    public static int RandomElement(float[] elementsPickFactors,float sum)
    {
        float pick = Random.Range(0f, sum);
        for(int i = 0; i < elementsPickFactors.Length; i++)
        {
            float val = elementsPickFactors[i];
            if (pick <= val) return i;
            pick -= val;
        }
        Debug.LogError("Aucun élément aléatoire choisi, impossible");
        return 0;
    }
    public static int RandomElement(float[] elementsPickFactors)
    {
        float sum = 0f;
        foreach(float epf in elementsPickFactors)
        {
            sum += epf;
        }
        return RandomElement(elementsPickFactors, sum);
    }
}
