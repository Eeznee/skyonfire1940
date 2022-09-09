using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Pinwheel.Jupiter
{
    [CustomEditor(typeof(JDayNightCycleProfile))]
    public class JDayNightCycleProfileInspector : Editor
    {
        private JDayNightCycleProfile instance;

        private void OnEnable()
        {
            instance = target as JDayNightCycleProfile;
        }
        
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Select the sky object in the scene to edit this profile.", JEditorCommon.WordWrapItalicLabel);
        }
    }
}
