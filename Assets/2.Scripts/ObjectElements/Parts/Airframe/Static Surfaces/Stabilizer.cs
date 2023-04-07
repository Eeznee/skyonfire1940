using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Stabilizer : ShapedAirframe
{
    public bool rudder = false;

    protected override Aero CreateAero()
    {
        return new ComplexAero(this, CreateQuad(), foil);
    }
    public override void CalculateAerofoilStructure()
    {
        base.CalculateAerofoilStructure();
        rudder = Mathf.Abs(transform.root.InverseTransformDirection(shapeTr.right).y) > 0.9f;
    }
    protected override void FixedUpdate()
    {
        aero.ApplyForces();
    }
#if UNITY_EDITOR
    protected override void Draw() 
    { 
        Color c = rudder ? Color.green : Color.red;
        c.a = 0.06f;
        aero.quad.Draw(c,Color.yellow,true); 
    }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(Stabilizer))]
public class StabilizerEditor : ShapedAirframeEditor
{
    public override void OnInspectorGUI()
    {
        Color backgroundColor = GUI.backgroundColor;
        Stabilizer stab = (Stabilizer)target;
        stab.shapeTr = FlightModel.AirfoilShapeTransform(stab.transform, stab.shapeTr);
        //stab.CalculateAerofoilStructure();

        GUILayout.Space(20f);
        GUI.color = Color.cyan;
        EditorGUILayout.HelpBox("Airfoil Configuration", MessageType.None); //Airfoil shape configuration
        GUI.color = backgroundColor;
        stab.foil = EditorGUILayout.ObjectField("Airfoil", stab.foil, typeof(Airfoil), false) as Airfoil;

        base.OnInspectorGUI();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(stab);
            EditorSceneManager.MarkSceneDirty(stab.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }

}
#endif
