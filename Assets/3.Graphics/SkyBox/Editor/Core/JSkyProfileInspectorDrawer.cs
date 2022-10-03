using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Pinwheel.Jupiter
{
    public class JSkyProfileInspectorDrawer
    {
        private JSkyProfile instance;

        private JSkyProfileInspectorDrawer(JSkyProfile instance)
        {
            this.instance = instance;
        }

        public static JSkyProfileInspectorDrawer Create(JSkyProfile instance)
        {
            return new JSkyProfileInspectorDrawer(instance);
        }

        public void DrawGUI()
        {
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
                instance.UpdateMaterialProperties();
            }
        }

        private void DrawSkyGUI()
        {
            string label = "Sky";
            string id = "sky" + instance.GetInstanceID(); ;

            JEditorCommon.Foldout(label, false, id, () =>
            {
                instance.SkyColor = EditorGUILayout.ColorField("Sky Color", instance.SkyColor);
                instance.HorizonColor = EditorGUILayout.ColorField("Horizon Color", instance.HorizonColor);
                instance.GroundColor = EditorGUILayout.ColorField("Ground Color", instance.GroundColor);
                if (instance.AllowStepEffect)
                {
                    instance.HorizonStep = EditorGUILayout.IntField("Horizon Step", instance.HorizonStep);
                }
                instance.HorizonExponent = EditorGUILayout.FloatField("Horizon Exponent", instance.HorizonExponent);
                instance.HorizonThickness = EditorGUILayout.Slider("Horizon Thickness", instance.HorizonThickness, 0f, 1f);
            });
        }

        private void DrawStarsGUI()
        {
            string label = "Stars";
            string id = "stars" + instance.GetInstanceID(); ;

            JEditorCommon.Foldout(label, false, id, () =>
            {
                instance.EnableStars = EditorGUILayout.Toggle("Enable", instance.EnableStars);
                if (instance.EnableStars)
                {
                    instance.UseBakedStars = EditorGUILayout.Toggle("Baked", instance.UseBakedStars);
                }
                if (instance.EnableStars && !instance.UseBakedStars)
                {
                    instance.StarsStartPosition = EditorGUILayout.Slider("Start", instance.StarsStartPosition, -1, 1);
                    instance.StarsEndPosition = EditorGUILayout.Slider("End", instance.StarsEndPosition, -1, 1);
                    instance.StarsOpacity = EditorGUILayout.Slider("Opacity", instance.StarsOpacity, 0f, 1f);
                    instance.StarsLayerCount = EditorGUILayout.DelayedIntField("Layers", instance.StarsLayerCount);

                    if (instance.StarsLayerCount > 0)
                    {
                        JEditorCommon.Separator();
                        EditorGUILayout.LabelField("Layer 0");
                        EditorGUI.indentLevel += 1;
                        instance.StarsColor0 = EditorGUILayout.ColorField(new GUIContent("Color"), instance.StarsColor0, true, true, true);
                        instance.StarsDensity0 = EditorGUILayout.Slider("Density", instance.StarsDensity0, 0.01f, 1f);
                        instance.StarsSize0 = EditorGUILayout.Slider("Size", instance.StarsSize0, 0.01f, 1f);
                        instance.StarsGlow0 = EditorGUILayout.Slider("Glow", instance.StarsGlow0, 0f, 1f);
                        instance.StarsTwinkle0 = EditorGUILayout.FloatField("Twinkle", instance.StarsTwinkle0);
                        EditorGUI.indentLevel -= 1;
                    }

                    if (instance.StarsLayerCount > 1)
                    {
                        JEditorCommon.Separator();
                        EditorGUILayout.LabelField("Layer 1");
                        EditorGUI.indentLevel += 1;
                        instance.StarsColor1 = EditorGUILayout.ColorField(new GUIContent("Color"), instance.StarsColor1, true, true, true);
                        instance.StarsDensity1 = EditorGUILayout.Slider("Density", instance.StarsDensity1, 0.01f, 1f);
                        instance.StarsSize1 = EditorGUILayout.Slider("Size", instance.StarsSize1, 0.01f, 1f);
                        instance.StarsGlow1 = EditorGUILayout.Slider("Glow", instance.StarsGlow1, 0f, 1f);
                        instance.StarsTwinkle1 = EditorGUILayout.FloatField("Twinkle", instance.StarsTwinkle1);
                        EditorGUI.indentLevel -= 1;
                    }

                    if (instance.StarsLayerCount > 2)
                    {
                        JEditorCommon.Separator();
                        EditorGUILayout.LabelField("Layer 2");
                        EditorGUI.indentLevel += 1;
                        instance.StarsColor2 = EditorGUILayout.ColorField(new GUIContent("Color"), instance.StarsColor2, true, true, true);
                        instance.StarsDensity2 = EditorGUILayout.Slider("Density", instance.StarsDensity2, 0.01f, 1f);
                        instance.StarsSize2 = EditorGUILayout.Slider("Size", instance.StarsSize2, 0.01f, 1f);
                        instance.StarsGlow2 = EditorGUILayout.Slider("Glow", instance.StarsGlow2, 0f, 1f);
                        instance.StarsTwinkle2 = EditorGUILayout.FloatField("Twinkle", instance.StarsTwinkle2);
                        EditorGUI.indentLevel -= 1;
                    }
                }
                if (instance.EnableStars && instance.UseBakedStars)
                {
                    instance.StarsCubemap = JEditorCommon.InlineCubemapField("Cubemap", instance.StarsCubemap, -1);
                    instance.StarsTwinkleMap = JEditorCommon.InlineTexture2DField("Twinkle Map", instance.StarsTwinkleMap, -1);
                    instance.StarsOpacity = EditorGUILayout.Slider("Opacity", instance.StarsOpacity, 0f, 1f);
                }
            });
        }

        private void DrawSunGUI()
        {
            string label = "Sun";
            string id = "sun" + instance.GetInstanceID(); ;

            JEditorCommon.Foldout(label, false, id, () =>
            {
                instance.EnableSun = EditorGUILayout.Toggle("Enable", instance.EnableSun);
                if (instance.EnableSun)
                {
                    instance.UseBakedSun = EditorGUILayout.Toggle("Baked", instance.UseBakedSun);
                }
                if (instance.EnableSun && !instance.UseBakedSun)
                {
                    instance.SunTexture = JEditorCommon.InlineTexture2DField("Texture", instance.SunTexture, -1);
                    instance.SunColor = EditorGUILayout.ColorField(new GUIContent("Color"), instance.SunColor, true, true, true);
                    instance.SunSize = EditorGUILayout.Slider("Size", instance.SunSize, 0f, 1f);
                    instance.SunSoftEdge = EditorGUILayout.Slider("Soft Edge", instance.SunSoftEdge, 0f, 1f);
                    instance.SunGlow = EditorGUILayout.Slider("Glow", instance.SunGlow, 0f, 1f);
                }
                if (instance.EnableSun && instance.UseBakedSun)
                {
                    instance.SunCubemap = JEditorCommon.InlineCubemapField("Cubemap", instance.SunCubemap, -1);
                }
                if (instance.EnableSun)
                {
                    instance.SunLightColor = EditorGUILayout.ColorField("Light Color", instance.SunLightColor);
                    instance.SunLightIntensity = EditorGUILayout.FloatField("Light Intensity", instance.SunLightIntensity);
                }
            });
        }

        private void DrawMoonGUI()
        {
            string label = "Moon";
            string id = "moon" + instance.GetInstanceID(); ;

            JEditorCommon.Foldout(label, false, id, () =>
            {
                instance.EnableMoon = EditorGUILayout.Toggle("Enable", instance.EnableMoon);
                if (instance.EnableMoon)
                {
                    instance.UseBakedMoon = EditorGUILayout.Toggle("Baked", instance.UseBakedMoon);
                }
                if (instance.EnableMoon && !instance.UseBakedMoon)
                {
                    instance.MoonTexture = JEditorCommon.InlineTexture2DField("Texture", instance.MoonTexture, -1);
                    instance.MoonColor = EditorGUILayout.ColorField(new GUIContent("Color"), instance.MoonColor, true, true, true);
                    instance.MoonSize = EditorGUILayout.Slider("Size", instance.MoonSize, 0f, 1f);
                    instance.MoonSoftEdge = EditorGUILayout.Slider("Soft Edge", instance.MoonSoftEdge, 0f, 1f);
                    instance.MoonGlow = EditorGUILayout.Slider("Glow", instance.MoonGlow, 0f, 1f);
                }
                if (instance.EnableMoon && instance.UseBakedMoon)
                {
                    instance.MoonCubemap = JEditorCommon.InlineCubemapField("Cubemap", instance.MoonCubemap, -1);
                }
                if (instance.EnableMoon)
                {
                    instance.MoonLightColor = EditorGUILayout.ColorField("Light Color", instance.MoonLightColor);
                    instance.MoonLightIntensity = EditorGUILayout.FloatField("Light Intensity", instance.MoonLightIntensity);
                }
            });
        }

        private void DrawHorizonCloudGUI()
        {
            string label = "Horizon Cloud";
            string id = "horizon-cloud" + instance.GetInstanceID(); ;

            JEditorCommon.Foldout(label, false, id, () =>
            {
                instance.EnableHorizonCloud = EditorGUILayout.Toggle("Enable", instance.EnableHorizonCloud);
                if (instance.EnableHorizonCloud)
                {
                    instance.CustomCloudTexture = JEditorCommon.InlineTexture2DField("Texture", instance.CustomCloudTexture, -1);
                    instance.HorizonCloudColor = EditorGUILayout.ColorField("Color", instance.HorizonCloudColor);
                    instance.HorizonCloudStartPosition = EditorGUILayout.Slider("Start", instance.HorizonCloudStartPosition, -1, 1);
                    instance.HorizonCloudEndPosition = EditorGUILayout.Slider("End", instance.HorizonCloudEndPosition, -1, 1);
                    instance.HorizonCloudSize = EditorGUILayout.FloatField("Size", instance.HorizonCloudSize);
                    if (instance.AllowStepEffect)
                    {
                        instance.HorizonCloudStep = EditorGUILayout.IntField("Step", instance.HorizonCloudStep);
                    }
                    instance.HorizonCloudAnimationSpeed = EditorGUILayout.FloatField("Animation Speed", instance.HorizonCloudAnimationSpeed);
                }
            });
        }

        private void DrawOverheadCloudGUI()
        {
            string label = "Overhead Cloud";
            string id = "overhead-cloud" + instance.GetInstanceID(); ;

            JEditorCommon.Foldout(label, false, id, () =>
            {
                instance.EnableOverheadCloud = EditorGUILayout.Toggle("Enable", instance.EnableOverheadCloud);
                if (instance.EnableOverheadCloud)
                {
                    instance.CustomCloudTexture = JEditorCommon.InlineTexture2DField("Texture", instance.CustomCloudTexture, -1);
                    instance.OverheadCloudColor = EditorGUILayout.ColorField("Color", instance.OverheadCloudColor);
                    instance.OverheadCloudAltitude = EditorGUILayout.FloatField("Altitude", instance.OverheadCloudAltitude);
                    instance.OverheadCloudSize = EditorGUILayout.FloatField("Size", instance.OverheadCloudSize);
                    if (instance.AllowStepEffect)
                    {
                        instance.OverheadCloudStep = EditorGUILayout.IntField("Step", instance.OverheadCloudStep);
                    }
                    instance.OverheadCloudAnimationSpeed = EditorGUILayout.FloatField("Animation Speed", instance.OverheadCloudAnimationSpeed);
                    instance.OverheadCloudFlowDirectionX = EditorGUILayout.Slider("Flow X", instance.OverheadCloudFlowDirectionX, -1, 1);
                    instance.OverheadCloudFlowDirectionZ = EditorGUILayout.Slider("Flow Z", instance.OverheadCloudFlowDirectionZ, -1, 1);
                }
            });
        }

        private void DrawDetailOverlayGUI()
        {
            string label = "Detail Overlay";
            string id = "detail-overlay" + instance.GetInstanceID(); ;

            JEditorCommon.Foldout(label, false, id, () =>
            {
                instance.EnableDetailOverlay = EditorGUILayout.Toggle("Enable", instance.EnableDetailOverlay);
                if (instance.EnableDetailOverlay)
                {
                    instance.DetailOverlayTintColor = EditorGUILayout.ColorField("Color", instance.DetailOverlayTintColor);
                    instance.DetailOverlayCubeMap = JEditorCommon.InlineCubemapField("Cubemap", instance.DetailOverlayCubeMap, -1);
                    instance.DetailOverlayLayer = (JDetailOverlayLayer)EditorGUILayout.EnumPopup("Layer", instance.DetailOverlayLayer);
                    instance.DetailOverlayRotationSpeed = EditorGUILayout.FloatField("Rotation Speed", instance.DetailOverlayRotationSpeed);
                }
            });
        }

        private void DrawUtilitiesGUI()
        {
            string label = "Utilities";
            string id = "utilities" + instance.GetInstanceID(); ;

            JEditorCommon.Foldout(label, false, id, () =>
            {
                instance.AllowStepEffect = EditorGUILayout.Toggle("Allow Step Effect", instance.AllowStepEffect);
            });
        }
    }
}
