using System;
using Unity.Collections;

namespace NWH.Common.Utility
{
    public static class ArrayExtensions
    {
        public static void Fill<T>(this T[] destinationArray, params T[] value)
        {
            int destinationLength = destinationArray.Length;
            if (destinationLength == 0)
            {
                return;
            }

            int valueLength = value.Length;
            
            // set the initial array value
            Array.Copy(value, destinationArray, valueLength);

            int arrayToFillHalfLength = destinationLength / 2;
            int copyLength;

            for (copyLength = valueLength; copyLength < arrayToFillHalfLength; copyLength <<= 1)
            {
                Array.Copy(destinationArray, 0, destinationArray, copyLength, copyLength);
            }

            Array.Copy(destinationArray, 0, destinationArray, copyLength, 
                destinationLength - copyLength);
        }
    }
}