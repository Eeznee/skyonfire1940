using UnityEngine;

public static class Extensions
{
    public static bool IsMobile
    {
        get
        {
#if UNITY_ANDROID
            return true;
#elif UNITY_IOS
            return true;
#else
            return false;
#endif
        }
    }
}
