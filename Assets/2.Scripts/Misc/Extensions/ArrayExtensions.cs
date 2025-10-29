using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ArrayExtensions
{
    public static T[] RemoveNulls<T>(this T[] mono) where T : Component
    {
        int noneNullElements = 0;
        for (int i = 0; i < mono.Length; i++)
            if (mono[i] != null) noneNullElements++;

        T[] newMono = new T[noneNullElements];

        int counter = 0;
        for (int i = 0; i < mono.Length; i++)
            if (mono[i] != null)
            {
                newMono[counter] = mono[i];
                counter++;
            }
        return newMono;
    }
    public static T[] RemoveElements<T>(this T[] array, int removeAt, int removeCount)
    {
        T[] result = new T[array.Length - removeCount];

        for (int i = 0; i < removeAt; i++)
            result[i] = array[i];

        for (int i = removeAt; i < result.Length; i++)
            result[i] = array[i + removeCount];

        return result;
    }
}
