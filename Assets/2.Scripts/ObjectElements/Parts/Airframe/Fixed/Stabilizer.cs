using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Stabilizer : Fuselage
{
    //Airfoil Settings
    public SimuFoil simuFoil;
    public Transform tr;
    public AirfoilPreset airfoil;
    public float angle = 0f;
    public float tipWidth = 100.0f;
    public float left = 1f;
    public bool rudder = false;

    private Vector3 rootTipLocal;

    //Control surface
    public ControlSurface controlSurface;
    public float csFractionSqrt;
    public float controlSpeed = 5f;

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        tr = FlightModel.AirfoilShapeTransform(transform, tr);

        if (firstTime)
        {
            vital = false;
            maxG = aircraft.maxG * 1.5f;
            CalculateAerofoilStructure();
        }
    }

    void FixedUpdate()
    {
        simuFoil.ApplyForces(tr.TransformDirection(rootTipLocal));
        ForcesStress(true, true);
    }

    public void CalculateAerofoilStructure()
    {
        rudder = Mathf.Abs(transform.root.InverseTransformDirection(tr.right).y) > 0.9f;
        left = Mathv.SignNoZero(transform.root.InverseTransformPoint(tr.position).x);
        if (rudder) left = 1f;
        float xScale = (left == 0f ? 1f : left) * Mathf.Abs(tr.localScale.x);
        tr.localScale = new Vector3(xScale,1f , tr.localScale.z);
        area = Mathf.Abs(tr.localScale.x) * tr.localScale.z * 0.5f * (1f + tipWidth/100f);

        rootTipLocal = ((Vector3.right * Mathf.Abs(tr.localScale.x)) + (Vector3.forward * Mathf.Abs(tr.localScale.x) / Mathf.Tan((90 - angle) * Mathf.Deg2Rad))).normalized;

        simuFoil.control = controlSurface;
        simuFoil.Init(tr, airfoil,Vector3.zero,area, Mathf.Abs(tr.localScale.x));

    }
#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        if (!tr) return;

        Color c = rudder ? Color.green : Color.red;
        c.a = 0.06f;
        Vector3 rootLiftPos = tr.position - (tr.right * (tr.localScale.x * 0.5f));
        Vector3 tipLiftPos = rootLiftPos + (tr.right * tr.localScale.x) + (tr.forward * Mathf.Abs(tr.localScale.x) / Mathf.Tan((90 - angle) * Mathf.Deg2Rad));
        Vector3 leadingBot = rootLiftPos + (tr.forward * tr.localScale.z * (1f - Quadrangle.liftLine));
        Vector3 trailingBot = rootLiftPos - (tr.forward * tr.localScale.z * Quadrangle.liftLine);
        Vector3 leadingTop = tipLiftPos + (tr.forward * tr.localScale.z * (1f - Quadrangle.liftLine) * tipWidth / 100f);
        Vector3 trailingTop = tipLiftPos - (tr.forward * tr.localScale.z * Quadrangle.liftLine * tipWidth / 100f);
        Features.DrawControlHandles(leadingBot, leadingTop, trailingTop, trailingBot, c, Color.yellow);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(rootLiftPos, tipLiftPos);
    }
#endif
}
#if UNITY_EDITOR
[CustomEditor(typeof(Stabilizer))]
public class StabilizerEditor : Editor
{
    Color backgroundColor;

    private static GUIContent deleteButton = new GUIContent("Remove", "Delete");
    public override void OnInspectorGUI()
    {
        backgroundColor = GUI.backgroundColor;
        Stabilizer stab = (Stabilizer)target;
        stab.tr = FlightModel.AirfoilShapeTransform(stab.transform, stab.tr);
        stab.CalculateAerofoilStructure();

        GUILayout.Space(20f);
        GUI.color = Color.cyan;
        EditorGUILayout.HelpBox("Airfoil Configuration", MessageType.None); //Airfoil shape configuration
        GUI.color = backgroundColor;

        stab.airfoil = EditorGUILayout.ObjectField("Airfoil", stab.airfoil, typeof(AirfoilPreset), false) as AirfoilPreset;
        stab.tipWidth = EditorGUILayout.Slider("Tip Width", stab.tipWidth, 5f, 100f);
        stab.angle = EditorGUILayout.Slider("Angle", stab.angle, -50f, 20f);
        EditorGUILayout.LabelField("Area", stab.area.ToString("0.00") + " m2");


        GUILayout.Space(20f);
        GUI.color = Color.green;
        EditorGUILayout.HelpBox("Damage Model", MessageType.None); //Damage and forces model
        GUI.color = backgroundColor;
        stab.detachable = true;
        SerializedProperty ripOnRip = serializedObject.FindProperty("ripOnRip");
        EditorGUILayout.PropertyField(ripOnRip, true);
        stab.emptyMass = EditorGUILayout.FloatField("Mass", stab.emptyMass);

        GUILayout.Space(20f);
        stab.controlSurface = stab.GetComponentInChildren<ControlSurface>();
        if (stab.controlSurface.transform.parent != stab.transform) stab.controlSurface = null;
        if (stab.controlSurface)
        {
            ControlSurface cs = stab.controlSurface;
            GUI.color = Color.magenta;
            EditorGUILayout.HelpBox("Control Surface", MessageType.None);
            GUI.color = backgroundColor;
            stab.controlSpeed = EditorGUILayout.FloatField("Control speed", stab.controlSpeed);
            cs.maxDeflection = Mathf.Abs(EditorGUILayout.FloatField("Positive Limit", cs.maxDeflection));
            cs.minDeflection = Mathf.Abs(EditorGUILayout.FloatField("Negative Limit", -cs.minDeflection));
            cs.effectiveSpeed = EditorGUILayout.FloatField("Eff Speed Km/h", Mathf.Round(cs.effectiveSpeed * 36f) / 10f) / 3.6f;
            cs.miniFoil.airfoil = EditorGUILayout.ObjectField("Airfoil", cs.miniFoil.airfoil, typeof(AirfoilPreset), false) as AirfoilPreset;
            cs.material = EditorGUILayout.ObjectField("Material", cs.material, typeof(PartMaterial), false) as PartMaterial;
            cs.emptyMass = EditorGUILayout.FloatField("Mass", cs.emptyMass);
            cs.detachable = true;
            EditorGUILayout.LabelField("Control Surface Area", cs.miniFoil.mainQuad.area.ToString("0.00") + " m2");
        }
        if (GUI.changed)
        {
            EditorUtility.SetDirty(stab);
            EditorSceneManager.MarkSceneDirty(stab.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }

}
#endif
