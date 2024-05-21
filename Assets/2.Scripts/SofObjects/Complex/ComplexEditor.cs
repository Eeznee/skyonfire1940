using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

#if UNITY_EDITOR
[CustomEditor(typeof(SofComplex))]
public class SofComplexEditor : SofObjectEditor
{
    private Mass emptyMass;
    private Mass loadedMass;



    SerializedProperty targetEmptyMass;
    SerializedProperty cogForwardDis;

    protected virtual void OnEnable()
    {
        targetEmptyMass = serializedObject.FindProperty("targetEmptyMass");
        cogForwardDis = serializedObject.FindProperty("cogForwardDistance");

        SofComplex complex = (SofComplex)target;

        SofPart[] partsArray = complex.parts.ToArray();
        emptyMass = new Mass(partsArray, true);
        loadedMass = new Mass(partsArray, false);

        serializedObject.ApplyModifiedProperties();
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

            EditorGUILayout.LabelField("Empty Mass", emptyMass.mass.ToString("0.00") + " kg");
            EditorGUILayout.LabelField("Loaded Mass", loadedMass.mass.ToString("0.00") + " kg");
            EditorGUILayout.LabelField("Empty COG", emptyMass.center.ToString("F2"));
            EditorGUILayout.LabelField("Loaded COG", loadedMass.center.ToString("F2"));

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
                OnEnable();
            }

            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif