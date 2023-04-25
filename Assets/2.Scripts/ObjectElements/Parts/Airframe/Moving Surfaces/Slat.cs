using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Slat : ShapedAirframe
{
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

    public override void Initialize(ObjectData d,bool firstTime)
    {
        if (firstTime)
        {
            extend = 0f;
            parentWing = transform.parent.GetComponent<Wing>();
            foil = parentWing.foil;
            defaultPos = transform.localPosition;
        }
        base.Initialize(d, firstTime);
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (aircraft)
        {
            Vector3 velocity = rb.GetPointVelocity(transform.position);
            float alpha = Vector3.SignedAngle(parentWing.shapeTr.forward, velocity, parentWing.shapeTr.right);
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
#if UNITY_EDITOR
    protected override void Draw() { aero.quad.Draw(new Color(0f, 1f, 0f, 0.06f),Color.yellow,false); }
#endif
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

        if (GUI.changed)
        {
            EditorUtility.SetDirty(slat);
            EditorSceneManager.MarkSceneDirty(slat.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }

}
#endif
