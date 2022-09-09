using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pinwheel.Jupiter
{
    [CreateAssetMenu(menuName = "Jupiter/Sky Profile")]
    public class JSkyProfile : ScriptableObject
    {
        [SerializeField]
        private Color skyColor;
        [JAnimatable("Sky/Sky Color", JCurveOrGradient.Gradient)]
        public Color SkyColor
        {
            get
            {
                return skyColor;
            }
            set
            {
                skyColor = value;
            }
        }

        [SerializeField]
        private Color horizonColor;
        [JAnimatable("Sky/Horizon Color", JCurveOrGradient.Gradient)]
        public Color HorizonColor
        {
            get
            {
                return horizonColor;
            }
            set
            {
                horizonColor = value;
            }
        }

        [SerializeField]
        private Color groundColor;
        [JAnimatable("Sky/Ground Color", JCurveOrGradient.Gradient)]
        public Color GroundColor
        {
            get
            {
                return groundColor;
            }
            set
            {
                groundColor = value;
            }
        }

        [SerializeField]
        private float horizonThickness;
        [JAnimatable("Sky/Horizon Thickness", JCurveOrGradient.Curve)]
        public float HorizonThickness
        {
            get
            {
                return horizonThickness;
            }
            set
            {
                horizonThickness = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private float horizonExponent;
        [JAnimatable("Sky/Horizon Exponent", JCurveOrGradient.Curve)]
        public float HorizonExponent
        {
            get
            {
                return horizonExponent;
            }
            set
            {
                horizonExponent = Mathf.Max(0.01f, value);
            }
        }

        [SerializeField]
        private int horizonStep;
        public int HorizonStep
        {
            get
            {
                return horizonStep;
            }
            set
            {
                horizonStep = Mathf.Max(1, value);
            }
        }

        [SerializeField]
        private JFogSyncOption fogSyncOption;
        public JFogSyncOption FogSyncOption
        {
            get
            {
                return fogSyncOption;
            }
            set
            {
                fogSyncOption = value;
            }
        }

        [SerializeField]
        private Color fogColor;
        [JAnimatable("Sky/Fog Color", JCurveOrGradient.Gradient)]
        public Color FogColor
        {
            get
            {
                return fogColor;
            }
            set
            {
                fogColor = value;
            }
        }

        [SerializeField]
        private bool enableStars;
        public bool EnableStars
        {
            get
            {
                return enableStars;
            }
            set
            {
                enableStars = value;
            }
        }

        [SerializeField]
        private float starsStartPosition;
        [JAnimatable("Stars/Start Position", JCurveOrGradient.Curve)]
        public float StarsStartPosition
        {
            get
            {
                return starsStartPosition;
            }
            set
            {
                starsStartPosition = Mathf.Min(value, starsEndPosition - 0.01f);
            }
        }

        [SerializeField]
        private float starsEndPosition;
        [JAnimatable("Stars/End Position", JCurveOrGradient.Curve)]
        public float StarsEndPosition
        {
            get
            {
                return starsEndPosition;
            }
            set
            {
                starsEndPosition = Mathf.Max(value, starsStartPosition + 0.01f);
            }
        }

        [SerializeField]
        private float starsOpacity;
        [JAnimatable("Stars/Opacity", JCurveOrGradient.Curve)]
        public float StarsOpacity
        {
            get
            {
                return starsOpacity;
            }
            set
            {
                starsOpacity = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private int starsLayerCount;
        public int StarsLayerCount
        {
            get
            {
                return starsLayerCount;
            }
            set
            {
                starsLayerCount = Mathf.Clamp(value, 1, 3);
            }
        }

        [SerializeField]
        private Color starsColor0;
        [JAnimatable("Stars/Color 0", JCurveOrGradient.Gradient)]
        public Color StarsColor0
        {
            get
            {
                return starsColor0;
            }
            set
            {
                starsColor0 = value;
            }
        }

        [SerializeField]
        private Color starsColor1;
        [JAnimatable("Stars/Color 1", JCurveOrGradient.Gradient)]
        public Color StarsColor1
        {
            get
            {
                return starsColor1;
            }
            set
            {
                starsColor1 = value;
            }
        }

        [SerializeField]
        private Color starsColor2;
        [JAnimatable("Stars/Color 2", JCurveOrGradient.Gradient)]
        public Color StarsColor2
        {
            get
            {
                return starsColor2;
            }
            set
            {
                starsColor2 = value;
            }
        }

        [SerializeField]
        private float starsDensity0;
        [JAnimatable("Stars/Density 0", JCurveOrGradient.Curve)]
        public float StarsDensity0
        {
            get
            {
                return starsDensity0;
            }
            set
            {
                starsDensity0 = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private float starsDensity1;
        [JAnimatable("Stars/Density 1", JCurveOrGradient.Curve)]
        public float StarsDensity1
        {
            get
            {
                return starsDensity1;
            }
            set
            {
                starsDensity1 = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private float starsDensity2;
        [JAnimatable("Stars/Density 2", JCurveOrGradient.Curve)]
        public float StarsDensity2
        {
            get
            {
                return starsDensity2;
            }
            set
            {
                starsDensity2 = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private float starsSize0;
        [JAnimatable("Stars/Size 0", JCurveOrGradient.Curve)]
        public float StarsSize0
        {
            get
            {
                return starsSize0;
            }
            set
            {
                starsSize0 = Mathf.Max(0.01f, value);
            }
        }

        [SerializeField]
        private float starsSize1;
        [JAnimatable("Stars/Size 1", JCurveOrGradient.Curve)]
        public float StarsSize1
        {
            get
            {
                return starsSize1;
            }
            set
            {
                starsSize1 = Mathf.Max(0.01f, value);
            }
        }

        [SerializeField]
        private float starsSize2;
        [JAnimatable("Stars/Size 2", JCurveOrGradient.Curve)]
        public float StarsSize2
        {
            get
            {
                return starsSize2;
            }
            set
            {
                starsSize2 = Mathf.Max(0.01f, value);
            }
        }

        [SerializeField]
        private float starsGlow0;
        [JAnimatable("Stars/Glow 0", JCurveOrGradient.Curve)]
        public float StarsGlow0
        {
            get
            {
                return starsGlow0;
            }
            set
            {
                starsGlow0 = Mathf.Max(0.01f, value);
            }
        }

        [SerializeField]
        private float starsGlow1;
        [JAnimatable("Stars/Glow 1", JCurveOrGradient.Curve)]
        public float StarsGlow1
        {
            get
            {
                return starsGlow1;
            }
            set
            {
                starsGlow1 = Mathf.Max(0.01f, value);
            }
        }

        [SerializeField]
        private float starsGlow2;
        [JAnimatable("Stars/Glow 2", JCurveOrGradient.Curve)]
        public float StarsGlow2
        {
            get
            {
                return starsGlow2;
            }
            set
            {
                starsGlow2 = Mathf.Max(0.01f, value);
            }
        }

        [SerializeField]
        private float starsTwinkle0;
        [JAnimatable("Stars/Twinkle 0", JCurveOrGradient.Curve)]
        public float StarsTwinkle0
        {
            get
            {
                return starsTwinkle0;
            }
            set
            {
                starsTwinkle0 = value;
            }
        }

        [SerializeField]
        private float starsTwinkle1;
        [JAnimatable("Stars/Twinkle 1", JCurveOrGradient.Curve)]
        public float StarsTwinkle1
        {
            get
            {
                return starsTwinkle1;
            }
            set
            {
                starsTwinkle1 = value;
            }
        }

        [SerializeField]
        private float starsTwinkle2;
        [JAnimatable("Stars/Twinkle 2", JCurveOrGradient.Curve)]
        public float StarsTwinkle2
        {
            get
            {
                return starsTwinkle2;
            }
            set
            {
                starsTwinkle2 = value;
            }
        }

        [SerializeField]
        private bool useBakedStars;
        public bool UseBakedStars
        {
            get
            {
                return useBakedStars;
            }
            set
            {
                useBakedStars = value;
            }
        }

        [SerializeField]
        private Cubemap starsCubemap;
        public Cubemap StarsCubemap
        {
            get
            {
                return starsCubemap;
            }
            set
            {
                starsCubemap = value;
            }
        }

        [SerializeField]
        private Texture2D starsTwinkleMap;
        public Texture2D StarsTwinkleMap
        {
            get
            {
                return starsTwinkleMap;
            }
            set
            {
                starsTwinkleMap = value;
            }
        }

        [SerializeField]
        private bool enableSun;
        public bool EnableSun
        {
            get
            {
                return enableSun;
            }
            set
            {
                enableSun = value;
            }
        }

        [SerializeField]
        private Texture2D sunTexture;
        public Texture2D SunTexture
        {
            get
            {
                return sunTexture;
            }
            set
            {
                sunTexture = value;
            }
        }

        [SerializeField]
        private Color sunColor;
        [JAnimatable("Sun/Color", JCurveOrGradient.Gradient)]
        public Color SunColor
        {
            get
            {
                return sunColor;
            }
            set
            {
                sunColor = value;
            }
        }

        [SerializeField]
        private float sunSize;
        [JAnimatable("Sun/Size", JCurveOrGradient.Curve)]
        public float SunSize
        {
            get
            {
                return sunSize;
            }
            set
            {
                sunSize = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private float sunSoftEdge;
        [JAnimatable("Sun/Soft Edge", JCurveOrGradient.Curve)]
        public float SunSoftEdge
        {
            get
            {
                return sunSoftEdge;
            }
            set
            {
                sunSoftEdge = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private float sunGlow;
        [JAnimatable("Sun/Glow", JCurveOrGradient.Curve)]
        public float SunGlow
        {
            get
            {
                return sunGlow;
            }
            set
            {
                sunGlow = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private bool useBakedSun;
        public bool UseBakedSun
        {
            get
            {
                return useBakedSun;
            }
            set
            {
                useBakedSun = value;
            }
        }

        [SerializeField]
        private Cubemap sunCubemap;
        public Cubemap SunCubemap
        {
            get
            {
                return sunCubemap;
            }
            set
            {
                sunCubemap = value;
            }
        }

        [SerializeField]
        private Color sunLightColor;
        [JAnimatable("Sun/Light Color", JCurveOrGradient.Gradient)]
        public Color SunLightColor
        {
            get
            {
                return sunLightColor;
            }
            set
            {
                sunLightColor = value;
            }
        }

        [SerializeField]
        private float sunLightIntensity;
        [JAnimatable("Sun/Light Intensity", JCurveOrGradient.Curve)]
        public float SunLightIntensity
        {
            get
            {
                return sunLightIntensity;
            }
            set
            {
                sunLightIntensity = value;
            }
        }

        [SerializeField]
        private bool enableMoon;
        public bool EnableMoon
        {
            get
            {
                return enableMoon;
            }
            set
            {
                enableMoon = value;
            }
        }

        [SerializeField]
        private Texture2D moonTexture;
        public Texture2D MoonTexture
        {
            get
            {
                return moonTexture;
            }
            set
            {
                moonTexture = value;
            }
        }

        [SerializeField]
        private Color moonColor;
        [JAnimatable("Moon/Color", JCurveOrGradient.Gradient)]
        public Color MoonColor
        {
            get
            {
                return moonColor;
            }
            set
            {
                moonColor = value;
            }
        }

        [SerializeField]
        private float moonSize;
        [JAnimatable("Moon/Size", JCurveOrGradient.Curve)]
        public float MoonSize
        {
            get
            {
                return moonSize;
            }
            set
            {
                moonSize = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private float moonSoftEdge;
        [JAnimatable("Moon/Soft Edge", JCurveOrGradient.Curve)]
        public float MoonSoftEdge
        {
            get
            {
                return moonSoftEdge;
            }
            set
            {
                moonSoftEdge = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private float moonGlow;
        [JAnimatable("Moon/Glow", JCurveOrGradient.Curve)]
        public float MoonGlow
        {
            get
            {
                return moonGlow;
            }
            set
            {
                moonGlow = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private bool useBakedMoon;
        public bool UseBakedMoon
        {
            get
            {
                return useBakedMoon;
            }
            set
            {
                useBakedMoon = value;
            }
        }

        [SerializeField]
        private Cubemap moonCubemap;
        public Cubemap MoonCubemap
        {
            get
            {
                return moonCubemap;
            }
            set
            {
                moonCubemap = value;
            }
        }

        [SerializeField]
        private Color moonLightColor;
        [JAnimatable("Moon/Light Color", JCurveOrGradient.Gradient)]
        public Color MoonLightColor
        {
            get
            {
                return moonLightColor;
            }
            set
            {
                moonLightColor = value;
            }
        }

        [SerializeField]
        private float moonLightIntensity;
        [JAnimatable("Moon/Light Intensity", JCurveOrGradient.Curve)]
        public float MoonLightIntensity
        {
            get
            {
                return moonLightIntensity;
            }
            set
            {
                moonLightIntensity = value;
            }
        }

        [SerializeField]
        private Texture2D customCloudTexture;
        public Texture2D CustomCloudTexture
        {
            get
            {
                return customCloudTexture;
            }
            set
            {
                customCloudTexture = value;
            }
        }

        [SerializeField]
        private bool enableHorizonCloud;
        public bool EnableHorizonCloud
        {
            get
            {
                return enableHorizonCloud;
            }
            set
            {
                enableHorizonCloud = value;
            }
        }

        [SerializeField]
        private Color horizonCloudColor;
        [JAnimatable("Horizon Cloud/Color", JCurveOrGradient.Gradient)]
        public Color HorizonCloudColor
        {
            get
            {
                return horizonCloudColor;
            }
            set
            {
                horizonCloudColor = value;
            }
        }

        [SerializeField]
        private float horizonCloudStartPosition;
        [JAnimatable("Horizon Cloud/Start Position", JCurveOrGradient.Curve)]
        public float HorizonCloudStartPosition
        {
            get
            {
                return horizonCloudStartPosition;
            }
            set
            {
                horizonCloudStartPosition = Mathf.Min(value, -0.01f);
            }
        }

        [SerializeField]
        private float horizonCloudEndPosition;
        [JAnimatable("Horizon Cloud/End Position", JCurveOrGradient.Curve)]
        public float HorizonCloudEndPosition
        {
            get
            {
                return horizonCloudEndPosition;
            }
            set
            {
                horizonCloudEndPosition = Mathf.Max(value, 0.01f);
            }
        }

        [SerializeField]
        private float horizonCloudSize;
        [JAnimatable("Horizon Cloud/Size", JCurveOrGradient.Curve)]
        public float HorizonCloudSize
        {
            get
            {
                return horizonCloudSize;
            }
            set
            {
                horizonCloudSize = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private int horizonCloudStep;
        public int HorizonCloudStep
        {
            get
            {
                return horizonCloudStep;
            }
            set
            {
                horizonCloudStep = Mathf.Max(1, value);
            }
        }

        [SerializeField]
        private float horizonCloudAnimationSpeed;
        [JAnimatable("Horizon Cloud/Animation Speed", JCurveOrGradient.Curve)]
        public float HorizonCloudAnimationSpeed
        {
            get
            {
                return horizonCloudAnimationSpeed;
            }
            set
            {
                horizonCloudAnimationSpeed = value;
            }
        }

        [SerializeField]
        private bool enableOverheadCloud;
        public bool EnableOverheadCloud
        {
            get
            {
                return enableOverheadCloud;
            }
            set
            {
                enableOverheadCloud = value;
            }
        }

        [SerializeField]
        private Color overheadCloudColor;
        [JAnimatable("Overhead Cloud/Color", JCurveOrGradient.Gradient)]
        public Color OverheadCloudColor
        {
            get
            {
                return overheadCloudColor;
            }
            set
            {
                overheadCloudColor = value;
            }
        }

        [SerializeField]
        private float overheadCloudAltitude;
        [JAnimatable("Overhead Cloud/Altitude", JCurveOrGradient.Curve)]
        public float OverheadCloudAltitude
        {
            get
            {
                return overheadCloudAltitude;
            }
            set
            {
                overheadCloudAltitude = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private float overheadCloudSize;
        [JAnimatable("Overhead Cloud/Size", JCurveOrGradient.Curve)]
        public float OverheadCloudSize
        {
            get
            {
                return overheadCloudSize;
            }
            set
            {
                overheadCloudSize = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private int overheadCloudStep;
        public int OverheadCloudStep
        {
            get
            {
                return overheadCloudStep;
            }
            set
            {
                overheadCloudStep = Mathf.Max(1, value);
            }
        }

        [SerializeField]
        private float overheadCloudAnimationSpeed;
        [JAnimatable("Overhead Cloud/Animation Speed", JCurveOrGradient.Curve)]
        public float OverheadCloudAnimationSpeed
        {
            get
            {
                return overheadCloudAnimationSpeed;
            }
            set
            {
                overheadCloudAnimationSpeed = value;
            }
        }

        [SerializeField]
        private float overheadCloudFlowDirectionX;
        [JAnimatable("Overhead Cloud/Flow X", JCurveOrGradient.Curve)]
        public float OverheadCloudFlowDirectionX
        {
            get
            {
                return overheadCloudFlowDirectionX;
            }
            set
            {
                overheadCloudFlowDirectionX = Mathf.Clamp(value, -1, 1);
            }
        }

        [SerializeField]
        private float overheadCloudFlowDirectionZ;
        [JAnimatable("Overhead Cloud/Flow Z", JCurveOrGradient.Curve)]
        public float OverheadCloudFlowDirectionZ
        {
            get
            {
                return overheadCloudFlowDirectionZ;
            }
            set
            {
                overheadCloudFlowDirectionZ = Mathf.Clamp(value, -1, 1);
            }
        }

        [SerializeField]
        private bool enableDetailOverlay;
        public bool EnableDetailOverlay
        {
            get
            {
                return enableDetailOverlay;
            }
            set
            {
                enableDetailOverlay = value;
            }
        }

        [SerializeField]
        private Color detailOverlayTintColor;
        [JAnimatable("Detail Overlay/Color", JCurveOrGradient.Gradient)]
        public Color DetailOverlayTintColor
        {
            get
            {
                return detailOverlayTintColor;
            }
            set
            {
                detailOverlayTintColor = value;
            }
        }

        [SerializeField]
        private Cubemap detailOverlayCubeMap;
        public Cubemap DetailOverlayCubeMap
        {
            get
            {
                return detailOverlayCubeMap;
            }
            set
            {
                detailOverlayCubeMap = value;
            }
        }

        [SerializeField]
        private JDetailOverlayLayer detailOverlayLayer;
        public JDetailOverlayLayer DetailOverlayLayer
        {
            get
            {
                return detailOverlayLayer;
            }
            set
            {
                detailOverlayLayer = value;
            }
        }

        [SerializeField]
        private float detailOverlayRotationSpeed;
        [JAnimatable("Detail Overlay/Rotation Speed", JCurveOrGradient.Curve)]
        public float DetailOverlayRotationSpeed
        {
            get
            {
                return detailOverlayRotationSpeed;
            }
            set
            {
                detailOverlayRotationSpeed = value;
            }
        }

        [SerializeField]
        private bool allowStepEffect;
        public bool AllowStepEffect
        {
            get
            {
                return allowStepEffect;
            }
            set
            {
                allowStepEffect = value;
            }
        }

        [SerializeField]
        private Material material;
        public Material Material
        {
            get
            {
                if (material == null)
                {
                    material = new Material(JJupiterSettings.Instance.InternalShaders.SkyShader);
                }
#if UNITY_EDITOR
                if (!AssetDatabase.Contains(material) && EditorUtility.IsPersistent(this))
                {
                    AssetDatabase.AddObjectToAsset(material, this);
                }
#endif
                material.name = material.shader.name;
                return material;
            }
        }

        public void Reset()
        {
            JSkyProfile defaultProfile = JJupiterSettings.Instance.DefaultProfileSunnyDay;
            if (defaultProfile != this)
            {
                CopyFrom(defaultProfile);
            }
        }

        public void UpdateMaterialProperties()
        {
            JMat.SetActiveMaterial(Material);

            JMat.SetColor(JMat.SKY_COLOR, SkyColor);
            JMat.SetColor(JMat.HORIZON_COLOR, HorizonColor);
            JMat.SetColor(JMat.GROUND_COLOR, GroundColor);
            JMat.SetFloat(JMat.HORIZON_THICKNESS, HorizonThickness);
            JMat.SetFloat(JMat.HORIZON_EXPONENT, HorizonExponent);
            JMat.SetFloat(JMat.HORIZON_STEP, HorizonStep);

            JMat.SetKeywordEnable(JMat.KW_STARS, EnableStars);
            JMat.SetKeywordEnable(JMat.KW_STARS_BAKED, UseBakedStars);
            JMat.SetFloat(JMat.STARS_OPACITY, StarsOpacity);
            if (UseBakedStars)
            {
                JMat.SetTexture(JMat.STARS_CUBEMAP, StarsCubemap);
                JMat.SetTexture(JMat.STARS_TWINKLE_MAP, StarsTwinkleMap);
            }
            else
            {
                JMat.SetKeywordEnable(JMat.KW_STARS_LAYER_0, StarsLayerCount > 0);
                JMat.SetKeywordEnable(JMat.KW_STARS_LAYER_1, StarsLayerCount > 1);
                JMat.SetKeywordEnable(JMat.KW_STARS_LAYER_2, StarsLayerCount > 2);
                JMat.SetFloat(JMat.STARS_START, StarsStartPosition);
                JMat.SetFloat(JMat.STARS_END, StarsEndPosition);
                JMat.SetColor(JMat.STARS_COLOR_0, StarsColor0);
                JMat.SetColor(JMat.STARS_COLOR_1, StarsColor1);
                JMat.SetColor(JMat.STARS_COLOR_2, StarsColor2);
                JMat.SetFloat(JMat.STARS_DENSITY_0, StarsDensity0);
                JMat.SetFloat(JMat.STARS_DENSITY_1, StarsDensity1);
                JMat.SetFloat(JMat.STARS_DENSITY_2, StarsDensity2);
                JMat.SetFloat(JMat.STARS_SIZE_0, StarsSize0);
                JMat.SetFloat(JMat.STARS_SIZE_1, StarsSize1);
                JMat.SetFloat(JMat.STARS_SIZE_2, StarsSize2);
                JMat.SetFloat(JMat.STARS_GLOW_0, StarsGlow0);
                JMat.SetFloat(JMat.STARS_GLOW_1, StarsGlow1);
                JMat.SetFloat(JMat.STARS_GLOW_2, StarsGlow2);
                JMat.SetFloat(JMat.STARS_TWINKLE_0, StarsTwinkle0);
                JMat.SetFloat(JMat.STARS_TWINKLE_1, StarsTwinkle1);
                JMat.SetFloat(JMat.STARS_TWINKLE_2, StarsTwinkle2);
            }

            JMat.SetKeywordEnable(JMat.KW_SUN, EnableSun);
            JMat.SetKeywordEnable(JMat.KW_SUN_BAKED, UseBakedSun);
            if (UseBakedSun)
            {
                JMat.SetTexture(JMat.SUN_CUBEMAP, SunCubemap);
            }
            else
            {
                JMat.SetKeywordEnable(JMat.KW_SUN_USE_TEXTURE, SunTexture != null);
                JMat.SetTexture(JMat.SUN_TEX, SunTexture);
                JMat.SetColor(JMat.SUN_COLOR, SunColor);
                JMat.SetFloat(JMat.SUN_SIZE, SunSize);
                JMat.SetFloat(JMat.SUN_SOFT_EDGE, SunSoftEdge);
                JMat.SetFloat(JMat.SUN_GLOW, SunGlow);
            }

            JMat.SetKeywordEnable(JMat.KW_MOON, EnableMoon);
            JMat.SetKeywordEnable(JMat.KW_MOON_BAKED, UseBakedMoon);
            if (UseBakedMoon)
            {
                JMat.SetTexture(JMat.MOON_CUBEMAP, MoonCubemap);
            }
            else
            {
                JMat.SetKeywordEnable(JMat.KW_MOON_USE_TEXTURE, MoonTexture != null);
                JMat.SetTexture(JMat.MOON_TEX, MoonTexture);
                JMat.SetColor(JMat.MOON_COLOR, MoonColor);
                JMat.SetFloat(JMat.MOON_SIZE, MoonSize);
                JMat.SetFloat(JMat.MOON_SOFT_EDGE, MoonSoftEdge);
                JMat.SetFloat(JMat.MOON_GLOW, MoonGlow);
            }

            JMat.SetKeywordEnable(JMat.KW_HORIZON_CLOUD, EnableHorizonCloud);
            JMat.SetColor(JMat.HORIZON_CLOUD_COLOR, HorizonCloudColor);
            JMat.SetFloat(JMat.HORIZON_CLOUD_START, HorizonCloudStartPosition);
            JMat.SetFloat(JMat.HORIZON_CLOUD_END, HorizonCloudEndPosition);
            JMat.SetFloat(JMat.HORIZON_CLOUD_SIZE, HorizonCloudSize);
            JMat.SetFloat(JMat.HORIZON_CLOUD_STEP, HorizonCloudStep);
            JMat.SetFloat(JMat.HORIZON_CLOUD_ANIMATION_SPEED, HorizonCloudAnimationSpeed);

            JMat.SetKeywordEnable(JMat.KW_OVERHEAD_CLOUD, EnableOverheadCloud);
            JMat.SetColor(JMat.OVERHEAD_CLOUD_COLOR, OverheadCloudColor);
            JMat.SetFloat(JMat.OVERHEAD_CLOUD_ALTITUDE, OverheadCloudAltitude);
            JMat.SetFloat(JMat.OVERHEAD_CLOUD_SIZE, OverheadCloudSize);
            JMat.SetFloat(JMat.OVERHEAD_CLOUD_STEP, OverheadCloudStep);
            JMat.SetFloat(JMat.OVERHEAD_CLOUD_ANIMATION_SPEED, OverheadCloudAnimationSpeed);
            JMat.SetFloat(JMat.OVERHEAD_CLOUD_FLOW_X, OverheadCloudFlowDirectionX);
            JMat.SetFloat(JMat.OVERHEAD_CLOUD_FLOW_Z, OverheadCloudFlowDirectionZ);

            JMat.SetKeywordEnable(JMat.KW_DETAIL_OVERLAY, EnableDetailOverlay);
            JMat.SetKeywordEnable(JMat.KW_DETAIL_OVERLAY_ROTATION, DetailOverlayRotationSpeed != 0);
            JMat.SetColor(JMat.DETAIL_OVERLAY_COLOR, DetailOverlayTintColor);
            JMat.SetTexture(JMat.DETAIL_OVERLAY_CUBEMAP, DetailOverlayCubeMap);
            JMat.SetFloat(JMat.DETAIL_OVERLAY_LAYER, (int)DetailOverlayLayer);
            JMat.SetFloat(JMat.DETAIL_OVERLAY_ROTATION_SPEED, DetailOverlayRotationSpeed);

            JMat.SetKeywordEnable(JMat.KW_ALLOW_STEP_EFFECT, AllowStepEffect);

            JMat.SetActiveMaterial(null);
        }

        public void CopyFrom(JSkyProfile p)
        {
            SkyColor = p.SkyColor;
            HorizonColor = p.HorizonColor;
            GroundColor = p.GroundColor;
            HorizonThickness = p.HorizonThickness;
            HorizonExponent = p.HorizonExponent;
            HorizonStep = p.HorizonStep;
            FogSyncOption = p.FogSyncOption;
            FogColor = p.FogColor;

            EnableStars = p.EnableStars;
            StarsStartPosition = p.StarsStartPosition;
            StarsEndPosition = p.StarsEndPosition;
            StarsOpacity = p.StarsOpacity;
            StarsLayerCount = p.StarsLayerCount;
            StarsColor0 = p.StarsColor0;
            StarsColor1 = p.StarsColor1;
            StarsColor2 = p.StarsColor2;
            StarsDensity0 = p.StarsDensity0;
            StarsDensity1 = p.StarsDensity1;
            StarsDensity2 = p.StarsDensity2;
            StarsSize0 = p.StarsSize0;
            StarsSize1 = p.StarsSize1;
            StarsSize2 = p.StarsSize2;
            StarsGlow0 = p.StarsGlow0;
            StarsGlow1 = p.StarsGlow1;
            StarsGlow2 = p.StarsGlow2;
            StarsTwinkle0 = p.StarsTwinkle0;
            StarsTwinkle1 = p.StarsTwinkle1;
            StarsTwinkle2 = p.StarsTwinkle2;
            UseBakedStars = p.UseBakedStars;
            StarsCubemap = p.StarsCubemap;
            StarsTwinkleMap = p.StarsTwinkleMap;

            EnableSun = p.EnableSun;
            SunTexture = p.SunTexture;
            SunColor = p.SunColor;
            SunSize = p.SunSize;
            SunSoftEdge = p.SunSoftEdge;
            SunGlow = p.SunGlow;
            UseBakedSun = p.UseBakedSun;
            SunCubemap = p.SunCubemap;
            SunLightColor = p.SunLightColor;
            SunLightIntensity = p.SunLightIntensity;

            EnableMoon = p.EnableMoon;
            MoonTexture = p.MoonTexture;
            MoonColor = p.MoonColor;
            MoonSize = p.MoonSize;
            MoonSoftEdge = p.MoonSoftEdge;
            MoonGlow = p.MoonGlow;
            UseBakedMoon = p.UseBakedMoon;
            MoonCubemap = p.MoonCubemap;
            MoonLightColor = p.MoonLightColor;
            MoonLightIntensity = p.MoonLightIntensity;

            CustomCloudTexture = p.CustomCloudTexture;

            EnableHorizonCloud = p.EnableHorizonCloud;
            HorizonCloudColor = p.HorizonCloudColor;
            HorizonCloudStartPosition = p.HorizonCloudStartPosition;
            HorizonCloudEndPosition = p.HorizonCloudEndPosition;
            HorizonCloudSize = p.HorizonCloudSize;
            HorizonCloudStep = p.HorizonCloudStep;
            HorizonCloudAnimationSpeed = p.HorizonCloudAnimationSpeed;

            EnableOverheadCloud = p.EnableOverheadCloud;
            OverheadCloudColor = p.OverheadCloudColor;
            OverheadCloudAltitude = p.OverheadCloudAltitude;
            OverheadCloudSize = p.OverheadCloudSize;
            OverheadCloudStep = p.OverheadCloudStep;
            OverheadCloudAnimationSpeed = p.OverheadCloudAnimationSpeed;
            OverheadCloudFlowDirectionX = p.OverheadCloudFlowDirectionX;
            OverheadCloudFlowDirectionZ = p.OverheadCloudFlowDirectionZ;

            EnableDetailOverlay = p.EnableDetailOverlay;
            DetailOverlayTintColor = p.DetailOverlayTintColor;
            DetailOverlayCubeMap = p.DetailOverlayCubeMap;
            DetailOverlayLayer = p.DetailOverlayLayer;
            DetailOverlayRotationSpeed = p.DetailOverlayRotationSpeed;

            AllowStepEffect = p.AllowStepEffect;

            UpdateMaterialProperties();
        }
    }
}
