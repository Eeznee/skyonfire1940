using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class ControlSurface : ShapedAirframe
{
    public enum Type { Aileron,Elevator,Rudder,Ruddervator}
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

    public override void Initialize(ObjectData d,bool firstTime)
    {
        if (firstTime)
            foil = GetComponentInParent<ShapedAirframe>().foil;
        base.Initialize(d,firstTime);
        if (firstTime)
        {
            right = Mathv.SignNoZero(transform.root.InverseTransformPoint(shapeTr.position).x);
            localRotateAxis = transform.parent.InverseTransformDirection(shapeTr.right);
            constantPos = Mathf.Sin(Mathf.Deg2Rad * Mathf.Abs(maxDeflection)) * effectiveSpeed * effectiveSpeed;
            constantNeg = Mathf.Sin(Mathf.Deg2Rad * Mathf.Abs(minDeflection)) * effectiveSpeed * effectiveSpeed;

            if (GetComponentInParent<Wing>()) type = Type.Aileron;
            else if (Mathf.Abs(shapeTr.localRotation.eulerAngles.z) < 20f) type = Type.Elevator;
            else if (Mathf.Abs(Mathf.Abs(shapeTr.localRotation.eulerAngles.z) - 90f) < 20f) type = Type.Rudder;
            else type = Type.Ruddervator;
        }
    }
    public float ControlState()
    {
        switch (type){
            case Type.Aileron: return aircraft.controlInput.z * right;
            case Type.Elevator: return aircraft.controlInput.x;
            case Type.Rudder: return aircraft.controlInput.y;
            case Type.Ruddervator: return (aircraft.controlInput.x + aircraft.controlInput.y * right) * 0.66f;
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
            if (data.ias > effectiveSpeed)
                maxAngleDeflection = Aerodynamics.MaxDeflection(data.ias, (controlState > 0) ? constantPos : constantNeg);
            controlAngle = controlState * maxAngle;
            controlAngle = Mathf.Clamp(controlAngle, -maxAngleDeflection, maxAngleDeflection);
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
    protected override void Draw() { aero.quad.Draw(new Color(0f, 1f, 1f, 0.06f), Color.yellow,false); }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(ControlSurface))]
public class ControlSurfaceEditor : ShapedAirframeEditor
{
    Color backgroundColor;

    private static GUIContent deleteButton = new GUIContent("Remove", "Delete");
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        backgroundColor = GUI.backgroundColor;
        serializedObject.Update();
        ControlSurface surface = (ControlSurface)target;

        GUILayout.Space(20f);
        GUI.color = Color.red;
        EditorGUILayout.HelpBox("Control Surface", MessageType.None);
        GUI.color = backgroundColor;
        surface.maxDeflection = Mathf.Abs(EditorGUILayout.FloatField("Positive Limit", surface.maxDeflection));
        surface.minDeflection = Mathf.Abs(EditorGUILayout.FloatField("Negative Limit", -surface.minDeflection));
        surface.effectiveSpeed = EditorGUILayout.FloatField("Eff Speed Km/h", Mathf.Round(surface.effectiveSpeed * 36f) / 10f) / 3.6f;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(surface);
            EditorSceneManager.MarkSceneDirty(surface.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }

}
#endif
