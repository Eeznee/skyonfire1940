using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Slat : Airframe
{
    public MiniAirfoil miniFoil;

    //Settings
    public float distance = 0.06f;
    public float extendedSpeed = 70f;
    public float lockedSpeed = 80f;
    public float straightLockedSpeed = 10f;
    public float aoaEffect = 5f;

    public float extend = 0f;

    private Airfoil parentFoil;
    private Vector3 defaultPos;

    const float lockAngle = 15f;

    public override void Initialize(ObjectData d,bool firstTime)
    {
        base.Initialize(d, firstTime);
        if (firstTime)
        {
            miniFoil.Init(transform);
            extend = 0f;
            parentFoil = transform.parent.GetComponent<Airfoil>();
            defaultPos = transform.localPosition;
        }
    }

    void FixedUpdate()
    {
        if (aircraft)
        {
            Vector3 velocity = rb.GetPointVelocity(transform.position);
            float alpha = Vector3.SignedAngle(parentFoil.tr.forward, velocity, parentFoil.tr.right);
            if (data.ias < 1f) alpha = 0f;

            float straightFactor = lockedSpeed / straightLockedSpeed;
            float aerodynamicForce = data.ias;
            if (alpha < 0f) aerodynamicForce *= straightFactor * Mathf.InverseLerp(-90f, 0f, alpha);
            else if (alpha < lockAngle) aerodynamicForce *= Mathf.Lerp(1f, straightFactor, Mathf.InverseLerp(lockAngle, 0f, alpha));
            else aerodynamicForce *= Mathf.InverseLerp(-90f,lockAngle,alpha);

            float targetExtend = Mathf.InverseLerp(lockedSpeed, extendedSpeed, aerodynamicForce);
            extend = Mathf.MoveTowards(extend, targetExtend, Time.fixedDeltaTime * 2f);
            transform.localPosition = defaultPos + Vector3.forward * distance * extend;
        }
        else miniFoil.ApplyForces(this);

    }
#if UNITY_EDITOR
    //GIZMOS
    void OnDrawGizmos()
    {
        //CALCULATE AEROFOIL STRUCTURE
        miniFoil.Init(transform);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(miniFoil.rootLiftPos, miniFoil.tipLiftPos);
        Color fill = Color.green;
        fill.a = 0.06f;
        Features.DrawControlHandles(miniFoil.mainQuad.leadingBot, miniFoil.mainQuad.leadingTop, miniFoil.mainQuad.trailingTop, miniFoil.mainQuad.trailingBot, fill, Color.yellow);
    }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(Slat))]
public class SlatEditor : Editor
{
    Color backgroundColor;

    private static GUIContent deleteButton = new GUIContent("Remove", "Delete");
    public override void OnInspectorGUI()
    {
        backgroundColor = GUI.backgroundColor;
        serializedObject.Update();
        Slat slat = (Slat)target;

        GUILayout.Space(20f);
        GUI.color = Color.cyan;
        EditorGUILayout.HelpBox("Configure with parent airfoil", MessageType.None);
        GUI.color = backgroundColor;

        //Damage model
        GUILayout.Space(20f);
        GUI.color = Color.green;
        EditorGUILayout.HelpBox("Flap", MessageType.None);
        GUI.color = backgroundColor;
        slat.lockedSpeed = EditorGUILayout.FloatField("Lock Speed", Mathf.Round(slat.lockedSpeed * 36f) / 10f) / 3.6f;
        EditorGUILayout.LabelField("Area", slat.miniFoil.mainQuad.area.ToString("0.00") + " m2");
        EditorGUILayout.LabelField("Mass", slat.emptyMass.ToString("0.00") + " kg");

        if (GUI.changed)
        {
            EditorUtility.SetDirty(slat);
            EditorSceneManager.MarkSceneDirty(slat.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }

}
#endif
