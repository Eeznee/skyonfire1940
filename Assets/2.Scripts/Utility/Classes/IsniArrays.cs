using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IsniArrays
{
    public static T[] RemoveElementsFromArray<T>(T[] originalArray,int removeAt, int removeCount)
    {
        T[] result = new T[originalArray.Length - removeCount];

        for (int i = 0; i < removeAt; i++)
            result[i] = originalArray[i];

        for (int i = removeAt; i < result.Length; i++)
            result[i] = originalArray[i + removeCount];

        return result;

    }
}
