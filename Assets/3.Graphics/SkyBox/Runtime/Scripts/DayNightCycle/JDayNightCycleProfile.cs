using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace Pinwheel.Jupiter
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    [CreateAssetMenu(menuName = "Jupiter/Day Night Cycle Profile")]
    public class JDayNightCycleProfile : ScriptableObject
    {
        private static List<PropertyInfo> propertiesInfo;
        private static List<PropertyInfo> PropertiesInfo
        {
            get
            {
                if (propertiesInfo == null)
                {
                    propertiesInfo = new List<PropertyInfo>();
                }
                return propertiesInfo;
            }
            set
            {
                propertiesInfo = value;
            }
        }

        static JDayNightCycleProfile()
        {
            InitPropertiesInfo();
        }

        private static void InitPropertiesInfo()
        {
            PropertiesInfo.Clear();
            Type type = typeof(JSkyProfile);
            PropertyInfo[] props = type.GetProperties();
            PropertiesInfo.AddRange(props);
        }

        [SerializeField]
        private List<JAnimatedProperty> animatedProperties;
        public List<JAnimatedProperty> AnimatedProperties
        {
            get
            {
                if (animatedProperties == null)
                {
                    animatedProperties = new List<JAnimatedProperty>();
                }
                return animatedProperties;
            }
            set
            {
                animatedProperties = value;
            }
        }

        public void AddProperty(JAnimatedProperty p, bool setDefaultValue = true)
        {
            if (setDefaultValue)
            {
                JDayNightCycleProfile defaultProfile = JJupiterSettings.Instance.DefaultDayNightCycleProfile;
                if (defaultProfile != null)
                {
                    JAnimatedProperty defaultProp = defaultProfile.AnimatedProperties.Find(p0 => p0.Name != null && p0.Name.Equals(p.Name));
                    if (defaultProp != null)
                    {
                        p.Curve = defaultProp.Curve;
                        p.Gradient = defaultProp.Gradient;
                    }
                }
            }

            AnimatedProperties.Add(p);
        }

        public void Animate(JSky sky, float t)
        {
            CheckDefaultProfileAndThrow(sky.Profile);

            for (int i = 0; i < AnimatedProperties.Count; ++i)
            {
                JAnimatedProperty aProp = AnimatedProperties[i];
                for (int p = 0; p < PropertiesInfo.Count; ++p)
                {
                    if (aProp.Name.Equals(PropertiesInfo[p].Name))
                    {
                        if (aProp.CurveOrGradient == JCurveOrGradient.Curve)
                        {
                            PropertiesInfo[p].SetValue(sky.Profile, aProp.EvaluateFloat(t));
                        }
                        else
                        {
                            PropertiesInfo[p].SetValue(sky.Profile, aProp.EvaluateColor(t));
                        }
                        break;
                    }
                }
            }

            sky.Profile.UpdateMaterialProperties();
        }

        private void CheckDefaultProfileAndThrow(JSkyProfile p)
        {
            if (p == null)
                return;
            if (p == JJupiterSettings.Instance.DefaultProfileSunnyDay ||
                p == JJupiterSettings.Instance.DefaultProfileStarryNight)
            {
                throw new ArgumentException("Animating default sky profile is prohibited. You must create a new profile for your sky to animate it.");
            }
        }
    }
}
