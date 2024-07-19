#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace NWH.Common.AssetInfo
{
    public class CommonInitializationMethods
    {
        public static void AddDefines(string symbol)
        {
            string currentSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            string newSymbols = string.Join(";", new HashSet<string>(currentSymbols.Split(';')) { symbol });
            if (currentSymbols != newSymbols)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newSymbols);
            }
        }

        public static void ShowWelcomeWindow(string assetName)
        {
            if (!GetAssetInfo(assetName, out AssetInfo assetInfo)) return;

            string key = $"{assetInfo.assetName}_{assetInfo.version}_WW"; // Welcome Window key
            if (EditorPrefs.GetBool(key, false) == false)
            {
                EditorPrefs.SetBool(key, true);

                ConstructWelcomeWindow(assetInfo);
            }
        }


        private static void ConstructWelcomeWindow(AssetInfo assetInfo)
        {
            WelcomeMessageWindow window = (WelcomeMessageWindow)EditorWindow.GetWindow(typeof(WelcomeMessageWindow));
            window.assetInfo = assetInfo;
            window.titleContent.text = assetInfo.assetName;
            window.Show();
        }


        private static bool GetAssetInfo(string assetName, out AssetInfo assetInfo)
        {
            string assetInfoPath = $"{assetName}/{assetName} AssetInfo";

            assetInfo = Resources.Load(assetInfoPath) as AssetInfo;
            if (assetInfo == null)
            {
                Debug.LogWarning($"Could not load Asset Info at path {assetInfoPath}");
                return false;
            }
            return true;
        }
    }
}
#endif
