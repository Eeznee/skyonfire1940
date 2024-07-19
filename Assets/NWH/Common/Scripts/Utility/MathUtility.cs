using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NWH.Common
{
    public static class MathUtility
    {
        /// <summary>
        /// Clamps the input value 'x' to the specified range '[-range, range]' and returns the signed remainder if 'x' is outside the range.
        /// </summary>
        /// <param name="x">The float value to be clamped.</param>
        /// <param name="range">The float value defining the range limits (-range, range).</param>
        /// <returns>The clamped value of 'x' within the specified range and the signed remainder if 'x' is outside the range.</returns>
        public static void ClampWithRemainder(ref float x, in float range, out float remainder)
        {
            if (x > range)
            {
                remainder = x - range;
                x = range;
            }
            else if (x < -range)
            {
                remainder = x + range;
                x = -range;
            }
            else
            {
                remainder = 0;
            }
        }
    }
}

