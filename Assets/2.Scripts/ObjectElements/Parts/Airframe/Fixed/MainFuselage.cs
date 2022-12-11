using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class MainFuselage : Fuselage
{
    public AirfoilPreset foil;

    public override float MaxSpeed()
    {
        return 500f;
    }
    public override void Initialize(ObjectData d,bool firstTime)
    {
        base.Initialize(d,firstTime);
        if (firstTime){ detachable = false; }
    }

    private void FixedUpdate()
    {
        Vector3 lift = Vector3.Cross(rb.velocity, transform.up).normalized * area * foil.Cl(data.angleOfSlip) * Mathv.SmoothStart(data.tas, 2) * data.airDensity / 2f;
        Vector3 drag = -rb.velocity.normalized * area * foil.Cd(data.angleOfSlip) * Mathv.SmoothStart(data.tas, 2) * data.airDensity / 2f;
        rb.AddForce(lift + drag);

        Drag();
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(MainFuselage))]
public class MainFuselageEditor : FuselageEditor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        //
        MainFuselage frame = (MainFuselage)target;
        frame.foil = EditorGUILayout.ObjectField("Airfoil Preset", frame.foil, typeof(AirfoilPreset), false) as AirfoilPreset;

        base.OnInspectorGUI();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(frame);
            EditorSceneManager.MarkSceneDirty(frame.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
