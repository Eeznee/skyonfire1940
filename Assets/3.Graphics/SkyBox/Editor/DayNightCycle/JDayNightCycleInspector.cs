using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using System;
using UnityEngine.Rendering;

namespace Pinwheel.Jupiter
{
    [CustomEditor(typeof(JDayNightCycle))]
    public class JDayNightCycleInspector : Editor
    {
        private static List<JAnimatedProperty> allProperties;
        private static List<JAnimatedProperty> AllProperties
        {
            get
            {
                if (allProperties == null)
                {
                    allProperties = new List<JAnimatedProperty>();
                }
                return allProperties;
            }
            set
            {
                allProperties = value;
            }
        }

        static JDayNightCycleInspector()
        {
            InitAllAnimatableProperties();
        }

        private static void InitAllAnimatableProperties()
        {
            AllProperties.Clear();
            Type type = typeof(JSkyProfile);
            PropertyInfo[] props = type.GetProperties();
            for (int i = 0; i < props.Length; ++i)
            {
                Attribute att = props[i].GetCustomAttribute(typeof(JAnimatableAttribute));
                if (att != null)
                {
                    JAnimatableAttribute animAtt = att as JAnimatableAttribute;
                    AllProperties.Add(JAnimatedProperty.Create(props[i].Name, animAtt.DisplayName, animAtt.CurveOrGradient));
                }
            }
        }

        private JDayNightCycle cycle;
        private JDayNightCycleProfile profile;
        bool isTimeFoldoutExpanded;

        private static readonly int[] resolutionValues = new int[]
        {
            16,32,64,128,256,512,1024,2048
        };

        private static readonly string[] resolutionLabels = new string[]
        {
            "16","32","64","128","256","512","1024","2048"
        };

        private void OnEnable()
        {
            cycle = target as JDayNightCycle;
            profile = cycle.Profile;
        }

        public override void OnInspectorGUI()
        {
            cycle.Profile = JEditorCommon.ScriptableObjectField<JDayNightCycleProfile>("Profile", cycle.Profile);
            profile = cycle.Profile;
            if (cycle.Profile == null)
                return;

            DrawSceneReferencesGUI();
            DrawTimeGUI();
            EditorGUI.BeginChangeCheck();
            DrawSkyGUI();
            DrawStarsGUI();
            DrawSunGUI();
            DrawMoonGUI();
            DrawHorizonCloudGUI();
            DrawOverheadCloudGUI();
            DrawDetailOverlayGUI();
            DrawEnvironmentReflectionGUI();
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(profile);
            }
        }

        private void DrawSceneReferencesGUI()
        {
            string label = "Scene References";
            string id = "scene-ref" + cycle.GetInstanceID();

            JEditorCommon.Foldout(label, false, id, () =>
            {
                cycle.Sky = EditorGUILayout.ObjectField("Sky", cycle.Sky, typeof(JSky), true) as JSky;
                cycle.SunOrbitPivot = EditorGUILayout.ObjectField("Orbit Pivot", cycle.SunOrbitPivot, typeof(Transform), true) as Transform;
            });
        }

        private void DrawTimeGUI()
        {
            string label = "Time";
            string id = "time" + cycle.GetInstanceID();

            isTimeFoldoutExpanded = JEditorCommon.Foldout(label, false, id, () =>
            {
                cycle.StartTime = EditorGUILayout.FloatField("Start Time", cycle.StartTime);
                cycle.TimeIncrement = EditorGUILayout.FloatField("Time Increment", cycle.TimeIncrement);
                GUI.enabled = !cycle.AutoTimeIncrement;
                cycle.Time = EditorGUILayout.Slider("Time", cycle.Time, 0f, 24f);
                GUI.enabled = true;
                cycle.AutoTimeIncrement = EditorGUILayout.Toggle("Auto", cycle.AutoTimeIncrement);
            });
        }

        private void DrawSkyGUI()
        {
            string label = "Sky";
            string id = "sky" + profile.GetInstanceID();

            JEditorCommon.Foldout(label, false, id, () =>
            {
                DisplayAddedProperties("Sky");
                if (GUILayout.Button("Add"))
                {
                    DisplayAllPropertiesAsContext("Sky");
                }
            });
        }

        private void DrawStarsGUI()
        {
            string label = "Stars";
            string id = "stars" + profile.GetInstanceID();

            JEditorCommon.Foldout(label, false, id, () =>
            {
                DisplayAddedProperties("Stars");
                if (GUILayout.Button("Add"))
                {
                    DisplayAllPropertiesAsContext("Stars");
                }
            });
        }

        private void DrawSunGUI()
        {
            string label = "Sun";
            string id = "sun" + profile.GetInstanceID();

            JEditorCommon.Foldout(label, false, id, () =>
            {
                cycle.UseSunPivot = EditorGUILayout.Toggle("Custom Pivot", cycle.UseSunPivot);
                if (cycle.UseSunPivot)
                {
                    cycle.SunOrbitPivot = EditorGUILayout.ObjectField("Pivot", cycle.SunOrbitPivot, typeof(Transform), true) as Transform;
                }
                JEditorCommon.Separator();

                DisplayAddedProperties("Sun");
                if (GUILayout.Button("Add"))
                {
                    DisplayAllPropertiesAsContext("Sun");
                }
            });
        }

        private void DrawMoonGUI()
        {
            string label = "Moon";
            string id = "moon" + profile.GetInstanceID();

            JEditorCommon.Foldout(label, false, id, () =>
            {
                cycle.UseMoonPivot = EditorGUILayout.Toggle("Custom Pivot", cycle.UseMoonPivot);
                if (cycle.UseMoonPivot)
                {
                    cycle.MoonOrbitPivot = EditorGUILayout.ObjectField("Pivot", cycle.MoonOrbitPivot, typeof(Transform), true) as Transform;
                }
                JEditorCommon.Separator();

                DisplayAddedProperties("Moon");
                if (GUILayout.Button("Add"))
                {
                    DisplayAllPropertiesAsContext("Moon");
                }
            });
        }

        private void DrawHorizonCloudGUI()
        {
            string label = "Horizon Cloud";
            string id = "horizon-cloud" + profile.GetInstanceID();

            JEditorCommon.Foldout(label, false, id, () =>
            {
                DisplayAddedProperties("Horizon Cloud");
                if (GUILayout.Button("Add"))
                {
                    DisplayAllPropertiesAsContext("Horizon Cloud");
                }
            });
        }

        private void DrawOverheadCloudGUI()
        {
            string label = "Overhead Cloud";
            string id = "overhead-cloud" + profile.GetInstanceID();

            JEditorCommon.Foldout(label, false, id, () =>
            {
                DisplayAddedProperties("Overhead Cloud");
                if (GUILayout.Button("Add"))
                {
                    DisplayAllPropertiesAsContext("Overhead Cloud");
                }
            });
        }

        private void DrawDetailOverlayGUI()
        {
            string label = "Detail Overlay";
            string id = "detail-overlay" + profile.GetInstanceID();

            JEditorCommon.Foldout(label, false, id, () =>
            {
                DisplayAddedProperties("Detail Overlay");
                if (GUILayout.Button("Add"))
                {
                    DisplayAllPropertiesAsContext("Detail Overlay");
                }
            });
        }

        private void DrawEnvironmentReflectionGUI()
        {
            string label = "Environment Reflection";
            string id = "env-reflection";

            JEditorCommon.Foldout(label, false, id, () =>
            {
                cycle.ShouldUpdateEnvironmentReflection = EditorGUILayout.Toggle("Enable", cycle.ShouldUpdateEnvironmentReflection);
                if (cycle.ShouldUpdateEnvironmentReflection)
                {
                    cycle.EnvironmentReflectionResolution = EditorGUILayout.IntPopup("Resolution", cycle.EnvironmentReflectionResolution, resolutionLabels, resolutionValues);
                    cycle.EnvironmentReflectionTimeSlicingMode = (ReflectionProbeTimeSlicingMode)EditorGUILayout.EnumPopup("Time Slicing", cycle.EnvironmentReflectionTimeSlicingMode);
                    EditorGUILayout.LabelField("Realtime Reflection Probe must be enabled in Quality Settings.", JEditorCommon.WordWrapItalicLabel);
                }
            });
        }

        private void DisplayAddedProperties(string group)
        {
            EditorGUI.indentLevel -= 1;
            JAnimatedProperty toRemoveProp = null;
            List<JAnimatedProperty> props = profile.AnimatedProperties.FindAll(p => p.DisplayName.StartsWith(group));
            for (int i = 0; i < props.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                JAnimatedProperty p = props[i];
                if (GUILayout.Button("▬", EditorStyles.miniLabel, GUILayout.Width(12)))
                {
                    toRemoveProp = p;
                }

                string itemLabel = p.DisplayName.Substring(p.DisplayName.IndexOf("/") + 1);
                itemLabel = ObjectNames.NicifyVariableName(itemLabel);
                if (p.CurveOrGradient == JCurveOrGradient.Curve)
                {
                    p.Curve = EditorGUILayout.CurveField(itemLabel, p.Curve);
                }
                else
                {
                    p.Gradient = EditorGUILayout.GradientField(itemLabel, p.Gradient);
                }

                EditorGUILayout.EndHorizontal();
            }
            if (props.Count > 0)
            {
                JEditorCommon.Separator();
            }

            if (toRemoveProp != null)
            {
                profile.AnimatedProperties.Remove(toRemoveProp);
            }
            EditorGUI.indentLevel += 1;
        }

        private void DisplayAllPropertiesAsContext(string group)
        {
            GenericMenu menu = new GenericMenu();
            List<JAnimatedProperty> props = AllProperties.FindAll(p => p.DisplayName.StartsWith(group));
            if (props.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("No item found"));
                menu.ShowAsContext();
                return;
            }

            for (int i = 0; i < props.Count; ++i)
            {
                JAnimatedProperty p = props[i];
                string itemLabel = p.DisplayName.Substring(p.DisplayName.IndexOf("/") + 1);
                bool added = profile.AnimatedProperties.FindIndex(p0 => p0.Name.Equals(p.Name)) >= 0;

                if (added)
                {
                    menu.AddDisabledItem(new GUIContent(itemLabel));
                }
                else
                {
                    menu.AddItem(
                        new GUIContent(itemLabel),
                        false,
                        () =>
                        {
                            profile.AddProperty(p);
                        });
                }
            }
            menu.ShowAsContext();
        }

        public override bool RequiresConstantRepaint()
        {
            return isTimeFoldoutExpanded;
        }

        private void OnSceneGUI()
        {
            if (cycle == null)
                return;

            float evalTime = Mathf.InverseLerp(0f, 24f, cycle.Time);

            if (cycle.Sky.Profile.EnableSun && cycle.Sky.SunLightSource != null)
            {
                Color c = cycle.Sky.Profile.SunColor;
                c.a = Mathf.Max(0.1f, c.a);

                Transform pivot = (cycle.UseSunPivot && cycle.SunOrbitPivot != null) ? cycle.SunOrbitPivot : cycle.transform;
                Vector3 normal = pivot.right;
                Handles.color = c;
                float radius = 10;
                Handles.DrawWireDisc(pivot.position, normal, radius);

                float angle = evalTime * 360f;
                Matrix4x4 localRotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(angle, 0, 0));
                Vector3 localDirection = localRotationMatrix.MultiplyVector(Vector3.up);

                Matrix4x4 localToWorld = pivot.localToWorldMatrix;
                Vector3 worldDirection = localToWorld.MultiplyVector(localDirection);

                Vector3 worldPos = pivot.transform.position - worldDirection * radius;
                Handles.color = c;
                Handles.DrawSolidDisc(worldPos, normal, 1);
            }

            if (cycle.Sky.Profile.EnableMoon && cycle.Sky.MoonLightSource != null)
            {
                Color c = cycle.Sky.Profile.MoonColor;
                c.a = Mathf.Max(0.1f, c.a);

                Transform pivot = (cycle.UseMoonPivot && cycle.MoonOrbitPivot != null) ? cycle.MoonOrbitPivot : cycle.transform;
                Vector3 normal = pivot.right;
                Handles.color = c;
                float radius = 10;
                Handles.DrawWireDisc(pivot.position, normal, radius);

                float angle = evalTime * 360f;
                Matrix4x4 localRotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(angle, 0, 0));
                Vector3 localDirection = localRotationMatrix.MultiplyVector(Vector3.down);

                Matrix4x4 localToWorld = pivot.localToWorldMatrix;
                Vector3 worldDirection = localToWorld.MultiplyVector(localDirection);

                Vector3 worldPos = pivot.transform.position - worldDirection * radius;
                Handles.color = c;
                Handles.DrawSolidDisc(worldPos, normal, 1);
            }
        }
    }
}
