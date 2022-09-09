using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Pinwheel.Jupiter
{
    [CustomEditor(typeof(JSky))]
    public class JSkyInspector : Editor
    {
        private JSky sky;
        private JSkyProfile profile;

        private void OnEnable()
        {
            sky = target as JSky;
            profile = sky.Profile;
        }

        public override void OnInspectorGUI()
        {
            //sky.Profile = EditorGUILayout.ObjectField("Profile", sky.Profile, typeof(JSkyProfile), false) as JSkyProfile;
            sky.Profile = JEditorCommon.ScriptableObjectField<JSkyProfile>("Profile", sky.Profile);
            profile = sky.Profile;
            if (sky.Profile == null)
                return;

            DrawSceneReferencesGUI();
            EditorGUI.BeginChangeCheck();
            DrawSkyGUI();
            DrawStarsGUI();
            DrawSunGUI();
            DrawMoonGUI();
            DrawHorizonCloudGUI();
            DrawOverheadCloudGUI();
            DrawDetailOverlayGUI();
            DrawUtilitiesGUI();

            if (EditorGUI.EndChangeCheck())
            {
                profile.UpdateMaterialProperties();
            }

            DrawAddDayNightCycleGUI();
        }

        private void DrawSceneReferencesGUI()
        {
            string label = "Scene References";
            string id = "scene-references";

            JEditorCommon.Foldout(label, false, id, () =>
            {
                sky.SunLightSource = EditorGUILayout.ObjectField("Sun Light Source", sky.SunLightSource, typeof(Light), true) as Light;
                sky.MoonLightSource = EditorGUILayout.ObjectField("Moon Light Source", sky.MoonLightSource, typeof(Light), true) as Light;
            });
        }

        private void DrawSkyGUI()
        {
            string label = "Sky";
            string id = "sky" + profile.GetInstanceID(); ;

            JEditorCommon.Foldout(label, false, id, () =>
            {
                profile.SkyColor = EditorGUILayout.ColorField("Sky Color", profile.SkyColor);
                profile.HorizonColor = EditorGUILayout.ColorField("Horizon Color", profile.HorizonColor);
                profile.GroundColor = EditorGUILayout.ColorField("Ground Color", profile.GroundColor);
                if (profile.AllowStepEffect)
                {
                    profile.HorizonStep = EditorGUILayout.IntField("Horizon Step", profile.HorizonStep);
                }
                profile.HorizonExponent = EditorGUILayout.FloatField("Horizon Exponent", profile.HorizonExponent);
                profile.HorizonThickness = EditorGUILayout.Slider("Horizon Thickness", profile.HorizonThickness, 0f, 1f);
                profile.FogSyncOption = (JFogSyncOption)EditorGUILayout.EnumPopup("Fog Sync", profile.FogSyncOption);
                if (profile.FogSyncOption == JFogSyncOption.CustomColor)
                {
                    profile.FogColor = EditorGUILayout.ColorField("Fog Color", profile.FogColor);
                }
            });
        }

        private void DrawStarsGUI()
        {
            string label = "Stars";
            string id = "stars" + profile.GetInstanceID(); ;

            JEditorCommon.Foldout(label, false, id, () =>
            {
                profile.EnableStars = EditorGUILayout.Toggle("Enable", profile.EnableStars);
                if (profile.EnableStars)
                {
                    profile.UseBakedStars = EditorGUILayout.Toggle("Baked", profile.UseBakedStars);
                }
                if (profile.EnableStars && !profile.UseBakedStars)
                {
                    profile.StarsStartPosition = EditorGUILayout.Slider("Start", profile.StarsStartPosition, -1, 1);
                    profile.StarsEndPosition = EditorGUILayout.Slider("End", profile.StarsEndPosition, -1, 1);
                    profile.StarsOpacity = EditorGUILayout.Slider("Opacity", profile.StarsOpacity, 0f, 1f);
                    profile.StarsLayerCount = EditorGUILayout.DelayedIntField("Layers", profile.StarsLayerCount);

                    if (profile.StarsLayerCount > 0)
                    {
                        JEditorCommon.Separator();
                        EditorGUILayout.LabelField("Layer 0");
                        EditorGUI.indentLevel += 1;
                        profile.StarsColor0 = EditorGUILayout.ColorField(new GUIContent("Color"), profile.StarsColor0, true, true, true);
                        profile.StarsDensity0 = EditorGUILayout.Slider("Density", profile.StarsDensity0, 0.01f, 1f);
                        profile.StarsSize0 = EditorGUILayout.Slider("Size", profile.StarsSize0, 0.01f, 1f);
                        profile.StarsGlow0 = EditorGUILayout.Slider("Glow", profile.StarsGlow0, 0f, 1f);
                        profile.StarsTwinkle0 = EditorGUILayout.FloatField("Twinkle", profile.StarsTwinkle0);
                        EditorGUI.indentLevel -= 1;
                    }

                    if (profile.StarsLayerCount > 1)
                    {
                        JEditorCommon.Separator();
                        EditorGUILayout.LabelField("Layer 1");
                        EditorGUI.indentLevel += 1;
                        profile.StarsColor1 = EditorGUILayout.ColorField(new GUIContent("Color"), profile.StarsColor1, true, true, true);
                        profile.StarsDensity1 = EditorGUILayout.Slider("Density", profile.StarsDensity1, 0.01f, 1f);
                        profile.StarsSize1 = EditorGUILayout.Slider("Size", profile.StarsSize1, 0.01f, 1f);
                        profile.StarsGlow1 = EditorGUILayout.Slider("Glow", profile.StarsGlow1, 0f, 1f);
                        profile.StarsTwinkle1 = EditorGUILayout.FloatField("Twinkle", profile.StarsTwinkle1);
                        EditorGUI.indentLevel -= 1;
                    }

                    if (profile.StarsLayerCount > 2)
                    {
                        JEditorCommon.Separator();
                        EditorGUILayout.LabelField("Layer 2");
                        EditorGUI.indentLevel += 1;
                        profile.StarsColor2 = EditorGUILayout.ColorField(new GUIContent("Color"), profile.StarsColor2, true, true, true);
                        profile.StarsDensity2 = EditorGUILayout.Slider("Density", profile.StarsDensity2, 0.01f, 1f);
                        profile.StarsSize2 = EditorGUILayout.Slider("Size", profile.StarsSize2, 0.01f, 1f);
                        profile.StarsGlow2 = EditorGUILayout.Slider("Glow", profile.StarsGlow2, 0f, 1f);
                        profile.StarsTwinkle2 = EditorGUILayout.FloatField("Twinkle", profile.StarsTwinkle2);
                        EditorGUI.indentLevel -= 1;
                    }
                }
                if (profile.EnableStars && profile.UseBakedStars)
                {
                    profile.StarsCubemap = JEditorCommon.InlineCubemapField("Cubemap", profile.StarsCubemap, -1);
                    profile.StarsTwinkleMap = JEditorCommon.InlineTexture2DField("Twinkle Map", profile.StarsTwinkleMap, -1);
                    profile.StarsOpacity = EditorGUILayout.Slider("Opacity", profile.StarsOpacity, 0f, 1f);
                }
            });
        }

        private void DrawSunGUI()
        {
            string label = "Sun";
            string id = "sun" + profile.GetInstanceID(); ;

            JEditorCommon.Foldout(label, false, id, () =>
            {
                profile.EnableSun = EditorGUILayout.Toggle("Enable", profile.EnableSun);
                if (profile.EnableSun)
                {
                    profile.UseBakedSun = EditorGUILayout.Toggle("Baked", profile.UseBakedSun);
                }
                if (profile.EnableSun && !profile.UseBakedSun)
                {
                    profile.SunTexture = JEditorCommon.InlineTexture2DField("Texture", profile.SunTexture, -1);
                    profile.SunColor = EditorGUILayout.ColorField(new GUIContent("Color"), profile.SunColor, true, true, true);
                    profile.SunSize = EditorGUILayout.Slider("Size", profile.SunSize, 0f, 1f);
                    profile.SunSoftEdge = EditorGUILayout.Slider("Soft Edge", profile.SunSoftEdge, 0f, 1f);
                    profile.SunGlow = EditorGUILayout.Slider("Glow", profile.SunGlow, 0f, 1f);
                }
                if (profile.EnableSun && profile.UseBakedSun)
                {
                    profile.SunCubemap = JEditorCommon.InlineCubemapField("Cubemap", profile.SunCubemap, -1);
                }
                if (profile.EnableSun)
                {
                    profile.SunLightColor = EditorGUILayout.ColorField("Light Color", profile.SunLightColor);
                    profile.SunLightIntensity = EditorGUILayout.FloatField("Light Intensity", profile.SunLightIntensity);
                }
            });
        }

        private void DrawMoonGUI()
        {
            string label = "Moon";
            string id = "moon" + profile.GetInstanceID(); ;

            JEditorCommon.Foldout(label, false, id, () =>
            {
                profile.EnableMoon = EditorGUILayout.Toggle("Enable", profile.EnableMoon);
                if (profile.EnableMoon)
                {
                    profile.UseBakedMoon = EditorGUILayout.Toggle("Baked", profile.UseBakedMoon);
                }
                if (profile.EnableMoon && !profile.UseBakedMoon)
                {
                    profile.MoonTexture = JEditorCommon.InlineTexture2DField("Texture", profile.MoonTexture, -1);
                    profile.MoonColor = EditorGUILayout.ColorField(new GUIContent("Color"), profile.MoonColor, true, true, true);
                    profile.MoonSize = EditorGUILayout.Slider("Size", profile.MoonSize, 0f, 1f);
                    profile.MoonSoftEdge = EditorGUILayout.Slider("Soft Edge", profile.MoonSoftEdge, 0f, 1f);
                    profile.MoonGlow = EditorGUILayout.Slider("Glow", profile.MoonGlow, 0f, 1f);
                }
                if (profile.EnableMoon && profile.UseBakedMoon)
                {
                    profile.MoonCubemap = JEditorCommon.InlineCubemapField("Cubemap", profile.MoonCubemap, -1);
                }
                if (profile.EnableMoon)
                {
                    profile.MoonLightColor = EditorGUILayout.ColorField("Light Color", profile.MoonLightColor);
                    profile.MoonLightIntensity = EditorGUILayout.FloatField("Light Intensity", profile.MoonLightIntensity);
                }
            });
        }

        private void DrawHorizonCloudGUI()
        {
            string label = "Horizon Cloud";
            string id = "horizon-cloud" + profile.GetInstanceID(); ;

            JEditorCommon.Foldout(label, false, id, () =>
            {
                profile.EnableHorizonCloud = EditorGUILayout.Toggle("Enable", profile.EnableHorizonCloud);
                if (profile.EnableHorizonCloud)
                {
                    profile.CustomCloudTexture = JEditorCommon.InlineTexture2DField("Texture", profile.CustomCloudTexture, -1);
                    profile.HorizonCloudColor = EditorGUILayout.ColorField("Color", profile.HorizonCloudColor);
                    profile.HorizonCloudStartPosition = EditorGUILayout.Slider("Start", profile.HorizonCloudStartPosition, -1, 1);
                    profile.HorizonCloudEndPosition = EditorGUILayout.Slider("End", profile.HorizonCloudEndPosition, -1, 1);
                    profile.HorizonCloudSize = EditorGUILayout.FloatField("Size", profile.HorizonCloudSize);
                    if (profile.AllowStepEffect)
                    {
                        profile.HorizonCloudStep = EditorGUILayout.IntField("Step", profile.HorizonCloudStep);
                    }
                    profile.HorizonCloudAnimationSpeed = EditorGUILayout.FloatField("Animation Speed", profile.HorizonCloudAnimationSpeed);
                }
            });
        }

        private void DrawOverheadCloudGUI()
        {
            string label = "Overhead Cloud";
            string id = "overhead-cloud" + profile.GetInstanceID(); ;

            JEditorCommon.Foldout(label, false, id, () =>
            {
                profile.EnableOverheadCloud = EditorGUILayout.Toggle("Enable", profile.EnableOverheadCloud);
                if (profile.EnableOverheadCloud)
                {
                    profile.CustomCloudTexture = JEditorCommon.InlineTexture2DField("Texture", profile.CustomCloudTexture, -1);
                    profile.OverheadCloudColor = EditorGUILayout.ColorField("Color", profile.OverheadCloudColor);
                    profile.OverheadCloudAltitude = EditorGUILayout.FloatField("Altitude", profile.OverheadCloudAltitude);
                    profile.OverheadCloudSize = EditorGUILayout.FloatField("Size", profile.OverheadCloudSize);
                    if (profile.AllowStepEffect)
                    {
                        profile.OverheadCloudStep = EditorGUILayout.IntField("Step", profile.OverheadCloudStep);
                    }
                    profile.OverheadCloudAnimationSpeed = EditorGUILayout.FloatField("Animation Speed", profile.OverheadCloudAnimationSpeed);
                    profile.OverheadCloudFlowDirectionX = EditorGUILayout.Slider("Flow X", profile.OverheadCloudFlowDirectionX, -1, 1);
                    profile.OverheadCloudFlowDirectionZ = EditorGUILayout.Slider("Flow Z", profile.OverheadCloudFlowDirectionZ, -1, 1);
                }
            });
        }

        private void DrawDetailOverlayGUI()
        {
            string label = "Detail Overlay";
            string id = "detail-overlay" + profile.GetInstanceID(); ;

            JEditorCommon.Foldout(label, false, id, () =>
            {
                profile.EnableDetailOverlay = EditorGUILayout.Toggle("Enable", profile.EnableDetailOverlay);
                if (profile.EnableDetailOverlay)
                {
                    profile.DetailOverlayTintColor = EditorGUILayout.ColorField("Color", profile.DetailOverlayTintColor);
                    profile.DetailOverlayCubeMap = JEditorCommon.InlineCubemapField("Cubemap", profile.DetailOverlayCubeMap, -1);
                    profile.DetailOverlayLayer = (JDetailOverlayLayer)EditorGUILayout.EnumPopup("Layer", profile.DetailOverlayLayer);
                    profile.DetailOverlayRotationSpeed = EditorGUILayout.FloatField("Rotation Speed", profile.DetailOverlayRotationSpeed);
                }
            });
        }

        private void DrawUtilitiesGUI()
        {
            string label = "Utilities";
            string id = "utilities" + profile.GetInstanceID(); ;

            JEditorCommon.Foldout(label, false, id, () =>
            {
                profile.AllowStepEffect = EditorGUILayout.Toggle("Allow Step Effect", profile.AllowStepEffect);
            });
        }

        private void DrawAddDayNightCycleGUI()
        {
            JDayNightCycle cycle = sky.GetComponent<JDayNightCycle>();
            if (cycle != null)
                return;

            string label = "Day Night Cycle";
            string id = "day-night-cycle" + sky.GetInstanceID();

            JEditorCommon.Foldout(label, false, id, () =>
            {
                if (GUILayout.Button("Add Day Night Cycle"))
                {
                    sky.gameObject.AddComponent<JDayNightCycle>();
                }
            });
        }
    }
}
