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

    public float visualExtension { get; private set; }


    private Vector3 defaultPos;
    private float straightFactor;

    const float lockAngle = 15f;


    public override void Initialize(SofModular _complex)
    {
        base.Initialize(_complex);
        visualExtension = 0f;
        defaultPos = transform.localPosition;
        straightFactor = lockedSpeed / straightLockedSpeed;

        aircraft.OnUpdateLOD0 += UpdateSlatPosition;
    }
    public override void SetReferences(SofModular _complex)
    {
        if (aircraft) aircraft.OnUpdateLOD0 -= UpdateSlatPosition;
        base.SetReferences(_complex);
    }
    public float SlatExtension(float speed, float alpha)
    {
        if (speed < 1f) alpha = 0f;

        float aerodynamicForce = speed;
        if (alpha < 0f) aerodynamicForce *= straightFactor * Mathf.InverseLerp(-90f, 0f, alpha);
        else if (alpha < lockAngle) aerodynamicForce *= Mathf.Lerp(1f, straightFactor, Mathf.InverseLerp(lockAngle, 0f, alpha));
        else aerodynamicForce *= Mathf.InverseLerp(-90f, lockAngle, alpha);

        return Mathf.InverseLerp(lockedSpeed, extendedSpeed, aerodynamicForce);
    }
    public float SlatEffect(float speed, float alpha)
    {
        float extension = SlatExtension(speed, alpha);
        return extension * aoaEffect * Mathf.InverseLerp(15f, 15f + aoaEffect * 2f, alpha);
    }
    private void UpdateSlatPosition()
    {
        Vector3 velocity = rb.GetPointVelocity(transform.position);
        float alpha = Vector3.SignedAngle(Parent.shape.Forward, velocity, Parent.shape.Right);

        float extension = SlatExtension(velocity.magnitude, alpha);
        visualExtension = Mathf.MoveTowards(visualExtension, extension, Time.fixedDeltaTime * 2f);

        transform.localPosition = defaultPos + Vector3.forward * distance * visualExtension;
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
