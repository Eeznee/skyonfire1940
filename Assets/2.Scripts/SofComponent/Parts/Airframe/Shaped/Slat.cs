using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Slat : ShapedAirframe
{
    public override float MaxHp => ModulesHPData.controlHpPerSq;
    //Settings
    public float distance = 0.06f;
    public float extendedSpeed = 70f;
    public float lockedSpeed = 80f;
    public float straightLockedSpeed = 10f;
    public float aoaEffect = 5f;

    public float extend = 0f;

    private Wing parentWing;
    private Vector3 defaultPos;

    const float lockAngle = 15f;


    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        parentWing = transform.parent.GetComponent<Wing>();
        extend = 0f;
        defaultPos = transform.localPosition;
        foil = parentWing.foil;
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (parentWing && aircraft)
        {
            Vector3 velocity = rb.GetPointVelocity(transform.position);
            float alpha = Vector3.SignedAngle(parentWing.shape.forward, velocity, parentWing.shape.right);
            if (data.ias.Get < 1f) alpha = 0f;

            float straightFactor = lockedSpeed / straightLockedSpeed;
            float aerodynamicForce = data.ias.Get;
            if (alpha < 0f) aerodynamicForce *= straightFactor * Mathf.InverseLerp(-90f, 0f, alpha);
            else if (alpha < lockAngle) aerodynamicForce *= Mathf.Lerp(1f, straightFactor, Mathf.InverseLerp(lockAngle, 0f, alpha));
            else aerodynamicForce *= Mathf.InverseLerp(-90f, lockAngle, alpha);

            float targetExtend = Mathf.InverseLerp(lockedSpeed, extendedSpeed, aerodynamicForce);
            extend = Mathf.MoveTowards(extend, targetExtend, Time.fixedDeltaTime * 2f);
            transform.localPosition = defaultPos + Vector3.forward * distance * extend;
        }
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(Slat))]
public class SlatEditor : ShapedAirframeEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Color backgroundColor = GUI.backgroundColor;
        serializedObject.Update();
        Slat slat = (Slat)target;

        GUILayout.Space(20f);
        GUI.color = Color.green;
        EditorGUILayout.HelpBox("Slat", MessageType.None);
        GUI.color = backgroundColor;
        slat.lockedSpeed = EditorGUILayout.FloatField("Lock Speed", Mathf.Round(slat.lockedSpeed * 36f) / 10f) / 3.6f;

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
