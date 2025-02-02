using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[AddComponentMenu("Sof Components/Aerodynamic Surfaces/Slat")]
public class Slat : Subsurface
{
    //Settings
    public float distance = 0.06f;
    public float extendedSpeed = 70f;
    public float lockedSpeed = 80f;
    public float straightLockedSpeed = 10f;
    public float aoaEffect = 5f;

    public float extend { get; private set; }


    private Vector3 defaultPos;

    const float lockAngle = 15f;


    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        extend = 0f;
        defaultPos = transform.localPosition;
    }



    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (Parent && aircraft)
        {
            Vector3 velocity = rb.GetPointVelocity(transform.position);
            float alpha = Vector3.SignedAngle(Parent.shape.Forward, velocity, Parent.shape.Right);
            if (data.ias.Get < 1f) alpha = 0f;

            float straightFactor = lockedSpeed / straightLockedSpeed;
            float aerodynamicForce = data.ias.Get;
            if (alpha < 0f) aerodynamicForce *= straightFactor * Mathf.InverseLerp(-90f, 0f, alpha);
            else if (alpha < lockAngle) aerodynamicForce *= Mathf.Lerp(1f, straightFactor, Mathf.InverseLerp(lockAngle, 0f, alpha));
            else aerodynamicForce *= Mathf.InverseLerp(-90f, lockAngle, alpha);

            float targetExtend = Mathf.InverseLerp(lockedSpeed, extendedSpeed, aerodynamicForce);
            extend = Mathf.MoveTowards(extend, targetExtend, Time.fixedDeltaTime * 2f);
        }
    }
    private void Update()
    {
        if (!aircraft) return;

        transform.localPosition = defaultPos + Vector3.forward * distance * extend;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(Slat))]
public class SlatEditor : ShapedAirframeEditor
{
    bool showSlat;
    public override void OnInspectorGUI()
    {
        Slat slat = (Slat)target;
        showSlat = EditorGUILayout.Foldout(showSlat, "Slat", true, EditorStyles.foldoutHeader);

        if (showSlat)
        {
            EditorGUI.indentLevel++;
            slat.lockedSpeed = EditorGUILayout.FloatField("Lock Speed km/h", Mathf.Round(slat.lockedSpeed * 36f) / 10f) / 3.6f;
            EditorGUI.indentLevel--;
        }

        base.OnInspectorGUI();

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
