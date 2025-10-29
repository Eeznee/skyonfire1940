using UnityEngine;
using System.Collections;
using System.IO;


public class Screenshot : MonoBehaviour
{
    public static void ScreenShot()
    {
        Screenshot screenshot = new GameObject().AddComponent<Screenshot>();
    }

    public void Awake()
    {
        StartCoroutine(ScreenshotCoroutine());
    }
    public IEnumerator ScreenshotCoroutine()
    {
        int screenshots = PlayerPrefs.GetInt("Screenshots", 0);
        string fileName = "Capture" + screenshots + ".png";
        screenshots++;
        PlayerPrefs.SetInt("Screenshots", screenshots);

        HideUI.instance.Toggle(true, true);

        yield return new WaitForSecondsRealtime(0.1f);

        SofAudioListener.localSource.PlayOneShot(StaticReferences.Instance.cameraShutterClip, 0.25f);

#if UNITY_EDITOR
        Directory.CreateDirectory(Application.persistentDataPath + "/Screenshots/");
        ScreenCapture.CaptureScreenshot(Application.persistentDataPath + "/Screenshots/" + fileName, 2);
#elif UNITY_IOS || UNITY_ANDROID
        Texture2D textureScreenshot = ScreenCapture.CaptureScreenshotAsTexture(1);
        NativeGallery.SaveImageToGallery(textureScreenshot.EncodeToJPG(), "SkyOnFire", fileName);
#else
        Directory.CreateDirectory(Application.persistentDataPath + "/Screenshots/");
        ScreenCapture.CaptureScreenshot(Application.persistentDataPath + "/Screenshots/"  + fileName, 2);
#endif

        yield return new WaitForSecondsRealtime(0.1f);

        HideUI.instance.Toggle(false, true);

        Destroy(gameObject);
    }
}
