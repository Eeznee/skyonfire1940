using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[AddComponentMenu("Sof Components/Aerodynamic Surfaces/Flap")]
public class Flap : Subsurface
{
    [SerializeField] private FlapsDesign flapDesign;
    [SerializeField] private float extendedRipSpeedKph = 240f;

    const float kphToMps = 1f / 3.6f;
    public float ExtendedRipSpeed => extendedRipSpeedKph * kphToMps;
    public FlapsDesign Design => flapDesign;


    public override float MaxSpd => Mathf.Lerp(base.MaxSpd, ExtendedRipSpeed, aircraft.hydraulics.flaps.state);


    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);

        if (flapDesign == null) Debug.LogError("You must assign a flap design to this flap", this);
    }

#if UNITY_EDITOR
    protected override Color FillColor()
    {
        return new Color(1f, 0f, 0.85f, 0.2f);
    }
#endif
}
#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(Flap))]
public class FlapEditor : ShapedAirframeEditor
{
    static bool showFlap = true;

    SerializedProperty flapDesign;
    SerializedProperty extendedRipSpeedKph;

    protected override void OnEnable()
    {
        base.OnEnable();

        flapDesign = serializedObject.FindProperty("flapDesign");
        extendedRipSpeedKph = serializedObject.FindProperty("extendedRipSpeedKph");
    }
    public override void OnInspectorGUI()
    {
        Flap flap = (Flap)target;
        base.OnInspectorGUI();

        showFlap = EditorGUILayout.Foldout(showFlap, "Flap", true, EditorStyles.foldoutHeader);
        if (showFlap)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(flapDesign);
            EditorGUILayout.PropertyField(extendedRipSpeedKph, new GUIContent("Rip Speed km/h"));

            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }

}
#endif
