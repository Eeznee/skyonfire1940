using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[AddComponentMenu("Sof Components/Aerodynamic Surfaces/Stabilizer")]
public class Stabilizer : ShapedAirframe
{
    private Propeller propeller;


    public override float HpPerSquareMeter => ModulesHPData.stabilizerHpPerSq;

    public override float AreaCd() { return foil ? area * foil.airfoilSim.minCd : 0f; }


    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);

        Propeller[] allPropellers = aircraft.GetComponentsInChildren<Propeller>();
        propeller = null;

        foreach(Propeller p in allPropellers)
        {
            Vector3 deltaPos = p.localPos - localPos;
            deltaPos.z = 0f;

            if (deltaPos.magnitude < p.preset.diameter) propeller = p;
        }
    }
    protected override AeroSurface CreateFoilSurface()
    {
        return new ComplexAeroSurface(this, CreateQuad(), foil);
    }
    public override float PropSpeedEffect()
    {
        if (!propeller) return 0f;
        if (!aircraft) return 0f;
        if (!data.grounded) return 0f;
        if (data.ias.Get < 0.1f) return 0f;

        float ias = data.ias.Get;
        float densArea = data.density.Get * propeller.preset.Area;

        float formula = Mathf.Abs(2f * propeller.thrust / densArea + ias * ias);
        float speedBoost = Mathf.Sqrt(formula) - ias;

        return speedBoost;
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
