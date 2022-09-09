using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Rendering;

namespace Pinwheel.Jupiter
{
    public static class JMat
    {
        public const string NOISE_TEX = "_NoiseTex";
        public const string CLOUD_TEX = "_CloudTex";

        public const string SKY_COLOR = "_SkyColor";
        public const string HORIZON_COLOR = "_HorizonColor";
        public const string GROUND_COLOR = "_GroundColor";
        public const string HORIZON_THICKNESS = "_HorizonThickness";
        public const string HORIZON_EXPONENT = "_HorizonExponent";
        public const string HORIZON_STEP = "_HorizonStep";

        public const string KW_STARS = "STARS";
        public const string KW_STARS_LAYER_0 = "STARS_LAYER_0";
        public const string KW_STARS_LAYER_1 = "STARS_LAYER_1";
        public const string KW_STARS_LAYER_2 = "STARS_LAYER_2";
        public const string STARS_START = "_StarsStart";
        public const string STARS_END = "_StarsEnd";
        public const string STARS_OPACITY = "_StarsOpacity";
        public const string STARS_COLOR_0 = "_StarsColor0";
        public const string STARS_COLOR_1 = "_StarsColor1";
        public const string STARS_COLOR_2 = "_StarsColor2";
        public const string STARS_DENSITY_0 = "_StarsDensity0";
        public const string STARS_DENSITY_1 = "_StarsDensity1";
        public const string STARS_DENSITY_2 = "_StarsDensity2";
        public const string STARS_SIZE_0 = "_StarsSize0";
        public const string STARS_SIZE_1 = "_StarsSize1";
        public const string STARS_SIZE_2 = "_StarsSize2";
        public const string STARS_GLOW_0 = "_StarsGlow0";
        public const string STARS_GLOW_1 = "_StarsGlow1";
        public const string STARS_GLOW_2 = "_StarsGlow2";
        public const string STARS_TWINKLE_0 = "_StarsTwinkle0";
        public const string STARS_TWINKLE_1 = "_StarsTwinkle1";
        public const string STARS_TWINKLE_2 = "_StarsTwinkle2";

        public const string KW_STARS_BAKED = "STARS_BAKED";
        public const string STARS_CUBEMAP = "_StarsCubemap";
        public const string STARS_TWINKLE_MAP = "_StarsTwinkleMap";

        public const string KW_SUN = "SUN";
        public const string KW_SUN_USE_TEXTURE = "SUN_USE_TEXTURE";
        public const string SUN_TEX = "_SunTex";
        public const string SUN_COLOR = "_SunColor";
        public const string SUN_SIZE = "_SunSize";
        public const string SUN_SOFT_EDGE = "_SunSoftEdge";
        public const string SUN_GLOW = "_SunGlow";
        public const string SUN_DIRECTION = "_SunDirection";
        public const string SUN_TRANSFORM_MATRIX = "_PositionToSunUV";

        public const string KW_SUN_BAKED = "SUN_BAKED";
        public const string SUN_CUBEMAP = "_SunCubemap";
        public const string SUN_ROTATION_MATRIX = "_SunRotationMatrix";

        public const string KW_MOON = "MOON";
        public const string KW_MOON_USE_TEXTURE = "MOON_USE_TEXTURE";
        public const string MOON_TEX = "_MoonTex";
        public const string MOON_COLOR = "_MoonColor";
        public const string MOON_SIZE = "_MoonSize";
        public const string MOON_SOFT_EDGE = "_MoonSoftEdge";
        public const string MOON_GLOW = "_MoonGlow";
        public const string MOON_DIRECTION = "_MoonDirection";
        public const string MOON_TRANSFORM_MATRIX = "_PositionToMoonUV";

        public const string KW_MOON_BAKED = "MOON_BAKED";
        public const string MOON_CUBEMAP = "_MoonCubemap";
        public const string MOON_ROTATION_MATRIX = "_MoonRotationMatrix";

        public const string KW_HORIZON_CLOUD = "HORIZON_CLOUD";
        public const string HORIZON_CLOUD_COLOR = "_HorizonCloudColor";
        public const string HORIZON_CLOUD_START = "_HorizonCloudStart";
        public const string HORIZON_CLOUD_END = "_HorizonCloudEnd";
        public const string HORIZON_CLOUD_SIZE = "_HorizonCloudSize";
        public const string HORIZON_CLOUD_STEP = "_HorizonCloudStep";
        public const string HORIZON_CLOUD_ANIMATION_SPEED = "_HorizonCloudAnimationSpeed";

        public const string KW_OVERHEAD_CLOUD = "OVERHEAD_CLOUD";
        public const string OVERHEAD_CLOUD_COLOR = "_OverheadCloudColor";
        public const string OVERHEAD_CLOUD_ALTITUDE = "_OverheadCloudAltitude";
        public const string OVERHEAD_CLOUD_SIZE = "_OverheadCloudSize";
        public const string OVERHEAD_CLOUD_STEP = "_OverheadCloudStep";
        public const string OVERHEAD_CLOUD_ANIMATION_SPEED = "_OverheadCloudAnimationSpeed";
        public const string OVERHEAD_CLOUD_FLOW_X = "_OverheadCloudFlowX";
        public const string OVERHEAD_CLOUD_FLOW_Z = "_OverheadCloudFlowZ";

        public const string KW_DETAIL_OVERLAY = "DETAIL_OVERLAY";
        public const string KW_DETAIL_OVERLAY_ROTATION = "DETAIL_OVERLAY_ROTATION";
        public const string DETAIL_OVERLAY_COLOR = "_DetailOverlayColor";
        public const string DETAIL_OVERLAY_CUBEMAP = "_DetailOverlayCubemap";
        public const string DETAIL_OVERLAY_LAYER = "_DetailOverlayLayer";
        public const string DETAIL_OVERLAY_ROTATION_SPEED = "_DetailOverlayRotationSpeed";

        public const string KW_ALLOW_STEP_EFFECT = "ALLOW_STEP_EFFECT";

        private static Material activeMaterial;

        public static void SetActiveMaterial(Material mat)
        {
            activeMaterial = mat;
        }

        public static void GetColor(string prop, ref Color value)
        {
            try
            {
                if (activeMaterial.HasProperty(prop))
                {
                    value = activeMaterial.GetColor(prop);
                }
            }
            catch (NullReferenceException nullEx)
            {
                Debug.LogError(nullEx.ToString());
            }
            catch { }
        }

        public static void GetFloat(string prop, ref float value)
        {
            try
            {
                if (activeMaterial.HasProperty(prop))
                {
                    value = activeMaterial.GetFloat(prop);
                }
            }
            catch (NullReferenceException nullEx)
            {
                Debug.LogError(nullEx.ToString());
            }
            catch { }
        }

        public static void GetVector(string prop, ref Vector4 value)
        {
            try
            {
                if (activeMaterial.HasProperty(prop))
                {
                    value = activeMaterial.GetVector(prop);
                }
            }
            catch (NullReferenceException nullEx)
            {
                Debug.LogError(nullEx.ToString());
            }
            catch { }
        }

        public static void GetTexture(string prop, ref Texture value)
        {
            try
            {
                if (activeMaterial.HasProperty(prop))
                {
                    value = activeMaterial.GetTexture(prop);
                }
            }
            catch (NullReferenceException nullEx)
            {
                Debug.LogError(nullEx.ToString());
            }
            catch { }
        }

        public static void GetKeywordEnabled(string kw, ref bool value)
        {
            try
            {
                value = activeMaterial.IsKeywordEnabled(kw);
            }
            catch (NullReferenceException nullEx)
            {
                Debug.LogError(nullEx.ToString());
            }
            catch { }
        }

        public static void SetColor(string prop, Color value)
        {
            try
            {
                if (activeMaterial.HasProperty(prop))
                {
                    activeMaterial.SetColor(prop, value);
                }
            }
            catch (NullReferenceException nullEx)
            {
                Debug.LogError(nullEx.ToString());
            }
            catch { }
        }

        public static void SetFloat(string prop, float value)
        {
            try
            {
                if (activeMaterial.HasProperty(prop))
                {
                    activeMaterial.SetFloat(prop, value);
                }
            }
            catch (NullReferenceException nullEx)
            {
                Debug.LogError(nullEx.ToString());
            }
            catch { }
        }

        public static void SetVector(string prop, Vector4 value)
        {
            try
            {
                if (activeMaterial.HasProperty(prop))
                {
                    activeMaterial.SetVector(prop, value);
                }
            }
            catch (NullReferenceException nullEx)
            {
                Debug.LogError(nullEx.ToString());
            }
            catch { }
        }

        public static void SetTexture(string prop, Texture value)
        {
            try
            {
                if (activeMaterial.HasProperty(prop))
                {
                    activeMaterial.SetTexture(prop, value);
                }
            }
            catch (NullReferenceException nullEx)
            {
                Debug.LogError(nullEx.ToString());
            }
            catch { }
        }

        public static void SetMatrix(string prop, Matrix4x4 value)
        {
            try
            {
                if (activeMaterial.HasProperty(prop))
                {
                    activeMaterial.SetMatrix(prop, value);
                }
            }
            catch (NullReferenceException nullEx)
            {
                Debug.LogError(nullEx.ToString());
            }
            catch { }
        }

        public static void SetKeywordEnable(string kw, bool enable)
        {
            try
            {
                if (enable)
                {
                    activeMaterial.EnableKeyword(kw);
                }
                else
                {
                    activeMaterial.DisableKeyword(kw);
                }
            }
            catch (NullReferenceException nullEx)
            {
                Debug.LogError(nullEx.ToString());
            }
            catch { }
        }

        public static void SetOverrideTag(string tag, string value)
        {
            activeMaterial.SetOverrideTag(tag, value);
        }

        public static void SetRenderQueue(int queue)
        {
            activeMaterial.renderQueue = queue;
        }

        public static void SetRenderQueue(RenderQueue queue)
        {
            activeMaterial.renderQueue = (int)queue;
        }

        public static void SetSourceBlend(BlendMode mode)
        {
            activeMaterial.SetInt("_SrcBlend", (int)mode);
        }

        public static void SetDestBlend(BlendMode mode)
        {
            activeMaterial.SetInt("_DstBlend", (int)mode);
        }

        public static void SetZWrite(bool value)
        {
            activeMaterial.SetInt("_ZWrite", value ? 1 : 0);
        }

        public static void SetBlend(bool value)
        {
            activeMaterial.SetInt("_Blend", value ? 1 : 0);
        }

        public static void SetShader(Shader shader)
        {
            activeMaterial.shader = shader;
        }
    }
}