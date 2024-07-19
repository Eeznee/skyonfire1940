using UnityEngine;

namespace NWH.Common.Input
{
    public class InputUtils
    {
        private static int _warningCount;


        /// <summary>
        ///     Tries to get the button value through input manager, if not falls back to hardcoded default value.
        /// </summary>
        public static bool TryGetButton(string buttonName, KeyCode altKey, bool showWarning = true)
        {
            try
            {
                return UnityEngine.Input.GetButton(buttonName);
            }
            catch
            {
                // Make sure warning is not spammed as some users tend to ignore the warning and never set up the input,
                // resulting in bad performance in editor.
                if (_warningCount < 100 && showWarning)
                {
                    Debug.LogWarning(buttonName +
                                     " input binding missing, falling back to default. Check Input section in manual for more info.");
                    _warningCount++;
                }

                return UnityEngine.Input.GetKey(altKey);
            }
        }


        /// <summary>
        ///     Tries to get the button value through input manager, if not falls back to hardcoded default value.
        /// </summary>
        public static bool TryGetButtonDown(string buttonName, KeyCode altKey, bool showWarning = true)
        {
            try
            {
                return UnityEngine.Input.GetButtonDown(buttonName);
            }
            catch
            {
                if (_warningCount < 100 && showWarning)
                {
                    Debug.LogWarning(buttonName +
                                     " input binding missing, falling back to default. Check Input section in manual for more info.");
                    _warningCount++;
                }

                return UnityEngine.Input.GetKeyDown(altKey);
            }
        }


        /// <summary>
        ///     Tries to get the axis value through input manager, if not returns 0.
        /// </summary>
        public static float TryGetAxis(string axisName, bool showWarning = true)
        {
            try
            {
                return UnityEngine.Input.GetAxis(axisName);
            }
            catch
            {
                if (_warningCount < 100 && showWarning)
                {
                    Debug.LogWarning(axisName +
                                     " input binding missing. Check Input section in manual for more info.");
                    _warningCount++;
                }
            }

            return 0;
        }


        /// <summary>
        ///     Tries to get the axis value through input manager, if not returns 0.
        /// </summary>
        public static float TryGetAxisRaw(string axisName, bool showWarning = true)
        {
            try
            {
                return UnityEngine.Input.GetAxisRaw(axisName);
            }
            catch
            {
                if (_warningCount < 100 && showWarning)
                {
                    Debug.LogWarning(axisName +
                                     " input binding missing. Check Input section in manual for more info.");
                    _warningCount++;
                }
            }

            return 0;
        }
    }
}