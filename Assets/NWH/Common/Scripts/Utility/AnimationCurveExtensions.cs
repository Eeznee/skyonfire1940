using UnityEngine;

namespace NWH.Common.Utility
{
    public static class AnimationCurveExtensions
    {
        /// <summary>
        ///     Smooths out scripting-generated AnimationCurve.
        ///     Runs only in editor.
        /// </summary>
        public static AnimationCurve MakeSmooth(this AnimationCurve inCurve)
        {
            AnimationCurve outCurve = new AnimationCurve();

            for (int i = 0; i < inCurve.keys.Length; i++)
            {
                float inTangent = 0;
                float outTangent = 0;
                bool intangentSet = false;
                bool outtangentSet = false;
                Vector2 point1;
                Vector2 point2;
                Vector2 deltapoint;
                Keyframe key = inCurve[i];

                if (i == 0)
                {
                    inTangent = 0;
                    intangentSet = true;
                }

                if (i == inCurve.keys.Length - 1)
                {
                    outTangent = 0;
                    outtangentSet = true;
                }

                if (!intangentSet)
                {
                    point1.x = inCurve.keys[i - 1].time;
                    point1.y = inCurve.keys[i - 1].value;
                    point2.x = inCurve.keys[i].time;
                    point2.y = inCurve.keys[i].value;

                    deltapoint = point2 - point1;

                    inTangent = deltapoint.y / deltapoint.x;
                }

                if (!outtangentSet)
                {
                    point1.x = inCurve.keys[i].time;
                    point1.y = inCurve.keys[i].value;
                    point2.x = inCurve.keys[i + 1].time;
                    point2.y = inCurve.keys[i + 1].value;

                    deltapoint = point2 - point1;

                    outTangent = deltapoint.y / deltapoint.x;
                }

                key.inTangent = inTangent;
                key.outTangent = outTangent;
                outCurve.AddKey(key);
            }

            return outCurve;
        }


        /// <summary>
        ///     Generates array from an AnimationCurve.
        /// </summary>
        public static float[] GenerateCurveArray(this AnimationCurve self, int resolution = 256)
        {
            float[] returnArray = new float[resolution];
            for (int j = 0; j < resolution; j++)
            {
                returnArray[j] = self.Evaluate(j / (float)resolution);
            }

            return returnArray;
        }
    }
}