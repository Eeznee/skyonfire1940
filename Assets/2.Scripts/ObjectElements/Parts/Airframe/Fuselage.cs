using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Fuselage : Airframe
{
    public bool detachable = true;
    public GameObject brokenModel;

    public HydraulicSystem hydraulics;
    public bool customRipSpeed = false;
    public bool drag = false;
    public float maxSpeed = 100f;
    public float maxDrag = 0.1f;

    public override bool Detachable()
    {
        return detachable;
    }
    public override float MaxSpeed()
    {
        if (drag)
            return hydraulics ? Mathf.Lerp(base.MaxSpeed(),maxSpeed,hydraulics.state)  : maxSpeed;
        else 
            return base.MaxSpeed();
    }
    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
    }
    protected void Drag()
    {
        if (drag)
        {
            float cd = hydraulics ? maxDrag * hydraulics.state : maxDrag;
            if (cd > 0f)
            {
                Vector3 velocity = rb.velocity;
                Vector3 drag = Aerodynamics.ComputeDrag(velocity, data.tas, data.airDensity, 1f, cd, 1f);
                rb.AddForceAtPosition(drag, transform.position, ForceMode.Force);
            }
        }
    }
    private void FixedUpdate()
    {
        ForcesStress(true, customRipSpeed);
        Floating();
        Drag();
    }
    public override void Rip()
    {
        if (ripped) return;
        base.Rip();
        if (brokenModel && !Detachable())
        {
            brokenModel.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Fuselage))]
public class FuselageEditor : AirframeEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();
        //
        Fuselage fuselage = (Fuselage)target;

        fuselage.brokenModel = EditorGUILayout.ObjectField("Broken Model", fuselage.brokenModel, typeof(GameObject), true) as GameObject;
        fuselage.detachable = EditorGUILayout.Toggle("Rippable", fuselage.detachable);
        fuselage.customRipSpeed = EditorGUILayout.Toggle("Custom Rip Speed",fuselage.customRipSpeed);
        if (fuselage.customRipSpeed)
            fuselage.maxSpeed = EditorGUILayout.FloatField(fuselage.hydraulics ? "Extended Max Speed" : "Max Speed",fuselage.maxSpeed * 3.6f) / 3.6f;
        fuselage.drag = EditorGUILayout.Toggle("Has Drag",fuselage.drag);
        if (fuselage.drag)
            fuselage.maxDrag = EditorGUILayout.FloatField(fuselage.hydraulics ? "Extended Max Drag" : "Max Drag",fuselage.maxDrag);


        if (fuselage.customRipSpeed || fuselage.drag)
        {
            fuselage.hydraulics = EditorGUILayout.ObjectField("Linked Hydraulics", fuselage.hydraulics, typeof(HydraulicSystem), true) as HydraulicSystem;
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(fuselage);
            EditorSceneManager.MarkSceneDirty(fuselage.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
