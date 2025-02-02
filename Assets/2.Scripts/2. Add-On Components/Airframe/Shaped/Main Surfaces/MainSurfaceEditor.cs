using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


#if UNITY_EDITOR
[CustomEditor(typeof(MainSurface)), CanEditMultipleObjects]
public class MainSurfaceEditor : ShapedAirframeEditor
{






    public override void OnInspectorGUI()
    {
        serializedObject.Update();



        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();
    }
}
#endif
