using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(Gun)), CanEditMultipleObjects]
public class GunEditor : SofComponentEditor
{
    SerializedProperty gunPreset;
    SerializedProperty controller;
    SerializedProperty noConvergeance;

    SerializedProperty ejectCasings;
    SerializedProperty ejectionPos;
    SerializedProperty muzzlePos;
    SerializedProperty separateBulletPos;
    SerializedProperty bulletPos;

    SerializedProperty magazine;
    SerializedProperty clipAmmo;
    SerializedProperty magStorage;
    SerializedProperty magazineAttachPoint;
    protected override void OnEnable()
    {
        base.OnEnable();

        gunPreset = serializedObject.FindProperty("gunPreset");
        controller = serializedObject.FindProperty("controller");
        noConvergeance = serializedObject.FindProperty("noConvergeance");

        ejectCasings = serializedObject.FindProperty("ejectCasings");
        ejectionPos = serializedObject.FindProperty("ejectionPos");
        muzzlePos = serializedObject.FindProperty("muzzlePos");
        separateBulletPos = serializedObject.FindProperty("separateBulletPos");
        bulletPos = serializedObject.FindProperty("bulletPos");

        magazineAttachPoint = serializedObject.FindProperty("magazineAttachPoint");

        magazine = serializedObject.FindProperty("magazine");
        clipAmmo = serializedObject.FindProperty("clipAmmo");
        magStorage = serializedObject.FindProperty("magStorage");
    }

    static bool showGunMain = true;
    static bool showPositions = true;
    static bool showAmmo = true;


    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        Gun gun = (Gun)target;

        showGunMain = EditorGUILayout.Foldout(showGunMain, "Gun", true, EditorStyles.foldoutHeader);
        if (showGunMain)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(gunPreset);
            EditorGUILayout.PropertyField(controller);
            EditorGUILayout.PropertyField(noConvergeance, new GUIContent("No Auto Convergeance"));

            EditorGUI.indentLevel--;
        }

        showAmmo = EditorGUILayout.Foldout(showAmmo, "Ammo & Mags", true, EditorStyles.foldoutHeader);
        if (showAmmo)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox(gun.magazine ? "Can be reloaded from a stock" : "No magazine system, cannot be reloaded", MessageType.None);
            EditorGUILayout.PropertyField(magazine);
            if (!gun.magazine)
                EditorGUILayout.PropertyField(clipAmmo, new GUIContent("Ammo Capacity"));
            else
                EditorGUILayout.PropertyField(magStorage, new GUIContent("Magazine Stock"));

            EditorGUI.indentLevel--;
        }

        showPositions = EditorGUILayout.Foldout(showPositions, "Positions", true, EditorStyles.foldoutHeader);
        if (showPositions)
        {
            EditorGUI.indentLevel++;
            if (gun.magazine) EditorGUILayout.PropertyField(magazineAttachPoint, new GUIContent("Mag Attach Point"));

            GUI.color = editShapePosition ? new Color(0.5f, 0.5f, 0.5f, 5f) : GUI.backgroundColor;
            if (GUILayout.Button(editShapePosition ? "Editing Positions" : "Edit Positions"))
            {
                editShapePosition = !editShapePosition;

                if (!editShapePosition) Tools.current = previousTool;
            }
            GUI.color = GUI.backgroundColor;
            EditorGUILayout.HelpBox("The gladiator uses split Bullet & SFX as it would fire on itself if that wasn't the case", MessageType.Info);
            EditorGUILayout.PropertyField(separateBulletPos, new GUIContent("Split Bullet & SFX"));

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(muzzlePos, new GUIContent(gun.separateBulletPos ? "Muzzle SFX Pos" : "Muzzle Pos"));
            if (gun.separateBulletPos)
            {
                EditorGUILayout.PropertyField(bulletPos, new GUIContent("Muzzle Bullets"));
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.PropertyField(ejectCasings, new GUIContent("Eject Casings"));
            if (gun.ejectCasings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(ejectionPos, new GUIContent("Ejected Casings Pos"));
                EditorGUI.indentLevel--;
            }


            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();
    }
    static Tool previousTool;
    static bool editShapePosition;

    protected virtual void OnSceneGUI()
    {
        Gun gun = (Gun)target;
        Transform tr = gun.transform;

        if (!editShapePosition) return;
        if (Tools.current != Tool.None) previousTool = Tools.current;
        Tools.current = Tool.None;

        Vector3 ejectionPos = tr.TransformPoint(gun.ejectionPos);
        Vector3 muzzlePos = tr.TransformPoint(gun.muzzlePos);
        Vector3 bulletPos = tr.TransformPoint(gun.bulletPos);

        if (gun.ejectCasings)
            Handles.Label(ejectionPos + SceneView.lastActiveSceneView.rotation * Vector3.down * 0.3f, "Casings Ejection");
        Handles.Label(muzzlePos + SceneView.lastActiveSceneView.rotation * Vector3.down * 0.3f, gun.separateBulletPos ? "SFX Muzzle" : "Muzzle");
        if (gun.separateBulletPos)
            Handles.Label(bulletPos + SceneView.lastActiveSceneView.rotation * Vector3.down * 0.3f, "Bullet Instantiation");

        EditorGUI.BeginChangeCheck();

        if (gun.ejectCasings)
            ejectionPos = Handles.PositionHandle(ejectionPos, tr.rotation);
        muzzlePos = Handles.PositionHandle(muzzlePos, tr.rotation);
        if (gun.separateBulletPos)
            bulletPos = Handles.PositionHandle(bulletPos, tr.rotation);

        if (EditorGUI.EndChangeCheck())
        {
            if (gun.ejectCasings)
                gun.ejectionPos = tr.InverseTransformPoint(ejectionPos);
            gun.muzzlePos = tr.InverseTransformPoint(muzzlePos);
            if (gun.separateBulletPos)
                gun.bulletPos = tr.InverseTransformPoint(bulletPos);
        }

    }
    protected void OnDisable()
    {
        if (editShapePosition) Tools.current = previousTool;
    }
}
#endif