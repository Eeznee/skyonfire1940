using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Pinwheel.Jupiter
{
    [CustomEditor(typeof(JSkyProfile))]
    public class JSkyProfileInspector : Editor
    {
        private JSkyProfile instance;

        private void OnEnable()
        {
            instance = target as JSkyProfile;
        }
        
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Select the sky object in the scene to edit this profile.", JEditorCommon.WordWrapItalicLabel);
        }
    }
}
