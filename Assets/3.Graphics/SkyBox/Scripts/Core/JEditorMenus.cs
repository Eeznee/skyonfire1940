using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Pinwheel.Jupiter
{
    public static class JEditorMenus
    {
        [MenuItem("GameObject/3D Object/Jupiter Sky/Sunny Day")]
        public static JSky CreateSunnyDaySky(MenuCommand cmd)
        {
            GameObject g = new GameObject("Sunny Day Sky");
            if (cmd != null && cmd.context != null)
            {
                GameObject root = cmd.context as GameObject;
                GameObjectUtility.SetParentAndAlign(g, root);
            }

            JSky skyComponent = g.AddComponent<JSky>();
            JSkyProfile profile = JSkyProfile.CreateInstance<JSkyProfile>();
            string fileName = "SkyProfile-" + JCommon.GetUniqueID();
            string filePath = string.Format("Assets/{0}.asset", fileName);
            AssetDatabase.CreateAsset(profile, filePath);

            profile.CopyFrom(JJupiterSettings.Instance.DefaultProfileSunnyDay);
            skyComponent.Profile = profile;
            skyComponent.MoonLightSource = null;

            Light[] lights = Object.FindObjectsOfType<Light>();
            for (int i = 0; i < lights.Length; ++i)
            {
                if (lights[i].type == LightType.Directional)
                {
                    skyComponent.SunLightSource = lights[i];
                    break;
                }
            }

            return skyComponent;
        }

        [MenuItem("GameObject/3D Object/Jupiter Sky/Starry Night")]
        public static JSky CreateStarryNightSky(MenuCommand cmd)
        {
            GameObject g = new GameObject("Starry Night Sky");
            if (cmd != null && cmd.context != null)
            {
                GameObject root = cmd.context as GameObject;
                GameObjectUtility.SetParentAndAlign(g, root);
            }

            JSky skyComponent = g.AddComponent<JSky>();
            JSkyProfile profile = JSkyProfile.CreateInstance<JSkyProfile>();
            string fileName = "SkyProfile-" + JCommon.GetUniqueID();
            string filePath = string.Format("Assets/{0}.asset", fileName);
            AssetDatabase.CreateAsset(profile, filePath);

            profile.CopyFrom(JJupiterSettings.Instance.DefaultProfileStarryNight);
            skyComponent.Profile = profile;
            skyComponent.SunLightSource = null;

            Light[] lights = Object.FindObjectsOfType<Light>();
            for (int i = 0; i < lights.Length; ++i)
            {
                if (lights[i].type == LightType.Directional)
                {
                    skyComponent.MoonLightSource = lights[i];
                    break;
                }
            }

            return skyComponent;
        }

        [MenuItem("Window/Jupiter/Tools/Cubemap Creator")]
        public static void ShowCubemapCreator()
        {
            JCubemapCreatorWindow.ShowWindow();
        }

        [MenuItem("Window/Jupiter/Project/Settings")]
        public static void ShowSettings()
        {
            Selection.activeObject = JJupiterSettings.Instance;
        }

        [MenuItem("Window/Jupiter/Project/Version Info")]
        public static void ShowVersionInfo()
        {
            EditorUtility.DisplayDialog(
                "Version Info",
                JVersionInfo.ProductNameAndVersion,
                "OK");
        }

        [MenuItem("Window/Jupiter/Project/Update Dependencies")]
        public static void UpdateDependencies()
        {
            JPackageInitializer.Init();
        }

        [MenuItem("Window/Jupiter/Learning Resources/Youtube Channel")]
        public static void ShowYoutubeChannel()
        {
            Application.OpenURL(JCommon.YOUTUBE_CHANNEL);
        }

        [MenuItem("Window/Jupiter/Learning Resources/Online Manual")]
        public static void ShowOnlineManual()
        {
            Application.OpenURL(JCommon.ONLINE_MANUAL);
        }

        [MenuItem("Window/Jupiter/Community/Forum")]
        public static void ShowForum()
        {
            Application.OpenURL(JCommon.FORUM);
        }

        [MenuItem("Window/Jupiter/Community/Discord")]
        public static void ShowDiscord()
        {
            Application.OpenURL(JCommon.DISCORD);
        }

        [MenuItem("Window/Jupiter/Contact/Support")]
        public static void ShowSupportEmailEditor()
        {
            JEditorCommon.OpenEmailEditor(
                JCommon.SUPPORT_EMAIL,
                "[Jupiter] SHORT_QUESTION_HERE",
                "YOUR_QUESTION_IN_DETAIL");
        }

        [MenuItem("Window/Jupiter/Contact/Business")]
        public static void ShowBusinessEmailEditor()
        {
            JEditorCommon.OpenEmailEditor(
                JCommon.BUSINESS_EMAIL,
                "[Jupiter] SHORT_MESSAGE_HERE",
                "YOUR_MESSAGE_IN_DETAIL");
        }

        [MenuItem("Window/Jupiter/Leave a Review")]
        public static void OpenStorePage()
        {
            Application.OpenURL("http://u3d.as/1Hry");
        }
    }
}
