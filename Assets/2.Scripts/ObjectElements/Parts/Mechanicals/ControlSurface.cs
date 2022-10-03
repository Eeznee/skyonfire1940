using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class ControlSurface : Airframe
{
    public enum Type { Aileron,Elevator,Rudder,Ruddervator}
    public Type type;
    public MiniAirfoil miniFoil;

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
        base.Initialize(d,firstTime);
        if (firstTime)
        {
            right = Mathv.SignNoZero(transform.root.InverseTransformPoint(miniFoil.tr.position).x);
            miniFoil.Init(transform);
            constantPos = Mathf.Sin(Mathf.Deg2Rad * Mathf.Abs(maxDeflection)) * effectiveSpeed * effectiveSpeed;
            constantNeg = Mathf.Sin(Mathf.Deg2Rad * Mathf.Abs(minDeflection)) * effectiveSpeed * effectiveSpeed;

            if (transform.parent.GetComponent<Airfoil>()) type = Type.Aileron;
            else if (Mathf.Abs(miniFoil.tr.localRotation.eulerAngles.z) < 20f) type = Type.Elevator;
            else if (Mathf.Abs(Mathf.Abs(miniFoil.tr.localRotation.eulerAngles.z) - 90f) < 20f) type = Type.Rudder;
            else type = Type.Ruddervator;
        }
    }
    private void Update()
    {
        transform.localRotation = Quaternion.identity;
        transform.Rotate(miniFoil.tr.right, controlAngle, Space.World);
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
    void FixedUpdate()
    {
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
        }
        else
            miniFoil.ApplyForces(this);
    }
#if UNITY_EDITOR
    //GIZMOS
    void OnDrawGizmos()
    {
        //CALCULATE AEROFOIL STRUCTURE
        miniFoil.Init(transform);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(miniFoil.rootLiftPos, miniFoil.tipLiftPos);
        Color fill = Color.cyan;
        fill.a = 0.06f;
        Features.DrawControlHandles(miniFoil.mainQuad.leadingBot, miniFoil.mainQuad.leadingTop, miniFoil.mainQuad.trailingTop, miniFoil.mainQuad.trailingBot, fill, Color.yellow);
    }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(ControlSurface))]
public class ControlSurfaceEditor : Editor
{
    Color backgroundColor;

    private static GUIContent deleteButton = new GUIContent("Remove", "Delete");
    public override void OnInspectorGUI()
    {
        backgroundColor = GUI.backgroundColor;
        serializedObject.Update();
        ControlSurface surface = (ControlSurface)target;

        GUILayout.Space(20f);
        GUI.color = Color.cyan;
        EditorGUILayout.HelpBox("Configure with parent airfoil", MessageType.None);
        GUI.color = backgroundColor;

        GUILayout.Space(20f);
        GUI.color = Color.green;
        EditorGUILayout.HelpBox("Control Surface", MessageType.None);
        GUI.color = backgroundColor;
        surface.emptyMass = EditorGUILayout.FloatField("Mass", surface.emptyMass);
        surface.maxDeflection = Mathf.Abs(EditorGUILayout.FloatField("Positive Limit", surface.maxDeflection));
        surface.minDeflection = Mathf.Abs(EditorGUILayout.FloatField("Negative Limit", -surface.minDeflection));
        surface.effectiveSpeed = EditorGUILayout.FloatField("Eff Speed Km/h", Mathf.Round(surface.effectiveSpeed * 36f) / 10f) / 3.6f;
        EditorGUILayout.LabelField("Area", surface.miniFoil.mainQuad.area.ToString("0.00") + " m2");

        if (GUI.changed)
        {
            EditorUtility.SetDirty(surface);
            EditorSceneManager.MarkSceneDirty(surface.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }

}
#endif
