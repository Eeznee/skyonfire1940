using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public abstract class ShapedAirframe : SofAirframe
{
    public bool vertical;
    public Transform shapeTr;
    public float angle = 0f;
    public float tipWidth = 100.0f;

    public override void SetReferences(SofComplex _complex)
    {
        base.SetReferences(_complex);
        shapeTr = FlightModel.AirfoilShapeTransform(transform, shapeTr);
    }
    protected override Quad CreateQuad()
    {
        Vector3 pos = transform.InverseTransformPoint(shapeTr.position);
        Vector3 forward = transform.InverseTransformDirection(shapeTr.forward);
        Vector3 right = transform.InverseTransformDirection(shapeTr.right);
        Vector3 rootLiftPos = pos - (right * (shapeTr.localScale.x * 0.5f));
        Vector3 tipLiftPos = rootLiftPos + (right * shapeTr.localScale.x) + (forward * Mathf.Abs(shapeTr.localScale.x) / Mathf.Tan((90f - angle) * Mathf.Deg2Rad));

        forward *= shapeTr.localScale.z;
        Vector3 lt = tipLiftPos + (forward * (1f - Quad.liftLine) * tipWidth / 100f);
        Vector3 lb = rootLiftPos + (forward * (1f - Quad.liftLine));
        Vector3 tt = tipLiftPos - (forward * Quad.liftLine * tipWidth / 100f);
        Vector3 tb = rootLiftPos - (forward * Quad.liftLine);

        return new Quad(transform, lt, lb, tt, tb);
    }
    public override void CalculateAerofoilStructure()
    {
        shapeTr = FlightModel.AirfoilShapeTransform(transform, shapeTr);

        vertical = Mathf.Abs(transform.root.InverseTransformDirection(shapeTr.right).y) > 0.9f;
        left = vertical ? 1f : Mathv.SignNoZero(transform.root.InverseTransformPoint(shapeTr.position).x + 0.05f);

        shapeTr.localScale = new Vector3(left * Mathf.Abs(shapeTr.localScale.x), 1f, shapeTr.localScale.z);

        base.CalculateAerofoilStructure();
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(ShapedAirframe)), CanEditMultipleObjects]
public class ShapedAirframeEditor : AirframeEditor
{
    SerializedProperty tipWidth;
    SerializedProperty angle;
    protected override void OnEnable()
    {
        base.OnEnable();
        tipWidth = serializedObject.FindProperty("tipWidth");
        angle = serializedObject.FindProperty("angle");
    }
    static bool showShapedAirframe = true;

    protected virtual void ShapeFoldout()
    {
        ShapedAirframe frame = (ShapedAirframe)target;
        EditorGUILayout.ObjectField("Shape Transform", frame.shapeTr, typeof(Transform), true);
        EditorGUILayout.Slider(tipWidth, 0f, 100f);
        EditorGUILayout.Slider(angle, -60f, 60f);
    }

    public override void OnInspectorGUI()
    {
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
