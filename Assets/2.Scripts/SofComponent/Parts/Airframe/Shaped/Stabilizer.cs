using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Stabilizer : ShapedAirframe
{
    public override float AreaCd() { return area * foil.airfoilSim.minCd; }

    protected override AirfoilSurface CreateFoilSurface()
    {
        return new ComplexAirfoilSurface(this, CreateQuad(), foil);
    }
    protected override void FixedUpdate()
    {
        foilSurface.ApplyForces();
    }
#if UNITY_EDITOR
    public override void Draw()
    {
        foilSurface.quad.Draw(vertical ? rudderColor : elevatorColor, bordersColor, true);
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
