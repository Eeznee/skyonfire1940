using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using System;

namespace Pinwheel.Jupiter
{
    [InitializeOnLoad]
    public class JDayNightCycleProfileInspectorDrawer
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

        static JDayNightCycleProfileInspectorDrawer()
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

        private JDayNightCycleProfile instance;

        private JDayNightCycleProfileInspectorDrawer(JDayNightCycleProfile instance)
        {
            this.instance = instance;
        }

        public static JDayNightCycleProfileInspectorDrawer Create(JDayNightCycleProfile instance)
        {
            return new JDayNightCycleProfileInspectorDrawer(instance);
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
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(instance);
            }
        }

        private void DrawSkyGUI()
        {
            string label = "Sky";
            string id = "sky" + instance.GetInstanceID();

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
            string id = "stars" + instance.GetInstanceID();

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
            string id = "sun" + instance.GetInstanceID();

            JEditorCommon.Foldout(label, false, id, () =>
            {
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
            string id = "moon" + instance.GetInstanceID();

            JEditorCommon.Foldout(label, false, id, () =>
            {
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
            string id = "horizon-cloud" + instance.GetInstanceID();

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
            string id = "overhead-cloud" + instance.GetInstanceID();

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
            string id = "detail-overlay" + instance.GetInstanceID();

            JEditorCommon.Foldout(label, false, id, () =>
            {
                DisplayAddedProperties("Detail Overlay");
                if (GUILayout.Button("Add"))
                {
                    DisplayAllPropertiesAsContext("Detail Overlay");
                }
            });
        }

        private void DisplayAddedProperties(string group)
        {
            EditorGUI.indentLevel -= 1;
            JAnimatedProperty toRemoveProp = null;
            List<JAnimatedProperty> props = instance.AnimatedProperties.FindAll(p => p.DisplayName.StartsWith(group));
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
                instance.AnimatedProperties.Remove(toRemoveProp);
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
                bool added = instance.AnimatedProperties.FindIndex(p0 => p0.Name.Equals(p.Name)) >= 0;

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
                            instance.AnimatedProperties.Add(p);
                        });
                }
            }
            menu.ShowAsContext();
        }
    }
}
