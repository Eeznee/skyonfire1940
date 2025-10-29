using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformSpecific : DynamicUI
{
    public enum Platform { PC, Mobile, Android, IOS}

    public Platform platform = Platform.PC;



    public override void ResetProperties()
    {
        
        if (!UsingDesiredPlatform(platform)) DestroyImmediate(gameObject);
    }

    
    public static bool UsingDesiredPlatform(Platform platform)
    {
#if UNITY_IOS
        return platform != Platform.Android && platform != Platform.PC;
#elif UNITY_ANDROID
        return platform != Platform.IOS && platform != Platform.PC;
#else
        return platform == Platform.PC;
#endif
    }
}
