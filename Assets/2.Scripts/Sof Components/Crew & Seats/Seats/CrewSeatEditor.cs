using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(CrewSeat)), CanEditMultipleObjects]
public class CrewSeatEditor : Editor
{
    SerializedProperty externalViewPoint;
    SerializedProperty goProViewPoint;
    SerializedProperty defaultPov;
    SerializedProperty lookBehindRightShift;
    SerializedProperty lookBehindLeftShift;

    SerializedProperty rightHandGrip;
    SerializedProperty leftHandGrip;
    SerializedProperty rightFootRest;
    SerializedProperty leftFootRest;

    SerializedProperty canopy;
    SerializedProperty audioRatio;
    SerializedProperty closedRatio;

    SerializedProperty visibility;

    SerializedProperty reloadableGuns;
    SerializedProperty magTrash;

    protected virtual void OnEnable()
    {
        externalViewPoint = serializedObject.FindProperty("externalViewPoint");
        goProViewPoint = serializedObject.FindProperty("goProViewPoint");
        defaultPov = serializedObject.FindProperty("defaultPOV");
        lookBehindRightShift = serializedObject.FindProperty("lookBehindRightShift");
        lookBehindLeftShift = serializedObject.FindProperty("lookBehindLeftShift");

        rightHandGrip = serializedObject.FindProperty("rightHandGrip");
        leftHandGrip = serializedObject.FindProperty("leftHandGrip");
        rightFootRest = serializedObject.FindProperty("rightFootRest");
        leftFootRest = serializedObject.FindProperty("leftFootRest");

        canopy = serializedObject.FindProperty("canopy");
        audioRatio = serializedObject.FindProperty("audioRatio");
        closedRatio = serializedObject.FindProperty("closedRatio");

        visibility = serializedObject.FindProperty("visibility");

        reloadableGuns = serializedObject.FindProperty("reloadableGuns");
        magTrash = serializedObject.FindProperty("magTrash");
    }

    static bool showCameraPos = true;
    static bool showLimbsPos = true;
    static bool showAudio = true;
    static bool showWeapons = true;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CrewSeat seat = (CrewSeat)target;

        showCameraPos = EditorGUILayout.Foldout(showCameraPos, "Head/Cams Positions", true, EditorStyles.foldoutHeader);
        if (showCameraPos)
        {
            EditorGUI.indentLevel++;
            CameraPositions();
            EditorGUI.indentLevel--;
        }
        showLimbsPos = EditorGUILayout.Foldout(showLimbsPos, "Limbs Positions", true, EditorStyles.foldoutHeader);
        if (showLimbsPos)
        {
            EditorGUI.indentLevel++;
            LimbsPositionsGUI();
            EditorGUI.indentLevel--;
        }

        showAudio = EditorGUILayout.Foldout(showAudio, "Audio", true, EditorStyles.foldoutHeader);
        if (showAudio)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(canopy, new GUIContent("Linked Canopy"));
            EditorGUILayout.Slider(audioRatio, 0f, 1f,new GUIContent(seat.canopy ? "Opened Canopy Ratio" : "Cockpit Audio Ratio"));
            if (seat.canopy)
                EditorGUILayout.Slider(closedRatio, 0f, 1f, new GUIContent("Closed Canopy Ratio"));

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.PropertyField(visibility, new GUIContent("Visibility"));

        showWeapons = EditorGUILayout.Foldout(showWeapons, "Weapons", true, EditorStyles.foldoutHeader);
        if (showWeapons)
        {
            EditorGUI.indentLevel++;
            WeaponsGUI();
            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }
    protected virtual void CameraPositions()
    {
        GUI.color = editShapePosition ? new Color(0.5f, 0.5f, 0.5f, 5f) : GUI.backgroundColor;
        if (GUILayout.Button(editShapePosition ? "Editing Positions" : "Edit Positions In Scene"))
        {
            editShapePosition = !editShapePosition;
            if (!editShapePosition) Tools.current = previousTool;
        }
        GUI.color = GUI.backgroundColor;

        EditorGUILayout.PropertyField(externalViewPoint, new GUIContent("3rd Person Cam Pos"));
        EditorGUILayout.PropertyField(goProViewPoint, new GUIContent("GoPro Cam Pos"));

        EditorGUILayout.PropertyField(defaultPov, new GUIContent("Head Pos"));
        EditorGUILayout.PropertyField(lookBehindRightShift, new GUIContent("Look Behind Right Shift"));
        EditorGUILayout.PropertyField(lookBehindLeftShift, new GUIContent("Look Behind Left Shift"));

    }
    protected virtual void LimbsPositionsGUI()
    {
        EditorGUILayout.PropertyField(rightHandGrip, new GUIContent("Right Hand"));
        EditorGUILayout.PropertyField(leftHandGrip, new GUIContent("Left Hand"));
        EditorGUILayout.PropertyField(rightFootRest, new GUIContent("Right Foot"));
        EditorGUILayout.PropertyField(leftFootRest, new GUIContent("Left Foot"));
    }
    protected virtual void WeaponsGUI()
    {
        CrewSeat seat = (CrewSeat)target;

        EditorGUILayout.PropertyField(reloadableGuns, new GUIContent("Reloadable Guns"));
        if (seat.reloadableGuns != null && seat.reloadableGuns.Length > 0)
            EditorGUILayout.PropertyField(magTrash, new GUIContent("Magazine Trash"));
    }


    //POSITION MOVE
    static Tool previousTool;
    static bool editShapePosition;

    protected virtual void OnSceneGUI()
    {
        CrewSeat seat = (CrewSeat)target;
        Transform tr = seat.transform;

        if (!editShapePosition) return;
        if (Tools.current != Tool.None) previousTool = Tools.current;
        Tools.current = Tool.None;

        Vector3 externalPos = tr.position + tr.root.TransformDirection(seat.externalViewPoint);
        Vector3 goProPos = tr.position + tr.root.TransformDirection(seat.goProViewPoint);

        Handles.Label(externalPos + SceneView.lastActiveSceneView.rotation * Vector3.down * 0.3f, "Third Person");
        Handles.Label(goProPos + SceneView.lastActiveSceneView.rotation * Vector3.down * 0.3f, "GoPro POV");

        EditorGUI.BeginChangeCheck();

        externalPos = Handles.PositionHandle(externalPos, tr.root.rotation);
        goProPos = Handles.PositionHandle(goProPos, tr.root.rotation);

        if (EditorGUI.EndChangeCheck())
        {
            seat.externalViewPoint = tr.root.InverseTransformDirection(externalPos - tr.position);
            seat.goProViewPoint = tr.root.InverseTransformDirection(goProPos - tr.position);
        }
        serializedObject.ApplyModifiedProperties();
    }
    protected void OnDisable()
    {
        if (editShapePosition) Tools.current = previousTool;
    }
}
#endif