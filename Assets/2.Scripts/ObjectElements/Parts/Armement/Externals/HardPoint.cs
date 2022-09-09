using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class HardPoint : ObjectElement
{
    public ExternalGroup[] options;
    public int selected = 0;
    ExternalGroup selectedGroup;

    public bool bombBay = false;
    public HardPoint symmetry;
    public int priority = 0;

    public float weightCarried = 0f;
    int bombIndex = 0;

    public void LoadGroup(int option)
    {
        if (option == 0) return;
        weightCarried = 0f;
        selected = Mathf.Clamp(option, 0, options.Length);
        selectedGroup = Instantiate(options[selected - 1], transform);
        foreach (Part p in GetComponentsInChildren<Part>())
        {
            if (p != this)  weightCarried += p.Mass();
            p.Initialize(data,true);
        }
    }
    public void LoadGroup()
    {
        LoadGroup(selected);
    }

    public bool Drop()
    {
        if (bombIndex >= selectedGroup.order.Length) return false;
        if (bombBay) { if (aircraft.bombBay.state < 1f) return false; }

        weightCarried -= selectedGroup.order[bombIndex].Mass();
        selectedGroup.order[bombIndex].Drop();
        bombIndex++;

        if (bombIndex == selectedGroup.order.Length) priority = -1;
        if (symmetry) symmetry.Drop();
        return true;
    }

    public HardPoint BestHardPoint()
    {
        HardPoint[] hardPoints = aircraft.hardPoints;
        float maxMass = 0f;
        int maxPriority = -1;
        int chosen = 0;
        for (int i = 0; i < hardPoints.Length; i++)
        {
            HardPoint hp = hardPoints[i];
            if ((hp.weightCarried > maxMass && hp.priority >= maxPriority) || (hp.priority > maxPriority))
            {
                chosen = i;
                maxMass = hp.weightCarried;
                maxPriority = hp.priority;
            }
        }
        if (maxPriority == -1) return null;
        return hardPoints[chosen];
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(HardPoint))]
public class HardPointEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        //
        HardPoint hp = (HardPoint)target;

        hp.bombBay = EditorGUILayout.Toggle("Bomb Bay", hp.bombBay);
        hp.priority = EditorGUILayout.IntField("Priority", hp.priority);
        hp.symmetry = EditorGUILayout.ObjectField("Symmetric HardPoint", hp.symmetry, typeof(HardPoint), true) as HardPoint;

        if (hp.symmetry)
        {
            hp.symmetry.bombBay = hp.bombBay;
            hp.symmetry.priority = -1000;
            hp.symmetry.selected = hp.selected; 
        }

        SerializedProperty options = serializedObject.FindProperty("options");
        EditorGUILayout.PropertyField(options, true);

        hp.selected = EditorGUILayout.IntField("Option n°", hp.selected);
        if (hp.selected > 0 && hp.selected <= hp.options.Length)
        {
            EditorGUILayout.LabelField(hp.options[hp.selected - 1].name);
            float weight = 0f;
            foreach (Bomb b in hp.options[hp.selected - 1].GetComponentsInChildren<Bomb>())
            {
                weight += b.Mass();
            }
        }
        else
        {
            hp.selected = 0;
            EditorGUILayout.LabelField("Empty Load");
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(hp);
            EditorSceneManager.MarkSceneDirty(hp.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
