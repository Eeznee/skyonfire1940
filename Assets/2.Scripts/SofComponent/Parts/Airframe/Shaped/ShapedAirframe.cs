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
    public bool vertical;
    protected float left = 1f;

    [SerializeField] public Shape shape;

    //OLD DEPRECATED KEPT FOR MODDERS MUST REMOVE IN A FEW MONTHS
    public float angle = 0f;
    public float tipWidth = 100.0f;

    protected override Quad CreateQuad() { return shape.ToQuad(); }
    public override void UpdateAerofoil()
    {
        shape.parent = transform;
        shape.snapped = GetComponent<Wing>() && tr.parent.GetComponent<Wing>();

        vertical = Mathf.Abs(shape.localRight.y) > 0.9f;
        left = vertical ? 1f : Mathv.SignNoZero(transform.root.InverseTransformPoint(shape.position).x + 0.05f);

        shape.scale.x = left * Mathf.Abs(shape.scale.x);

        base.UpdateAerofoil();
    }
    protected readonly Color aileronColor = new Color(0f, 0.2f, 1f, 0.05f);
    protected readonly Color rudderColor = new Color(0f, 1f, 0f, 0.05f);
    protected readonly Color elevatorColor = new Color(1f, 0f, 0f, 0.05f);
    protected readonly Color flapColor = new Color(1f, 0f, 0.85f, 0.05f);
    protected readonly Color bordersColor = new Color(1f, 1f, 0f, 0.35f);


    protected virtual Color FillColor() { return Vector4.zero; }
#if UNITY_EDITOR
    public override void Draw() { foilSurface.quad.Draw(FillColor(), bordersColor, false); }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(ShapedAirframe)), CanEditMultipleObjects]
public class ShapedAirframeEditor : AirframeEditor
{
    SerializedProperty shape;
    SerializedProperty tipWidth;
    SerializedProperty angle;


    protected override void OnEnable()
    {
        base.OnEnable();
        shape = serializedObject.FindProperty("shape");

        tipWidth = serializedObject.FindProperty("tipWidth");
        angle = serializedObject.FindProperty("angle");
    }
    static bool showShapedAirframe = true;

    static bool showOldShape = false;

    protected virtual void OnSceneGUI()
    {
        ShapedAirframe frame = (ShapedAirframe)target;
        frame.UpdateAerofoil();

        if (!editShapePosition) return;
        if (Tools.current != Tool.None) previousTool = Tools.current;
        Tools.current = Tool.None;

        Vector3 shapePos = frame.shape.position;

        Handles.Label(shapePos + SceneView.lastActiveSceneView.rotation * Vector3.down * 0.5f, "Airfoil Surface Position");

        EditorGUI.BeginChangeCheck();

        Vector3 newShapePosition = Handles.PositionHandle(shapePos, frame.tr.rotation);

        if (EditorGUI.EndChangeCheck())
            frame.shape.localPosition = frame.tr.InverseTransformPoint(newShapePosition);
    }
    protected void OnDisable()
    {
        if(editShapePosition) Tools.current = previousTool;
    }
    static bool editShapePosition;
    static Tool previousTool;
    protected virtual void ShapeFoldout()
    {
        GUI.color = editShapePosition ? new Color(0.5f, 0.5f, 0.5f, 5f) : GUI.backgroundColor;
        if (GUILayout.Button(editShapePosition ? "Editing Shape" : "Edit Shape"))
        {
            editShapePosition = !editShapePosition;

            if (!editShapePosition) Tools.current = previousTool;
        }
        GUI.color = GUI.backgroundColor;

        EditorGUILayout.PropertyField(shape);
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

        showOldShape = EditorGUILayout.Foldout(showOldShape, "Old Shape System", true, EditorStyles.foldoutHeader);
        if (showOldShape)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.Slider(tipWidth, 0f, 100f);
            EditorGUILayout.Slider(angle, -60f, 60f);
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
