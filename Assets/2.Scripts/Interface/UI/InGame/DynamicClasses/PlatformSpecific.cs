using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformSpecific : MonoBehaviour
{
    public enum Platform { PC, Mobile, Android, IOS}

    public Platform platform = Platform.PC;
    void Start()
    {
#if UNITY_IOS
        if (platform == Platform.PC || platform == Platform.Android) Destroy(gameObject);
#elif UNITY_ANDROID
        if (platform == Platform.PC || platform == Platform.IOS) Destroy(gameObject);
#else
        if (platform != Platform.PC) Destroy(gameObject);
#endif
    }
}
