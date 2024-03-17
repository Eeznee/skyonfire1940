using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Stabilizer : ShapedAirframe
{
    public override float AreaCd() { return area * foil.airfoilSim.minCd; }

    protected override Aero CreateAero()
    {
        return new ComplexAero(this, CreateQuad(), foil);
    }
    protected override void FixedUpdate()
    {
        aero.ApplyForces();
    }
#if UNITY_EDITOR
    protected override void Draw()
    {
        Color c = vertical ? Color.green : Color.red;
        c.a = 0.06f;
        aero.quad.Draw(c, Color.yellow, true);
    }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(Stabilizer)), CanEditMultipleObjects]
public class StabilizerEditor : ShapedAirframeEditor
{
    SerializedProperty foil;
    protected override void OnEnable()
    {
        base.OnEnable();
        foil = serializedObject.FindProperty("foil");
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        EditorGUILayout.PropertyField(foil);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
