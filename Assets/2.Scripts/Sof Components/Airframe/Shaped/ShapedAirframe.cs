using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public abstract class ShapedAirframe : SofAirframe
{
    public bool Vertical { get; private set; }
    public float Left { get; private set; }

    [SerializeField] public Shape shape;

    public override void UpdateQuad() { quad = shape.ToQuad(); }
    public override void UpdateAerofoil()
    {
        shape.Update(this);
        Left = shape.left;
        Vertical = shape.vertical;
        base.UpdateAerofoil();
    }

    protected readonly Color aileronColor = new Color(0f, 0.2f, 1f, 0.05f);
    protected readonly Color rudderColor = new Color(0f, 1f, 0f, 0.05f);
    protected readonly Color elevatorColor = new Color(1f, 0f, 0f, 0.05f);
    protected readonly Color flapColor = new Color(1f, 0f, 0.85f, 0.05f);
    protected readonly Color bordersColor = new Color(1f, 1f, 0f, 0.35f);


    protected virtual Color FillColor() { return Vector4.zero; }
#if UNITY_EDITOR
    public override void Draw() { quad.Draw(FillColor(), bordersColor, false); }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(ShapedAirframe)), CanEditMultipleObjects]
public class ShapedAirframeEditor : SofAirframeEditor
{
    SerializedProperty shape;

    static bool editShapePosition;
    static Tool previousTool;
    static bool previousShowWings;

    protected override void OnEnable()
    {
        base.OnEnable();
        shape = serializedObject.FindProperty("shape");
    }
    static bool showShapedAirframe = true;

    protected virtual void OnSceneGUI()
    {

        ShapedAirframe frame = (ShapedAirframe)target;
        frame.UpdateAerofoil();

        if (!editShapePosition) return;

        if (Tools.current != Tool.None) previousTool = Tools.current;
        Tools.current = Tool.None;
        SofWindow.showWingsOverlay = true;

        Vector3 shapePos = frame.shape.Position;

        Handles.Label(shapePos + SceneView.lastActiveSceneView.rotation * Vector3.down * 0.5f, "Airfoil Surface Position");

        EditorGUI.BeginChangeCheck();

        Vector3 newShapePosition = Handles.PositionHandle(shapePos, frame.tr.rotation);

        if (EditorGUI.EndChangeCheck())
            frame.shape.localPosition = frame.tr.InverseTransformPoint(newShapePosition);

        serializedObject.ApplyModifiedProperties();
    }
    protected void OnDisable()
    {
        if (editShapePosition) OnEditingShapeStopped();
    }

    protected virtual void ShapeFoldout()
    {
        GUI.color = editShapePosition ? new Color(0.5f, 0.5f, 0.5f, 5f) : GUI.backgroundColor;
        if (GUILayout.Button(editShapePosition ? "Editing Shape" : "Edit Shape"))
        {
            editShapePosition = !editShapePosition;

            if (editShapePosition) OnEditingShapeStarted();
            if (!editShapePosition) OnEditingShapeStopped();
        }
        GUI.color = GUI.backgroundColor;

        EditorGUILayout.PropertyField(shape);
    }
    private void OnEditingShapeStarted()
    {
        previousShowWings = SofWindow.showWingsOverlay;
    }
    private void OnEditingShapeStopped()
    {
        Tools.current = previousTool;
        SofWindow.showWingsOverlay = previousShowWings;
    }

    public override void OnInspectorGUI()
    {
        ShapedAirframe frame = (ShapedAirframe)target;

        base.OnInspectorGUI();
        serializedObject.Update();

        showShapedAirframe = EditorGUILayout.Foldout(showShapedAirframe, "Shape", true, EditorStyles.foldoutHeader);
        if (showShapedAirframe)
        {
            EditorGUI.indentLevel++;
            ShapeFoldout();
            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
