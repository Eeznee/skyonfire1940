using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathCheap
{
    private static float[] sqrt1000Values;

    private static void ComputeSqrt1000Values()
    {

    }
    public static float Sqrt1000()
    {
        if (sqrt1000Values.Length == 0) ComputeSqrt1000Values();
        return 0f;
    }
}
