#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


namespace NWH.Common.AssetInfo
{
    public class WelcomeMessageWindow : EditorWindow
    {
        public AssetInfo assetInfo;

        public static void DrawWelcomeMessage(AssetInfo assetInfo, float width = 300f)
        {
            if (assetInfo == null)
            {
                Debug.LogWarning("AssetInfo is null");
                return;
            }

            GUIStyle style = new GUIStyle(EditorStyles.helpBox);
            style.margin = new RectOffset(10, 10, 10, 12);
            style.padding = new RectOffset(10, 10, 10, 12);

            GUILayout.BeginVertical(style, GUILayout.Width(width - 35f));
            GUILayout.Space(8);
            GUILayout.Label($"Welcome to {assetInfo.assetName}", EditorStyles.boldLabel);
            GUILayout.Space(15);
            GUILayout.Label($"Thank you for purchasing {assetInfo.assetName}.\n" +
                            "Check out the following links:");
            GUILayout.Space(10);
            GUILayout.Label("Existing customer?", EditorStyles.centeredGreyMiniLabel);
            if (GUILayout.Button("Upgrade Notes"))
            {
                Application.OpenURL(assetInfo.upgradeNotesURL);
            }

            if (GUILayout.Button("Changelog"))
            {
                Application.OpenURL(assetInfo.changelogURL);
            }

            GUILayout.Space(5);
            GUILayout.Label("New to the asset?", EditorStyles.centeredGreyMiniLabel);
            if (GUILayout.Button("Quick Start"))
            {
                Application.OpenURL(assetInfo.quickStartURL);
            }


            if (GUILayout.Button("Documentation"))
            {
                Application.OpenURL(assetInfo.documentationURL);
            }

            GUILayout.Space(15);
            GUILayout.Label("Also, don't forget to join us at Discord:", EditorStyles.centeredGreyMiniLabel);
            if (GUILayout.Button("Discord Server"))
            {
                Application.OpenURL(assetInfo.discordURL);
            }

            GUILayout.Space(15);
            GUILayout.Label("Don't have Discord? You can also contact us through:", EditorStyles.centeredGreyMiniLabel);

            if (GUILayout.Button("Email"))
            {
                Application.OpenURL(assetInfo.emailURL);
            }

            if (GUILayout.Button("Forum"))
            {
                Application.OpenURL(assetInfo.forumURL);
            }


            GUILayout.Space(15);
            GUILayout.Label("Enjoying the asset? Please consider leaving a review, \n" +
                            "it means a lot to us developers. Thank you.", EditorStyles.centeredGreyMiniLabel);
            if (GUILayout.Button("Leave a Review"))
            {
                Application.OpenURL(assetInfo.assetURL);
            }

            GUILayout.EndVertical();
        }

        private void OnGUI()
        {
            if (assetInfo == null)
            {
                return;
            }

            DrawWelcomeMessage(assetInfo, 280f);
        }
    }
}
#endif