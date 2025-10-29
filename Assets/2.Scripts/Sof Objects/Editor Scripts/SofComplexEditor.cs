using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


#if UNITY_EDITOR
[CustomEditor(typeof(SofComplex))]
public class SofComplexEditor : SofModularEditor
{
    SerializedProperty targetEmptyMass;
    SerializedProperty cogForwardDis;

    protected override void OnEnable()
    {
        targetEmptyMass = serializedObject.FindProperty("targetEmptyMass");
        cogForwardDis = serializedObject.FindProperty("cogForwardDistance");

        base.OnEnable();
    }

    static bool showMass = true;
    static bool showAutoMass = true;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        SofComplex complex = (SofComplex)target;

        showMass = EditorGUILayout.Foldout(showMass, "Mass Infos", true, EditorStyles.foldoutHeader);
        if (showMass)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("Empty Mass", complex.EmptyMass.mass.ToString("0.00") + " kg");
            EditorGUILayout.LabelField("Loaded Mass", complex.LoadedMass.mass.ToString("0.00") + " kg");
            EditorGUILayout.LabelField("Empty COG", complex.EmptyMass.center.ToString("F2"));
            EditorGUILayout.LabelField("Loaded COG", complex.LoadedMass.center.ToString("F2"));

            EditorGUI.indentLevel--;
        }

        showAutoMass = EditorGUILayout.Foldout(showAutoMass, "Auto Mass Tool", true, EditorStyles.foldoutHeader);
        if (showAutoMass)
        {
            EditorGUI.indentLevel++;


            EditorGUILayout.HelpBox("Set a target mass and center of gravity position (Z axis) then press the button. A new mass will be assigned to each airframe to match desired values.", MessageType.Info);

            EditorGUILayout.PropertyField(targetEmptyMass);
            EditorGUILayout.PropertyField(cogForwardDis);

            if (GUILayout.Button("Compute new parts mass to match target mass & COG"))
            {
                complex.ComputeAutoMass(new Mass(complex.targetEmptyMass, Vector3.forward * complex.cogForwardDistance));
                complex.SetReferences();
                EditorUtility.SetDirty(complex);
                EditorSceneManager.MarkSceneDirty(complex.gameObject.scene);
            }


            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();


    }
}
#endif