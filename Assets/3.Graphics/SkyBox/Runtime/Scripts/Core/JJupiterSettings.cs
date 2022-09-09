using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Jupiter
{
    //[CreateAssetMenu(menuName = "Jupiter/Settings")]
    public class JJupiterSettings : ScriptableObject
    {
        private static JJupiterSettings instance;
        public static JJupiterSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<JJupiterSettings>("JupiterSettings");
                    if (instance == null)
                    {
                        instance = ScriptableObject.CreateInstance<JJupiterSettings>() as JJupiterSettings;
                    }
                }
                return instance;
            }
        }

        [SerializeField]
        private Material defaultSkybox;
        public Material DefaultSkybox
        {
            get
            {
                return defaultSkybox;
            }
            set
            {
                defaultSkybox = value;
            }
        }

        [SerializeField]
        private Texture2D noiseTexture;
        public Texture2D NoiseTexture
        {
            get
            {
                return noiseTexture;
            }
            set
            {
                noiseTexture = value;
            }
        }

        [SerializeField]
        private Texture2D cloudTexture;
        public Texture2D CloudTexture
        {
            get
            {
                return cloudTexture;
            }
            set
            {
                cloudTexture = value;
            }
        }

        [SerializeField]
        private JSkyProfile defaultProfileSunnyDay;
        public JSkyProfile DefaultProfileSunnyDay
        {
            get
            {
                return defaultProfileSunnyDay;
            }
            set
            {
                defaultProfileSunnyDay = value;
            }
        }

        [SerializeField]
        private JSkyProfile defaultProfileStarryNight;
        public JSkyProfile DefaultProfileStarryNight
        {
            get
            {
                return defaultProfileStarryNight;
            }
            set
            {
                defaultProfileStarryNight = value;
            }
        }

        [SerializeField]
        private JDayNightCycleProfile defaultDayNightCycleProfile;
        public JDayNightCycleProfile DefaultDayNightCycleProfile
        {
            get
            {
                return defaultDayNightCycleProfile;
            }
            set
            {
                defaultDayNightCycleProfile = value;
            }
        }

        [SerializeField]
        private JInternalShaderSettings internalShaders;
        public JInternalShaderSettings InternalShaders
        {
            get
            {
                return internalShaders;
            }
            set
            {
                internalShaders = value;
            }
        }
    }
}
