using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public abstract class ShapedAirframe : AirframeBase
{
    public Transform shapeTr;
    public float angle = 0f;
    public float tipWidth = 100.0f;


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
        left = Mathv.SignNoZero(transform.root.InverseTransformPoint(shapeTr.position).x + 0.05f);
        shapeTr.localScale = new Vector3(left * Mathf.Abs(shapeTr.localScale.x), 1f, shapeTr.localScale.z);
        base.CalculateAerofoilStructure();
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(ShapedAirframe))]
public class ShapedAirframeEditor : AirframeEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(20f);

        Color backgroundColor = GUI.backgroundColor;
        serializedObject.Update();

        ShapedAirframe frame = (ShapedAirframe)target;
        frame.shapeTr = FlightModel.AirfoilShapeTransform(frame.transform, frame.shapeTr);

        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox("Shape", MessageType.None);
        GUI.color = backgroundColor;

        frame.shapeTr = EditorGUILayout.ObjectField("Shape Transform", frame.shapeTr, typeof(Transform), true) as Transform;
        frame.tipWidth = EditorGUILayout.Slider("Tip Width", frame.tipWidth, 5f, 100f);
        frame.angle = EditorGUILayout.Slider("Angle", frame.angle, -50f, 20f);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(frame);
            EditorSceneManager.MarkSceneDirty(frame.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
