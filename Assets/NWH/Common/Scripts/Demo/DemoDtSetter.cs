using UnityEditor;
using UnityEngine;

namespace NWH.Common.Demo
{
    [DefaultExecutionOrder(-500)]
    public class DemoDtSetter : MonoBehaviour
    {
        public float fixedDeltaTime = 0.008333f; // 120Hz default.

        private void Awake()
        {
#if UNITY_EDITOR
            if (fixedDeltaTime <= 0.0001f) return;
            if (Time.fixedDeltaTime <= 0.0001f) return;

            if (!EditorPrefs.GetBool("DemoDtSetter Warning"))
            {
                Debug.Log($"[Show Once] DemoDtSetter: Setting Time.fixedDeltaTime to {fixedDeltaTime} ({1f / fixedDeltaTime} Hz) " +
                          $"from the current {Time.fixedDeltaTime} ({1f / Time.fixedDeltaTime} Hz). " +
                          $"Remove the script from the __SceneManager to disable this, but note that the Sports Car damper stiffness " +
                          $"might need to be reduced to prevent jitter.");
                Time.fixedDeltaTime = fixedDeltaTime;
                EditorPrefs.SetBool("DemoDtSetter Warning", true);
            }
#endif
        }
    }
}
