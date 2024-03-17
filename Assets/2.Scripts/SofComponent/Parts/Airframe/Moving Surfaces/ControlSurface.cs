using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class ControlSurface : ShapedAirframe
{
    public enum Type { Aileron, Elevator, Rudder, Ruddervator }
    public Type type;

    //Control angle model rotation cheating variables
    const float controlAngleStep = 0.5f;
    private Vector3 localRotateAxis = Vector3.right;
    private float controlAngleVisual = 0f;

    //Input
    public float controlState = 0f;
    public float controlAngle = 0f;
    public float sinControlAngle;

    //Settings
    public float maxDeflection = 20f;
    public float minDeflection = 20f;
    public float effectiveSpeed = 80f;

    private float right;
    private float constantPos;
    private float constantNeg;

    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        foil = GetComponentInParent<ShapedAirframe>().foil;
        right = Mathv.SignNoZero(transform.root.InverseTransformPoint(shapeTr.position).x);
        localRotateAxis = transform.parent.InverseTransformDirection(shapeTr.right);
        constantPos = Mathf.Sin(Mathf.Deg2Rad * Mathf.Abs(maxDeflection)) * effectiveSpeed * effectiveSpeed;
        constantNeg = Mathf.Sin(Mathf.Deg2Rad * Mathf.Abs(minDeflection)) * effectiveSpeed * effectiveSpeed;
    }
    public override void CalculateAerofoilStructure()
    {
        base.CalculateAerofoilStructure();
        if (GetComponentInParent<Wing>()) type = Type.Aileron;
        else if (Mathf.Abs(shapeTr.localRotation.eulerAngles.z) < 20f) type = Type.Elevator;
        else if (Mathf.Abs(Mathf.Abs(shapeTr.localRotation.eulerAngles.z) - 90f) < 20f) type = Type.Rudder;
        else type = Type.Ruddervator;
    }
    public float ControlState()
    {
        switch (type)
        {
            case Type.Aileron: return aircraft.controlValue.z * right;
            case Type.Elevator: return aircraft.controlValue.x;
            case Type.Rudder: return aircraft.controlValue.y;
            case Type.Ruddervator: return (aircraft.controlValue.x + aircraft.controlValue.y * right) * 0.66f;
        }
        return 0f;
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (aircraft)
        {
            controlState = ControlState();
            float maxAngle = (controlState > 0) ? maxDeflection : minDeflection;
            float maxAngleDeflection = maxAngle;
            if (data.ias.Get > effectiveSpeed)
                maxAngleDeflection = Aerodynamics.MaxDeflection(data.ias.Get, (controlState > 0f) ? constantPos : constantNeg);
            controlAngle = controlState * Mathf.Min(maxAngleDeflection, maxAngle);
            controlAngle = Mathf.Clamp(controlState * maxAngle, -maxAngleDeflection, maxAngleDeflection);
            sinControlAngle = Mathv.QuickSin(-controlAngle * Mathf.Deg2Rad);
            if (Mathf.Abs(controlAngle - controlAngleVisual) > controlAngleStep)
            {
                controlAngleVisual = controlAngle;
                transform.localRotation = Quaternion.AngleAxis(controlAngle, localRotateAxis);
            }
        }
    }
#if UNITY_EDITOR
    //GIZMOS
    protected override void Draw() { aero.quad.Draw(new Color(0f, 1f, 1f, 0.06f), Color.yellow, false); }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(ControlSurface)), CanEditMultipleObjects]
public class ControlSurfaceEditor : ShapedAirframeEditor
{
    SerializedProperty minDeflection;
    SerializedProperty maxDeflection;
    SerializedProperty effectiveSpeed;
    protected override void OnEnable()
    {
        base.OnEnable();
        minDeflection = serializedObject.FindProperty("minDeflection");
        maxDeflection = serializedObject.FindProperty("maxDeflection");
        effectiveSpeed = serializedObject.FindProperty("effectiveSpeed");
    }
    static bool showFoil = true;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        ControlSurface surface = (ControlSurface)target;

        showFoil = EditorGUILayout.Foldout(showFoil, "Control Surface", true, EditorStyles.foldoutHeader);
        if (showFoil)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(minDeflection);
            EditorGUILayout.PropertyField(maxDeflection);
            surface.effectiveSpeed = EditorGUILayout.FloatField("Eff Speed Km/h", Mathf.Round(surface.effectiveSpeed * 36f) / 10f) / 3.6f;

            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
